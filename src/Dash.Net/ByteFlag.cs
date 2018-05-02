using System;
using System.Runtime.InteropServices;

/* ByteFlag.cs
 * Author: Ethan Lafrenais
 * Last Update: 4/28/15
*/

namespace Dash.Net
{
    /// <summary>
    /// Represents a byte that holds 8 boolean values.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ByteFlag
    {
        /// <summary>
        /// The actual byte data.
        /// </summary>
        [FieldOffset(0)]
        public byte internalByte;

        /// <summary>
        /// Creates a new byte flag.
        /// </summary>
        /// <param name="b">The byte to start off with.</param>
        public ByteFlag(byte b)
        {
            this.internalByte = b;
        }

        /// <summary>
        /// Sets a flag in the byte.
        /// </summary>
        /// <param name="pos">Position of the flag (0-7)</param>
        /// <param name="value">The value to set the flag to.</param>
        public void Set(int pos, bool value)
        {
            if (pos < 0)
                throw new InvalidOperationException("Cannot set bit position less than 0.");
            if (pos >= 8)
                throw new InvalidOperationException("Cannot set bit position greater than 7.");

            Set(ref internalByte, pos, value);
        }

        /// <summary>
        /// Returns the value of a flag at a position.
        /// </summary>
        /// <param name="pos">Position of the flag (0-7)</param>
        /// <returns>Flag value at specified position.</returns>
        public bool Get(int pos)
        {
            if (pos < 0)
                throw new InvalidOperationException("Cannot set bit position less than 0.");
            if (pos >= 8)
                throw new InvalidOperationException("Cannot set bit position greater than 7.");

            return Get(internalByte, pos);
        }

        public override int GetHashCode()
        {
            return internalByte;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == GetType())
            {
                ByteFlag other = (ByteFlag)obj;
                return internalByte == other.internalByte;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns all of the flags as a formatted string.
        /// </summary>
        /// <returns>Formatted string of all flags</returns>
        public override string ToString()
        {
            return String.Format("[ByteFlag] {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}",
                Get(0), Get(1), Get(2), Get(3), Get(4), Get(5), Get(6), Get(7));
        }

        /// <summary>
        /// Sets a bit in a byte.
        /// </summary>
        /// <param name="b">The byte to set.</param>
        /// <param name="pos">The position of the bit.</param>
        /// <param name="value">The value to set the bit to.</param>
        public static void Set(ref byte b, int pos, bool value)
        {
            if (value)
            {
                //left-shift 1, then bitwise OR
                b = (byte)(b | (1 << pos));
            }
            else
            {
                //left-shift 1, then take complement, then bitwise AND
                b = (byte)(b & ~(1 << pos));
            }
        }

        /// <summary>
        /// Gets the value of a bit in a byte.
        /// </summary>
        /// <param name="b">The byte to look in.</param>
        /// <param name="pos">The position of the bit.</param>
        /// <returns>The value of a bit at the specified position.</returns>
        public static bool Get(byte b, int pos)
        {
            //left-shift 1, then bitwise AND, then check for non-zero
            return ((b & (1 << pos)) != 0);
        }
    }
}
