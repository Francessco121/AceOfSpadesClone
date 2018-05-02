using System;
using Microsoft.CSharp.RuntimeBinder;
using System.Text;

/* NetBuffer.cs
 * Author: Ethan Lafrenais
 * Last Update: 4/28/15
*/

namespace Dash.Net
{
    /// <summary>
    /// A buffer of byte data.
    /// </summary>
    public partial class NetBuffer
    {
        /// <summary>
        /// Equal to two gigabytes.
        /// </summary>
        internal static readonly int MaxLength = (int)2e+9;

        #region Properties
        /// <summary>
        /// Gets the actual byte array of the buffer.
        /// </summary>
        public byte[] Data { get { return data; } }
        internal byte[] data;

        /// <summary>
        /// <para>Gets or sets the current read/write position of the buffer.</para>
        /// <para>Set value must be smaller than the buffers length.</para>
        /// </summary>
        public int Position
        {
            get { return position; }
            set
            {
                if (value <= data.Length)
                    if (!WriteOnly)
                        position = value;
                    else
                        throw NetException.WriteOnly;
                else
                    throw new NetException(String.Format("Could not move NetBuffer position to {0}, buffer is not that long!", value));
            }
        }
        internal int position = 0;

        /// <summary>
        /// Gets the length of the data as an Int32.
        /// </summary>
        public int Length { get { return unpaddedIndex; } }

        /// <summary>
        /// Gets the length of the data as an Int64.
        /// </summary>
        public long LongLength { get { return (long)unpaddedIndex; } }

        internal int unpaddedIndex = 0;

