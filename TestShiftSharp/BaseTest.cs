/*
MIT License

Copyright (c) 2016 Kent Randall

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using Point85.ShiftSharp.Schedule;
using System;
using System.Collections.Generic;

namespace TestShiftSharp
{
	/// <summary>
	///  Base class for testing shift plans
	/// </summary>
	public abstract class BaseTest
	{
		// reference date for start of shift rotations
		protected LocalDate referenceDate = new LocalDate(2016, 10, 31);

		// partial test flags
		protected static bool testToString = true;

		protected static bool testDeletions = true;

		// a work schedule
		protected WorkSchedule schedule;

		[ClassInitialize()]
		public static void SetFlags(TestContext context)
		{
			testToString = true;
			testDeletions = true;
		}

		private void TestShifts(WorkSchedule ws)
		{
			Assert.IsTrue(ws.Shifts.Count > 0);

			foreach (Shift shift in ws.Shifts)
			{
				Duration total = shift.Duration;
				LocalTime start = shift.StartTime;
				LocalTime end = shift.GetEnd();

				Assert.IsTrue(shift.Name.Length > 0);
				Assert.IsTrue(shift.Description.Length > 0);

				Assert.IsTrue(total.TotalMinutes > 0);
				Assert.IsTrue(shift.Breaks != null);

				Duration worked;
				bool spansMidnight = shift.SpansMidnight();
				if (spansMidnight)
				{
					// get the interval before midnight
					worked = shift.CalculateWorkingTime(start, end, true);
				}
				else
				{
					worked = shift.CalculateWorkingTime(start, end);
				}
				Assert.IsTrue(worked.Equals(total));

				if (spansMidnight)
				{
					worked = shift.CalculateWorkingTime(start, start, true);
				}
				else
				{
					worked = shift.CalculateWorkingTime(start, start);
				}

				// 24 hour shift on midnight is a special case
				if (total.Equals(Duration.FromHours(24)))
				{
					Assert.IsTrue(worked.TotalHours == 24);
				}
				else
				{
					Assert.IsTrue(worked.TotalHours == 0);
				}

				if (spansMidnight)
				{
					worked = shift.CalculateWorkingTime(end, end, true);
				}
				else
				{
					worked = shift.CalculateWorkingTime(end, end);
				}

				if (total.Equals(Duration.FromHours(24)))
				{
					Assert.IsTrue(worked.TotalHours == 24);
				}
				else
				{
					Assert.IsTrue(worked.TotalHours == 0);
				}

				try
				{
					LocalTime t = start.Minus(Period.FromMinutes(1));
					worked = shift.CalculateWorkingTime(t, end);

					if (!total.Equals(shift.Duration))
					{
						Assert.Fail("Bad working time");
					}
				}
				catch (Exception)
				{
				}

				try
				{
					LocalTime t = end.Plus(Period.FromMinutes(1));
					worked = shift.CalculateWorkingTime(start, t);
					if (!total.Equals(shift.Duration))
					{
						Assert.Fail("Bad working time");
					}
				}
				catch (Exception)
				{
				}
			}
		}

		private void TestTeams(WorkSchedule ws, Duration hoursPerRotation, Duration rotationDays)
		{
			Assert.IsTrue(ws.Teams.Count > 0);

			foreach (Team team in ws.Teams)
			{
				Assert.IsTrue(team.Name.Length > 0);
				Assert.IsTrue(team.Description.Length > 0);
				Assert.IsTrue(team.GetDayInRotation(team.RotationStart) == 1);
				Duration hours = team.Rotation.GetWorkingTime();
				Assert.IsTrue(hours.Equals(hoursPerRotation));
				Assert.IsTrue(team.GetPercentageWorked() > 0.0f);
				Assert.IsTrue(team.GetRotationDuration().Equals(rotationDays));

				Rotation rotation = team.Rotation;
				Assert.IsTrue(rotation.GetDuration().Equals(rotationDays));
				Assert.IsTrue(rotation.GetPeriods().Count > 0);
				Assert.IsTrue(rotation.GetWorkingTime().TotalSeconds <= rotation.GetDuration().TotalSeconds);
			}

			Assert.IsTrue(ws.NonWorkingPeriods != null);
		}

		private void TestShiftInstances(WorkSchedule ws, LocalDate instanceReference)
		{
			Rotation rotation = ws.Teams[0].Rotation;

			// shift instances
			LocalDate startDate = instanceReference;
			LocalDate endDate = instanceReference.PlusDays(rotation.GetDuration().Days);
			
			long days = TimePeriod.DeltaDays(instanceReference, endDate) + 1;
			LocalDate day = startDate;

			for (long i = 0; i < days; i++)
			{
				List<ShiftInstance> instances = ws.GetShiftInstancesForDay(day);

				foreach (ShiftInstance instance in instances)
				{
					int isBefore = instance.StartDateTime.CompareTo(instance.GetEndTime());
					Assert.IsTrue(isBefore < 0);
					Assert.IsTrue(instance.Shift != null);
					Assert.IsTrue(instance.Team != null);

					Shift shift = instance.Shift;
					LocalTime startTime = shift.StartTime;
					LocalTime endTime = shift.GetEnd();

					Assert.IsTrue(shift.IsInShift(startTime));
					Assert.IsTrue(shift.IsInShift(startTime.PlusSeconds(1)));

					Duration shiftDuration = instance.Shift.Duration;

					// midnight is special case
					if (!shiftDuration.Equals(Duration.FromHours(24)))
					{
						Assert.IsFalse(shift.IsInShift(startTime.PlusSeconds(-1)));
					}

					Assert.IsTrue(shift.IsInShift(endTime));
					Assert.IsTrue(shift.IsInShift(endTime.PlusSeconds(-1)));

					if (!shiftDuration.Equals(Duration.FromHours(24)))
					{
						Assert.IsFalse(shift.IsInShift(endTime.PlusSeconds(1)));
					}

					LocalDateTime ldt = day.At(startTime);
					Assert.IsTrue(ws.GetShiftInstancesForTime(ldt).Count > 0);

					ldt = day.At(startTime.PlusSeconds(1));
					Assert.IsTrue(ws.GetShiftInstancesForTime(ldt).Count > 0);

					ldt = day.At(startTime.PlusSeconds(-1));

					foreach (ShiftInstance si in ws.GetShiftInstancesForTime(ldt))
					{
						if (!shiftDuration.Equals(Duration.FromHours(24)))
						{
							Assert.IsFalse(shift.Name.Equals(si.Shift.Name));
						}
					}

					ldt = day.At(endTime);
					Assert.IsTrue(ws.GetShiftInstancesForTime(ldt).Count > 0);

					ldt = day.At(endTime.PlusSeconds(-1));
					Assert.IsTrue(ws.GetShiftInstancesForTime(ldt).Count > 0);

					ldt = day.At(endTime.PlusSeconds(1));

					foreach (ShiftInstance si in ws.GetShiftInstancesForTime(ldt))
					{
						if (!shiftDuration.Equals(Duration.FromHours(24)))
						{
							Assert.IsFalse(shift.Name.Equals(si.Shift.Name));
						}
					}
				}

				day = day.PlusDays(1);
			}
		}

		protected void RunBaseTest(WorkSchedule ws, Duration hoursPerRotation, Duration rotationDays,
				LocalDate instanceReference)
		{

			// toString
			if (testToString)
			{
				Console.WriteLine(ws.ToString());
				ws.PrintShiftInstances(instanceReference, instanceReference.PlusDays(rotationDays.Days));
			}

			Assert.IsTrue(ws.Name.Length > 0);
			Assert.IsTrue(ws.Description.Length > 0);
			Assert.IsTrue(ws.NonWorkingPeriods != null);

			// shifts
			TestShifts(ws);

			// teams
			TestTeams(ws, hoursPerRotation, rotationDays);

			// shift instances
			TestShiftInstances(ws, instanceReference.PlusDays(rotationDays.Days));

			if (testDeletions)
			{
				TestDeletions();
			}
		}

		private void TestDeletions()
		{
			// team deletions
			Team[] teams = schedule.Teams.ToArray();

			foreach (Team team in teams)
			{
				schedule.DeleteTeam(team);
			}
			Assert.IsTrue(schedule.Teams.Count == 0);

			// shift deletions
			Shift[] shifts = schedule.Shifts.ToArray();

			foreach (Shift shift in shifts)
			{
				schedule.DeleteShift(shift);
			}
			Assert.IsTrue(schedule.Shifts.Count == 0);

			// non-working period deletions
			NonWorkingPeriod[] periods = schedule.NonWorkingPeriods.ToArray();

			foreach (NonWorkingPeriod period in periods)
			{
				schedule.DeleteNonWorkingPeriod(period);
			}
			Assert.IsTrue(schedule.NonWorkingPeriods.Count == 0);
		}
	}
}
