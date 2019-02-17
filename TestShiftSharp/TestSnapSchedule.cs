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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using Point85.ShiftSharp.Schedule;

namespace TestShiftSharp
{
	[TestClass]
	public class TestSnapSchedule : BaseTest
	{
		[TestMethod]
		public void TestLowNight()
		{
			string description = "Low night demand";

			schedule = new WorkSchedule("Low Night Demand Plan", description);

			// 3 shifts
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(8));
			Shift swing = schedule.CreateShift("Swing", "Swing shift", new LocalTime(15, 0, 0), Duration.FromHours(8));
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(23, 0, 0), Duration.FromHours(8));

			// Team rotation
			Rotation rotation = schedule.CreateRotation("Low night demand", "Low night demand");
			rotation.AddSegment(day, 3, 0);
			rotation.AddSegment(swing, 4, 3);
			rotation.AddSegment(day, 4, 0);
			rotation.AddSegment(swing, 3, 4);
			rotation.AddSegment(day, 3, 0);
			rotation.AddSegment(night, 4, 3);
			rotation.AddSegment(day, 4, 0);
			rotation.AddSegment(night, 3, 4);

			// 6 teams
			schedule.CreateTeam("Team1", "First team", rotation, referenceDate);
			schedule.CreateTeam("Team2", "Second team", rotation, referenceDate.PlusDays(-21));
			schedule.CreateTeam("Team3", "Third team", rotation, referenceDate.PlusDays(-7));
			schedule.CreateTeam("Team4", "Fourth team", rotation, referenceDate.PlusDays(-28));
			schedule.CreateTeam("Team5", "Fifth team", rotation, referenceDate.PlusDays(-14));
			schedule.CreateTeam("Team6", "Sixth team", rotation, referenceDate.PlusDays(-35));

