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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class Team is a named group of individuals who rotate through a shift schedule.
	/// </summary>
	public class Team : Named, IComparable<Team>
	{
		/// <summary>
		/// owning work schedule
		/// </summary>
		public WorkSchedule WorkSchedule { get; set; }

		/// <summary>
		/// reference date for starting the rotations
		/// </summary>
		public LocalDate RotationStart { get; set; }

		/// <summary>
		/// shift rotation days
		/// </summary>
		public Rotation Rotation { get; set; }

		/// <summary>
		/// Team members assigned to work regular shifts
		/// </summary>
		public List<TeamMember> AssignedMembers { get; private set; } = new List<TeamMember>();

		/// <summary>
		/// Team member exceptions to regular shifts
		/// </summary>
		public List<TeamMemberException> MemberExceptions { get; private set; } = new List<TeamMemberException>();

		// team member exception cache by shift instance start
		private ConcurrentDictionary<LocalDateTime, TeamMemberException> ExceptionCache;

		/// <summary>
		/// Constructor
		/// </summary>
		public Team() : base()
		{
		}

		internal Team(string name, string description, Rotation rotation, LocalDate rotationStart) : base(name, description)
		{
			this.Rotation = rotation;
			this.RotationStart = rotationStart;
		}

		/// <summary>
		/// Get the duration of the shift rotation
		/// </summary>
		/// <returns>Duration</returns>
		public Duration GetRotationDuration()
		{
			return Rotation.GetDuration();
		}

		/// <summary>
		/// Get the shift rotation's working time as a percentage of the rotation duration
		/// </summary>
		/// <returns>Percentage</returns>
		public float GetPercentageWorked()
		{
			double ratio = Rotation.GetWorkingTime().TotalSeconds / GetRotationDuration().TotalSeconds;
			return (float)ratio * 100.0f;
		}

		/// <summary>
		/// Get the average number of hours worked each week by this team
		/// </summary>
		/// <returns>Duration</returns>
		public Duration GetHoursWorkedPerWeek()
		{
			double days = Rotation.GetDuration().TotalDays;
			double secPerWeek = Rotation.GetWorkingTime().TotalSeconds * (7.0f / days);
			return Duration.FromSeconds(secPerWeek);
		}

		/// <summary>
		/// Get the day number in the rotation for this local date
		/// </summary>
		/// <param name="date">Date in rotation</param>
		/// <returns>Day number</returns>
		public int GetDayInRotation(LocalDate date)
		{
			// calculate total number of days from start of rotation
			long deltaDays = TimePeriod.DeltaDays(RotationStart, date);

			if (deltaDays < 0)
			{
				string msg = string.Format(WorkSchedule.GetMessage("end.earlier.than.start"), RotationStart, date);
				throw new Exception(msg);
			}

			long rotationDays = Rotation.GetDuration().Days;
			long dayInRotation = (deltaDays % rotationDays) + 1;
			
			// Safety check for int conversion
			if (dayInRotation > int.MaxValue)
			{
				throw new Exception("Day in rotation exceeds maximum value");
			}
			
			return (int)dayInRotation;
		}

		/// <summary>
		/// Get the ShiftInstance for the specified day
		/// </summary>
		/// <param name="day">Date</param>
		/// <returns>Shift instance</returns>
		public ShiftInstance GetShiftInstanceForDay(LocalDate day)
		{
			ShiftInstance instance = null;

			Rotation shiftRotation = Rotation;
			int dayInRotation = GetDayInRotation(day);

			// shift or off shift
			TimePeriod period = shiftRotation.GetPeriods()[dayInRotation - 1];

			if (period.IsWorkingPeriod())
			{
				LocalDateTime startDateTime = day.At(period.StartTime);
				instance = new ShiftInstance((Shift)period, startDateTime, this);
			}
			return instance;
		}

		/// <summary>
		/// Check to see if this day is a day off
		/// </summary>
		/// <param name="day">Date</param>
		/// <returns>True if this is a day off</returns>
		public bool IsDayOff(LocalDate day)
		{
			bool dayOff = false;

			Rotation shiftRotation = Rotation;
			int dayInRotation = GetDayInRotation(day);

			// shift or off shift
			TimePeriod period = shiftRotation.GetPeriods()[dayInRotation - 1];

			if (!period.IsWorkingPeriod())
			{
				dayOff = true;
			}

			return dayOff;
		}

		/// <summary>
		/// Calculate the schedule working time between the specified dates and times
		/// </summary>
		/// <param name="from">Starting date and time</param>
		/// <param name="to">Ending date and time</param>
		/// <returns>Duration</returns>
		public Duration CalculateWorkingTime(LocalDateTime from, LocalDateTime to)
		{
			if (from.CompareTo(to) > 0)
			{
				string msg = string.Format(WorkSchedule.GetMessage("end.earlier.than.start"), to, from);
				throw new Exception(msg);
			}

			Duration sum = Duration.Zero;

			LocalDate thisDate = from.Date;
			LocalTime thisTime = from.TimeOfDay;
			LocalDate toDate = to.Date;
			LocalTime toTime = to.TimeOfDay;
			int dayCount = Rotation.GetDayCount();

			// get the working shift from yesterday
			Shift lastShift = null;

			LocalDate yesterday = thisDate.PlusDays(-1);
			ShiftInstance yesterdayInstance = GetShiftInstanceForDay(yesterday);

			if (yesterdayInstance != null)
			{
				lastShift = yesterdayInstance.Shift;
			}

			// step through each day until done
			while (thisDate.CompareTo(toDate) < 1)
			{
				if (lastShift != null && lastShift.SpansMidnight())
				{
					// check for days in the middle of the time period
					bool lastDay = thisDate.CompareTo(toDate) == 0 ? true : false;

					if (!lastDay || (lastDay && !toTime.Equals(LocalTime.Midnight)))
					{
						// add time after midnight in this day
						int afterMidnightSecond = TimePeriod.SecondOfDay(lastShift.GetEnd());
						int fromSecond = TimePeriod.SecondOfDay(thisTime);

						if (afterMidnightSecond > fromSecond)
						{
							Duration seconds = Duration.FromSeconds(afterMidnightSecond - fromSecond);
							sum = sum.Plus(seconds);
						}
					}
				}

				// today's shift
				ShiftInstance instance = GetShiftInstanceForDay(thisDate);

				Duration duration;

				if (instance != null)
				{
					lastShift = instance.Shift;
					// check for last date
					if (thisDate.CompareTo(toDate) == 0)
					{
						duration = lastShift.CalculateWorkingTime(thisTime, toTime, true);
					}
					else
					{
						duration = lastShift.CalculateWorkingTime(thisTime, LocalTime.MaxValue, true);
					}
					sum = sum.Plus(duration);
				}
				else
				{
					lastShift = null;
				}

				int n = 1;
				if (GetDayInRotation(thisDate) == dayCount)
				{
					// move ahead by the rotation count if possible
					LocalDate rotationEndDate = thisDate.PlusDays(dayCount);

					if (rotationEndDate.CompareTo(toDate) < 0)
					{
						n = dayCount;
						sum = sum.Plus(Rotation.GetWorkingTime());
					}
				}

				// move ahead n days starting at midnight
				thisDate = thisDate.PlusDays(n);
				thisTime = LocalTime.Midnight;
			} // end day loop

			return sum;
		}

		/// <summary>
		/// Compare one team to another
		/// </summary>
		/// <param name="other">Other team</param>
		/// <returns>-1 if less than, 0 if equal and 1 if greater than</returns>
		public int CompareTo(Team other)
		{
			return Name.CompareTo(other.Name);
		}

		/// <summary>
		/// Build a string value for this team
		/// </summary>
		/// <returns>String</returns>
		public override string ToString()
		{
		try
		{
			string rpct = WorkSchedule.GetMessage("rotation.percentage");
			string rs = WorkSchedule.GetMessage("rotation.start");
			string avg = WorkSchedule.GetMessage("team.hours");
			string members = WorkSchedule.GetMessage("team.members");

			StringBuilder sb = new StringBuilder();
			sb.Append(base.ToString())
				.Append(", ").Append(rs).Append(": ").Append(RotationStart)
				.Append(", ").Append(Rotation)
				.Append(", ").Append(rpct).Append(": ").Append(GetPercentageWorked().ToString("0.00")).Append("%")
				.Append(", ").Append(avg).Append(": ").Append(GetHoursWorkedPerWeek())
				.Append("\n").Append(members);

			foreach (TeamMember member in AssignedMembers)
			{
				sb.Append("\n\t").Append(member);
			}
			
			return sb.ToString();
		}
		catch (Exception)
		{
			// Return partial information if formatting fails
			return base.ToString();
		}
	}

	/// <summary>
	/// Add a member to this team
	/// </summary>
	/// <param name="member">Team member</param>
	public void AddMember(TeamMember member)
		{
			if (member == null)
			{
				throw new ArgumentNullException(nameof(member));
			}
			
			if (!this.AssignedMembers.Contains(member))
			{
				this.AssignedMembers.Add(member);
			}
		}

		/// <summary>
		/// Remove a member from this team
		/// </summary>
		/// <param name="member">Team member</param>
		public void RemoveMember(TeamMember member)
		{
			if (member != null)
			{
				this.AssignedMembers.Remove(member);
			}
		}

		/// <summary>
		/// True if member is assigned to this team
		/// </summary>
		/// <param name="member">Team member</param>
		/// <returns>bool</returns>
		public bool HasMember(TeamMember member)
		{
			return this.AssignedMembers.Contains(member);
		}

		/// <summary>
		/// Add a member exception for this team
		/// </summary>
		/// <param name="memberException">Team member exception</param>
		public void AddMemberException(TeamMemberException memberException)
		{
			this.MemberExceptions.Add(memberException);

			// invalidate cache
			this.ExceptionCache = null;
		}

		/// <summary>
		/// Remove a member exception for this team
		/// </summary>
		/// <param name="memberException">Team member exception</param>
		public void RemoveMemberException(TeamMemberException memberException)
		{
			this.MemberExceptions.Remove(memberException);

			// invalidate cache
			this.ExceptionCache = null;
		}

		/// <summary>
		/// Build a list of team member for the specified shift start
		/// </summary>
		/// <param name="shiftStart">Shift instance starting date and time</param>
		/// <returns>List of team members</returns>

		public List<TeamMember> GetMembers(LocalDateTime shiftStart)
		{
			List<TeamMember> members = new List<TeamMember>();

			// build the cache if not already done
			BuildMemberCache();

			// members assigned to the team
			foreach (TeamMember member in AssignedMembers)
			{
				members.Add(member);
			}

			// any exceptions?
			if (ExceptionCache.TryGetValue(shiftStart, out TeamMemberException tme))
			{
				if (tme.Addition != null)
				{
					members.Add(tme.Addition);
				}

				if (tme.Removal!= null)
				{
					members.Remove(tme.Removal);
				}
			}
			return members;
		}

		private void BuildMemberCache()
		{
			if (ExceptionCache != null)
			{
				// already built
				return;
			}

			// create and populate
			ExceptionCache = new ConcurrentDictionary<LocalDateTime, TeamMemberException>();

			foreach (TeamMemberException tme in MemberExceptions)
			{
				ExceptionCache[tme.DateTime] = tme;
			}
		}
	}
}
