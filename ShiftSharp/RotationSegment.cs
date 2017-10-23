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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Point85.ShiftSharp.Schedule
{
	/// <summary>
	/// This class represents part of an entire rotation. The segment starts with a shift and includes 
	/// a count of the number of days on followed by the number of days off.
	/// </summary>
	public class RotationSegment : IComparable<RotationSegment>
	{
		// parent rotation
		private Rotation rotation;

		// strict ordering
		private int sequence = 0;

		// shift that starts this segment
		private Shift startingShift;

		// number of days on
		private int daysOn = 0;

		// number of days off
		private int daysOff = 0;

		/**
		 * Constructor
		 */
		public RotationSegment()
		{

		}

		internal RotationSegment(Shift startingShift, int daysOn, int daysOff, Rotation rotation)
		{
			this.startingShift = startingShift;
			this.daysOn = daysOn;
			this.daysOff = daysOff;
			this.rotation = rotation;
		}

		/**
		 * Get the starting shift
		 * 
		 * @return {@link Shift}
		 */
		public Shift GetStartingShift()
		{
			return startingShift;
		}

		/**
		 * Set the starting shift
		 * 
		 * @param startingShift
		 *            {@link Shift}
		 */
		public void SetStartingShift(Shift startingShift)
		{
			this.startingShift = startingShift;
		}

		/**
		 * Get the number of days on shift
		 * 
		 * @return Day count
		 */
		public int GetDaysOn()
		{
			return daysOn;
		}

		/**
		 * Set the number of days on shift
		 * 
		 * @param daysOn
		 *            Day count
		 */
		public void SetDaysOn(int daysOn)
		{
			this.daysOn = daysOn;
		}

		/**
		 * Get the number of days off shift
		 * 
		 * @return Day count
		 */
		public int GetDaysOff()
		{
			return daysOff;
		}

		/**
		 * Set the number of days off shift
		 * 
		 * @param daysOff
		 *            Day count
		 */
		public void SetDaysOff(int daysOff)
		{
			this.daysOff = daysOff;
		}

		/**
		 * Get the rotation for this segment
		 * 
		 * @return {@link Rotation}
		 */
		public Rotation GetRotation()
		{
			return rotation;
		}

		/**
		 * Get the sequence in the rotation
		 * 
		 * @return Sequence
		 */
		public int GetSequence()
		{
			return sequence;
		}

		/**
		 * Get the sequence in the rotation
		 * 
		 * @param sequence
		 *            Sequence
		 */
		public void SetSequence(int sequence)
		{
			this.sequence = sequence;
		}

		/**
		 * Compare this rotation segment to another one.
		 * 
		 * @param other
		 *            rotation segment
		 * @return -1 if less than, 0 if equal and 1 if greater than
		 */
	public int CompareTo(RotationSegment other)
		{
			int value = 0;
			if (this.GetSequence() < other.GetSequence())
			{
				value = -1;
			}
			else if (this.GetSequence() > other.GetSequence())
			{
				value = 1;
			}
			return value;
		}
	}
}