			runBaseTest(schedule, Duration.FromHours(224), Duration.FromDays(42), referenceDate);
		}

		[TestMethod]
		public void Test3TeamFixed24()
		{
			string description = "Fire departments";

			schedule = new WorkSchedule("3 Team Fixed 24 Plan", description);

			// Shift starts at 00:00 for 24 hours
			Shift shift = schedule.CreateShift("24 Hour", "24 hour shift", new LocalTime(0, 0, 0), Duration.FromHours(24));

			// Team rotation
			Rotation rotation = schedule.CreateRotation("3 Team Fixed 24 Plan", "3 Team Fixed 24 Plan");
			rotation.AddSegment(shift, 1, 1);
			rotation.AddSegment(shift, 1, 1);
			rotation.AddSegment(shift, 1, 4);

			// 3 teams
			schedule.CreateTeam("Team1", "First team", rotation, referenceDate);
			schedule.CreateTeam("Team2", "Second team", rotation, referenceDate.PlusDays(-3));
			schedule.CreateTeam("Team3", "Third team", rotation, referenceDate.PlusDays(-6));

			runBaseTest(schedule, Duration.FromHours(72), Duration.FromDays(9), referenceDate);
		}

		[TestMethod]
		public void Test549()
		{
			string description = "Compressed work schedule.";

			schedule = new WorkSchedule("5/4/9 Plan", description);

			// Shift 1 starts at 07:00 for 9 hours
			Shift day1 = schedule.CreateShift("Day1", "Day shift #1", new LocalTime(7, 0, 0), Duration.FromHours(9));

			// Shift 2 starts at 07:00 for 8 hours
			Shift day2 = schedule.CreateShift("Day2", "Day shift #2", new LocalTime(7, 0, 0), Duration.FromHours(8));

			// Team rotation (28 days)
			Rotation rotation = schedule.CreateRotation("5/4/9 ", "5/4/9 ");
			rotation.AddSegment(day1, 4, 0);
			rotation.AddSegment(day2, 1, 3);
			rotation.AddSegment(day1, 4, 3);
			rotation.AddSegment(day1, 4, 2);
			rotation.AddSegment(day1, 4, 0);
			rotation.AddSegment(day2, 1, 2);

			// 2 teams
			schedule.CreateTeam("Team1", "First team", rotation, referenceDate);
			schedule.CreateTeam("Team2", "Second team", rotation, referenceDate.PlusDays(-14));

			runBaseTest(schedule, Duration.FromHours(160), Duration.FromDays(28), referenceDate);
		}

		[TestMethod]
		public void Test9to5()
		{
			string description = "This is the basic 9 to 5 schedule plan for office employees. Every employee works 8 hrs a day from Monday to Friday.";

			schedule = new WorkSchedule("9 To 5 Plan", description);

			// Shift starts at 09:00 for 8 hours
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(9, 0, 0), Duration.FromHours(8));

			// Team1 rotation (5 days)
			Rotation rotation = schedule.CreateRotation("9 To 5 ", "9 To 5 ");
			rotation.AddSegment(day, 5, 2);

			// 1 team, 1 shift
			schedule.CreateTeam("Team", "One team", rotation, referenceDate);

			runBaseTest(schedule, Duration.FromHours(40), Duration.FromDays(7), referenceDate);
		}

		[TestMethod]
		public void Test8Plus12()
		{
			string description = "This is a fast rotation plan that uses 4 teams and a combination of three 8-hr shifts on weekdays "
						+ "and two 12-hr shifts on weekends to provide 24/7 coverage.";

			// work schedule
			schedule = new WorkSchedule("8 Plus 12 Plan", description);

			// Day shift #1, starts at 07:00 for 12 hours
			Shift day1 = schedule.CreateShift("Day1", "Day shift #1", new LocalTime(7, 0, 0), Duration.FromHours(12));

			// Day shift #2, starts at 07:00 for 8 hours
			Shift day2 = schedule.CreateShift("Day2", "Day shift #2", new LocalTime(7, 0, 0), Duration.FromHours(8));

			// Swing shift, starts at 15:00 for 8 hours
			Shift swing = schedule.CreateShift("Swing", "Swing shift", new LocalTime(15, 0, 0), Duration.FromHours(8));

			// Night shift #1, starts at 19:00 for 12 hours
			Shift night1 = schedule.CreateShift("Night1", "Night shift #1", new LocalTime(19, 0, 0), Duration.FromHours(12));

			// Night shift #2, starts at 23:00 for 8 hours
			Shift night2 = schedule.CreateShift("Night2", "Night shift #2", new LocalTime(23, 0, 0), Duration.FromHours(8));

			// shift rotation (28 days)
			Rotation rotation = schedule.CreateRotation("8 Plus 12", "8 Plus 12");
			rotation.AddSegment(day2, 5, 0);
			rotation.AddSegment(day1, 2, 3);
			rotation.AddSegment(night2, 2, 0);
			rotation.AddSegment(night1, 2, 0);
			rotation.AddSegment(night2, 3, 4);
			rotation.AddSegment(swing, 5, 2);

			// 4 teams, rotating through 5 shifts
			schedule.CreateTeam("Team 1", "First team", rotation, referenceDate);
			schedule.CreateTeam("Team 2", "Second team", rotation, referenceDate.PlusDays(-7));
			schedule.CreateTeam("Team 3", "Third team", rotation, referenceDate.PlusDays(-14));
			schedule.CreateTeam("Team 4", "Fourth team", rotation, referenceDate.PlusDays(-21));

			runBaseTest(schedule, Duration.FromHours(168), Duration.FromDays(28), referenceDate);
		}

		[TestMethod]
		public void TestICUInterns()
		{
			string description = "This plan supports a combination of 14-hr day shift , 15.5-hr cross-cover shift , and a 14-hr night shift for medical interns. "
						+ "The day shift and the cross-cover shift have the same start time (7:00AM). "
						+ "The night shift starts at around 10:00PM and ends at 12:00PM on the next day.";

			schedule = new WorkSchedule("ICU Interns Plan", description);

			// Day shift #1, starts at 07:00 for 15.5 hours
			Shift crossover = schedule.CreateShift("Crossover", "Day shift #1 cross-over", new LocalTime(7, 0, 0),
					Duration.FromHours(15).Plus(Duration.FromMinutes(30)));

			// Day shift #2, starts at 07:00 for 14 hours
			Shift day = schedule.CreateShift("Day", "Day shift #2", new LocalTime(7, 0, 0), Duration.FromHours(14));

			// Night shift, starts at 22:00 for 14 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(22, 0, 0), Duration.FromHours(14));

			// Team1 rotation
			Rotation rotation = schedule.CreateRotation("ICU", "ICU");
			rotation.AddSegment(day, 1, 0);
			rotation.AddSegment(crossover, 1, 0);
			rotation.AddSegment(night, 1, 1);

			schedule.CreateTeam("Team 1", "First team", rotation, referenceDate);
			schedule.CreateTeam("Team 2", "Second team", rotation, referenceDate.PlusDays(-3));
			schedule.CreateTeam("Team 3", "Third team", rotation, referenceDate.PlusDays(-2));
			schedule.CreateTeam("Team 4", "Forth team", rotation, referenceDate.PlusDays(-1));

			runBaseTest(schedule, Duration.FromMinutes(2610), Duration.FromDays(4), referenceDate);
		}

		[TestMethod]
		public void TestDupont()
		{
			string description = "The DuPont 12-hour rotating shift schedule uses 4 teams (crews) and 2 twelve-hour shifts to provide 24/7 coverage. "
						+ "It consists of a 4-week cycle where each team works 4 consecutive night shifts, "
						+ "followed by 3 days off duty, works 3 consecutive day shifts, followed by 1 day off duty, works 3 consecutive night shifts, "
						+ "followed by 3 days off duty, work 4 consecutive day shift, then have 7 consecutive days off duty. "
						+ "Personnel works an average 42 hours per week.";

			schedule = new WorkSchedule("DuPont Shift Schedule", description);

			// Day shift, starts at 07:00 for 12 hours
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(12));

			// Night shift, starts at 19:00 for 12 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(19, 0, 0), Duration.FromHours(12));

			// Team1 rotation
			Rotation rotation = schedule.CreateRotation("DuPont", "DuPont");
			rotation.AddSegment(night, 4, 3);
			rotation.AddSegment(day, 3, 1);
			rotation.AddSegment(night, 3, 3);
			rotation.AddSegment(day, 4, 7);

			schedule.CreateTeam("Team 1", "First team", rotation, referenceDate);
			schedule.CreateTeam("Team 2", "Second team", rotation, referenceDate.PlusDays(-7));
			schedule.CreateTeam("Team 3", "Third team", rotation, referenceDate.PlusDays(-14));
			schedule.CreateTeam("Team 4", "Forth team", rotation, referenceDate.PlusDays(-21));

			runBaseTest(schedule, Duration.FromHours(168), Duration.FromDays(28), referenceDate);
		}

		[TestMethod]
		public void TestDNO()
		{
			string description = "This is a fast rotation plan that uses 3 teams and two 12-hr shifts to provide 24/7 coverage. "
						+ "Each team rotates through the following sequence every three days: 1 day shift, 1 night shift, and 1 day off.";

			schedule = new WorkSchedule("DNO Plan", description);

			// Day shift, starts at 07:00 for 12 hours
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(12));

			// Night shift, starts at 19:00 for 12 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(19, 0, 0), Duration.FromHours(12));

			// rotation
			Rotation rotation = schedule.CreateRotation("DNO", "DNO");
			rotation.AddSegment(day, 1, 0);
			rotation.AddSegment(night, 1, 1);

			schedule.CreateTeam("Team 1", "First team", rotation, referenceDate);
			schedule.CreateTeam("Team 2", "Second team", rotation, referenceDate.PlusDays(-1));
			schedule.CreateTeam("Team 3", "Third team", rotation, referenceDate.PlusDays(-2));

			// rotation working time
			LocalDateTime from = referenceDate.PlusDays(rotation.GetDayCount()).At(new LocalTime(7, 0, 0));
			Duration duration = schedule.CalculateWorkingTime(from, from.PlusDays(3));
			Assert.IsTrue(duration.Equals(Duration.FromHours(72)));

			runBaseTest(schedule, Duration.FromHours(24), Duration.FromDays(3), referenceDate);
		}

		[TestMethod]
		public void Test21TeamFixed()
		{
			string description = "This plan is a fixed (no rotation) plan that uses 21 teams and three 8-hr shifts to provide 24/7 coverage. "
						+ "It maximizes the number of consecutive days off while still averaging 40 hours per week. "
						+ "Over a 7 week cycle, each employee has two 3 consecutive days off and is required to work 6 consecutive days on 5 of the 7 weeks. "
						+ "On any given day, 15 teams will be scheduled to work and 6 teams will be off. "
						+ "Each shift will be staffed by 5 teams so the minimum number of employees per shift is five. ";

			schedule = new WorkSchedule("21 Team Fixed 8 6D Plan", description);

			// Day shift, starts at 07:00 for 8 hours
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(8));

			// Swing shift, starts at 15:00 for 8 hours
			Shift swing = schedule.CreateShift("Swing", "Swing shift", new LocalTime(15, 0, 0), Duration.FromHours(8));

			// Night shift, starts at 15:00 for 8 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(23, 0, 0), Duration.FromHours(8));

			// day rotation
			Rotation dayRotation = schedule.CreateRotation("Day", "Day");
			dayRotation.AddSegment(day, 6, 3);
			dayRotation.AddSegment(day, 5, 3);
			dayRotation.AddSegment(day, 6, 2);
			dayRotation.AddSegment(day, 6, 2);
			dayRotation.AddSegment(day, 6, 2);
			dayRotation.AddSegment(day, 6, 2);

			// swing rotation
			Rotation swingRotation = schedule.CreateRotation("Swing", "Swing");
			swingRotation.AddSegment(swing, 6, 3);
			swingRotation.AddSegment(swing, 5, 3);
			swingRotation.AddSegment(swing, 6, 2);
			swingRotation.AddSegment(swing, 6, 2);
			swingRotation.AddSegment(swing, 6, 2);
			swingRotation.AddSegment(swing, 6, 2);

			// night rotation
			Rotation nightRotation = schedule.CreateRotation("Night", "Night");
			nightRotation.AddSegment(night, 6, 3);
			nightRotation.AddSegment(night, 5, 3);
			nightRotation.AddSegment(night, 6, 2);
			nightRotation.AddSegment(night, 6, 2);
			nightRotation.AddSegment(night, 6, 2);
			nightRotation.AddSegment(night, 6, 2);

			// day teams
			schedule.CreateTeam("Team 1", "1st day team", dayRotation, referenceDate);
			schedule.CreateTeam("Team 2", "2nd day team", dayRotation, referenceDate.PlusDays(7));
			schedule.CreateTeam("Team 3", "3rd day team", dayRotation, referenceDate.PlusDays(14));
			schedule.CreateTeam("Team 4", "4th day team", dayRotation, referenceDate.PlusDays(21));
			schedule.CreateTeam("Team 5", "5th day team", dayRotation, referenceDate.PlusDays(28));
			schedule.CreateTeam("Team 6", "6th day team", dayRotation, referenceDate.PlusDays(35));
			schedule.CreateTeam("Team 7", "7th day team", dayRotation, referenceDate.PlusDays(42));

			// swing teams
			schedule.CreateTeam("Team 8", "1st swing team", swingRotation, referenceDate);
			schedule.CreateTeam("Team 9", "2nd swing team", swingRotation, referenceDate.PlusDays(7));
			schedule.CreateTeam("Team 10", "3rd swing team", swingRotation, referenceDate.PlusDays(14));
			schedule.CreateTeam("Team 11", "4th swing team", swingRotation, referenceDate.PlusDays(21));
			schedule.CreateTeam("Team 12", "5th swing team", swingRotation, referenceDate.PlusDays(28));
			schedule.CreateTeam("Team 13", "6th swing team", swingRotation, referenceDate.PlusDays(35));
			schedule.CreateTeam("Team 14", "7th swing team", swingRotation, referenceDate.PlusDays(42));

			// night teams
			schedule.CreateTeam("Team 15", "1st night team", nightRotation, referenceDate);
			schedule.CreateTeam("Team 16", "2nd night team", nightRotation, referenceDate.PlusDays(7));
			schedule.CreateTeam("Team 17", "3rd night team", nightRotation, referenceDate.PlusDays(14));
			schedule.CreateTeam("Team 18", "4th night team", nightRotation, referenceDate.PlusDays(21));
			schedule.CreateTeam("Team 19", "5th night team", nightRotation, referenceDate.PlusDays(28));
			schedule.CreateTeam("Team 20", "6th night team", nightRotation, referenceDate.PlusDays(35));
			schedule.CreateTeam("Team 21", "7th night team", nightRotation, referenceDate.PlusDays(42));

			runBaseTest(schedule, Duration.FromHours(280), Duration.FromDays(49), referenceDate.PlusDays(49));

		}

		[TestMethod]
		public void TestTwoTeam()
		{
			string description = "This is a fixed (no rotation) plan that uses 2 teams and two 12-hr shifts to provide 24/7 coverage. "
						+ "One team will be permanently on the day shift and the other will be on the night shift.";

			schedule = new WorkSchedule("2 Team Fixed 12 Plan", description);

			// Day shift, starts at 07:00 for 12 hours
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(12));

			// Night shift, starts at 19:00 for 12 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(19, 0, 0), Duration.FromHours(12));

			// Team1 rotation
			Rotation team1Rotation = schedule.CreateRotation("Team1", "Team1");
			team1Rotation.AddSegment(day, 1, 0);

			// Team1 rotation
			Rotation team2Rotation = schedule.CreateRotation("Team2", "Team2");
			team2Rotation.AddSegment(night, 1, 0);

			schedule.CreateTeam("Team 1", "First team", team1Rotation, referenceDate);
			schedule.CreateTeam("Team 2", "Second team", team2Rotation, referenceDate);

			runBaseTest(schedule, Duration.FromHours(12), Duration.FromDays(1), referenceDate);
		}

		[TestMethod]
		public void TestPanama()
		{
			string description = "This is a slow rotation plan that uses 4 teams and two 12-hr shifts to provide 24/7 coverage. "
						+ "The working and non-working days follow this pattern: 2 days on, 2 days off, 3 days on, 2 days off, 2 days on, 3 days off. "
						+ "Each team works the same shift (day or night) for 28 days then switches over to the other shift for the next 28 days. "
						+ "After 56 days, the same sequence starts over.";

			string scheduleName = "Panama";
			schedule = new WorkSchedule(scheduleName, description);

			// Day shift, starts at 07:00 for 12 hours
			Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(12));

			// Night shift, starts at 19:00 for 12 hours
			Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(19, 0, 0), Duration.FromHours(12));

			// rotation
			Rotation rotation = schedule.CreateRotation("Panama",
					"2 days on, 2 days off, 3 days on, 2 days off, 2 days on, 3 days off");
			// 2 days on, 2 off, 3 on, 2 off, 2 on, 3 off (and repeat)
			rotation.AddSegment(day, 2, 2);
			rotation.AddSegment(day, 3, 2);
			rotation.AddSegment(day, 2, 3);
			rotation.AddSegment(day, 2, 2);
			rotation.AddSegment(day, 3, 2);
			rotation.AddSegment(day, 2, 3);

			// 2 nights on, 2 off, 3 on, 2 off, 2 on, 3 off (and repeat)
			rotation.AddSegment(night, 2, 2);
			rotation.AddSegment(night, 3, 2);
			rotation.AddSegment(night, 2, 3);
			rotation.AddSegment(night, 2, 2);
			rotation.AddSegment(night, 3, 2);
			rotation.AddSegment(night, 2, 3);

			// reference date for start of shift rotations
			LocalDate referenceDate = new LocalDate(2016, 10, 31);

			schedule.CreateTeam("Team 1", "First team", rotation, referenceDate);
			schedule.CreateTeam("Team 2", "Second team", rotation, referenceDate.PlusDays(-28));
			schedule.CreateTeam("Team 3", "Third team", rotation, referenceDate.PlusDays(-7));
			schedule.CreateTeam("Team 4", "Fourth team", rotation, referenceDate.PlusDays(-35));

			runBaseTest(schedule, Duration.FromHours(336), Duration.FromDays(56), referenceDate);
		}
	}
}
