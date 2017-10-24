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
		// owning work schedule
		private WorkSchedule workSchedule;

		// breaks
		private List<Break> breaks = new List<Break>();

		/**
		 * Default constructor
		 */
		public Shift() : base()
		{
		}

		internal Shift(String name, String description, LocalTime start, Duration duration) : base(name, description, start, duration)
		{
		}

		/**
		 * Get the break periods for this shift
		 * 
		 * @return List {@link Break}
		 */
		public List<Break> GetBreaks()
		{
			return this.breaks;
		}

		/**
		 * Add a break period to this shift
		 * 
		 * @param breakPeriod
		 *            {@link Break}
		 */
		public void AddBreak(Break breakPeriod)
		{
			if (!this.breaks.Contains(breakPeriod))
			{
				this.breaks.Add(breakPeriod);
			}
		}

		/**
		 * Remove a break from this shift
		 * 
		 * @param breakPeriod
		 *            {@link Break}
		 */
		public void RemoveBreak(Break breakPeriod)
		{
			if (this.breaks.Contains(breakPeriod))
			{
				this.breaks.Remove(breakPeriod);
			}
		}

		/**
		 * Create a break for this shift
		 * 
		 * @param name
		 *            Name of break
		 * @param description
		 *            Description of break
		 * @param startTime
		 *            Start of break
		 * @param duration
		 *            Duration of break
		 * @return {@link Break}
		 * @throws Exception
		 *             exception
		 */
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

		/**
		 * Calculate the working time between the specified times of day. The shift
		 * must not span midnight.
		 * 
		 * @param from
		 *            starting time
		 * @param to
		 *            Ending time
		 * @return Duration of working time
		 * @throws Exception
		 *             exception
		 */
		public Duration CalculateWorkingTime(LocalTime from, LocalTime to)
		{
			if (SpansMidnight())
			{
				String msg = String.Format(WorkSchedule.GetMessage("shift.spans.midnight"), GetName(), from, to);
				throw new Exception(msg);
			}

			return this.CalculateWorkingTime(from, to, true);
		}

		/**
		 * Check to see if this shift crosses midnight
		 * 
		 * @return True if the shift extends over midnight, otherwise false
		 * @throws Exception
		 *             exception
		 */
		public bool SpansMidnight()
		{
			int startSecond = ToRoundedSecond(GetStart());
			int endSecond = ToRoundedSecond(GetEnd());
			return endSecond <= startSecond ? true : false;
		}

		/**
		 * Calculate the working time between the specified times of day
		 * 
		 * @param from
		 *            starting time
		 * @param to
		 *            Ending time
		 * @param beforeMidnight
		 *            If true, and a shift spans midnight, calculate the time before
		 *            midnight. Otherwise calculate the time after midnight.
		 * @return Duration of working time
		 * @throws Exception
		 *             exception
		 */
		public Duration CalculateWorkingTime(LocalTime from, LocalTime to, bool beforeMidnight)
		{
			Duration duration = Duration.Zero;

			int startSecond = ToRoundedSecond(GetStart());
			int endSecond = ToRoundedSecond(GetEnd());
			int fromSecond = ToRoundedSecond(from);
			int toSecond = ToRoundedSecond(to);

			int delta = toSecond - fromSecond;

			// check for 24 hour shift
			if (delta == 0 && fromSecond == startSecond && GetDuration().TotalHours == 24)
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

		/**
		 * Test if the specified time falls within the shift
		 * 
		 * @param time
		 *            {@link LocalTime}
		 * @return True if in the shift
		 * @throws Exception
		 *             exception
		 */
		public bool IsInShift(LocalTime time)
		{
			bool answer = false;

			LocalTime start = GetStart();
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

		/**
		 * Calculate the total break time for the shift
		 * 
		 * @return Duration of all breaks
		 */
		public Duration CalculateBreakTime()
		{
			Duration sum = Duration.Zero;

			List<Break> breaks = this.GetBreaks();

			foreach (Break b in breaks)
			{
				sum = sum.Plus(b.GetDuration());
			}

			return sum;
		}

		/**
		 * Get the work schedule that owns this shift
		 * 
		 * @return {@link WorkSchedule}
		 */
		public WorkSchedule GetWorkSchedule()
		{
			return workSchedule;
		}

		internal void SetWorkSchedule(WorkSchedule workSchedule)
		{
			this.workSchedule = workSchedule;
		}

		/**
		 * Compare one shift to another one
		 */
		public int CompareTo(Shift other)
		{
			return this.GetName().CompareTo(other.GetName());
		}

		/**
		 * Build a string representation of this shift
		 * 
		 * @return String
		 */
		public override string ToString()
		{
			string text = base.ToString();

			if (GetBreaks().Count > 0)
			{
				text += "\n      " + GetBreaks().Count + " " + WorkSchedule.GetMessage("breaks") + ":";
			}

			foreach (Break breakPeriod in GetBreaks())
			{
				text += "\n      " + breakPeriod.ToString();
			}
			return text;
		}

		public override bool IsWorkingPeriod()
		{
			return true;
		}
	}
}
