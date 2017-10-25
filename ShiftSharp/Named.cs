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

using System;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// Class Named represents a named object such as a Shift or Team.
	/// </summary>
	public abstract class Named
	{
		/// <summary>
		/// name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// description
		/// </summary>
		public string Description { get; set; }

		protected Named()
		{

		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		protected Named(string name, string description)
		{
			Name = name ?? throw new Exception(WorkSchedule.GetMessage("name.not.defined"));
			Description = description;
		}

		/// <summary>
		/// Check for equality
		/// </summary>
		/// <param name="other">other Quantity</param>
		/// <returns>True if equal</returns>
		public override bool Equals(Object other)
		{
			if (Name == null || other == null || GetType() != other.GetType())
			{
				return false;
			}

			return Name.Equals(((Named)other).Name);
		}

		/// <summary>
		/// Compute a hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		/// <summary>
		/// Get a string representation of a named object
		/// </summary>
		/// <returns>String value</returns>
		public override string ToString()
		{
			return Name + " (" + Description + ")";
		}
	}
}
