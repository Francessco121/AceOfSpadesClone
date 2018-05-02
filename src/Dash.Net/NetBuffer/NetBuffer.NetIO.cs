using System.Net;

/* NetBuffer.NetIO.cs
 * Author: Ethan Lafrenais
 * Last Update: 4/28/15
*/

namespace Dash.Net
{
    public partial class NetBuffer
    {
        #region Writing
        /// <summary>
        /// Writes an IPAddress to the buffer.
        /// </summary>
        /// <param name="ip">The IPAddress to write.</param>
        public void Write(IPAddress ip)
        {
            /* Format
             * ushort ipLength
             * bytes... ip
            */

            byte[] ipAsBytes = ip.GetAddressBytes();
            ushort ipLen = (ushort)ipAsBytes.Length;

            Write(ipLen);
            WriteBytes(ipAsBytes);
        }

        /// <summary>
        /// Writes an IPEndPoint to the buffer.
        /// </summary>
        /// <param name="endPoint">The IPEndPoint to write.</param>
        public void Write(IPEndPoint endPoint)
        {
            /* Format
             * byte ipLength
             * bytes... ip
             * ushort port
            */

            Write(endPoint.Address);
            Write((ushort)endPoint.Port);
        }
        #endregion

        #region Reading
        /// <summary>
        /// Reads an IPAddress from the buffer.
        /// </summary>
        /// <returns>The read IPAddress.</returns>
        public IPAddress ReadIPAddress()
        {
            ushort ipLen = ReadUInt16();
            byte[] ipAsBytes = ReadBytes(ipLen);

            return new IPAddress(ipAsBytes);
        }

        /// <summary>
        /// Reads an IPEndPoint from the buffer.
        /// </summary>
        /// <returns>The read IPEndPoint.</returns>
        public IPEndPoint ReadIPEndPoint()
        {
            IPAddress address = ReadIPAddress();
            int port = (int)ReadUInt16();

            return new IPEndPoint(address, port);
        }
        #endregion

        #region Peeking
        /// <summary>
        /// Peeks an IPAddress in the buffer.
        /// </summary>
        /// <returns>The peeked IPAddress.</returns>
        public IPAddress PeekIPAddress()
        {
            int startPos = Position;
            ushort ipLen = ReadUInt16();
            byte[] ipAsBytes = ReadBytes(ipLen, Position + 2);
            Position = startPos;

            return new IPAddress(ipAsBytes);
        }

        /// <summary>
        /// Peeks an IPEndPoint in the buffer.
        /// </summary>
        /// <returns>The peeked IPEndPoint.</returns>
        public IPEndPoint PeekIPEndPoint()
        {
            int startPos = Position;
            IPEndPoint endPoint = ReadIPEndPoint();
            Position = startPos;
            return endPoint;
        }
        #endregion
    }
}
