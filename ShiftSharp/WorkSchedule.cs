﻿/*
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
	/// Class WorkSchedule represents a named group of teams who collectively work
	/// one or more Shifts with off-shift periods.A work schedule can have periods
	/// of non-working time.
	/// </summary>
	public class WorkSchedule : Named
	{
		// name of resource with translatable strings for exception messages
		private const string MESSAGE_RESOURCE_NAME = "./Resources/Message.properties";

		// resource manager for exception messages
		internal static PropertyManager MessagesManager = new PropertyManager(MESSAGE_RESOURCE_NAME);

		// cached time zone for working time calculations
		private DateTimeZone ZONE_ID = DateTimeZone.Utc;

		// list of teams
		public List<Team> Teams { get; private set; } = new List<Team>();

		// list of Shifts
		public List<Shift> Shifts { get; private set; } = new List<Shift>();

		// holidays and planned downtime
		public List<NonWorkingPeriod> NonWorkingPeriods { get; private set; } = new List<NonWorkingPeriod>();

		public WorkSchedule() : base()
		{
		}

		/**
		 * Construct a work schedule
		 * 
		 * @param name
		 *            Schedule name
		 * @param description
		 *            Schedule description
		 * @throws Exception
		 *             exception
		 */
		public WorkSchedule(string name, string description) : base(name, description)
		{
		}

		/// <summary>
		/// get a particular message by its key
		/// </summary>
		public static string GetMessage(string key)
		{
			return MessagesManager.GetString(key);
		}


		/**
		 * Remove this team from the schedule
		 * 
		 * @param team
		 *            {@link Team}
		 */
		public void DeleteTeam(Team team)
		{
			if (Teams.Contains(team))
			{
				Teams.Remove(team);
			}
		}

		/**
		 * Remove a non-working period from the schedule
		 * 
		 * @param period
		 *            {@link NonWorkingPeriod}
		 */
		public void DeleteNonWorkingPeriod(NonWorkingPeriod period)
		{
			if (this.NonWorkingPeriods.Contains(period))
			{
				this.NonWorkingPeriods.Remove(period);
			}
		}

		/**
		 * Get the list of shift instances for the specified date that start in that
		 * date
		 * 
		 * @param day
		 *            LocalDate
		 * @return List of {@link ShiftInstance}
		 * @throws Exception
		 *             exception
		 */
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

		/**
		 * Get the list of shift instances for the specified date and time of day
		 * 
		 * @param dateTime
		 *            Date and time of day
		 * @return List of {@link ShiftInstance}
		 * @throws Exception
		 *             exception
		 */
		public List<ShiftInstance> GetShiftInstancesForTime(LocalDateTime dateTime)
		{
			List<ShiftInstance> workingShifts = new List<ShiftInstance>();

			// day
			List<ShiftInstance> candidateShifts = GetShiftInstancesForDay(dateTime.Date);

			// check time now
			foreach (ShiftInstance instance in candidateShifts)
			{
				if (instance.Shift.IsInShift(dateTime.TimeOfDay))
				{
					workingShifts.Add(instance);
				}
			}
			return workingShifts;
		}

		/**
		 * Create a team
		 * 
		 * @param name
		 *            Name of team
		 * @param description
		 *            Team description
		 * @param rotation
		 *            Shift rotation
		 * @param rotationStart
		 *            Start of rotation
		 * @return {@link Team}
		 * @throws Exception
		 *             exception
		 */
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

		/**
		 * Create a shift
		 * 
		 * @param name
		 *            Name of shift
		 * @param description
		 *            Description of shift
		 * @param start
		 *            Shift start time of day
		 * @param duration
		 *            Shift duration
		 * @return {@link Shift}
		 * @throws Exception
		 *             exception
		 */
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

		/**
		 * Delete this shift
		 * 
		 * @param shift
		 *            {@link Shift} to delete
		 * @throws Exception
		 *             exception
		 */
		public void DeleteShift(Shift shift)
		{
			if (!Shifts.Contains(shift))
			{
				return;
			}

			// can't be in use
			foreach (Shift inUseShift in Shifts)
			{
				foreach (Team team in Teams)
				{
					Rotation rotation = team.Rotation;

					foreach (TimePeriod period in rotation.GetPeriods())
					{
						if (period.Equals(inUseShift))
						{
							string msg = String.Format(WorkSchedule.GetMessage("shift.in.use"), shift.Name);
							throw new Exception(msg);
						}
					}
				}
			}

			Shifts.Remove(shift);
		}

		/**
		 * Create a non-working period of time
		 * 
		 * @param name
		 *            Name of period
		 * @param description
		 *            Description of period
		 * @param startDateTime
		 *            Starting date and time of day
		 * @param duration
		 *            Duration of period
		 * @return {@link NonWorkingPeriod}
		 * @throws Exception
		 *             exception
		 */
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

		/**
		 * Get total duration of rotation across all teams.
		 * 
		 * @return Duration of rotation
		 * @throws Exception Exception
		 */
		public Duration GetRotationDuration()
		{
			Duration sum = Duration.Zero;

			foreach (Team team in Teams)
			{
				sum = sum.Plus(team.GetRotationDuration());
			}
			return sum;
		}

		/**
		 * Get the total working time for all team rotations
		 * 
		 * @return Team rotation working time
		 */
		public Duration GetRotationWorkingTime()
		{
			Duration sum = Duration.Zero;

			foreach (Team team in Teams)
			{
				sum = sum.Plus(team.Rotation.GetWorkingTime());
			}
			return sum;
		}

		/**
		 * Calculate the scheduled working time between the specified dates and
		 * times of day. Non-working periods are removed.
		 * 
		 * @param from
		 *            Starting date and time
		 * @param to
		 *            Ending date and time
		 * @return Working time duration
		 * @throws Exception
		 *             exception
		 */
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

		/**
		 * Calculate the non-working time between the specified dates and times of
		 * day.
		 * 
		 * @param from
		 *            Starting date and time
		 * @param to
		 *            Ending date and time
		 * @return Non-working time duration
		 * @throws Exception
		 *             exception
		 */
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

		/**
		 * Print shift instances
		 * 
		 * @param start
		 *            Starting date
		 * @param end
		 *            Ending date
		 * @throws Exception
		 *             exception
		 */
		public void PrintShiftInstances(LocalDate start, LocalDate end)
		{
			if (start.CompareTo(end) < 0)
			{
				string msg = String.Format(WorkSchedule.GetMessage("end.earlier.than.start"), start, end);
				throw new Exception(msg);
			}

			long days = TimePeriod.DeltaDays(start, end) + 1;

			LocalDate day = start;

			for (long i = 0; i < days; i++)
			{
				Console.WriteLine("[" + (i + 1) + "] " + GetMessage("Shifts.day") + ": " + day);

				List<ShiftInstance> instances = GetShiftInstancesForDay(day);

				if (instances.Count == 0)
				{
					Console.WriteLine("   " + GetMessage("Shifts.non.working"));
				}
				else
				{
					int count = 1;
					foreach (ShiftInstance instance in instances)
					{
						Console.WriteLine("   (" + count + ")" + instance);
						count++;
					}
				}
				day = day.PlusDays(1);
			}
		}

		/**
		 * Build a string value for the work schedule
		 * 
		 * @return string
		 */
		public override string ToString()
		{
			string sch = GetMessage("schedule");
			string rd = GetMessage("rotation.duration");
			string sw = GetMessage("schedule.working");
			string sf = GetMessage("schedule.Shifts");
			string st = GetMessage("schedule.teams");
			string sc = GetMessage("schedule.coverage");
			string sn = GetMessage("schedule.non");
			string stn = GetMessage("schedule.total");

			string text = sch + ": " + base.ToString();
			try
			{
				text += "\n" + rd + ": " + GetRotationDuration() + ", " + sw + ": " + GetRotationWorkingTime();

				// Shifts
				text += "\n" + sf + ": ";
				int count = 1;
				foreach (Shift shift in Shifts)
				{
					text += "\n   (" + count + ") " + shift;
					count++;
				}

				// teams
				text += "\n" + st + ": ";
				count = 1;
				float teamPercent = 0.0f;
				foreach (Team team in Teams)
				{
					text += "\n   (" + count + ") " + team;
					teamPercent += team.GetPercentageWorked();
					count++;
				}
				text += "\n" + sc + ": " + teamPercent.ToString("0.00") + "%";

				// non-working periods
				List<NonWorkingPeriod> periods = NonWorkingPeriods;

				if (periods.Count > 0)
				{
					text += "\n" + sn + ":";

					Duration totalMinutes = Duration.Zero;

					count = 1;
					foreach (NonWorkingPeriod period in periods)
					{
						totalMinutes = totalMinutes.Plus(period.Duration);
						text += "\n   (" + count + ") " + period;
						count++;
					}
					text += "\n" + stn + ": " + totalMinutes.Minutes;
				}
			}
			catch (Exception)
			{
			}

			return text;
		}
	}
}
