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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class TeamMember represents a person assigned to a Team
	/// </summary>
	public class TeamMember : Named, IComparable<TeamMember>
	{
		/// <summary>
		/// Identifier of the team member, e.g. employee ID
		/// </summary>
		public string MemberID { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public TeamMember() : base()
		{
		}

		/// <summary>
		/// Constructor with id
		/// </summary>
		/// <param name="name">Member name</param>
		/// <param name="description">Member description, e.g. title</param>
		/// <param name="id">Member ID, e.g. employee ID</param>
		public TeamMember(string name, string description, string id) : base(name, description)
		{
			this.MemberID = id;
		}

		/// <summary>
		/// Compare one team member to another
		/// </summary>
		/// <param name="other">Other team member</param>
		/// <returns>-1 if less than, 0 if equal and 1 if greater than</returns>
		public int CompareTo(TeamMember other)
		{
			return Name.CompareTo(other.Name);
		}

		/// <summary>
		/// Check for equality
		/// </summary>
		/// <param name="other">other Quantity</param>
		/// <returns>True if equal</returns>
		public override bool Equals(Object other)
		{
			if (MemberID == null || other == null || GetType() != other.GetType())
			{
				return false;
			}

			return MemberID.Equals(((TeamMember)other).MemberID);
		}

		/// <summary>
		/// Compute a hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			unchecked 
			{
				int hash = 17;
				hash = hash * 23 + Name.GetHashCode();
				hash = hash * 23 + (MemberID != null ? MemberID.GetHashCode() : 0);
				return hash;
			}
		}

		/// <summary>
		/// Build a string value for this team member
		/// </summary>
		/// <returns>String</returns>
		public override string ToString()
		{
			string id = WorkSchedule.GetMessage("member.id");

			string text = "";

			try
			{
				text = base.ToString() + ", " + id + ": " + MemberID;
			}
			catch (Exception)
			{
			}
			return text;
		}
	}
}
