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

		// starting time of day from midnight 
		private LocalTime startTime;

		// length of time period
		private Duration duration;

		protected TimePeriod() : base()
		{
		}

		protected TimePeriod(string name, string description, LocalTime startTime, Duration duration) : base(name, description)
		{
			SetStart(startTime);
			SetDuration(duration);
		}

		/// <summary>
		/// Get the period duration
		/// </summary>
		/// <returns></returns>
		public Duration GetDuration()
		{
			return duration;
		}

		/// <summary>
		/// Set the duration
		/// </summary>
		/// <param name="duration">Duration</param>
		public void SetDuration(Duration duration)
		{
			if (duration == null || duration.Seconds == 0)
			{
				throw new Exception(WorkSchedule.GetMessage("duration.not.defined"));
			}

			if (duration.Seconds > SECONDS_PER_DAY)
			{
				throw new Exception(WorkSchedule.GetMessage("duration.not.allowed"));
			}
			this.duration = duration;
		}

		/// <summary>
		/// Get period start time
		/// </summary>
		/// <returns></returns>
		public LocalTime GetStart()
		{
			return startTime;
		}

		/// <summary>
		/// Set period start time
		/// </summary>
		/// <param name="startTime">Start time</param>
		public void SetStart(LocalTime startTime)
		{
			if (startTime == null)
			{
				throw new Exception(WorkSchedule.GetMessage("start.not.defined"));
			}
			this.startTime = startTime;
		}

		/// <summary>
		/// Get period end
		/// </summary>
		/// <returns>Period end time</returns>
		public LocalTime GetEnd()
		{
			return startTime.PlusSeconds(duration.Seconds);
		}

		// breaks are considered to be in the shift's working period
		abstract public bool IsWorkingPeriod();

		public static int SecondOfDay(LocalTime time)
		{
			return (int)(time.NanosecondOfDay / 1E+09);
		}

		public static long DeltaDays(LocalDate start, LocalDate end)
		{
			Period delta = Period.Between(start, end, PeriodUnits.Days);
			return delta.Days;
		}

		/**
		 * Build a string value for this period
		 */
		public override string ToString()
		{
			string text = "";
			string start = WorkSchedule.GetMessage("period.start");
			string end = WorkSchedule.GetMessage("period.end");

			try
			{
				text = base.ToString() + ", " + start + ": " + GetStart() + " (" + GetDuration() + ")" + ", " + end + ": "
						+ GetEnd();
			}
			catch (Exception)
			{
				// ignore
			}

			return text;
		}
	}
}
