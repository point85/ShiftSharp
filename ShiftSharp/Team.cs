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

using NodaTime;
using System;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class Team is a named group of individuals who rotate through a shift schedule.
	/// </summary>
	public class Team : Named, IComparable<Team>
	{
		// owning work schedule
		public WorkSchedule WorkSchedule { get; set; }

		// reference date for starting the rotations
		public LocalDate RotationStart { get; set; }

		// shift rotation days
		public Rotation Rotation { get; set; }

		/**
		 * Default constructor
		 */
		public Team() : base()
		{
		}

		internal Team(string name, string description, Rotation rotation, LocalDate rotationStart) : base(name, description)
		{
			this.Rotation = rotation;
			this.RotationStart = rotationStart;
		}

		/**
		 * Get the duration of the shift rotation
		 * 
		 * @return Duration
		 * @throws Exception
		 *             exception
		 */
		public Duration GetRotationDuration()
		{
			return Rotation.GetDuration();
		}

		/**
		 * Get the shift rotation's working time as a percentage of the rotation
		 * duration
		 * 
		 * @return Percentage
		 * @throws Exception
		 *             exception
		 */
		public float GetPercentageWorked()
		{
			double ratio = Rotation.GetWorkingTime().TotalSeconds / GetRotationDuration().TotalSeconds;
			return (float)ratio * 100.0f;
		}

		/**
		 * Get the average number of hours worked each week by this team
		 * 
		 * @return Duration of hours worked per week
		 */
		public Duration GetHoursWorkedPerWeek()
		{
			double days = Rotation.GetDuration().TotalDays;
			double secPerWeek = Rotation.GetWorkingTime().TotalSeconds * (7.0f / days);
			return Duration.FromSeconds(secPerWeek);
		}

		/**
		 * Get the day number in the rotation for this local date
		 * 
		 * @param date
		 *            LocalDate
		 * @return day number in the rotation, starting at 1
		 * @throws Exception
		 *             exception
		 */
		public int GetDayInRotation(LocalDate date)
		{
			// calculate total number of days from start of rotation
			long deltaDays = TimePeriod.DeltaDays(RotationStart, date);

			if (deltaDays < 0)
			{
				string msg = string.Format(WorkSchedule.GetMessage("end.earlier.than.start"), RotationStart, date);
				throw new Exception(msg);
			}

			int dayInRotation = (int)(deltaDays % Rotation.GetDuration().Days) + 1;
			return dayInRotation;
		}

		/**
		 * Get the {@link ShiftInstance} for the specified day
		 * 
		 * @param day
		 *            Day with a shift instance
		 * @return {@link ShiftInstance}
		 * @throws Exception
		 *             exception
		 */
		public ShiftInstance GetShiftInstanceForDay(LocalDate day)
		{
			ShiftInstance instance = null;

			Rotation shiftRotation = Rotation;
			int dayInRotation = GetDayInRotation(day);

			// shift or off shift
			TimePeriod period = shiftRotation.GetPeriods()[dayInRotation - 1];

			if (period.IsWorkingPeriod())
			{
				LocalDateTime startDateTime = day.At(period.StartTime);
				instance = new ShiftInstance((Shift)period, startDateTime, this);
			}

			return instance;
		}

		/**
		 * Check to see if this day is a day off
		 * 
		 * @param day
		 *            Date to check
		 * @return True if a day off
		 * @throws Exception
		 *             Exception
		 */
		public bool IsDayOff(LocalDate day)
		{

			bool dayOff = false;

			Rotation shiftRotation = Rotation;
			int dayInRotation = GetDayInRotation(day);

			// shift or off shift
			TimePeriod period = shiftRotation.GetPeriods()[dayInRotation - 1];

			if (!period.IsWorkingPeriod())
			{
				dayOff = true;
			}

			return dayOff;

		}

		/**
		 * Calculate the schedule working time between the specified dates and times
		 * 
		 * @param from
		 *            Starting date and time of day
		 * @param to
		 *            Ending date and time of day
		 * @return Duration of working time
		 * @throws Exception
		 *             exception
		 */
		public Duration CalculateWorkingTime(LocalDateTime from, LocalDateTime to)
		{
			if (from.CompareTo(to) > 0)
			{
				string msg = string.Format(WorkSchedule.GetMessage("end.earlier.than.start"), to, from);
				throw new Exception(msg);
			}

			Duration sum = Duration.Zero;

			LocalDate thisDate = from.Date;
			LocalTime thisTime = from.TimeOfDay;
			LocalDate toDate = to.Date;
			LocalTime toTime = to.TimeOfDay;
			int dayCount = Rotation.GetDayCount();

			// get the working shift from yesterday
			Shift lastShift = null;

			LocalDate yesterday = thisDate.PlusDays(-1);
			ShiftInstance yesterdayInstance = GetShiftInstanceForDay(yesterday);

			if (yesterdayInstance != null)
			{
				lastShift = yesterdayInstance.Shift;
			}

			// step through each day until done
			while (thisDate.CompareTo(toDate) < 1)
			{
				if (lastShift != null && lastShift.SpansMidnight())
				{
					// check for days in the middle of the time period
					bool lastDay = thisDate.CompareTo(toDate) == 0 ? true : false;

					if (!lastDay || (lastDay && !toTime.Equals(LocalTime.Midnight)))
					{
						// add time after midnight in this day
						int afterMidnightSecond = TimePeriod.SecondOfDay(lastShift.GetEnd());
						int fromSecond = TimePeriod.SecondOfDay(thisTime);

						if (afterMidnightSecond > fromSecond)
						{
							Duration seconds = Duration.FromSeconds(afterMidnightSecond - fromSecond);
							sum = sum.Plus(seconds);
						}
					}
				}

				// today's shift
				ShiftInstance instance = GetShiftInstanceForDay(thisDate);

				Duration duration;

				if (instance != null)
				{
					lastShift = instance.Shift;
					// check for last date
					if (thisDate.CompareTo(toDate) == 0)
					{
						duration = lastShift.CalculateWorkingTime(thisTime, toTime, true);
					}
					else
					{
						duration = lastShift.CalculateWorkingTime(thisTime, LocalTime.MaxValue, true);
					}
					sum = sum.Plus(duration);
				}
				else
				{
					lastShift = null;
				}

				int n = 1;
				if (GetDayInRotation(thisDate) == dayCount)
				{
					// move ahead by the rotation count if possible
					LocalDate rotationEndDate = thisDate.PlusDays(dayCount);

					if (rotationEndDate.CompareTo(toDate) < 0)
					{
						n = dayCount;
						sum = sum.Plus(Rotation.GetWorkingTime());
					}
				}

				// move ahead n days starting at midnight
				thisDate = thisDate.PlusDays(n);
				thisTime = LocalTime.Midnight;
			} // end day loop

			return sum;
		}

		/**
		 * Compare one team to another
		 */
		public int CompareTo(Team other)
		{
			return Name.CompareTo(other.Name);
		}

		/**
		 * Build a string value for this team
		 */
		public override string ToString()
		{
			string rpct = WorkSchedule.GetMessage("rotation.percentage");

			string rs = WorkSchedule.GetMessage("rotation.start");
			string avg = WorkSchedule.GetMessage("team.hours");

			string text = "";
			try
			{
				text = base.ToString() + ", " + rs + ": " + RotationStart + ", " + Rotation + ", " + rpct + ": "
						+ GetPercentageWorked().ToString("0.00") + "%" + ", " + avg + ": " + GetHoursWorkedPerWeek();
			}
			catch (Exception)
			{
			}

			return text;
		}
	}
}
