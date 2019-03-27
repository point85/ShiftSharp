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
	/// Class ShiftInstance is an instance of a Shift. A shift instance is worked by a Team.
	/// </summary>
	public class ShiftInstance : IComparable<ShiftInstance>
	{
		/// <summary>
		/// definition of the shift
		/// </summary>
		public Shift Shift { get; private set; }

		/// <summary>
		/// team working it
		/// </summary>
		public Team Team { get; private set; }

		/// <summary>
		/// start date and time of day
		/// </summary>
		public LocalDateTime StartDateTime { get; private set; }

		internal ShiftInstance(Shift shift, LocalDateTime startDateTime, Team team)
		{
			this.Shift = shift;
			this.StartDateTime = startDateTime;
			this.Team = team;
		}

		/// <summary>
		/// Get the end date and time of day
		/// </summary>
		/// <returns>Ending time</returns>
		public LocalDateTime GetEndTime()
		{
			Duration duration = Shift.Duration;
			return StartDateTime.PlusSeconds((long)duration.TotalSeconds);
		}

		/// <summary>
		/// Compare this non-working period to another such period by start time of day
		/// </summary>
		/// <param name="other">Other shift instance</param>
		/// <returns>-1 if less than, 0 if equal and 1 if greater than</returns>
		public int CompareTo(ShiftInstance other)
		{
			return StartDateTime.CompareTo(other.StartDateTime);
		}

		/// <summary>
		/// Determine if this time falls within the shift instance period
		/// </summary>
		/// <param name="ldt">Date and time to check</param>
		/// <returns>True if the specified time is in this shift instance</returns>
		public Boolean IsInShiftInstance(LocalDateTime ldt)
		{
			if (ldt.CompareTo(StartDateTime) >= 0 && ldt.CompareTo(GetEndTime()) <= 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Build a string representation of a shift instance
		/// </summary>
		/// <returns>String</returns>
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
