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
using System.Collections.Generic;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class Rotation maintains a sequenced list of shift and off-shift time periods.
	/// </summary>
	public class Rotation : Named, IComparable<Rotation>
	{
		// working periods in the rotation
		private List<RotationSegment> rotationSegments = new List<RotationSegment>();

		// list of working and non-working days
		private List<TimePeriod> periods;

		// name of the day off time period
		private const string DAY_OFF_NAME = "DAY_OFF";

		// 24-hour day off period
		private static DayOff DAY_OFF;// = InitializeDayOff();

		static Rotation()
		{
			DayOff dayOff = null;
			try
			{
				dayOff = new DayOff(DAY_OFF_NAME, "24 hour off period", LocalTime.Midnight, Duration.FromHours(24));
			}
			catch (Exception)
			{
				// ignore
			}
			DAY_OFF = dayOff;
		}

		/**
		 * Default constructor
		 */
		public Rotation() : base()
		{
		}

		/**
		 * Constructor
		 * 
		 * @param name
		 *            Rotation name
		 * @param description
		 *            Description
		 * @throws Exception
		 *             Exception
		 */
		public Rotation(string name, string description) : base(name, description)
		{

		}

		/**
		 * Get the shifts and off-shifts in the rotation
		 * 
		 * @return List of periods
		 */
		public List<TimePeriod> GetPeriods()
		{
			if (periods == null)
			{
				periods = new List<TimePeriod>();

				// sort by sequence number
				rotationSegments.Sort();

				foreach (RotationSegment segment in rotationSegments)
				{
					// add the on days
					if (segment.GetStartingShift() != null)
					{
						for (int i = 0; i < segment.GetDaysOn(); i++)
						{
							periods.Add(segment.GetStartingShift());
						}
					}

					// add the off days
					for (int i = 0; i < segment.GetDaysOff(); i++)
					{
						periods.Add(Rotation.DAY_OFF);
					}
				}
			}

			return periods;
		}

		/**
		 * Get the number of days in the rotation
		 * 
		 * @return Day count
		 */

		public int GetDayCount()
		{
			return GetPeriods().Count;
		}

		/**
		 * Get the duration of this rotation
		 * 
		 * @return Duration
		 */
		public Duration GetDuration()
		{
			return Duration.FromDays(GetPeriods().Count);
		}

		/**
		 * Get the shift rotation's total working time
		 * 
		 * @return Duration of working time
		 */
		public Duration GetWorkingTime()
		{
			Duration sum = Duration.Zero;

			foreach (TimePeriod period in GetPeriods())
			{
				if (period.IsWorkingPeriod())
				{
					sum = sum.Plus(period.GetDuration());
				}
			}
			return sum;
		}

		/**
		 * Get the rotation's working periods
		 * 
		 * @return List of {@link RotationSegment}
		 */
		public List<RotationSegment> GetRotationSegments()
		{
			return rotationSegments;
		}

		/**
		 * Add a working period to this rotation. A working period starts with a
		 * shift and specifies the number of days on and days off
		 * 
		 * @param startingShift
		 *            {@link Shift} that starts the period
		 * @param daysOn
		 *            Number of days on shift
		 * @param daysOff
		 *            Number of days off shift
		 * @return {@link RotationSegment}
		 * @throws Exception
		 *             Exception
		 */
		public RotationSegment AddSegment(Shift startingShift, int daysOn, int daysOff)
		{
			if (startingShift == null)
			{
				throw new Exception("The starting shift must be specified.");
			}
			RotationSegment segment = new RotationSegment(startingShift, daysOn, daysOff, this);
			rotationSegments.Add(segment);
			segment.SetSequence(rotationSegments.Count);
			return segment;
		}

		public int CompareTo(Rotation other)
		{
			return GetName().CompareTo(other.GetName());
		}

		/**
		 * Build a string representation of this rotation
		 */
		public override string ToString()
		{
			string named = base.ToString();
			string rd = WorkSchedule.GetMessage("rotation.duration");
			string rda = WorkSchedule.GetMessage("rotation.days");
			string rw = WorkSchedule.GetMessage("rotation.working");
			string rper = WorkSchedule.GetMessage("rotation.periods");
			string on = WorkSchedule.GetMessage("rotation.on");
			string off = WorkSchedule.GetMessage("rotation.off");

			string periodsString = "";

			foreach (TimePeriod period in GetPeriods())
			{
				if (periodsString.Length > 0)
				{
					periodsString += ", ";
				}

				string onOff = period.IsWorkingPeriod() ? on : off;
				periodsString += period.GetName() + " (" + onOff + ")";
			}

			string text = named + "\n" + rper + ": [" + periodsString + "], " + rd + ": " + GetDuration() + ", " + rda
					+ ": " + GetDuration().Days + ", " + rw + ": " + GetWorkingTime();

			return text;
		}
	}
}
