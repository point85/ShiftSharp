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
	/// Class ShiftInstance is an instance of a {@link Shift}. A shift instance is worked by a Team.
	/// </summary>
	public class ShiftInstance : IComparable<ShiftInstance>
	{
		// definition of the shift
		public Shift Shift { get; private set; }

		// team working it
		public Team Team { get; private set; }

		// start date and time of day
		public LocalDateTime StartDateTime { get; private set; }

		internal ShiftInstance(Shift shift, LocalDateTime startDateTime, Team team)
		{
			this.Shift = shift;
			this.StartDateTime = startDateTime;
			this.Team = team;
		}

		/**
		 * Get the end date and time of day
		 * 
		 * @return LocalDateTime
		 */
		public LocalDateTime GetEndTime()
		{
			Duration duration = Shift.GetDuration();
			return StartDateTime.PlusSeconds((long)duration.TotalSeconds);
		}

		/**
		 * Compare this non-working period to another such period by start time of
		 * day
		 * 
		 * @return -1 if less than, 0 if equal and 1 if greater than
		 */
		public int CompareTo(ShiftInstance other)
		{
			return StartDateTime.CompareTo(other.StartDateTime);
		}

		/**
		 * Build a string representation of a shift instance
		 */
	public override string ToString()
		{
			string t = WorkSchedule.GetMessage("team");
			string s = WorkSchedule.GetMessage("shift");
			string ps = WorkSchedule.GetMessage("period.start");
			string pe = WorkSchedule.GetMessage("period.end");

			string text = " " + t + ": " + Team.Name + ", " + s + ": " + Shift.Name + ", " + ps + ": "
					+ StartDateTime + ", " + pe + ": " + GetEndTime();
			return text;
		}
	}
}
