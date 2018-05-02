using System;

/* NetOutboundPacket.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public class NetOutboundPacket : NetPacket
    {
        internal const byte PacketHeaderSize = 6;

        public NetDeliveryMethod DeliveryMethod;
        public NetPacketCompression Compression;
        public bool Encrypt;
        public bool SendImmediately;

        internal bool HasId { get; private set; }

        bool headerPrepended;

        public NetOutboundPacket(NetDeliveryMethod deliveryMethod)
        {
            DeliveryMethod = deliveryMethod;
        }
        public NetOutboundPacket(NetDeliveryMethod deliveryMethod, byte[] data)
            : base(data)
        {
            DeliveryMethod = deliveryMethod;
        }
        public NetOutboundPacket(NetDeliveryMethod deliveryMethod, int size)
            : base(size)
        {
            DeliveryMethod = deliveryMethod;
        }
        internal NetOutboundPacket(NetDeliveryMethod deliveryMethod, NetPacketType type)
        {
            DeliveryMethod = deliveryMethod;
            Type = type;
        }

        internal void SetId(ushort id)
        {
            if (HasId)
                throw new NetException(string.Format("Cannot set packet id, it already has an id! Type: {0}", Type));

            Id = id;
            HasId = true;
        }

        /// <summary>
        /// 6 Bytes
        /// </summary>
        internal void PrependHeader()
        {
            if (!HasId)
                throw new NetException(string.Format("Cannot set packet header, it does not have an Id! Type: {0}", Type));
            if (headerPrepended)
                throw new NetException(string.Format("Header already prepended for this packet! Id: {0}, Type: {1}", Id, Type));

            headerPrepended = true;

            byte[] packetData = data;
            data = new byte[6 + packetData.Length];
            position = 0;

            Write(PACKET_PREFIX);           // 2
            Write((byte)Type);              // 1
            Write(Id);                      // 2
            ByteFlag flag = new ByteFlag();
            flag.Set(0, DeliveryMethod != NetDeliveryMethod.Unreliable);
            flag.Set(1, isChunked);
            flag.Set(2, isCompressed);
            flag.Set(3, isEncrypted);
            flag.Set(4, isPartial);
            Write(flag);                    // 1

            // Put the original data back
            WriteBytes(packetData);

            ReadOnly = true;
        }

        internal override string ToInternalString()
        {
            return NetLogger.MinimalPacketHeaderLogs
                //? string.Format("Id: {0}, Type: {1}", Id, Type)
                ? string.Format("[{0}:{1}:{2}]", Id, Type, DeliveryMethod)
                : string.Format(
                "Id: {0}, DeliveryMethod: {1}, Type: {2}," +
                " Chunked: {3}, Compressed: {4}, Encrypted: {5}, Partial: {6}",
                Id, DeliveryMethod, Type, isChunked, isCompressed, isEncrypted, isPartial);
        }

        /// <summary>
        /// Returns an exact copy of this packet.
        /// </summary>
        public NetOutboundPacket Clone(bool onlyCopyProperties = false)
        {
            if (!onlyCopyProperties)
            {
                // Save the last position
                int lastPos = position;

                // Clone the data
                byte[] clonedData;
                if (headerPrepended)
                {
                    throw new InvalidOperationException("This packet has already been processed, to send multiple times make a copy BEFORE the first send.");

                    // If the header was already added, we need to skip it.
                    //position = PacketHeaderSize;
                    //clonedData = ReadBytes(data.Length - PacketHeaderSize);
                }
                else
                {
                    // If the header wasn't added, just clone all the data.
                    position = 0;
                    clonedData = ReadBytes(data.Length); // Make we have a new pointer
                }

                // Reset the position so we don't screw up this packet
                position = lastPos;

                // And create the cloned packet.
                return new NetOutboundPacket(DeliveryMethod, clonedData)
                {
                    Encrypt = Encrypt,
                    Compression = Compression,
                    SendImmediately = SendImmediately,
                    isChunked = isChunked,
                    isCompressed = isCompressed,
                    isEncrypted = isEncrypted,
                    isPartial = isPartial,
                    unpaddedIndex = unpaddedIndex,
                    position = position,
                    Type = Type
                };
            }
            else
            {
                return new NetOutboundPacket(DeliveryMethod)
                {
                    Encrypt = Encrypt,
                    Compression = Compression,
                    SendImmediately = SendImmediately,
                    unpaddedIndex = unpaddedIndex,
                    position = position,
                    Type = Type
                };
            }
        }
    }
}
