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
using System;

namespace TestShiftSharp
{
	[TestClass]
	public class TestSnippet : BaseTest
	{
		[TestMethod]
		public void TestPartial()
		{
			schedule = new WorkSchedule("Working Time1", "Test working time");
			/*
			// shift does not cross midnight
			Duration shiftDuration = Duration.FromHours(8);
			LocalTime shiftStart = new LocalTime(7, 0, 0);

			Shift shift = schedule.CreateShift("Work Shift1", "Working time shift", shiftStart, shiftDuration);
			LocalTime shiftEnd = shift.GetEnd();

			// case #1
			Duration time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(2)));
			Assert.IsTrue(time.TotalSeconds == 0);
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(3)));
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #2
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftStart.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #3
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(2));
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #4
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #5
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(2));
			Assert.IsTrue(time.TotalSeconds == 0);
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #6
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #7
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(1));
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #8
			time = shift.CalculateWorkingTime(shiftStart, shiftEnd);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #9
			time = shift.CalculateWorkingTime(shiftStart, shiftStart);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #10
			time = shift.CalculateWorkingTime(shiftEnd, shiftEnd);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #11
			time = shift.CalculateWorkingTime(shiftStart, shiftStart.PlusSeconds(1));
			Assert.IsTrue(time.TotalSeconds == 1);

			// case #12
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromSeconds(1)), shiftEnd);
			Assert.IsTrue(time.TotalSeconds == 1);

			// 8 hr shift crossing midnight
			shiftStart = new LocalTime(22, 0, 0);

			shift = schedule.CreateShift("Work Shift2", "Working time shift", shiftStart, shiftDuration);
			shiftEnd = shift.GetEnd();

			// case #1
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(2)), true);
			Assert.IsTrue(time.TotalSeconds == 0);
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(3)), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #2
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftStart.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #3
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(2), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #4
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1), false);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #5
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(2), true);
			Assert.IsTrue(time.TotalSeconds == 0);
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #6
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #7
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #8
			time = shift.CalculateWorkingTime(shiftStart, shiftEnd, true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #9
			time = shift.CalculateWorkingTime(shiftStart, shiftStart, true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #10
			time = shift.CalculateWorkingTime(shiftEnd, shiftEnd, true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #11
			time = shift.CalculateWorkingTime(shiftStart, shiftStart.PlusSeconds(1), true);
			Assert.IsTrue(time.TotalSeconds == 1);

			// case #12
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromSeconds(1)), shiftEnd, false);
			Assert.IsTrue(time.TotalSeconds == 1);
			*/
			// 24 hr shift crossing midnight
			Duration shiftDuration = Duration.FromHours(24);
			LocalTime shiftStart = new LocalTime(7, 0, 0);

			Shift shift = schedule.CreateShift("Work Shift3", "Working time shift", shiftStart, shiftDuration);
			LocalTime shiftEnd = shift.GetEnd();
			/*
			// case #1
			Duration time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(2)), false);
			Assert.IsTrue(time.TotalSeconds == 3600);
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(3)), shiftStart.Minus(Period.FromHours(3)), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #2
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftStart.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #3
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(2), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #4
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #5
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(2), true);
			Assert.IsTrue(time.TotalSeconds == 3600);
			time = shift.CalculateWorkingTime(shiftEnd.PlusHours(1), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 0);

			// case #6
			time = shift.CalculateWorkingTime(shiftStart.Minus(Period.FromHours(1)), shiftEnd.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 3600);

			// case #7
			time = shift.CalculateWorkingTime(shiftStart.PlusHours(1), shiftStart.PlusHours(1), true);
			Assert.IsTrue(time.TotalSeconds == 0);
			*/
			// case #8
			Duration time = shift.CalculateWorkingTime(shiftStart, shiftEnd, true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #9
			time = shift.CalculateWorkingTime(shiftStart, shiftStart, true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #10
			time = shift.CalculateWorkingTime(shiftEnd, shiftEnd, true);
			Assert.IsTrue(time.TotalSeconds == shiftDuration.TotalSeconds);

			// case #11
			time = shift.CalculateWorkingTime(shiftStart, shiftStart.PlusSeconds(1), true);
			Assert.IsTrue(time.TotalSeconds == 1);

			// case #12
			time = shift.CalculateWorkingTime(shiftEnd.Minus(Period.FromSeconds(1)), shiftEnd, false);
			Assert.IsTrue(time.TotalSeconds == 1);
		}
	}
}
