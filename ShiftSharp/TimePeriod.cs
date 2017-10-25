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
using NodaTime;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class TimePeriod is a named period of time with a specified duration and starting time of day.
	/// </summary>
	public abstract class TimePeriod : Named
	{
		private const int SECONDS_PER_DAY = 24 * 60 * 60;

		/// <summary>
		/// starting time of day from midnight 
		/// </summary>
		public LocalTime StartTime { get; set; }

		/// <summary>
		/// length of time period
		/// </summary>
		public Duration Duration { get; set; }

		protected TimePeriod() : base()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Desription</param>
		/// <param name="startTime">Starting time</param>
		/// <param name="duration">Duration</param>
		protected TimePeriod(string name, string description, LocalTime startTime, Duration duration) : base(name, description)
		{
			if (startTime == null)
			{
				throw new Exception(WorkSchedule.GetMessage("start.not.defined"));
			}

			if (duration == null || duration.TotalSeconds == 0)
			{
				throw new Exception(WorkSchedule.GetMessage("duration.not.defined"));
			}

			if (duration.TotalSeconds > SECONDS_PER_DAY)
			{
				throw new Exception(WorkSchedule.GetMessage("duration.not.allowed"));
			}

			this.StartTime = startTime;
			this.Duration = duration;
		}

		/// <summary>
		/// Get period end
		/// </summary>
		/// <returns>Period end time</returns>
		public LocalTime GetEnd()
		{
			return StartTime.PlusSeconds((long)Duration.TotalSeconds);
		}

		/// <summary>
		/// Check to see if this period a working period
		/// </summary>
		/// <returns>True if it is</returns>
		abstract public bool IsWorkingPeriod();

		/// <summary>
		/// Calculate the second in the day for this time
		/// </summary>
		/// <param name="time">Local time</param>
		/// <returns>Second in day</returns>
		public static int SecondOfDay(LocalTime time)
		{
			return (int)(time.NanosecondOfDay / 1E+09);
		}

		/// <summary>
		/// Calculate the number of days between the start and end
		/// </summary>
		/// <param name="start">Starting date</param>
		/// <param name="end">Ending date</param>
		/// <returns>Number of days</returns>
		public static long DeltaDays(LocalDate start, LocalDate end)
		{
			Period delta = Period.Between(start, end, PeriodUnits.Days);
			return delta.Days;
		}

		/// <summary>
		/// Build a string value for this period
		/// </summary>
		/// <returns>String</returns>
		public override string ToString()
		{
			string text = "";
			string start = WorkSchedule.GetMessage("period.start");
			string end = WorkSchedule.GetMessage("period.end");

			try
			{
				text = base.ToString() + ", " + start + ": " + StartTime + " (" + Duration + ")" + ", " + end + ": "
						+ GetEnd();
			}
			catch (Exception)
			{
			}
			return text;
		}
	}
}
