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
	/// Class Shift is a scheduled working time period, and can include breaks.
	/// </summary>
	public class Shift : TimePeriod, IComparable<Shift>
	{
		/// <summary>
		/// owning work schedule
		/// </summary>
		public WorkSchedule WorkSchedule { get; internal set; }

		/// <summary>
		/// breaks
		/// </summary>
		public List<Break> Breaks { get; private set; } = new List<Break>();

		/// <summary>
		/// Constructor
		/// </summary>
		public Shift() : base()
		{
		}

		internal Shift(String name, String description, LocalTime start, Duration duration) : base(name, description, start, duration)
		{
		}

		/// <summary>
		/// Add a break period to this shift
		/// </summary>
		/// <param name="breakPeriod">Break</param>
		public void AddBreak(Break breakPeriod)
		{
			if (!this.Breaks.Contains(breakPeriod))
			{
				this.Breaks.Add(breakPeriod);
			}
		}

		/// <summary>
		/// Remove a break from this shift
		/// </summary>
		/// <param name="breakPeriod">Break</param>
		public void RemoveBreak(Break breakPeriod)
		{
			if (this.Breaks.Contains(breakPeriod))
			{
				this.Breaks.Remove(breakPeriod);
			}
		}

		/// <summary>
		/// Create a break for this shift
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		/// <param name="startTime">Staring time</param>
		/// <param name="duration">Duration</param>
		/// <returns></returns>
		public Break CreateBreak(String name, String description, LocalTime startTime, Duration duration)
		{
			Break period = new Break(name, description, startTime, duration);
			AddBreak(period);
			return period;
		}

		private int ToRoundedSecond(LocalTime time)
		{
			int second = SecondOfDay(time);

			if (time.NanosecondOfSecond > 500E+06)
			{
				second++;
			}

			return second;
		}

		/// <summary>
		/// Calculate the working time between the specified times of day. The shift
		/// must not span midnight.
		/// </summary>
		/// <param name="from">Starting local time</param>
		/// <param name="to">Ending local time</param>
		/// <returns></returns>
		public Duration CalculateWorkingTime(LocalTime from, LocalTime to)
		{
			if (SpansMidnight())
			{
				String msg = String.Format(WorkSchedule.GetMessage("shift.spans.midnight"), Name, from, to);
				throw new Exception(msg);
			}

			return this.CalculateWorkingTime(from, to, true);
		}

		/// <summary>
		/// Check to see if this shift crosses midnight
		/// </summary>
		/// <returns>True if it does</returns>
		public bool SpansMidnight()
		{
			int startSecond = ToRoundedSecond(StartTime);
			int endSecond = ToRoundedSecond(GetEnd());
			return endSecond <= startSecond ? true : false;
		}

		/// <summary>
		/// Calculate the working time between the specified times of day
		/// </summary>
		/// <param name="from">Starting local time</param>
		/// <param name="to">Ending local time</param>
		/// <param name="beforeMidnight">If true, shifts ends before midnight</param>
		/// <returns></returns>
		public Duration CalculateWorkingTime(LocalTime from, LocalTime to, bool beforeMidnight)
		{
			Duration duration = Duration.Zero;

			int startSecond = ToRoundedSecond(StartTime);
			int endSecond = ToRoundedSecond(GetEnd());
			int fromSecond = ToRoundedSecond(from);
			int toSecond = ToRoundedSecond(to);

			int delta = toSecond - fromSecond;

			// check for 24 hour shift
			if (delta == 0 && fromSecond == startSecond && Duration.TotalHours == 24)
			{
				delta = 86400;
			}

			if (delta < 0)
			{
				delta = 86400 + toSecond - fromSecond;
			}

			if (SpansMidnight())
			{
				// adjust for shift crossing midnight
				if (fromSecond < startSecond && fromSecond < endSecond)
				{
					if (!beforeMidnight)
					{
						fromSecond = fromSecond + 86400;
					}
				}
				toSecond = fromSecond + delta;
				endSecond = endSecond + 86400;
			}

			// clip seconds on edge conditions
			if (fromSecond < startSecond)
			{
				fromSecond = startSecond;
			}

			if (toSecond < startSecond)
			{
				toSecond = startSecond;
			}

			if (fromSecond > endSecond)
			{
				fromSecond = endSecond;
			}

			if (toSecond > endSecond)
			{
				toSecond = endSecond;
			}

			duration = Duration.FromSeconds(toSecond - fromSecond);

			return duration;
		}

		/// <summary>
		/// Test if the specified time falls within the shift
		/// </summary>
		/// <param name="time">Local time</param>
		/// <returns>True if this time is in the shift</returns>
		public bool IsInShift(LocalTime time)
		{
			bool answer = false;

			LocalTime start = StartTime;
			LocalTime end = GetEnd();

			int onStart = time.CompareTo(start);
			int onEnd = time.CompareTo(end);

			int timeSecond = SecondOfDay(time);

			if (start.CompareTo(end) < 0)
			{
				// shift did not cross midnight
				if (onStart >= 0 && onEnd <= 0)
				{
					answer = true;
				}
			}
			else
			{
				// shift crossed midnight, check before and after midnight
				if (timeSecond <= SecondOfDay(end))
				{
					// after midnight
					answer = true;
				}
				else
				{
					// before midnight
					if (timeSecond >= SecondOfDay(start))
					{
						answer = true;
					}
				}
			}
			return answer;
		}

		/// <summary>
		/// Calculate the total break time for the shift
		/// </summary>
		/// <returns>Sum of breaks</returns>
		public Duration CalculateBreakTime()
		{
			Duration sum = Duration.Zero;

			List<Break> breaks = this.Breaks;

			foreach (Break b in breaks)
			{
				sum = sum.Plus(b.Duration);
			}

			return sum;
		}

		/// <summary>
		/// Compare one shift to another one
		/// </summary>
		/// <param name="other">Other shift</param>
		/// <returns>-1 if less than, 0 if equal and 1 if greater than</returns>
		public int CompareTo(Shift other)
		{
			return this.Name.CompareTo(other.Name);
		}

		/// <summary>
		/// Build a string representation of this shift
		/// </summary>
		/// <returns>String</returns>
		public override string ToString()
		{
			string text = base.ToString();

			if (Breaks.Count > 0)
			{
				text += "\n      " + Breaks.Count + " " + WorkSchedule.GetMessage("breaks") + ":";
			}

			foreach (Break breakPeriod in Breaks)
			{
				text += "\n      " + breakPeriod.ToString();
			}
			return text;
		}

		/// <summary>
		/// Shift is a working period
		/// </summary>
		/// <returns>True</returns>
		public override bool IsWorkingPeriod()
		{
			return true;
		}
	}
}
