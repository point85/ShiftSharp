/*
MIT License

Copyright (c) 2024 Kent Randall

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// This class provides information for adding or removing a team member for a team working an instance of a shift.
	/// The shift instance is identified by its starting date and time.
	/// </summary>
	public class TeamMemberException
	{
		/// <summary>
		/// start date and time of day of the shift
		/// </summary>
		public LocalDateTime DateTime { get; private set; }

		/// <summary>
		/// reason for the change 
		/// </summary>
		public string Reason { get; set; }

		/// <summary>
		/// team member to add 
		/// </summary>
		public TeamMember Addition { get; set; }

		/// <summary>
		/// team member to remove 
		/// </summary>
		public TeamMember Removal { get; set; }

		/// <summary>
		/// Construct an exception for the shift instance at this starting date and time
		/// </summary>
		/// <param name="dateTime">Shift instance starting date and time</param>
		public TeamMemberException(LocalDateTime dateTime)
		{
			this.DateTime = dateTime;
		}
	}
}
