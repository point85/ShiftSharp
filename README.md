# ShiftSharp
The ShiftSharp library project manages work schedules.  A work schedule consists of one or more teams who rotate through a sequence of shift and off-shift periods of time.  The ShiftSharp project allows breaks during shifts to be defined as well as non-working periods of time (e.g. holidays and scheduled maintenance periods) that are applicable to the entire work schedule.  The C# project is a port of the java project at https://github.com/point85/Shift.

## Concepts

The diagram below illustrates Business Management Systems' DNO (Day, Night, Off) work schedule with three teams and two 12-hour shifts with a 3-day rotation.  This schedule is explained in http://community.bmscentral.com/learnss/ZC/3T/c3tr12-1.

![Work Schedule Diagram](https://github.com/point85/shift/blob/master/doc/DNO.png)

*Shift*

A shift is defined with a name, description, starting time of day and duration.  An off-shift period is associated with a shift.  In the example above for Team1, there are two shifts followed by one off-shift period.  Shifts can be overlapped (typically when a handoff of duties is important such as in the nursing profession).  A rotation is a sequence of shifts and off-shift days.  The DNO rotation is Day on, Night on and Night off.  An instance of a shift has a starting date and time of day and has an associated shift definition.

*Team*

A team is defined with a name and description.  It has a rotation with a starting date.  The starting date shift will have an instance with that date and a starting time of day as defined by the shift.  The same rotation can be shared between more than one team, but with different starting times.

*Work Schedule*

A work schedule is defined with a name and description.  It has one or more teams.  Zero or more non-working periods can be defined.  A non-working period has a defined starting date and time of day and duration.  For example, the New Year's Day holiday starting at midnight for 24 hours, or three consecutive days for preventive maintenance of manufacturing equipment starting at the end of the night shift. 

After a work schedule is defined, the working time for all shifts can be computed for a defined time interval.  For example, this duration of time is the maximum available productive time as an input to the calculation of the utilization of equipment in a metric known as the Overall Equipment Effectiveness (OEE).

*Rotation*

A rotation is a sequence of working periods (segments).  Each segment starts with a shift and specifies the number of days on-shift and off-shift.  A work schedule can have more than one rotation.

*Non-Working Period*

A non-working period is a duration of time where no teams are working.  For example, a holiday or a period of time when a plant is shutdown for preventative maintenance.  A non-working period starts at a defined day and time of day and continues for the specified duration of time.

*Shift Instance*

A shift instance is the duration of time from a specified date and time of day and continues for the duration of the associated shift.  A team works this shift instance.

## Examples
The DNO schedule discussed above is defined as follows.

```cs
String description = "This is a fast rotation plan that uses 3 teams and two 12-hr shifts to provide 24/7 coverage. "
	+ "Each team rotates through the following sequence every three days: 1 day shift, 1 night shift, and 1 day off.";

WorkSchedule schedule = new WorkSchedule("DNO Plan", description);

// Day shift, starts at 07:00 for 12 hours
Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(12));

// Night shift, starts at 19:00 for 12 hours
Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(19, 0, 0), Duration.FromHours(12));

// rotation
Rotation rotation = new Rotation("DNO", "DNO");
rotation.AddSegment(day, 1, 0);
rotation.AddSegment(night, 1, 1);

schedule.CreateTeam("Team 1", "First team", rotation, referenceDate);
schedule.CreateTeam("Team 2", "Second team", rotation, referenceDate.Minus(Period.FromDays(1)));
schedule.CreateTeam("Team 3", "Third team", rotation, referenceDate.Minus(Period.FromDays(2)));
```
To obtain the working time over three days starting at 07:00, the following methods are called:

```cs
LocalDateTime from = referenceDate.At(new LocalTime(7, 0, 0));
Duration duration = schedule.CalculateWorkingTime(from, from.PlusDays(3));
```

To obtain the shift instances for a date, the following method is called:

```cs
List<ShiftInstance> instances = schedule.GetShiftInstancesForDay(new LocalDate(2017, 3, 1));
```

To print a work schedule, call the ToString() method.  For example:

```cs
Schedule: DNO Plan (This is a fast rotation plan that uses 3 teams and two 12-hr shifts to provide 24/7 coverage. Each team rotates through the following sequence every three days: 1 day shift, 1 night shift, and 1 day off.)
Rotation duration: PT216H, Scheduled working time: PT72H
Shifts: 
   (1) Day (Day shift), Start : 07:00 (PT12H), End : 19:00
   (2) Night (Night shift), Start : 19:00 (PT12H), End : 07:00
Teams: 
   (1) Team 1 (First team), Rotation start: 2016-10-31, Rotation periods: [Day (on), Night (on), (off) ], Rotation duration: PT72H, Days in rotation: 3, Scheduled working time: PT24H, Percentage worked: 33.33%, Average hours worked per week: PT56H
   (2) Team 2 (Second team), Rotation start: 2016-10-30, Rotation periods: [Day (on), Night (on),(off) ], Rotation duration: PT72H, Days in rotation: 3, Scheduled working time: PT24H, Percentage worked: 33.33%, Average hours worked per week: PT56H
   (3) Team 3 (Third team), Rotation start: 2016-10-29, Rotation periods: [Day (on), Night (on), (off) ], Rotation duration: PT72H, Days in rotation: 3, Scheduled working time: PT24H, Percentage worked: 33.33%, Average hours worked per week: PT56H
Total team coverage: 100%
```

To print shift instances between two dates, the following method is called:

 ```cs
schedule.PrintShiftInstances(new LocalDate(2016, 10, 31), new LocalDate(2016, 11, 3));
```
with output:

```cs
Working shifts
[1] Day: 2016-10-31
   (1) Team: Team 1, Shift: Day, Start : 2016-10-31T07:00, End : 2016-10-31T19:00
   (2) Team: Team 2, Shift: Night, Start : 2016-10-31T19:00, End : 2016-11-01T07:00
[2] Day: 2016-11-01
   (1) Team: Team 3, Shift: Day, Start : 2016-11-01T07:00, End : 2016-11-01T19:00
   (2) Team: Team 1, Shift: Night, Start : 2016-11-01T19:00, End : 2016-11-02T07:00
[3] Day: 2016-11-02
   (1) Team: Team 2, Shift: Day, Start : 2016-11-02T07:00, End : 2016-11-02T19:00
   (2) Team: Team 3, Shift: Night, Start : 2016-11-02T19:00, End : 2016-11-03T07:00
[4] Day: 2016-11-03
   (1) Team: Team 1, Shift: Day, Start : 2016-11-03T07:00, End : 2016-11-03T19:00
   (2) Team: Team 2, Shift: Night, Start : 2016-11-03T19:00, End : 2016-11-04T07:00
```
 
For a second example, the 24/7 schedule below has two rotations for four teams in two shifts.  It is used by manufacturing companies.

```cs
WorkSchedule schedule = new WorkSchedule("Manufacturing Company - four twelves",
	"Four 12 hour alternating day/night shifts");

// day shift, start at 07:00 for 12 hours
Shift day = schedule.CreateShift("Day", "Day shift", new LocalTime(7, 0, 0), Duration.FromHours(12));

// night shift, start at 19:00 for 12 hours
Shift night = schedule.CreateShift("Night", "Night shift", new LocalTime(19, 0, 0), Duration.FromHours(12));

// 7 days ON, 7 OFF
Rotation dayRotation = new Rotation("Day", "Day");
dayRotation.AddSegment(day, 7, 7);

// 7 nights ON, 7 OFF
Rotation nightRotation = new Rotation("Night", "Night");
nightRotation.AddSegment(night, 7, 7);

schedule.CreateTeam("A", "A day shift", dayRotation, new LocalDate(2014, 1, 2));
schedule.CreateTeam("B", "B night shift", nightRotation, new LocalDate(2014, 1, 2));
schedule.CreateTeam("C", "C day shift", dayRotation, new LocalDate(2014, 1, 9));
schedule.CreateTeam("D", "D night shift", nightRotation, new LocalDate(2014, 1, 9));
```

When printed out for a week of shift instances, the output is:

```cs
Schedule: Manufacturing Company - four twelves (Four 12 hour alternating day/night shifts)
Rotation duration: PT1344H, Scheduled working time: PT336H
Shifts: 
   (1) Day (Day shift), Start : 07:00 (PT12H), End : 19:00
   (2) Night (Night shift), Start : 19:00 (PT12H), End : 07:00
Teams: 
   (1) A (A day shift), Rotation start: 2014-01-02, Rotation periods: [Day (on), Day (on), Day (on), Day (on), Day (on), Day (on), Day (on), (off), (off), (off), (off), (off), (off), (off) ], Rotation duration: PT336H, Days in rotation: 14, Scheduled working time: PT84H, Percentage worked: 25%, Average hours worked per week: PT42H
   (2) B (B night shift), Rotation start: 2014-01-02, Rotation periods: [Night (on), Night (on), Night (on), Night (on), Night (on), Night (on), Night (on), (off), (off), (off), (off), (off), (off), (off) ], Rotation duration: PT336H, Days in rotation: 14, Scheduled working time: PT84H, Percentage worked: 25%, Average hours worked per week: PT42H
   (3) C (C day shift), Rotation start: 2014-01-09, Rotation periods: [Day (on), Day (on), Day (on), Day (on), Day (on), Day (on), Day (on), (off), (off), (off), (off), (off), (off), (off) ], Rotation duration: PT336H, Days in rotation: 14, Scheduled working time: PT84H, Percentage worked: 25%, Average hours worked per week: PT42H
   (4) D (D night shift), Rotation start: 2014-01-09, Rotation periods: [Night (on), Night (on), Night (on), Night (on), Night (on), Night (on), Night (on), (off), (off), (off), (off), (off), (off), (off) ], Rotation duration: PT336H, Days in rotation: 14, Scheduled working time: PT84H, Percentage worked: 25%, Average hours worked per week: PT42H
Total team coverage: 100%
Working shifts
[1] Day: 2014-01-09
   (1) Team: C, Shift: Day, Start : 2014-01-09T07:00, End : 2014-01-09T19:00
   (2) Team: D, Shift: Night, Start : 2014-01-09T19:00, End : 2014-01-10T07:00
[2] Day: 2014-01-10
   (1) Team: C, Shift: Day, Start : 2014-01-10T07:00, End : 2014-01-10T19:00
   (2) Team: D, Shift: Night, Start : 2014-01-10T19:00, End : 2014-01-11T07:00
[3] Day: 2014-01-11
   (1) Team: C, Shift: Day, Start : 2014-01-11T07:00, End : 2014-01-11T19:00
   (2) Team: D, Shift: Night, Start : 2014-01-11T19:00, End : 2014-01-12T07:00
[4] Day: 2014-01-12
   (1) Team: C, Shift: Day, Start : 2014-01-12T07:00, End : 2014-01-12T19:00
   (2) Team: D, Shift: Night, Start : 2014-01-12T19:00, End : 2014-01-13T07:00
[5] Day: 2014-01-13
   (1) Team: C, Shift: Day, Start : 2014-01-13T07:00, End : 2014-01-13T19:00
   (2) Team: D, Shift: Night, Start : 2014-01-13T19:00, End : 2014-01-14T07:00
[6] Day: 2014-01-14
   (1) Team: C, Shift: Day, Start : 2014-01-14T07:00, End : 2014-01-14T19:00
   (2) Team: D, Shift: Night, Start : 2014-01-14T19:00, End : 2014-01-15T07:00
[7] Day: 2014-01-15
   (1) Team: C, Shift: Day, Start : 2014-01-15T07:00, End : 2014-01-15T19:00
   (2) Team: D, Shift: Night, Start : 2014-01-15T19:00, End : 2014-01-16T07:00
```

For a third example, the work schedule below with one 24 hour shift over an 18 day rotation for three platoons is used by Kern County California firefighters.

```cs
WorkSchedule schedule = new WorkSchedule("Kern Co.", "Three 24 hour alternating shifts");

// shift, start 07:00 for 24 hours
Shift shift = schedule.CreateShift("24 Hour", "24 hour shift", new LocalTime(7, 0, 0), Duration.FromHours(24));

// 2 days ON, 2 OFF, 2 ON, 2 OFF, 2 ON, 8 OFF
Rotation rotation = new Rotation("24 Hour", "2 days ON, 2 OFF, 2 ON, 2 OFF, 2 ON, 8 OFF");
rotation.AddSegment(shift, 2, 2);
rotation.AddSegment(shift, 2, 2);
rotation.AddSegment(shift, 2, 8);

Team platoon1 = schedule.CreateTeam("Red", "A Shift", rotation, new LocalDate(2017, 1, 8));
Team platoon2 = schedule.CreateTeam("Black", "B Shift", rotation, new LocalDate(2017, 2, 1));
Team platoon3 = schedule.CreateTeam("Green", "C Shift", rotation, new LocalDate(2017, 1, 2));
```

When printed out for a week of shift instances, the output is:
```cs
Schedule: Kern Co. (Three 24 hour alternating shifts)
Rotation duration: PT1296H, Scheduled working time: PT432H
Shifts: 
   (1) 24 Hour (24 hour shift), Start : 07:00 (PT24H), End : 07:00
Teams: 
   (1) Red (A Shift), Rotation start: 2017-01-08, Rotation periods: [24 Hour (on), 24 Hour (on), (off), (off), 24 Hour (on), 24 Hour (on), (off), (off), 24 Hour (on), 24 Hour (on), (off), (off), (off), (off), (off), (off), (off), (off) ], Rotation duration: PT432H, Days in rotation: 18, Scheduled working time: PT144H, Percentage worked: 33.33%, Average hours worked per week: PT56H
   (2) Black (B Shift), Rotation start: 2017-02-01, Rotation periods: [24 Hour (on), 24 Hour (on), (off), (off), 24 Hour (on), 24 Hour (on), (off), (off), 24 Hour (on), 24 Hour (on), (off), (off), (off), (off), (off), (off), (off), (off) ], Rotation duration: PT432H, Days in rotation: 18, Scheduled working time: PT144H, Percentage worked: 33.33%, Average hours worked per week: PT56H
   (3) Green (C Shift), Rotation start: 2017-01-02, Rotation periods: [24 Hour (on), 24 Hour (on), (off), (off), 24 Hour (on), 24 Hour (on), (off), (off), 24 Hour (on), 24 Hour (on), (off), (off), (off), (off), (off), (off), (off), (off) ], Rotation duration: PT432H, Days in rotation: 18, Scheduled working time: PT144H, Percentage worked: 33.33%, Average hours worked per week: PT56H
Total team coverage: 100%
Working shifts
[1] Day: 2017-02-01
   (1) Team: Black, Shift: 24 Hour, Start : 2017-02-01T07:00, End : 2017-02-02T07:00
[2] Day: 2017-02-02
   (1) Team: Black, Shift: 24 Hour, Start : 2017-02-02T07:00, End : 2017-02-03T07:00
[3] Day: 2017-02-03
   (1) Team: Red, Shift: 24 Hour, Start : 2017-02-03T07:00, End : 2017-02-04T07:00
[4] Day: 2017-02-04
   (1) Team: Red, Shift: 24 Hour, Start : 2017-02-04T07:00, End : 2017-02-05T07:00
[5] Day: 2017-02-05
   (1) Team: Black, Shift: 24 Hour, Start : 2017-02-05T07:00, End : 2017-02-06T07:00
[6] Day: 2017-02-06
   (1) Team: Black, Shift: 24 Hour, Start : 2017-02-06T07:00, End : 2017-02-07T07:00
[7] Day: 2017-02-07
   (1) Team: Green, Shift: 24 Hour, Start : 2017-02-07T07:00, End : 2017-02-08T07:00
```

## Project Structure
ShiftSharp depends upon .Net Framework 4.5+ due to use of the NodaTime date and time classes.

ShiftSharp has the following structure:
 * `/Documentation/html` Doxygen HTML files
 * `.` - C# source files
 * `/Resources` - localizable Message.properties file to define error messages.
 * `../TestShiftSharp` - unit test C# project
 * `../dotnetcore` - csproj files for .NET Core 3.0 framework
 
The compiled ShiftSharp.dll can be found under the "resources" link.
