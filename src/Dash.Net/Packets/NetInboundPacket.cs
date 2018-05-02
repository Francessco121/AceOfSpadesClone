using System;
using System.Net;

/* NetInboundPacket.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public class NetInboundPacket : NetInboundPacketBase
    {
        public NetConnection Sender { get; private set; }

        internal NetInboundPacket(NetConnection sender, byte[] data)
            : base(data)
        {
            Sender = sender;
        }

        internal NetInboundPacket(NetInboundPacket original, byte[] data)
            : base(data)
        {
            Sender = original.Sender;
            Id = original.Id;
            Type = original.Type;
            isReliable = original.isReliable;
            isEncrypted = original.isEncrypted;
            isCompressed = original.isCompressed;
        }

        internal override string ToInternalString()
        {
            return NetLogger.MinimalPacketHeaderLogs
                //? string.Format("Sender: {0}, Id: {1}, Type: {2}", Sender, Id, Type)
                ? string.Format("[{0}:{1}]", Id, Type)
                : string.Format(
                "Sender: {0}, Id: {1}, IsReliable: {2}, Type: {3}," +
                " Chunked: {4}, Compressed: {5}, Encrypted: {6}, Partial: {7}",
                Sender, Id, isReliable, Type, isChunked, isCompressed, isEncrypted, isPartial);
        }
    }
}