        public bool WriteOnly { get; internal set; }
        public bool ReadOnly { get; internal set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new empty NetBuffer.
        /// </summary>
        public NetBuffer()
        {
            data = new byte[0];
        }

        /// <summary>
        /// Creates a new empty NetBuffer with a starting size.
        /// </summary>
        /// <param name="size">The size of the buffer to start at.</param>
        public NetBuffer(int size)
        {
            data = new byte[size];
        }

        /// <summary>
        /// Creates a new NetBuffer that starts off with the provided data.
        /// </summary>
        /// <param name="startData">The data to start the NetBuffer off with.</param>
        public NetBuffer(byte[] startData)
        {
            data = startData;
            unpaddedIndex = startData.Length;
        }
        #endregion

        #region Clear
        /// <summary>
        /// Clears the entire buffer.
        /// </summary>
        public void Clear()
        {
            Clear(0);
        }

        /// <summary>
        /// Clears the entire buffer, with a new size.
        /// </summary>
        /// <param name="size">The size to start the buffer at.</param>
        public void Clear(int size)
        {
            data = new byte[size];
            position = 0;
            unpaddedIndex = 0;
        }
        #endregion

        /// <summary>
        /// Ensure position is at the end 
        /// of the packets data before calling!
        /// <para>Doesn't resize if not needed</para>
        /// </summary>
        internal void RemovePadding()
        {
            if (data.Length > unpaddedIndex)
                Array.Resize(ref data, position);
        }

        void PreWrite()
        {
            if (ReadOnly)
                throw NetException.ReadOnly;
        }

        void PreRead()
        {
            if (WriteOnly)
                throw NetException.WriteOnly;
        }

        void AfterWrite()
        {
            unpaddedIndex = Math.Max(position, unpaddedIndex);
        }


        // WRITING //
        #region Writing
        /// <summary>
        /// Writes a byte to the buffer.
        /// </summary>
        /// <param name="value">The byte.</param>
        public void Write(byte value)
        {
            PreWrite();
            NetBufferIO.MakeRoom(ref data, 1, position);
            data[position] = value;
            position++;
            AfterWrite();
        }

        /// <summary>
        /// Writes a signed byte to the buffer.
        /// </summary>
        /// <param name="value">The signed byte.</param>
        public void Write(sbyte value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes the specified bytes to the buffer
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public void WriteBytes(byte[] bytes)
        {
            PreWrite();
            NetBufferIO.WriteBytes(bytes, ref data, ref position, 0, bytes.Length);
            AfterWrite();
        }

        /// <summary>
        /// Writes the specified bytes to the buffer.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        /// <param name="offset">The offset to start from in the bytes to write.</param>
        /// <param name="readSize">The length to write from the bytes to the buffer.</param>
        public void WriteBytes(byte[] bytes, int offset, int readSize)
        {
            PreWrite();
            NetBufferIO.WriteBytes(bytes, ref data, ref position, offset, readSize);
            AfterWrite();
        }

        /// <summary>
        /// Writes a boolean to the buffer.
        /// </summary>
        /// <param name="value">The boolean.</param>
        public void Write(bool value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes an Int16 to the buffer.
        /// </summary>
        /// <param name="value">The Int16.</param>
        public void Write(short value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes an Int32 to the buffer.
        /// </summary>
        /// <param name="value">The Int32.</param>
        public void Write(int value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes an Int64 to the buffer.
        /// </summary>
        /// <param name="value">The Int64.</param>
        public void Write(long value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes a char to the buffer.
        /// </summary>
        /// <param name="value">The char.</param>
        public void Write(char value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes a single floating point to the buffer.
        /// </summary>
        /// <param name="value">The single.</param>
        public void Write(float value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes a double floating point to the buffer.
        /// </summary>
        /// <param name="value">The double.</param>
        public void Write(double value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes an unsigned Int16 to the buffer.
        /// </summary>
        /// <param name="value">The unsigned Int16.</param>
        public void Write(ushort value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes an unsigned Int32 to the buffer.
        /// </summary>
        /// <param name="value">The unsigned Int32.</param>
        public void Write(uint value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes an unsigned Int64 to the buffer.
        /// </summary>
        /// <param name="value">The unsigned Int64.</param>
        public void Write(ulong value)
        {
            PreWrite();
            NetBufferIO.WriteBytes(BitConverter.GetBytes(value), ref data, ref position, 0, -1);
            AfterWrite();
        }

        /// <summary>
        /// Writes a string to the buffer.
        /// </summary>
        /// <param name="value">The string.</param>
        public void Write(string value)
        {
            /*  String Formatting
             * Int16 length
             * bytes...
            */

            byte[] strBytes = Encoding.Unicode.GetBytes(value);
            Write((short)strBytes.Length); // Write length
            NetBufferIO.WriteBytes(strBytes, ref data, ref position, 0, -1); // Write string
            AfterWrite();
        }

        /// <summary>
        /// Writes a ByteFlag to the buffer (as 1 byte).
        /// </summary>
        /// <param name="byteFlag">The ByteFlag.</param>
        public void Write(ByteFlag byteFlag)
        {
            Write(byteFlag.internalByte);
        }

        /// <summary>
        /// <para>Writes an object to the buffer, infers it's actual datatype.</para>
        /// <para>WARNING: Must be passed one of the writable datatypes.</para>
        /// </summary>
        /// <param name="value">String object.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public void WriteDynamic(dynamic value)
        {
            try
            { Write(value); }
            catch (RuntimeBinderException)
            {
                throw new InvalidOperationException(
                  String.Format("Type \"{0}\" is not writable to NetBuffers.", value.GetType().Name));
            }
        }
        #endregion

        // READING //
        #region Reading

        /// <summary>
        /// Reads the next byte from the buffer.
        /// </summary>
        /// <returns>The next byte in the buffer.</returns>
        public byte ReadByte()
        {
            PreRead();
            NetException.Assert(position + 1 > data.Length, NetException.PastBuffer);
            return data[position++];
        }

        /// <summary>
        /// Reads the next specified number of bytes from the buffer.
        /// </summary>
        /// <param name="readSize">The number of bytes to read.</param>
        /// <returns>The array of read bytes.</returns>
        public byte[] ReadBytes(int readSize)
        {
            PreRead();
            byte[] bytes = NetBufferIO.ReadBytes(data, position, readSize);
            position += readSize;
            return bytes;
        }

        /// <summary>
        /// Reads the specified bytes from the buffer.
        /// </summary>
        /// <param name="offset">The starting point to read from. (inclusive)</param>
        /// <param name="readSize">The number of bytes to read.</param>
        /// <returns>The array of read bytes.</returns>
        public byte[] ReadBytes(int offset, int readSize)
        {
            PreRead();
            return NetBufferIO.ReadBytes(data, offset, readSize);
        }

        /// <summary>
        /// Reads the specified bytes into a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The starting point to read from. (inclusive)</param>
        /// <param name="readSize">The number of bytes to read.</param>
        public void ReadBytes(ref byte[] buffer, int offset, int readSize)
        {
            PreRead();
            NetBufferIO.ReadBytes(data, ref buffer, offset, readSize);
        }

        /// <summary>
        /// Reads the next boolean from the buffer.
        /// </summary>
        /// <returns>The next boolean.</returns>
        public bool ReadBool()
        {
            PreRead();
            return ReadByte() == 1;
        }

        /// <summary>
        /// Reads the next Int16 from the buffer.
        /// </summary>
        /// <returns>The next Int16.</returns>
        public short ReadInt16()
        {
            PreRead();
            NetException.Assert(position + 2 > data.Length, NetException.PastBuffer);
            short value = BitConverter.ToInt16(data, position);
            position += 2;
            return value;
        }

        /// <summary>
        /// Reads the next Int32 from the buffer.
        /// </summary>
        /// <returns>The next Int32.</returns>
        public int ReadInt32()
        {
            PreRead();
            NetException.Assert(position + 4 > data.Length, NetException.PastBuffer);
            int value = BitConverter.ToInt32(data, position);
            position += 4;
            return value;
        }

        /// <summary>
        /// Reads the next Int64 from the buffer.
        /// </summary>
        /// <returns>The next Int64.</returns>
        public long ReadInt64()
        {
            PreRead();
            NetException.Assert(position + 8 > data.Length, NetException.PastBuffer);
            long value = BitConverter.ToInt64(data, position);
            position += 8;
            return value;
        }

        /// <summary>
        /// Reads the next char from the buffer.
        /// </summary>
        /// <returns>The next char.</returns>
        public char ReadChar()
        {
            PreRead();
            NetException.Assert(position + 2 > data.Length, NetException.PastBuffer);
            char value = BitConverter.ToChar(data, position);
            position += 2;
            return value;
        }

        /// <summary>
        /// Reads the next single floating number from the buffer.
        /// </summary>
        /// <returns>The next single floating number.</returns>
        public float ReadFloat()
        {
            PreRead();
            NetException.Assert(position + 4 > data.Length, NetException.PastBuffer);
            float value = BitConverter.ToSingle(data, position);
            position += 4;
            return value;
        }

        /// <summary>
        /// Reads the next double floating number from the buffer.
        /// </summary>
        /// <returns>The next double floating number.</returns>
        public double ReadDouble()
        {
            PreRead();
            NetException.Assert(position + 8 > data.Length, NetException.PastBuffer);
            double value = BitConverter.ToDouble(data, position);
            position += 8;
            return value;
        }

        /// <summary>
        /// Reads the next unsigned Int16 from the buffer.
        /// </summary>
        /// <returns>The next unsigned Int16.</returns>
        public ushort ReadUInt16()
        {
            PreRead();
            NetException.Assert(position + 2 > data.Length, NetException.PastBuffer);
            ushort value = BitConverter.ToUInt16(data, position);
            position += 2;
            return value;
        }

        /// <summary>
        /// Reads the next unsigned Int32 from the buffer.
        /// </summary>
        /// <returns>The next unsigned Int32.</returns>
        public uint ReadUInt32()
        {
            PreRead();
            NetException.Assert(position + 4 > data.Length, NetException.PastBuffer);
            uint value = BitConverter.ToUInt32(data, position);
            position += 4;
            return value;
        }

        /// <summary>
        /// Reads the next unsigned Int64 from the buffer.
        /// </summary>
        /// <returns>The next unsigned Int64.</returns>
        public ulong ReadUInt64()
        {
            PreRead();
            NetException.Assert(position + 8 > data.Length, NetException.PastBuffer);
            ulong value = BitConverter.ToUInt64(data, position);
            position += 8;
            return value;
        }

        /// <summary>
        /// Reads the next string from the buffer.
        /// </summary>
        /// <returns>The next string.</returns>
        public string ReadString()
        {
            /*  String Formatting
             * Int16 length
             * bytes...
            */

            short stringLength = ReadInt16(); // This is done first to ensure position is advanced
            string str = Encoding.Unicode.GetString(NetBufferIO.ReadBytes(data, position, stringLength));
            position += stringLength;
            return str;
        }

        /// <summary>
        /// Reads the next ByteFlag from the buffer.
        /// </summary>
        /// <returns>The next ByteFlag.</returns>
        public ByteFlag ReadByteFlag()
        {
            PreRead();
            return new ByteFlag(ReadByte());
        }
        #endregion

        // PEEKING //
        #region Peeking

        /// <summary>
        /// Peeks the next byte in the buffer.
        /// </summary>
        /// <returns>The next byte.</returns>
        public byte PeekByte()
        {
            PreRead();
            NetException.Assert(position + 1 > data.Length, NetException.PastBuffer);
            return data[position];
        }

        /// <summary>
        /// Peeks the specified bytes in the buffer.
        /// </summary>
        /// <param name="readSize">The number of bytes to peek.</param>
        /// <returns>The peeked bytes.</returns>
        public byte[] PeekBytes(int readSize)
        {
            PreRead();
            byte[] bytes = NetBufferIO.ReadBytes(data, position, readSize);
            return bytes;
        }

        /// <summary>
        /// Peeks the specified bytes into a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read the peeked bytes into.</param>
        /// <param name="offset">The starting point to peek from.</param>
        /// <param name="readSize">The number of bytes to peek.</param>
        public void PeekBytes(ref byte[] buffer, int offset, int readSize)
        {
            PreRead();
            NetBufferIO.ReadBytes(data, ref buffer, offset, readSize);
        }

        /// <summary>
        /// Peeks the next boolean.
        /// </summary>
        /// <returns>The next boolean.</returns>
        public bool PeekBool()
        {
            return PeekByte() == 1;
        }

        /// <summary>
        /// Peeks the next Int16.
        /// </summary>
        /// <returns>The next Int16.</returns>
        public short PeekInt16()
        {
            PreRead();
            NetException.Assert(position + 2 > data.Length, NetException.PastBuffer);
            short value = BitConverter.ToInt16(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next Int32.
        /// </summary>
        /// <returns>The next Int32.</returns>
        public int PeekInt32()
        {
            PreRead();
            NetException.Assert(position + 4 > data.Length, NetException.PastBuffer);
            int value = BitConverter.ToInt32(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next Int64.
        /// </summary>
        /// <returns>The next Int64.</returns>
        public long PeekInt64()
        {
            PreRead();
            NetException.Assert(position + 8 > data.Length, NetException.PastBuffer);
            long value = BitConverter.ToInt64(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next char.
        /// </summary>
        /// <returns>The next char.</returns>
        public char PeekChar()
        {
            PreRead();
            NetException.Assert(position + 2 > data.Length, NetException.PastBuffer);
            char value = BitConverter.ToChar(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next single floating number.
        /// </summary>
        /// <returns>The next single floating.</returns>
        public float PeekFloat()
        {
            PreRead();
            NetException.Assert(position + 4 > data.Length, NetException.PastBuffer);
            float value = BitConverter.ToSingle(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next double floating number.
        /// </summary>
        /// <returns>The next double floating number.</returns>
        public double PeekDouble()
        {
            PreRead();
            NetException.Assert(position + 8 > data.Length, NetException.PastBuffer);
            double value = BitConverter.ToDouble(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next unsigned Int16.
        /// </summary>
        /// <returns>The next unsigned Int16.</returns>
        public ushort PeekUInt16()
        {
            PreRead();
            NetException.Assert(position + 2 > data.Length, NetException.PastBuffer);
            ushort value = BitConverter.ToUInt16(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next unsigned Int32.
        /// </summary>
        /// <returns>The next unsigned Int32.</returns>
        public uint PeekUInt32()
        {
            PreRead();
            NetException.Assert(position + 4 > data.Length, NetException.PastBuffer);
            uint value = BitConverter.ToUInt32(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next unsigned Int64.
        /// </summary>
        /// <returns>The next unsigned Int64.</returns>
        public ulong PeekUInt64()
        {
            PreRead();
            NetException.Assert(position + 8 > data.Length, NetException.PastBuffer);
            ulong value = BitConverter.ToUInt64(data, position);
            return value;
        }

        /// <summary>
        /// Peeks the next string.
        /// </summary>
        /// <returns>The next string.</returns>
        public string PeekString()
        {
            /*  String Formatting
             * Int16 length
             * bytes...
            */

            short stringLength = PeekInt16();
            return Encoding.Unicode.GetString(NetBufferIO.ReadBytes(data, position + 2, stringLength));
        }

        /// <summary>
        /// Peeks the next ByteFlag.
        /// </summary>
        /// <returns>The next ByteFlag.</returns>
        public ByteFlag PeekByteFlag()
        {
            PreRead();
            return new ByteFlag(PeekByte());
        }
        #endregion
    }
}
