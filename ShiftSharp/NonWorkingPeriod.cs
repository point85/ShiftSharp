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
	/// Class NonWorkingPeriod represents named non-working, non-recurring periods.  
	/// For example holidays and scheduled outages such as for preventive maintenance.
	/// </summary>
	public class NonWorkingPeriod : Named, IComparable<NonWorkingPeriod>
	{
		/// <summary>
		/// owning work schedule
		/// </summary>
		public WorkSchedule WorkSchedule { get; internal set; }

		/// <summary>
		/// starting date and time of day
		/// </summary>
		public LocalDateTime StartDateTime { get; set; }

		/// <summary>
		/// duration of period
		/// </summary>
		public Duration Duration { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public NonWorkingPeriod() : base()
		{
		}

		internal NonWorkingPeriod(string name, string description, LocalDateTime startDateTime, Duration duration) : base(name, description)
		{
			if (duration.TotalSeconds == 0)
			{
				throw new Exception(WorkSchedule.GetMessage("duration.not.defined"));
			}

			StartDateTime = startDateTime;
			Duration = duration;
		}

		/// <summary>
		/// Get period end date and time
		/// </summary>
		/// <returns>Period end</returns>
		public LocalDateTime GetEndDateTime()
		{
			return StartDateTime.PlusSeconds((long)Duration.TotalSeconds);
		}

		/// <summary>
		/// Build a string representation of this non-working period
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			try
			{
				string start = WorkSchedule.GetMessage("period.start");
				string end = WorkSchedule.GetMessage("period.end");

				return base.ToString() + ", " + start + ": " + StartDateTime + " (" + Duration + ")" + ", " + end
						+ ": " + GetEndDateTime();
			}
			catch (Exception)
			{
				// Return partial information if formatting fails
				return base.ToString();
			}
		}

		/// <summary>
		/// Compare this non-working period to another such period by start date and time of day
		/// </summary>
		/// <param name="other">Non-working period</param>
		/// <returns>negative if less than, 0 if equal and positive if greater than</returns>
		public int CompareTo(NonWorkingPeriod other)
		{
			return StartDateTime.CompareTo(other.StartDateTime);
		}

		/// <summary>
		/// Check to see if this day is contained in the non-working period
		/// </summary>
		/// <param name="day">Date to check</param>
		/// <returns>True if in the non-working period</returns>
		public bool IsInPeriod(LocalDate day)
		{
			bool isInPeriod = false;

			LocalDate periodStart = StartDateTime.Date;
			LocalDate periodEnd = GetEndDateTime().Date;

			if (day.CompareTo(periodStart) >= 0 && day.CompareTo(periodEnd) <= 0)
			{
				isInPeriod = true;
			}

			return isInPeriod;
		}
	}
}
