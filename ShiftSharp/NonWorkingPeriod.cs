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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class NonWorkingPeriod represents named non-working, non-recurring periods.  
	/// For example holidays and scheduled outages such as for preventive maintenance.
	/// </summary>
	public class NonWorkingPeriod : Named, IComparable<NonWorkingPeriod>
	{
		// owning work schedule
		private WorkSchedule workSchedule;

		// starting date and time of day
		private LocalDateTime startDateTime;

		// duration of period
		private Duration duration;

		public NonWorkingPeriod() : base()
		{
		}

		NonWorkingPeriod(string name, string description, LocalDateTime startDateTime, Duration duration) : base(name, description)
		{
			SetStartDateTime(startDateTime);
			SetDuration(duration);
		}

		/// <summary>
		/// Get period start date and time
		/// </summary>
		/// <returns>Start date and time</returns>
		public LocalDateTime GetStartDateTime()
		{
			return startDateTime;
		}

		/// <summary>
		/// Set period start date and time
		/// </summary>
		/// <param name="startDateTime">Period start</param>
		public void SetStartDateTime(LocalDateTime startDateTime)
		{
			if (startDateTime == null)
			{
				throw new Exception(WorkSchedule.GetMessage("start.not.defined"));
			}

			this.startDateTime = startDateTime;
		}

		/// <summary>
		/// Get period end date and time
		/// </summary>
		/// <returns>Period end</returns>
		public LocalDateTime GetEndDateTime()
		{
			return startDateTime.PlusSeconds(duration.Seconds);
		}

		/// <summary>
		/// Get period duration
		/// </summary>
		/// <returns>Duration</returns>
		public Duration GetDuration()
		{
			return duration;
		}

		/// <summary>
		/// Set duration
		/// </summary>
		/// <param name="duration">Duration</param>
		public void SetDuration(Duration duration)
		{
			if (duration == null || duration.Seconds == 0)
			{
				throw new Exception(WorkSchedule.GetMessage("duration.not.defined"));
			}

			this.duration = duration;
		}

		/// <summary>
		/// Build a string representation of this non-working period
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string text = "";
			string start = WorkSchedule.GetMessage("period.start");
			string end = WorkSchedule.GetMessage("period.end");

			try
			{
				text = base.ToString() + ", " + start + ": " + GetStartDateTime() + " (" + GetDuration() + ")" + ", " + end
						+ ": " + GetEndDateTime();
			}
			catch (Exception)
			{
				// ignore
			}
			return text;
		}

		/// <summary>
		/// Compare this non-working period to another such period by start date and time of day
		/// </summary>
		/// <param name="other">Non-working period</param>
		/// <returns>negative if less than, 0 if equal and positive if greater than</returns>
		public int CompareTo(NonWorkingPeriod other)
		{
			return GetStartDateTime().CompareTo(other.GetStartDateTime());
		}

		/// <summary>
		/// Get the work schedule that owns this non-working period
		/// </summary>
		/// <returns></returns>
		public WorkSchedule GetWorkSchedule()
		{
			return workSchedule;
		}

		void SetWorkSchedule(WorkSchedule workSchedule)
		{
			this.workSchedule = workSchedule;
		}

		/// <summary>
		/// Check to see if this day is contained in the non-working period
		/// </summary>
		/// <param name="day">Date to check</param>
		/// <returns>True if in the non-working period</returns>
		public bool IsInPeriod(LocalDate day)
		{
			bool isInPeriod = false;

			LocalDate periodStart = GetStartDateTime().Date;
			LocalDate periodEnd = GetEndDateTime().Date;

			if (day.CompareTo(periodStart) >= 0 && day.CompareTo(periodEnd) <= 0)
			{
				isInPeriod = true;
			}

			return isInPeriod;
		}
	}
}
