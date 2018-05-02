/* NetPacket.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public abstract class NetPacket : NetBuffer
    {
        internal const char PACKET_PREFIX = 'D';

        public ushort Id { get; protected set; }
        internal NetPacketType Type;
        internal bool isChunked;
        internal bool isEncrypted;
        internal bool isCompressed;
        internal bool isPartial;
        internal bool isReliable;

        public NetPacket() { }

        public NetPacket(int size) 
            : base(size)
        { }

        public NetPacket(byte[] startData) 
            : base(startData)
        { }

        internal abstract string ToInternalString();
    }
}
