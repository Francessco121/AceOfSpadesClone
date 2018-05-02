using System;

/* NetBufferIO.cs
 * Author: Ethan Lafrenais
 * Last Update: 4/28/15
*/

namespace Dash.Net
{
    /// <summary>
    /// Provides methods for reading and writing bytes to a NetBuffer.
    /// </summary>
    public static class NetBufferIO
    {
        internal static readonly int bufferAddSize = 30;

        /// <summary>
        /// Resizes the byte array.
        /// </summary>
        /// <param name="data">The byte array to resize.</param>
        /// <param name="newSize">The new size of the byte array.</param>
        public static void ResizeData(ref byte[] data, int newSize)
        {
            if (newSize < NetBuffer.MaxLength)
                Array.Resize<byte>(ref data, newSize);
            else
                // This really should never happen
                NetLogger.LogWarning("Could not resize buffer, buffer is too long!");
        }

        /// <summary>
        /// Adds the specified size to the existing byte array length.
        /// </summary>
        /// <param name="data">The byte array to change.</param>
        /// <param name="addSize">The size to add to it's length.</param>
        /// <param name="position">The current position in the byte array.</param>
        public static void MakeRoom(ref byte[] data, int addSize, int position)
        {
            if (position + addSize > data.Length)
                ResizeData(ref data, data.Length + ((addSize > bufferAddSize) ? addSize : bufferAddSize));
        }


        /// <summary>
        /// Writes the specified bytes to the other byte array.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        /// <param name="to">The byte array to write to.</param>
        /// <param name="position">The current position in the array (used for resizing)</param>
        /// <param name="offset">The offset from the current position of the array.</param>
        /// <param name="readSize">How many bytes to read from the bytes array. 
        /// Specify -1 to make equal to the bytes array's length</param>
        /// <exception cref="Dash.Net.NetException"></exception>
        public static void WriteBytes(byte[] bytes, ref byte[] to, ref int position, int offset, int readSize)
        {
            if (readSize == -1) readSize = bytes.Length; // Special case to save memory

            int start = position;
            MakeRoom(ref to, readSize, position);
            position += readSize;

            NetException.Assert(readSize > to.Length - offset || offset > to.Length, NetException.PastBuffer);

            if (readSize > bytes.Length) readSize = bytes.Length;
            int forEnd = Math.Min(to.Length - start, bytes.Length);
            for (int i = offset; i < forEnd; i++)
                to[start + i] = bytes[i];
        }

        /// <summary>
        /// <para>Reads the specified bytes from the byte array.</para>
        /// <para>offset is Inclusive!</para>
        /// </summary>
        /// <param name="data">The byte array to read from.</param>
        /// <param name="offset">The position to start reading from in the array.</param>
        /// <param name="readSize">The length to read.</param>
        /// <exception cref="Dash.Net.NetException"></exception>
        /// <returns>The read bytes.</returns>
        public static byte[] ReadBytes(byte[] data, int offset, int readSize)
        {
            NetException.Assert(offset + readSize > data.Length, NetException.PastBuffer);

            byte[] readBytes = new byte[readSize];
            for (int i = 0; i < readSize; i++)
                readBytes[i] = data[offset + i];

            return readBytes;
        }

        /// <summary>
        /// <para>Reads the specified bytes from the byte array into another byte array.</para>
        /// <para>offset is Inclusive!</para>
        /// </summary>
        /// <param name="data">The byte array to read from.</param>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The position to start reading from in the array.</param>
        /// <param name="readSize">The length to read.</param>
        /// <exception cref="Dash.Net.NetException"></exception>
        public static void ReadBytes(byte[] data, ref byte[] buffer, int offset, int readSize)
        {
            NetException.Assert(offset + readSize > data.Length
                                || readSize > buffer.Length, NetException.PastBuffer);

            for (int i = 0; i < readSize; i++)
                buffer[i] = data[offset + i];
        }
    }
}
