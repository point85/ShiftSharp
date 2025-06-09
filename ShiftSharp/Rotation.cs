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
		/// <summary>
		/// owning work schedule
		/// </summary>
		public WorkSchedule WorkSchedule { get; internal set; }

		/// <summary>
		/// working periods in the rotation
		/// </summary>
		public List<RotationSegment> RotationSegments { get; private set; } = new List<RotationSegment>();

		/// <summary>
		/// list of working and non-working days
		/// </summary>
		private List<TimePeriod> periods;

		// name of the day off time period
		private const string DAY_OFF_NAME = "DAY_OFF";

		// 24-hour day off period
		private static DayOff DAY_OFF;

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

		/// <summary>
		/// default constructor
		/// </summary>
		public Rotation() : base()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">name of rotation</param>
		/// <param name="description">description of rotation</param>
		internal Rotation(string name, string description) : base(name, description)
		{
		}

		/// <summary>
		/// get the shifts and off-shifts in the rotation
		/// </summary>
		/// <returns>List of time periods</returns>
		public List<TimePeriod> GetPeriods()
		{
			if (periods == null)
			{
				periods = new List<TimePeriod>();

				// sort by sequence number
				RotationSegments.Sort();

				foreach (RotationSegment segment in RotationSegments)
				{
					// add the on days
					if (segment.StartingShift != null)
					{
						for (int i = 0; i < segment.DaysOn; i++)
						{
							periods.Add(segment.StartingShift);
						}
					}

					// add the off days
					for (int i = 0; i < segment.DaysOff; i++)
					{
						periods.Add(Rotation.DAY_OFF);
					}
				}
			}
			return periods;
		}

		/// <summary>
		/// Get the number of days in the rotation
		/// </summary>
		/// <returns></returns>
		public int GetDayCount()
		{
			return GetPeriods().Count;
		}

		/// <summary>
		/// Get the duration of this rotation
		/// </summary>
		/// <returns>Duration</returns>
		public Duration GetDuration()
		{
			return Duration.FromDays(GetPeriods().Count);
		}

		/// <summary>
		/// Get the shift rotation's total working time
		/// </summary>
		/// <returns>Duration</returns>
		public Duration GetWorkingTime()
		{
			Duration sum = Duration.Zero;

			foreach (TimePeriod period in GetPeriods())
			{
				if (period.IsWorkingPeriod())
				{
					sum = sum.Plus(period.Duration);
				}
			}
			return sum;
		}

		/// <summary>
		/// Add a working period to this rotation. A working period starts with a
		/// shift and specifies the number of days on and days off.
		/// </summary>
		/// <param name="startingShift">Starting shift of rotation</param>
		/// <param name="daysOn">Number of day on shift</param>
		/// <param name="daysOff">Number of days off shift</param>
		/// <returns>Part of the rotation</returns>
		public RotationSegment AddSegment(Shift startingShift, int daysOn, int daysOff)
		{
			if (startingShift == null)
			{
				throw new Exception("The starting shift must be specified.");
			}
			RotationSegment segment = new RotationSegment(startingShift, daysOn, daysOff, this);
			RotationSegments.Add(segment);
			segment.Sequence = RotationSegments.Count;
			// invalidate cache
			periods = null;
			return segment;
		}

		/// <summary>
		/// Compare thsi rotation to another rotation
		/// </summary>
		/// <param name="other">Other rotation</param>
		/// <returns></returns>
		public int CompareTo(Rotation other)
		{
			return Name.CompareTo(other.Name);
		}

		/// <summary>
		/// Build a string representation of this rotation
		/// </summary>
		/// <returns>String</returns>
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
				periodsString += period.Name + " (" + onOff + ")";
			}

			string text = named + "\n" + rper + ": [" + periodsString + "], " + rd + ": " + GetDuration() + ", " + rda
					+ ": " + GetDuration().Days + ", " + rw + ": " + GetWorkingTime();

			return text;
		}
	}
}
