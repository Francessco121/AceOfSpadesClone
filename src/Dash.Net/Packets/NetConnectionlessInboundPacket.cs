using System.Net;

/* NetConnectionlessInboundPacket.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    class NetConnectionlessInboundPacket : NetInboundPacketBase
    {
        public IPEndPoint SenderIP { get; private set; }

        internal NetConnectionlessInboundPacket(IPEndPoint sender, byte[] data)
            : base(data)
        {
            SenderIP = sender;
        }

        internal override string ToInternalString()
        {
            return NetLogger.MinimalPacketHeaderLogs
                ? string.Format("[{1}:{2}]", SenderIP, Id, Type)
                : string.Format(
                "Sender: {0}, Id: {1}, IsReliable: {2}, Type: {3}," +
                " Chunked: {4}, Compressed: {5}, Encrypted: {6}, Partial: {7}",
                SenderIP, Id, isReliable, Type, isChunked, isCompressed, isEncrypted, isPartial);
        }
    }
}
