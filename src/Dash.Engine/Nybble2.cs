using System;
using System.Runtime.InteropServices;

/* Nybble2.cs
 * Ethan Lafrenais
 * Represents two nybbles stored in a single byte.
*/

namespace Dash.Engine
{
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public struct Nybble2
    {
        [FieldOffset(0)]
        public byte Value;

        public byte Lower
        {
            get { return GetLower(); }
            set { SetLower(value); }
        }

        public byte Upper
        {
            get { return GetUpper(); }
            set { SetUpper(value); }
        }

        public Nybble2(byte data)
        {
            Value = data;
        }

        public Nybble2(byte lower, byte upper)
        {
            Value = 0;

            SetLower(lower);
            SetUpper(upper);
        }

        public byte this[int index]
        {
            get
            {
                if (index == 0) return GetLower();
                else if (index == 1) return GetUpper();
                else throw new IndexOutOfRangeException();
            }
            set
            {
                if (index == 0) SetLower(value);
                else if (index == 1) SetUpper(value);
                else throw new IndexOutOfRangeException();
            }
        }

        public byte GetLower()
        {
            return (byte)(Value & 15);
        }

        public byte GetUpper()
        {
            return (byte)(Value >> 4);
        }

        public void SetLower(byte data)
        {
            Value = (byte)((Value & 240) | (data & 15));
        }

        public void SetUpper(byte data)
        {
            Value = (byte)((data << 4) | (Value & 15));
        }
    }
}
