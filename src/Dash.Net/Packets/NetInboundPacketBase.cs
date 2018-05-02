/* NetInboundPacketBase.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public abstract class NetInboundPacketBase : NetPacket
    {
        internal bool HasHeader = true;

        protected NetInboundPacketBase(byte[] data)
            : base(data)
        { }

        internal bool ReadHeader()
        {
            Position = 0;
            char prefix = ReadChar();

            if (prefix != PACKET_PREFIX)
                return false;
            else
            {
                Type = (NetPacketType)ReadByte();
                Id = ReadUInt16();

                ByteFlag flags = ReadByteFlag();
                isReliable = flags.Get(0);
                isChunked = flags.Get(1);
                isCompressed = flags.Get(2);
                isEncrypted = flags.Get(3);
                isPartial = flags.Get(4);

                return true;
            }
        }

        internal byte[][] GetChunked()
        {
            if (!isChunked)
                throw new NetException("Attempt to split non-chunked packet into chunks!");

            // Get the number of packets
            ushort numPackets = ReadUInt16();
            byte[][] packets = new byte[numPackets][];

            // Read and handle each packet
            for (int i = 0; i < numPackets; i++)
            {
                // Get the size of the packet
                ushort len = ReadUInt16();

                // Read the packet
                packets[i] = ReadBytes(len);
            }

            return packets;
        }
    }
}
