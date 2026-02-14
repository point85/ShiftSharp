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
using System.Text;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class WorkSchedule represents a named group of teams who collectively work
	/// one or more Shifts with off-shift periods.A work schedule can have periods
	/// of non-working time.
	/// </summary>
	public class WorkSchedule : Named
	{
		// cached time zone for working time calculations
		private readonly DateTimeZone ZONE_ID = DateTimeZone.Utc;

		/// <summary>
		/// list of teams
		/// </summary>
		public List<Team> Teams { get; private set; } = new List<Team>();

		/// <summary>
		/// list of Shifts
		/// </summary>
		public List<Shift> Shifts { get; private set; } = new List<Shift>();

		/// <summary>
		/// list of Rotations
		/// </summary>
		public List<Rotation> Rotations { get; private set; } = new List<Rotation>();

		/// <summary>
		/// holidays and planned downtime
		/// </summary>
		public List<NonWorkingPeriod> NonWorkingPeriods { get; private set; } = new List<NonWorkingPeriod>();

		/// <summary>
		/// Constructor
		/// </summary>
		public WorkSchedule() : base()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		public WorkSchedule(string name, string description) : base(name, description)
		{
		}

		/// <summary>
		/// get a particular message by its key
		/// </summary>
		public static string GetMessage(string key)
		{
			return Properties.Resources.ResourceManager.GetString(key);
		}

		/// <summary>
		/// Remove this team from the schedule
		/// </summary>
		/// <param name="team">Team</param>
		public void DeleteTeam(Team team)
		{
			if (team != null)
			{
				Teams.Remove(team);
			}
		}

		/// <summary>
		/// Remove a non-working period from the schedule
		/// </summary>
		/// <param name="period">Non-working period</param>
		public void DeleteNonWorkingPeriod(NonWorkingPeriod period)
		{
			if (period != null)
			{
				this.NonWorkingPeriods.Remove(period);
			}
		}

		/// <summary>
		/// Get the list of shift instances for the specified date that start in that date
		/// </summary>
		/// <param name="day">Date</param>
		/// <returns>List of shift instances</returns>
		public List<ShiftInstance> GetShiftInstancesForDay(LocalDate day)
		{
			List<ShiftInstance> workingShifts = new List<ShiftInstance>();

			// for each team see if there is a working shift
			foreach (Team team in Teams)
			{
				ShiftInstance instance = team.GetShiftInstanceForDay(day);

				if (instance == null)
				{
					continue;
				}

				// check to see if this is a non-working day
				bool addShift = true;

				LocalDate startDate = instance.StartDateTime.Date;

				foreach (NonWorkingPeriod nonWorkingPeriod in NonWorkingPeriods)
				{
					if (nonWorkingPeriod.IsInPeriod(startDate))
					{
						addShift = false;
						break;
					}
				}

				if (addShift)
				{
					workingShifts.Add(instance);
				}
			}

			workingShifts.Sort();

			return workingShifts;
		}

		/// <summary>
		/// Get the list of shift instances for the specified date that start in that date 
		/// or cross over from midnight the previous day
		/// </summary>
		/// <param name="day">Date</param>
		/// <returns>List of shift instances</returns>
		public List<ShiftInstance> GetAllShiftInstancesForDay(LocalDate day)
		{
			// starting in this day
			List<ShiftInstance> workingShifts = GetShiftInstancesForDay(day);

			// now check previous day
			LocalDate yesterday = day.PlusDays(-1);

			foreach (ShiftInstance instance in GetShiftInstancesForDay(yesterday)) {
				if (instance.GetEndTime().Date.Equals(day)) {
				// shift ends in this day
				workingShifts.Add(instance);
				}
			}

			workingShifts.Sort();

			return workingShifts;
		}

		/// <summary>
		/// Get the list of shift instances for the specified date and time of day
		/// </summary>
		/// <param name="dateTime">Date and time of day</param>
		/// <returns>List of shift instances</returns>
		public List<ShiftInstance> GetShiftInstancesForTime(LocalDateTime dateTime)
		{
			List<ShiftInstance> workingShifts = new List<ShiftInstance>();

			// shifts from this date and yesterday
			List<ShiftInstance> candidateShifts = GetAllShiftInstancesForDay(dateTime.Date);

			foreach (ShiftInstance instance in candidateShifts)
			{
				if (instance.IsInShiftInstance(dateTime))
				{
					workingShifts.Add(instance);
				}
			}

			workingShifts.Sort();

			return workingShifts;
		}

		/// <summary>
		/// Create a team
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		/// <param name="rotation">Shift rotation</param>
		/// <param name="rotationStart">Start of rotation</param>
		/// <returns>Team</returns>
		public Team CreateTeam(string name, string description, Rotation rotation, LocalDate rotationStart)
		{
			Team team = new Team(name, description, rotation, rotationStart);

			if (Teams.Contains(team))
			{
				string msg = String.Format(WorkSchedule.GetMessage("team.already.exists"), name);
				throw new Exception(msg);
			}

			Teams.Add(team);
			team.WorkSchedule = this;
			return team;
		}

		/// <summary>
		/// Create a shift
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		/// <param name="start">Start of shift</param>
		/// <param name="duration">Duration of shift</param>
		/// <returns>Shift</returns>
		public Shift CreateShift(string name, string description, LocalTime start, Duration duration)
		{
			Shift shift = new Shift(name, description, start, duration);

			if (Shifts.Contains(shift))
			{
				string msg = String.Format(WorkSchedule.GetMessage("shift.already.exists"), name);
				throw new Exception(msg);
			}
			Shifts.Add(shift);
			shift.WorkSchedule = this;
			return shift;
		}

		/// <summary>
		/// Create a rotation
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		/// <returns>Rotation</returns>
		public Rotation CreateRotation(string name, string description)
		{
			Rotation rotation = new Rotation(name, description);

			if (Rotations.Contains(rotation))
			{
				string msg = String.Format(WorkSchedule.GetMessage("rotation.already.exists"), name);
				throw new Exception(msg);
			}
			Rotations.Add(rotation);
			rotation.WorkSchedule = this;
			return rotation;
		}

		/// <summary>
		/// Delete this shift
		/// </summary>
		/// <param name="shift">Shift</param>
		public void DeleteShift(Shift shift)
		{
			if (!Shifts.Contains(shift))
			{
				return;
			}

			// can't be in use
			foreach (Team team in Teams)
			{
				Rotation rotation = team.Rotation;
				foreach (TimePeriod period in rotation.GetPeriods())
				{
					if (period.Equals(shift))
					{
						string msg = String.Format(WorkSchedule.GetMessage("shift.in.use"), shift.Name);
						throw new Exception(msg);
					}
				}
			}
			Shifts.Remove(shift);
		}

		/// <summary>
		/// Create a non-working period of time
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		/// <param name="startDateTime">Starting date and time of day</param>
		/// <param name="duration">Duration of period</param>
		/// <returns>NonWorkingPeriod</returns>
		public NonWorkingPeriod CreateNonWorkingPeriod(string name, string description, LocalDateTime startDateTime,
				Duration duration)
		{
			NonWorkingPeriod period = new NonWorkingPeriod(name, description, startDateTime, duration);

			if (NonWorkingPeriods.Contains(period))
			{
				string msg = String.Format(WorkSchedule.GetMessage("nonworking.period.already.exists"), name);
				throw new Exception(msg);
			}
			period.WorkSchedule = this;
			NonWorkingPeriods.Add(period);

			NonWorkingPeriods.Sort();

			return period;
		}

		/// <summary>
		/// Get total duration of rotation across all teams.
		/// </summary>
		/// <returns>Duration of rotation</returns>
		public Duration GetRotationDuration()
		{
			Duration sum = Duration.Zero;

			foreach (Team team in Teams)
			{
				sum = sum.Plus(team.GetRotationDuration());
			}
			return sum;
		}

		/// <summary>
		/// Get the total working time for all team rotations
		/// </summary>
		/// <returns>Duration of working time</returns>
		public Duration GetRotationWorkingTime()
		{
			Duration sum = Duration.Zero;

			foreach (Team team in Teams)
			{
				sum = sum.Plus(team.Rotation.GetWorkingTime());
			}
			return sum;
		}

		/// <summary>
		/// Calculate the scheduled working time between the specified dates and
		/// times of day.Non-working periods are removed.
		/// </summary>
		/// <param name="from">Starting date and time of day</param>
		/// <param name="to">Ending date and time of day</param>
		/// <returns>Duration of working time</returns>
		public Duration CalculateWorkingTime(LocalDateTime from, LocalDateTime to)
		{
			Duration sum = Duration.Zero;

			// now add up scheduled time by team
			foreach (Team team in Teams)
			{
				sum = sum.Plus(team.CalculateWorkingTime(from, to));
			}

			// remove the non-working time
			Duration nonWorking = CalculateNonWorkingTime(from, to);
			sum = sum.Minus(nonWorking);

			// clip if negative
			if (sum.CompareTo(Duration.Zero) < 0)
			{
				sum = Duration.Zero;
			}
			return sum;
		}

		/// <summary>
		/// Calculate the non-working time between the specified dates and times of day.
		/// </summary>
		/// <param name="from">Starting date and time of day</param>
		/// <param name="to">Ending date and time of day</param>
		/// <returns>Duration of non-working time</returns>
		public Duration CalculateNonWorkingTime(LocalDateTime from, LocalDateTime to)
		{
			Duration sum = Duration.Zero;

			Instant fromInstant = from.InZoneStrictly(ZONE_ID).ToInstant();
			long fromSeconds = fromInstant.ToUnixTimeSeconds();

			Instant toInstant = to.InZoneStrictly(ZONE_ID).ToInstant();
			long toSeconds = toInstant.ToUnixTimeSeconds();

			foreach (NonWorkingPeriod period in NonWorkingPeriods)
			{
				LocalDateTime start = period.StartDateTime;

				Instant startInstant = start.InZoneStrictly(ZONE_ID).ToInstant();
				long startSeconds = startInstant.ToUnixTimeSeconds();

				LocalDateTime end = period.GetEndDateTime();
				Instant endInstant = end.InZoneStrictly(ZONE_ID).ToInstant();
				long endSeconds = endInstant.ToUnixTimeSeconds();

				if (fromSeconds >= endSeconds)
				{
					// look at next period
					continue;
				}

				if (toSeconds <= startSeconds)
				{
					// done with periods
					break;
				}

				if (fromSeconds <= endSeconds)
				{
					// found a period, check edge conditions
					if (fromSeconds > startSeconds)
					{
						startSeconds = fromSeconds;
					}

					if (toSeconds < endSeconds)
					{
						endSeconds = toSeconds;
					}

					sum = sum.Plus(Duration.FromSeconds(endSeconds - startSeconds));
				}

				if (toSeconds <= endSeconds)
				{
					break;
				}
			}
			return sum;
		}

		/// <summary>
		/// Output shift instances to a string
		/// </summary>
		/// <param name="start">Starting date</param>
		/// <param name="end">Ending date</param>
		public string BuildShiftInstances(LocalDate start, LocalDate end)
		{
			if (start.CompareTo(end) > 0)
			{
				string msg = String.Format(WorkSchedule.GetMessage("end.earlier.than.start"), start, end);
				throw new Exception(msg);
			}

			long days = TimePeriod.DeltaDays(start, end) + 1;

			LocalDate day = start;

			StringBuilder sb = new StringBuilder();

			for (long i = 0; i < days; i++)
			{
				sb.Append("[" + (i + 1) + "] " + GetMessage("Shifts.day") + ": " + day).Append('\n');

				List<ShiftInstance> instances = GetShiftInstancesForDay(day);

				if (instances.Count == 0)
				{
					sb.Append("   " + GetMessage("Shifts.non.working")).Append('\n'); ;
				}
				else
				{
					int count = 1;
					foreach (ShiftInstance instance in instances)
					{
						sb.Append("   (" + count + ")" + instance).Append('\n'); ;
						count++;
					}
				}
				day = day.PlusDays(1);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Build a string value for the work schedule
		/// </summary>
		/// <returns>String</returns>
		public override string ToString()
		{
			try
			{
				string sch = GetMessage("schedule");
				string rd = GetMessage("rotation.duration");
				string sw = GetMessage("schedule.working");
				string sf = GetMessage("schedule.Shifts");
				string st = GetMessage("schedule.teams");
				string sc = GetMessage("schedule.coverage");
				string sn = GetMessage("schedule.non");
				string stn = GetMessage("schedule.total");

				StringBuilder sb = new StringBuilder();
				sb.Append(sch).Append(": ").Append(base.ToString());
				sb.Append("\n").Append(rd).Append(": ").Append(GetRotationDuration())
					.Append(", ").Append(sw).Append(": ").Append(GetRotationWorkingTime());

				// Shifts
				sb.Append("\n").Append(sf).Append(": ");
				int count = 1;
				foreach (Shift shift in Shifts)
				{
					sb.Append("\n   (").Append(count).Append(") ").Append(shift);
					count++;
				}

				// teams
				sb.Append("\n").Append(st).Append(": ");
				count = 1;
				float teamPercent = 0.0f;
				foreach (Team team in Teams)
				{
					sb.Append("\n   (").Append(count).Append(") ").Append(team);
					teamPercent += team.GetPercentageWorked();
					count++;
				}
				sb.Append("\n").Append(sc).Append(": ").Append(teamPercent.ToString("0.00")).Append("%");

				// non-working periods
				if (NonWorkingPeriods.Count > 0)
				{
					sb.Append("\n").Append(sn).Append(":");

					Duration totalMinutes = Duration.Zero;

					count = 1;
					foreach (NonWorkingPeriod period in NonWorkingPeriods)
					{
						totalMinutes = totalMinutes.Plus(period.Duration);
						sb.Append("\n   (").Append(count).Append(") ").Append(period);
						count++;
					}
					sb.Append("\n").Append(stn).Append(": ").Append(totalMinutes.Minutes);
				}
				
				return sb.ToString();
			}
			catch (Exception)
			{
				// Return partial information if formatting fails
				return base.ToString();
			}
		}
	}
}
