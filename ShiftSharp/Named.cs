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
		// name
		private string name;

		// description
		private string description;

		protected Named()
		{

		}

		protected Named(string name, string description)
		{
			SetName(name);
			SetDescription(description);
		}

		/// <summary>
		/// Get name
		/// </summary>
		/// <returns>Name</returns>
		public string GetName()
		{
			return name;
		}

		/// <summary>
		/// Set name
		/// </summary>
		/// <param name="name">Name</param>
		public void SetName(string name)
		{
			if (name == null)
			{
				throw new Exception(WorkSchedule.GetMessage("name.not.defined"));
			}
			this.name = name;
		}

		/// <summary>
		/// Get description
		/// </summary>
		/// <returns>Description</returns>
		public string GetDescription()
		{
			return description;
		}

		/// <summary>
		/// Set description
		/// </summary>
		/// <param name="description"> Description</param>
		public void SetDescription(string description)
		{
			this.description = description;
		}

		/// <summary>
		/// Check for equality
		/// </summary>
		/// <param name="other">other Quantity</param>
		/// <returns>True if equal</returns>
		public override bool Equals(Object other)
		{

			if (other == null || GetType() != other.GetType())
			{
				return false;
			}

			return GetName().Equals(((Named)other).GetName());
		}

		/// <summary>
		/// Compute a hash code
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			return GetName().GetHashCode();
		}

		/// <summary>
		/// Get a string representation of a named object
		/// </summary>
		/// <returns>String value</returns>
		public override string ToString()
		{
			return GetName() + " (" + GetDescription() + ")";
		}
	}
}
