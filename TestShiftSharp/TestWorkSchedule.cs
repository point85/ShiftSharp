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
	[TestClass]
	public class TestWorkSchedule : BaseTest
	{
		[TestMethod]
		public void TestNursingICUShifts()
		{
			// ER nursing schedule
			schedule = new WorkSchedule("Nursing ICU",
					"Two 12 hr back-to-back shifts, rotating every 14 days");

			// day shift, starts at 06:00 for 12 hours
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(6, 0, 0), Duration.FromHours(12));

			// night shift, starts at 18:00 for 12 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(18, 0, 0), Duration.FromHours(12));

			// day rotation
			Rotation dayRotation = new Rotation("Day", "Day");
			dayRotation.AddSegment(day, 3, 4);
			dayRotation.AddSegment(day, 4, 3);

			// inverse day rotation (day + 3 days)
			Rotation inverseDayRotation = new Rotation("Inverse Day", "Inverse Day");
			inverseDayRotation.AddSegment(day, 0, 3);
			inverseDayRotation.AddSegment(day, 4, 4);
			inverseDayRotation.AddSegment(day, 3, 0);

			// night rotation
			Rotation nightRotation = new Rotation("Night", "Night");
			nightRotation.AddSegment(night, 4, 3);
			nightRotation.AddSegment(night, 3, 4);

			// inverse night rotation
			Rotation inverseNightRotation = new Rotation("Inverse Night", "Inverse Night");
			inverseNightRotation.AddSegment(night, 0, 4);
			inverseNightRotation.AddSegment(night, 3, 3);
			inverseNightRotation.AddSegment(night, 4, 0);

			LocalDate rotationStart = new LocalDate(2014, 1, 6);

			schedule.CreateTeam("A", "Day shift", dayRotation, rotationStart);
			schedule.CreateTeam("B", "Day inverse shift", inverseDayRotation, rotationStart);
			schedule.CreateTeam("C", "Night shift", nightRotation, rotationStart);
			schedule.CreateTeam("D", "Night inverse shift", inverseNightRotation, rotationStart);

			runBaseTest(schedule, Duration.FromHours(84), Duration.FromDays(14), rotationStart);
		}

		[TestMethod]
		public void TestPostalServiceShifts()
		{
			// United States Postal Service
			schedule = new WorkSchedule("USPS", "Six 9 hr shifts, rotating every 42 days");

			// shift, start at 08:00 for 9 hours
			Shift day = schedule.CreateShift("Day", "day shift", new LocalTime(8, 0, 0), Duration.FromHours(9));

			Rotation rotation = new Rotation("Day", "Day");
			rotation.AddSegment(day, 3, 7);
			rotation.AddSegment(day, 1, 7);
			rotation.AddSegment(day, 1, 7);
			rotation.AddSegment(day, 1, 7);
			rotation.AddSegment(day, 1, 7);

			LocalDate rotationStart = new LocalDate(2017, 1, 27);

			// day teams
			schedule.CreateTeam("Team A", "A team", rotation, rotationStart);
			schedule.CreateTeam("Team B", "B team", rotation, rotationStart.Minus(Period.FromDays(7)));
			schedule.CreateTeam("Team C", "C team", rotation, rotationStart.Minus(Period.FromDays(14)));
			schedule.CreateTeam("Team D", "D team", rotation, rotationStart.Minus(Period.FromDays(21)));
			schedule.CreateTeam("Team E", "E team", rotation, rotationStart.Minus(Period.FromDays(28)));
			schedule.CreateTeam("Team F", "F team", rotation, rotationStart.Minus(Period.FromDays(35)));

			runBaseTest(schedule, Duration.FromHours(63), Duration.FromDays(42), rotationStart);
		}

		[TestMethod]
		public void TestFirefighterShifts2()
		{
			// Seattle, WA fire shifts
			schedule = new WorkSchedule("Seattle", "Four 24 hour alternating shifts");

			// shift, start at 07:00 for 24 hours
			Shift shift = schedule.CreateShift("24 Hours", "24 hour shift", new LocalTime(7, 0, 0), Duration.FromHours(24));

			// 1 day ON, 4 OFF, 1 ON, 2 OFF
			Rotation rotation = new Rotation("24 Hours", "24 Hours");
			rotation.AddSegment(shift, 1, 4);
			rotation.AddSegment(shift, 1, 2);

			schedule.CreateTeam("A", "Platoon1", rotation, new LocalDate(2014, 2, 2));
			schedule.CreateTeam("B", "Platoon2", rotation, new LocalDate(2014, 2, 4));
			schedule.CreateTeam("C", "Platoon3", rotation, new LocalDate(2014, 1, 31));
			schedule.CreateTeam("D", "Platoon4", rotation, new LocalDate(2014, 1, 29));

			runBaseTest(schedule, Duration.FromHours(48), Duration.FromDays(8), new LocalDate(2014, 2, 4));
		}

		[TestMethod]
		public void TestFirefighterShifts1()
		{
			// Kern Co, CA
			schedule = new WorkSchedule("Kern Co.", "Three 24 hour alternating shifts");

			// shift, start 07:00 for 24 hours
			Shift shift = schedule.CreateShift("24 Hour", "24 hour shift", new LocalTime(7, 0, 0), Duration.FromHours(24));

			// 2 days ON, 2 OFF, 2 ON, 2 OFF, 2 ON, 8 OFF
			Rotation rotation = new Rotation("24 Hour", "2 days ON, 2 OFF, 2 ON, 2 OFF, 2 ON, 8 OFF");
			rotation.AddSegment(shift, 2, 2);
			rotation.AddSegment(shift, 2, 2);
			rotation.AddSegment(shift, 2, 8);

			Team platoon1 = schedule.CreateTeam("Red", "A Shift", rotation, new LocalDate(2017, 1, 8));
			Team platoon2 = schedule.CreateTeam("Black", "B Shift", rotation, new LocalDate(2017, 2, 1));
			Team platoon3 = schedule.CreateTeam("Green", "C Shift", rotation, new LocalDate(2017, 1, 2));

			List<ShiftInstance> instances = schedule.GetShiftInstancesForDay(new LocalDate(2017, 3, 1));
			Assert.IsTrue(instances.Count == 1);
			Assert.IsTrue(instances[0].GetTeam().Equals(platoon3));

			instances = schedule.GetShiftInstancesForDay(new LocalDate(2017, 3, 3));
			Assert.IsTrue(instances.Count == 1);
			Assert.IsTrue(instances[0].GetTeam().Equals(platoon1));

			instances = schedule.GetShiftInstancesForDay(new LocalDate(2017, 3, 9));
			Assert.IsTrue(instances.Count == 1);
			Assert.IsTrue(instances[0].GetTeam().Equals(platoon2));

			runBaseTest(schedule, Duration.FromHours(144), Duration.FromDays(18), new LocalDate(2017, 2, 1));
		}

		[TestMethod]
		public void TestManufacturingShifts()
		{
			// manufacturing company
			schedule = new WorkSchedule("Manufacturing Company - four twelves",
						"Four 12 hour alternating day/night shifts");

			// day shift, start at 07:00 for 12 hours
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(12));

			// night shift, start at 19:00 for 12 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(19, 0, 0), Duration.FromHours(12));

			// 7 days ON, 7 OFF
			Rotation dayRotation = new Rotation("Day", "Day");
			dayRotation.AddSegment(day, 7, 7);

			// 7 nights ON, 7 OFF
			Rotation nightRotation = new Rotation("Night", "Night");
			nightRotation.AddSegment(night, 7, 7);

			schedule.CreateTeam("A", "A day shift", dayRotation, new LocalDate(2014, 1, 2));
			schedule.CreateTeam("B", "B night shift", nightRotation, new LocalDate(2014, 1, 2));
			schedule.CreateTeam("C", "C day shift", dayRotation, new LocalDate(2014, 1, 9));
			schedule.CreateTeam("D", "D night shift", nightRotation, new LocalDate(2014, 1, 9));

			runBaseTest(schedule, Duration.FromHours(84), Duration.FromDays(14), new LocalDate(2014, 1, 9));
		}

		[TestMethod]
		public void TestGenericShift()
		{
			// regular work week with holidays and breaks
			schedule = new WorkSchedule("Regular 40 hour work week", "9 to 5");

			try
			{
				schedule.SetName(null);
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			// holidays
			NonWorkingPeriod memorialDay = schedule.CreateNonWorkingPeriod("MEMORIAL DAY", "Memorial day", new LocalDateTime(2016, 5, 30, 0, 0, 0),
					Duration.FromHours(24));
			schedule.CreateNonWorkingPeriod("INDEPENDENCE DAY", "Independence day", new LocalDateTime(2016, 7, 4, 0, 0, 0),
							Duration.FromHours(24));
			schedule.CreateNonWorkingPeriod("LABOR DAY", "Labor day", new LocalDateTime(2016, 9, 5, 0, 0, 0),
					Duration.FromHours(24));
			schedule.CreateNonWorkingPeriod("THANKSGIVING", "Thanksgiving day and day after",
					new LocalDateTime(2016, 11, 24, 0, 0, 0), Duration.FromHours(48));
			schedule.CreateNonWorkingPeriod("CHRISTMAS SHUTDOWN", "Christmas week scheduled maintenance",
					new LocalDateTime(2016, 12, 25, 0, 30, 0), Duration.FromHours(168));

			// each shift duration
			Duration shiftDuration = Duration.FromHours(8);
			LocalTime shift1Start = new LocalTime(7, 0, 0);
			LocalTime shift2Start = new LocalTime(15, 0, 0);

			// shift 1
			Shift shift1 = schedule.CreateShift("Shift1", "Shift #1", shift1Start, shiftDuration);

			// breaks
			shift1.CreateBreak("10AM", "10 am break", new LocalTime(10, 0, 0), Duration.FromMinutes(15));
			shift1.CreateBreak("LUNCH", "lunch", new LocalTime(12, 0, 0), Duration.FromHours(1));
			shift1.CreateBreak("2PM", "2 pm break", new LocalTime(14, 0, 0), Duration.FromMinutes(15));

			// shift 2
			Shift shift2 = schedule.CreateShift("Shift2", "Shift #2", shift2Start, shiftDuration);

			// shift 1, 5 days ON, 2 OFF
			Rotation rotation1 = new Rotation("Shift1", "Shift1");
			rotation1.AddSegment(shift1, 5, 2);

			// shift 2, 5 days ON, 2 OFF
			Rotation rotation2 = new Rotation("Shift2", "Shift2");
			rotation2.AddSegment(shift2, 5, 2);

			LocalDate startRotation = new LocalDate(2016, 1, 1);
			Team team1 = schedule.CreateTeam("Team1", "Team #1", rotation1, startRotation);
			Team team2 = schedule.CreateTeam("Team2", "Team #2", rotation2, startRotation);

			// same day
			LocalDateTime from = startRotation.PlusDays(7).At(shift1Start);
			LocalDateTime to;

			Duration totalWorking = Duration.Zero;

			// 21 days, team1
			Duration d = Duration.Zero;

			for (int i = 0; i < 21; i++)
			{
				to = from.PlusDays(i);
				totalWorking = team1.CalculateWorkingTime(from, to);
				int dir = team1.GetDayInRotation(to.Date);

				Assert.IsTrue(totalWorking.Equals(d));

				TimePeriod period = rotation1.GetPeriods()[dir - 1];
				if (period is Shift)
				{
					d = d.Plus(shiftDuration);
				}
			}
			Duration totalSchedule = totalWorking;

			// 21 days, team2
			from = startRotation.PlusDays(7).At(shift2Start);
			d = Duration.Zero;

			for (int i = 0; i < 21; i++)
			{
				to = from.PlusDays(i);
				totalWorking = team2.CalculateWorkingTime(from, to);
				int dir = team2.GetDayInRotation(to.Date);

				Assert.IsTrue(totalWorking.Equals(d));

				if (rotation1.GetPeriods()[dir - 1] is Shift)
				{
					d = d.Plus(shiftDuration);
				}
			}
			totalSchedule = totalSchedule.Plus(totalWorking);

			Duration scheduleDuration = schedule.CalculateWorkingTime(from, from.PlusDays(21));
			Duration nonWorkingDuration = schedule.CalculateNonWorkingTime(from, from.PlusDays(21));
			Assert.IsTrue(scheduleDuration.Plus(nonWorkingDuration).Equals(totalSchedule));

			// breaks
			Duration allBreaks = Duration.FromMinutes(90);
			Assert.IsTrue(shift1.CalculateBreakTime().Equals(allBreaks));

			// misc
			WorkSchedule schedule2 = new WorkSchedule();

			Shift shift3 = new Shift();
			shift3.SetName("Shift3");
			Assert.IsTrue(shift3.GetWorkSchedule() == null);
			Assert.IsTrue(shift3.CompareTo(shift3) == 0);

			Team team3 = new Team();
			Assert.IsTrue(team3.GetWorkSchedule() == null);

			RotationSegment segment = new RotationSegment();
			segment.SetSequence(1);
			segment.SetStartingShift(shift2);
			segment.SetDaysOn(5);
			segment.SetDaysOff(2);
			Assert.IsTrue(segment.GetRotation() == null);

			Rotation rotation3 = new Rotation();
			rotation3.SetName("Rotation3");
			Assert.IsTrue(rotation3.CompareTo(rotation3) == 0);
			Assert.IsTrue(rotation3.GetRotationSegments().Count == 0);

			NonWorkingPeriod nwp = new NonWorkingPeriod();
			Assert.IsTrue(nwp.GetWorkSchedule() == null);


			Assert.IsTrue(team1.GetWorkSchedule().Equals(schedule));


			Assert.IsTrue(!team1.IsDayOff(startRotation));


			Assert.IsTrue(team1.CompareTo(team1) == 0);
			team3.SetRotation(rotation1);


			Assert.IsTrue(!memorialDay.IsInPeriod(new LocalDate(2016, 1, 1)));

			runBaseTest(schedule, Duration.FromHours(40), Duration.FromDays(7), new LocalDate(2016, 1, 1));

		}

		[TestMethod]
		public void TestExceptions()
		{
			schedule = new WorkSchedule("Exceptions", "Test exceptions");
			Duration shiftDuration = Duration.FromHours(24);
			LocalTime shiftStart = new LocalTime(7, 0, 0);

			NonWorkingPeriod period = schedule.CreateNonWorkingPeriod("Non-working", "Non-working period",
					new LocalDateTime(2017, 1, 1, 0, 0, 0), Duration.FromHours(24));

			try
			{
				period.SetDuration(Duration.FromSeconds(0));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			try
			{
				// same period
				schedule.CreateNonWorkingPeriod("Non-working", "Non-working period", new LocalDateTime(2017, 1, 1, 0, 0, 0),
					Duration.FromHours(24));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			// shift
			Shift shift = schedule.CreateShift("Test", "Test shift", shiftStart, shiftDuration);

			try
			{
				// crosses midnight
				shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shift.GetEnd().PlusHours(1));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			try
			{
				shift.SetDuration(Duration.FromSeconds(0));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			try
			{
				shift.SetDuration(Duration.FromSeconds(48 * 3600));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			try
			{
				// same shift
				shift = schedule.CreateShift("Test", "Test shift", shiftStart, shiftDuration);
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			Rotation rotation = new Rotation("Rotation", "Rotation");
			rotation.AddSegment(shift, 5, 2);

			LocalDate startRotation = new LocalDate(2016, 12, 31);
			Team team = schedule.CreateTeam("Team", "Team", rotation, startRotation);

			// ok
			schedule.CalculateWorkingTime(new LocalDateTime(2017, 1, 1, 7, 0, 0), new LocalDateTime(2017, 2, 1, 0, 0, 0));

			try
			{
				// end before start
				schedule.CalculateWorkingTime(new LocalDateTime(2017, 1, 2, 0, 0, 0), new LocalDateTime(2017, 1, 1, 0, 0, 0));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			try
			{
				// same team
				team = schedule.CreateTeam("Team", "Team", rotation, startRotation);
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			try
			{
				// date before start
				team.GetDayInRotation(new LocalDate(2016, 1, 1));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			try
			{
				// end before start
				schedule.PrintShiftInstances(new LocalDate(2017, 1, 2), new LocalDate(2017, 1, 1));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			try
			{
				// delete in-use shift
				schedule.DeleteShift(shift);
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			// breaks
			Break lunch = shift.CreateBreak("Lunch", "Lunch", new LocalTime(12, 0, 0), Duration.FromMinutes(60));
			lunch.SetDuration(Duration.FromMinutes(30));
			lunch.SetStart(new LocalTime(11, 30, 0));
			shift.RemoveBreak(lunch);
			shift.RemoveBreak(lunch);

			Shift shift2 = schedule.CreateShift("Test2", "Test shift2", shiftStart, shiftDuration);
			Assert.IsFalse(shift.Equals(shift2));

			Break lunch2 = shift2.CreateBreak("Lunch2", "Lunch", new LocalTime(12, 0, 0), Duration.FromMinutes(60));
			shift.RemoveBreak(lunch2);

			// ok to delete
			WorkSchedule schedule2 = new WorkSchedule("Exceptions2", "Test exceptions2");
			schedule2.SetName("Schedule 2");
			schedule2.SetDescription("a description");

			schedule2.DeleteShift(shift);
			schedule2.DeleteTeam(team);
			schedule2.DeleteNonWorkingPeriod(period);

			// nulls
			try
			{
				schedule.CreateShift(null, "1", shiftStart, Duration.FromMinutes(60));
				Assert.Fail();
			}
			catch (Exception)
			{
			}

			Assert.IsFalse(shift.Equals(rotation));

			// hashcode()
			team.GetHashCode();
			String name = team.GetName();
			Dictionary<String, Team> teams = new Dictionary<String, Team>();
			teams[name] = team;
			Team t = teams[name];
		}

		[TestMethod]
		public void TestShiftWorkingTime()
		{
			schedule = new WorkSchedule("Working Time1", "Test working time");

			// shift does not cross midnight
			Duration shiftDuration = Duration.FromHours(8);
			LocalTime shiftStart = new LocalTime(7, 0, 0);

			Shift shift = schedule.CreateShift("Work Shift1", "Working time shift", shiftStart, shiftDuration);
			LocalTime shiftEnd = shift.GetEnd();

			// case #1
			Duration time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(2)));
			Assert.IsTrue(time.TotalSeconds == 0);
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(3)));
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #2
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftStart.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #3
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(2));
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #4
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #5
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(2));
			Assert.IsTrue(time.TotalSeconds == 0);
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #6
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #7
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #8
			time = shift.CalculateWorkingTime(shiftStart, shiftEnd);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #9
			time = shift.CalculateWorkingTime(shiftStart, shiftStart);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #10
			time = shift.CalculateWorkingTime(shiftEnd, shiftEnd);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #11
			time = shift.CalculateWorkingTime(shiftStart, shiftStart.PlusSeconds(1));
			Assert.IsTrue(time.TotalSeconds == 1);

			// case #12
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromSeconds(1)), shiftEnd);
			Assert.IsTrue(time.TotalSeconds == 1);

			// 8 hr shift crossing midnight
			shiftStart = new LocalTime(22, 0, 0);

			shift = schedule.CreateShift("Work Shift2", "Working time shift", shiftStart, shiftDuration);
			shiftEnd = shift.GetEnd();

			// case #1
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(2)), true);
			Assert.IsTrue(time.TotalSeconds == 0);
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(3)), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #2
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftStart.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #3
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(2), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #4
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1), false);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #5
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(2), true);
			Assert.IsTrue(time.TotalSeconds == 0);
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #6
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #7
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #8
			time = shift.CalculateWorkingTime(shiftStart, shiftEnd, true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #9
			time = shift.CalculateWorkingTime(shiftStart, shiftStart, true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #10
			time = shift.CalculateWorkingTime(shiftEnd, shiftEnd, true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #11
			time = shift.CalculateWorkingTime(shiftStart, shiftStart.PlusSeconds(1), true);
			Assert.IsTrue(time.TotalSeconds == 1);

			// case #12
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromSeconds(1)), shiftEnd, false);
			Assert.IsTrue(time.TotalSeconds == 1);

			// 24 hr shift crossing midnight
			shiftDuration = Duration.FromHours(24);
			shiftStart = new LocalTime(7, 0, 0);

			shift = schedule.CreateShift("Work Shift3", "Working time shift", shiftStart, shiftDuration);
			shiftEnd = shift.GetEnd();

			// case #1
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(2)), false);
			Assert.IsTrue(time.TotalSeconds == 3600);
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(3)), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #2
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftStart.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #3
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(2), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #4
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #5
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(2), true);
			Assert.IsTrue(time.TotalSeconds == 3600);
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #6
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #7
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #8
			time = shift.CalculateWorkingTime(shiftStart, shiftEnd, true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #9
			time = shift.CalculateWorkingTime(shiftStart, shiftStart, true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #10
			time = shift.CalculateWorkingTime(shiftEnd, shiftEnd, true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #11
			time = shift.CalculateWorkingTime(shiftStart, shiftStart.PlusSeconds(1), true);
			Assert.IsTrue(time.TotalSeconds == 1);

			// case #12
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromSeconds(1)), shiftEnd, false);
			Assert.IsTrue(time.TotalSeconds == 1);
		}

		[TestMethod]
		public void TestTeamWorkingTime()
		{
			schedule = new WorkSchedule("Team Working Time", "Test team working time");
			Duration shiftDuration = Duration.FromHours(12);
			Duration halfShift = Duration.FromHours(6);
			LocalTime shiftStart = new LocalTime(7, 0, 0);

			Shift shift = schedule.CreateShift("Team Shift1", "Team shift 1", shiftStart, shiftDuration);

			Rotation rotation = new Rotation("Team", "Rotation");
			rotation.AddSegment(shift, 1, 1);

			LocalDate startRotation = new LocalDate(2017, 1, 1);
			Team team = schedule.CreateTeam("Team", "Team", rotation, startRotation);
			team.SetRotationStart(startRotation);

			// case #1
			LocalDateTime from = startRotation.PlusDays(rotation.GetDayCount()).At(shiftStart);
			LocalDateTime to = from.PlusDays(1);
			Duration time = team.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration));

			// case #2
			to = from.PlusDays(2);
			time = team.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration));

			// case #3
			to = from.PlusDays(3);
			time = team.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration.Plus(shiftDuration)));

			// case #4
			to = from.PlusDays(4);
			time = team.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration.Plus(shiftDuration)));

			// case #5
			from = startRotation.PlusDays(rotation.GetDayCount()).At(shiftStart.PlusHours(6));
			to = from.PlusDays(1);
			time = team.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(halfShift));

			// case #6
			to = from.PlusDays(2);
			time = team.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration));

			// case #7
			to = from.PlusDays(3);
			time = team.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration.Plus(halfShift)));

			// case #8
			to = from.PlusDays(4);
			time = team.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration.Plus(shiftDuration)));

			// now crossing midnight
			shiftStart = new LocalTime(18, 0, 0);
			Shift shift2 = schedule.CreateShift("Team Shift2", "Team shift 2", shiftStart, shiftDuration);

			Rotation rotation2 = new Rotation("Case 8", "Case 8");
			rotation2.AddSegment(shift2, 1, 1);

			Team team2 = schedule.CreateTeam("Team2", "Team 2", rotation2, startRotation);
			team2.SetRotationStart(startRotation);

			// case #1
			from = startRotation.PlusDays(rotation.GetDayCount()).At(shiftStart);
			to = from.PlusDays(1);
			time = team2.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration));

			// case #2
			to = from.PlusDays(2);
			time = team2.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration));

			// case #3
			to = from.PlusDays(3);
			time = team2.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration.Plus(shiftDuration)));

			// case #4
			to = from.PlusDays(4);
			time = team2.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration.Plus(shiftDuration)));

			// case #5
			from = startRotation.PlusDays(rotation.GetDayCount()).At(LocalTime.MaxValue);
			to = from.PlusDays(1);
			time = team2.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(halfShift));

			// case #6
			to = from.PlusDays(2);
			time = team2.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration));

			// case #7
			to = from.PlusDays(3);
			time = team2.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration.Plus(halfShift)));

			// case #8
			to = from.PlusDays(4);
			time = team2.CalculateWorkingTime(from, to);
			Assert.IsTrue(time.Equals(shiftDuration.Plus(shiftDuration)));
		}

		[TestMethod]
		public void TestNonWorkingTime()
		{
			schedule = new WorkSchedule("Non Working Time", "Test non working time");
			LocalDate date = new LocalDate(2017, 1, 1);
			LocalTime time = new LocalTime(7, 0, 0);

			NonWorkingPeriod period1 = schedule.CreateNonWorkingPeriod("Day1", "First test day",
					date.At(LocalTime.Midnight), Duration.FromHours(24));
			NonWorkingPeriod period2 = schedule.CreateNonWorkingPeriod("Day2", "First test day",
					date.PlusDays(7).At(time), Duration.FromHours(24));

			LocalDateTime from = date.At(time);
			LocalDateTime to = date.At(time.PlusHours(1));

			// case #1
			Duration duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(1)));

			// case #2
			from = date.Minus(Period.FromDays(1)).At(time);
			to = date.PlusDays(1).At(time);
			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(24)));

			// case #3
			from = date.Minus(Period.FromDays(1)).At(time);
			to = date.Minus(Period.FromDays(1)).At(time.PlusHours(1));
			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(0)));

			// case #4
			from = date.PlusDays(1).At(time);
			to = date.PlusDays(1).At(time.PlusHours(1));
			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(0)));

			// case #5
			from = date.Minus(Period.FromDays(1)).At(time);
			to = date.At(time);
			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(7)));

			// case #6
			from = date.At(time);
			to = date.PlusDays(1).At(time);
			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(17)));

			// case #7
			from = date.At(LocalTime.Noon);
			to = date.PlusDays(7).At(LocalTime.Noon);
			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(17)));

			// case #8
			from = date.Minus(Period.FromDays(1)).At(LocalTime.Noon);
			to = date.PlusDays(8).At(LocalTime.Noon);
			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(48)));

			// case #9
			schedule.DeleteNonWorkingPeriod(period1);
			schedule.DeleteNonWorkingPeriod(period2);
			from = date.At(time);
			to = date.At(time.PlusHours(1));

			// case #10
			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(0)));

			Duration shiftDuration = Duration.FromHours(8);
			LocalTime shiftStart = new LocalTime(7, 0, 0);

			Shift shift = schedule.CreateShift("Work Shift1", "Working time shift", shiftStart, shiftDuration);

			Rotation rotation = new Rotation("Case 10", "Case10");
			rotation.AddSegment(shift, 1, 1);

			LocalDate startRotation = new LocalDate(2017, 1, 1);
			Team team = schedule.CreateTeam("Team", "Team", rotation, startRotation);
			team.SetRotationStart(startRotation);

			period1 = schedule.CreateNonWorkingPeriod("Day1", "First test day", date.At(LocalTime.Midnight),
					Duration.FromHours(24));

			LocalDate mark = date.PlusDays(rotation.GetDayCount());
			from = mark.At(time.Minus(Period.FromHours(2)));
			to = mark.At(time.Minus(Period.FromHours(1)));

			// case #11
			duration = schedule.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(0)));

			// case #12
			from = date.At(shiftStart);
			to = date.At(time.PlusHours(8));

			duration = schedule.CalculateNonWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(8)));
		}

		[TestMethod]
		public void TestTeamWorkingTime2()
		{
			schedule = new WorkSchedule("4 Team Plan", "test schedule");

			// Day shift #1, starts at 07:00 for 15.5 hours
			Shift crossover = schedule.CreateShift("Crossover", "Day shift #1 cross-over", new LocalTime(7, 0, 0),
					Duration.FromHours(15).Plus(Duration.FromMinutes(30)));

			// Day shift #2, starts at 07:00 for 14 hours
			Shift day = schedule.CreateShift("Day", "Day shift #2", new LocalTime(7, 0, 0), Duration.FromHours(14));

			// Night shift, starts at 22:00 for 14 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(22, 0, 0), Duration.FromHours(14));

			// Team 4-day rotation
			Rotation rotation = new Rotation("4 Team", "4 Team");
			rotation.AddSegment(day, 1, 0);
			rotation.AddSegment(crossover, 1, 0);
			rotation.AddSegment(night, 1, 1);

			Team team1 = schedule.CreateTeam("Team 1", "First team", rotation, referenceDate);

			// partial in Day 1

			LocalTime am7 = new LocalTime(7, 0, 0);
			LocalDate testStart = referenceDate.PlusDays(rotation.GetDayCount());
			LocalDateTime from = testStart.At(am7);
			LocalDateTime to = testStart.At(am7.PlusHours(1));


			//------------------------------------------------------------------
			// from first day in rotation for Team1
			from = testStart.At(LocalTime.Midnight);
			to = testStart.At(LocalTime.MaxValue);

			Duration duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(14)));

			to = testStart.PlusDays(1).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(29).Plus(Duration.FromMinutes(30))));

			to = testStart.PlusDays(2).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(31).Plus(Duration.FromMinutes(30))));

			to = testStart.PlusDays(3).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(43).Plus(Duration.FromMinutes(30))));

			to = testStart.PlusDays(4).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(57).Plus(Duration.FromMinutes(30))));

			to = testStart.PlusDays(5).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(73)));

			to = testStart.PlusDays(6).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(75)));

			to = testStart.PlusDays(7).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(87)));

			// from third day in rotation for Team1
			from = testStart.PlusDays(2).At(LocalTime.Midnight);
			to = testStart.PlusDays(2).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(2)));

			to = testStart.PlusDays(3).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(14)));

			to = testStart.PlusDays(4).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(28)));

			to = testStart.PlusDays(5).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(43).Plus(Duration.FromMinutes(30))));

			to = testStart.PlusDays(6).At(LocalTime.MaxValue);
			duration = team1.CalculateWorkingTime(from, to);
			Assert.IsTrue(duration.Equals(Duration.FromHours(45).Plus(Duration.FromMinutes(30))));
		}

		[TestMethod]
		public void TestPeriod()
		{
			// second of day
			LocalTime t = new LocalTime(0, 0, 0);
			Assert.IsTrue(TimePeriod.SecondOfDay(t) == 0);

			t = LocalTime.Midnight;
			Assert.IsTrue(TimePeriod.SecondOfDay(t) == 0);

			t = LocalTime.Noon;
			Assert.IsTrue(TimePeriod.SecondOfDay(t) == 43200);

			t = new LocalTime(23, 59, 59);
			Assert.IsTrue(TimePeriod.SecondOfDay(t) == 86399);

			t = LocalTime.MaxValue;
			Assert.IsTrue(TimePeriod.SecondOfDay(t) == 86399);

			// delta days
			LocalDate start = new LocalDate(2107, 1, 1);
			LocalDate end = new LocalDate(2107, 1, 1);

			Assert.IsTrue(TimePeriod.DeltaDays(start, end) == 0);

			for (int i = 0; i < 10; i++)
			{
				LocalDate next = end.PlusDays(i);
				long n = TimePeriod.DeltaDays(start, next);
				Assert.IsTrue(n == i);
			}
		}
	} // class
} // namespace
