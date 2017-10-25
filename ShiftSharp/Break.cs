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

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class Break is a defined working period of time during a shift, for example lunch.
	/// </summary> 
	public class Break : TimePeriod
	{
		/// <summary>
		/// Break constructor
		/// </summary>
		/// <param name="name">Name of break</param>
		/// <param name="description">Description of break</param>
		/// <param name="start">Start time of break</param>
		/// <param name="duration">Duration of break</param>
		public Break(string name, string description, LocalTime start, Duration duration) : base(name, description, start, duration)
		{
		}

		/// <summary>
		/// A break is a working period
		/// </summary>
		/// <returns>True</returns>
		public override bool IsWorkingPeriod()
		{
			return true;
		}
	}
}
