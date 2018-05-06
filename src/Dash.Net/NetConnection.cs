using Dash.Net.Helpers;
using System;
using System.Collections.Concurrent;
using System.Net;

/* NetConnection.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public delegate void ConnectionChangedHandler(NetConnection connection);
    public delegate void DisconnectedHandler(NetConnection connection, string reason, bool lostConnection);

    public partial class NetConnection
    {
        class ReliablePacket
        {
            public NetOutboundPacket Packet;
            public int SendAttempts;
            public int LastSendAttempt;

            public ReliablePacket(NetOutboundPacket packet)
            {
                Packet = packet;
                LastSendAttempt = packet.DeliveryMethod == NetDeliveryMethod.ReliableOrdered ? 0 : NetTime.Now;
            }
        }

        public IPEndPoint EndPoint { get; private set; }
        public readonly NetConnectionStats Stats;
        public int PacketSendRate { get; private set; }

        static ushort nextPartialId;

        internal ConcurrentDictionary<ushort, NetMessenger.PartialPacket> Partials;

        
        ConcurrentUniqueList<ushort> handledPacketIds;

        ConcurrentDictionary<ushort, ReliablePacket> reliableOutboundPacketQueue;
        ConcurrentQueue<ReliablePacket> reliableOrderedOutboundPacketQueue;

        ConcurrentHashSet<NetOutboundPacket> packetChunkQueue;
        NetMessenger messenger;
        NetMessengerConfig config;

        int chunkSendDelay = 12; // ~ 80 packet a second
        int maxChunkSize = 512;
        int lastChunkSend;
        int currentChunkSize;

        int physicalPacketsSentInLastSecond;
        int packetsSentInLastSecond;

        int packetPerSecondLastReset;

        int physicalPacketsReceivedInLastSecond;
        int packetsReceivedInLastSecond;

        int packetsRecPerSecondLastReset;

        int lastPingRequest;
        int lastPingResponse;

        bool ignorePing = false;

        public NetConnection(IPEndPoint endPoint, NetMessenger messenger)
        {
            this.messenger = messenger;
            EndPoint = endPoint;

            Stats = new NetConnectionStats();

            handledPacketIds = new ConcurrentUniqueList<ushort>();
            reliableOutboundPacketQueue = new ConcurrentDictionary<ushort, ReliablePacket>();
            reliableOrderedOutboundPacketQueue = new ConcurrentQueue<ReliablePacket>();
            packetChunkQueue = new ConcurrentHashSet<NetOutboundPacket>();
            Partials = new ConcurrentDictionary<ushort, NetMessenger.PartialPacket>();

            config = messenger.GetBaseConfig();

            // Ensure we don't immediatly time out or send a ping request
            lastPingRequest = NetTime.Now + 1000;
            lastPingResponse = NetTime.Now;

            PacketSendRate = 79;
        }

        /// <summary>
        /// Disconnects from a connection.
        /// </summary>
        /// <param name="reason">The reason for disconnecting.</param>
        public void Disconnect(string reason = "No reason given")
        {
            Disconnect(reason, false);
        }

        internal void Disconnect(string reason, bool connectionLost)
        {
            // Create the disconnect packet to let the other side know
            NetOutboundPacket disconnection = new NetOutboundPacket(NetDeliveryMethod.Unreliable);
            disconnection.Type = NetPacketType.Disconnected;
            disconnection.SendImmediately = true;
            disconnection.Write(reason);
            disconnection.Write(connectionLost);

            // Send it
            SendPacket(disconnection);

            // Remove this connection from our messenger
            messenger.DropConnection(EndPoint, reason, connectionLost);
        }

        /// <summary>
        /// Sends a packet to the connection.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        public void SendPacket(NetOutboundPacket packet)
        {
            if (packet.HasId)
                throw new NetException(
                    "Cannot send packet twice, please create a new packet or clone this one before sending another time.");

            // Remove the packets padding
            packet.RemovePadding();

            // Setup the packet and make it ready for sending
            SetupOutboundPacket(packet);

            // Write the packet's header and set it's Id
            WriteOutboundPacketHeader(packet);

            // Check if this packet needs to be split into a partial
            if (MTU > 0 && packet.Length > MTU && !packet.isPartial)
            {
                // Split the packet into partials
                NetOutboundPacket[] partials = SplitPacketIntoPartials(packet);
                for (int i = 0; i < partials.Length; i++)
                    // Send the partials as seperate packets
                    SendPacket(partials[i]);

                // Stop handling the too large packet
                return;
            }

            // Handle the sending based on delivery method
            if (packet.DeliveryMethod == NetDeliveryMethod.Unreliable)
                // No extra work needed, just place in outbound queue
                AddPacketToChunkQueue(packet);
            else if (packet.DeliveryMethod == NetDeliveryMethod.Reliable)
            {
                // Create the reliable packet handler and add it to the reliable queue
                ReliablePacket reliablePacket = new ReliablePacket(packet);
                if (!reliableOutboundPacketQueue.TryAdd(packet.Id, reliablePacket))
                    NetLogger.LogError("Failed to add packet to the reliable outbound queue. Id: {0}, Type: {1}, DeliveryMethod: {2}",
                        packet.Id, packet.Type, packet.DeliveryMethod);

                // Attempt first send
                AddPacketToChunkQueue(packet);
            }
            else if (packet.DeliveryMethod == NetDeliveryMethod.ReliableOrdered)
            {
                // Create the reliable packet handler and add it to the ordered reliable queue
                ReliablePacket reliablePacket = new ReliablePacket(packet);
                reliableOrderedOutboundPacketQueue.Enqueue(reliablePacket);
            }
        }

        NetOutboundPacket[] SplitPacketIntoPartials(NetOutboundPacket packet)
        {
            packet.position = 0;

            // Calculate the number of packets to split it into,
            // and each of their sizes
            int numPackets = (int)Math.Ceiling((double)(packet.Length) / (MTU - 12));
            int newPacketSize = MTU - 12;
            int packetChunkIndex = 0;
            ushort id = nextPartialId++;

            NetOutboundPacket[] partialPackets = new NetOutboundPacket[numPackets];

            for (byte i = 0; i < numPackets; i++)
            {
                int packetSize = Math.Min(newPacketSize, packet.Length - packetChunkIndex);

                // Create the new partial
                NetOutboundPacket partial = packet.Clone(true);
                partial.isPartial = true;
                partial.Encrypt = false;
                partial.Compression = NetPacketCompression.None;
                partial.Clear(6 + packetSize);

                // Write the partial header
                partial.Write(id);
                partial.Write(i);
                partial.Write((byte)numPackets);
                partial.Write((ushort)packetSize);

                // Write the data allocated to this partial
                partial.WriteBytes(packet.ReadBytes(packetChunkIndex, packetSize));

                // Add it to the list of partials
                partialPackets[i] = partial;
                packetChunkIndex += packetSize;
            }

            if (NetLogger.LogPartials)
                NetLogger.LogVerbose("[Partial] Split Packet into {0} parts; Size: {1}b, Original Size: {2}b",
                    numPackets, newPacketSize, packet.Length);
            return partialPackets;
        }

        void SetupOutboundPacket(NetOutboundPacket packet)
        {
            // Compress Packet
            if (packet.Compression != NetPacketCompression.None && !packet.isCompressed 
                && ((packet.Compression == NetPacketCompression.Compress && packet.Length > 40)
                || (config.AutoCompressPackets && packet.Length > config.CompressPacketAfter)))
                NetCompressor.Compress(packet);

            // Encrypt Packet
            if (packet.Encrypt)
                NetEncryption.EncryptPacket(packet);
        }

        void WriteOutboundPacketHeader(NetOutboundPacket packet)
        {
            // Allocate the packet's Id and prepend its header
            packet.SetId(messenger.AllocatePacketId());
            packet.PrependHeader();
        }

        void TryMarkReliablePacket(NetOutboundPacket packet)
        {
            // If this packet is reliable, mark is as a new send
            if (packet.DeliveryMethod == NetDeliveryMethod.Reliable)
            {
                ReliablePacket reliablePacket;
                if (reliableOutboundPacketQueue.TryGetValue(packet.Id, out reliablePacket))
                {
                    reliablePacket.LastSendAttempt = NetTime.Now;
                    reliablePacket.SendAttempts++;
                }
                else
                    // Could be from a connection issue
                    NetLogger.LogError("Reliable packet was attempted to be sent, but it doesnt exist in the queue! Id: {0}",
                        packet.Id);
            }
            else if (packet.DeliveryMethod == NetDeliveryMethod.ReliableOrdered)
            {
                ReliablePacket reliablePacket;
                // Try and get the current ordered packet
                if (reliableOrderedOutboundPacketQueue.TryPeek(out reliablePacket))
                {
                    // Only set if it really is the first
                    if (packet.Id == reliablePacket.Packet.Id)
                    {
                        reliablePacket.LastSendAttempt = NetTime.Now;
                        reliablePacket.SendAttempts++;
                    }
                    else
                        // Bad news if it is not the current
                        NetLogger.LogError("ReliableOrdered packet was attempted to be sent, without being first in queue! Id: {0}",
                            packet.Id);
                }
            }
        }

        internal void PartialPacketReceived(int partialSections)
        {
            packetsReceivedInLastSecond -= (partialSections - 1);
            Stats.PacketsReceived -= (partialSections - 1);
        }

        internal void PhysicalPacketReceived()
        {
            physicalPacketsReceivedInLastSecond++;
            Stats.PhysicalPacketsReceived++;

            PacketReceived();
        }

        internal void PacketReceived()
        {
            Stats.PacketsReceived++;
            packetsReceivedInLastSecond++;
        }

        bool CanLogSend(NetPacketType type)
        {
            return (type != NetPacketType.AckResponse || NetLogger.LogAcks)
                && type != NetPacketType.MTUTest;
        }

        bool IsPingPacket(NetPacketType type)
        {
            return type == NetPacketType.PingResponse
                || type == NetPacketType.PingRequest;
        }

        void AddPacketToChunkQueue(NetOutboundPacket packet)
        {
            // If it is reliable we need to mark it as a new send
            TryMarkReliablePacket(packet);

            // Log send
            if (NetLogger.LogPacketSends && CanLogSend(packet.Type) && !IsPingPacket(packet.Type))
                NetLogger.LogVerbose("[Outbound:{0}] {1}", EndPoint, packet.ToInternalString());

            // Send packet or add it to queue
            if (packet.SendImmediately)
            {
                // Just send it if needed
                SendPacketToSocket(packet); // Send
            }
            else
            {
                // Try to add the packet to the chunked queue
                if (!packetChunkQueue.Add(packet))
                    NetLogger.LogError(
                        "Failed to add packet to the chunk queue. Id: {0}, Type: {1}, DeliveryMethod: {2}, Duplicate Id Added: {3}",
                        packet.Id, packet.Type, packet.DeliveryMethod, packetChunkQueue.Contains(packet));
                else
                {
                    // Increment the chunks current size
                    currentChunkSize += packet.Length;

                    // If the chunk is too big, send it before any other chunks are added
                    if (currentChunkSize >= maxChunkSize)
                        SendChunk();
                }
            }
        }

        void SendChunk()
        {
            lastChunkSend = NetTime.Now;

            // If theres nothing to send, don't bother!
            if (packetChunkQueue.Count == 0)
                return;

            // Safely grab the packets
            NetOutboundPacket[] packetsToChunk = packetChunkQueue.ToArrayAndClear();
            currentChunkSize = 0; // Reset size
            if (packetsToChunk == null)
            {
                NetLogger.LogError("Failed to convert packetChunkQueue to an array!");
                // Cannot continue
                return;
            }

            // If there is only one packet to send,
            // then don't create a chunk, just send it to
            // save cpu and bandwidth.
            if (packetsToChunk.Length == 1)
            {
                TryMarkReliablePacket(packetsToChunk[0]);
                SendPacketToSocket(packetsToChunk[0]);
            }
            else // Otherwise send it as a chunk
            {
                // Log the sends
                packetsSentInLastSecond += packetsToChunk.Length;
                Stats.PacketsSent += packetsToChunk.Length;

                // Create the packet
                NetOutboundPacket chunkPacket = new NetOutboundPacket(NetDeliveryMethod.Unreliable,
                    2 + (packetsToChunk.Length * 2) + currentChunkSize);
                chunkPacket.isChunked = true;

                // Write the header, the number of chunked packets
                chunkPacket.Write((ushort)packetsToChunk.Length);

                // Write each packet
                for (int i = 0; i < packetsToChunk.Length; i++)
                {
                    NetOutboundPacket packet = packetsToChunk[i];
                    // If it is reliable we need to mark it as a new send
                    TryMarkReliablePacket(packet);

                    // Write packet
                    chunkPacket.Write((ushort)packet.Length); // Size
                    chunkPacket.WriteBytes(packet.data, 0, packet.Length); // Packet Data
                }

                // Setup header
                //SetupOutboundPacket(chunkPacket);
                WriteOutboundPacketHeader(chunkPacket);

                // And Send
                SendPacketToSocket(chunkPacket);
            }
        }

        void SendPacketToSocket(NetOutboundPacket packet)
        {
            Stats.PhysicalPacketsSent++;
            Stats.PacketsSent++;
            physicalPacketsSentInLastSecond++;
            packetsSentInLastSecond++;

            messenger.SendDataToSocket(packet.data, EndPoint);
        }

        internal bool TryHandlePacket(ushort id)
        {
            // Cap the maximum amount of ids to be stored
            if (handledPacketIds.Count >= config.MaxMessageIdCache)
                handledPacketIds.RemoveFirst();

            // If packet wasn't already handled, add it.
            return handledPacketIds.Add(id);
        }

        internal void HandleAck(ushort ackId)
        {
            ReliablePacket temp;
            // Try removing from both queues
            bool reliableSuccess = reliableOutboundPacketQueue.TryRemove(ackId, out temp);
            bool orderedSuccess = reliableOrderedOutboundPacketQueue.TryPeek(out temp);

            // If this is the first in the ordered queue, dequeue it
            if (orderedSuccess && temp.Packet.Id == ackId)
                orderedSuccess = reliableOrderedOutboundPacketQueue.TryDequeue(out temp);
            else
                orderedSuccess = false; // Reset to false incase the id didn't match

            // If we couldn't find the packet in either queue, then log it as an error
            if (!reliableSuccess && !orderedSuccess && NetLogger.LogAlreadyHandledAcks)
                NetLogger.LogError("[ACK:{0}] Received already handled (or non-existant) ack! Id: {1}", 
                    EndPoint, ackId);
        }

        void Ping()
        {
            if (ignorePing)
                return;

            // Reset the timer
            lastPingRequest = NetTime.Now;

            // Create the request
            NetOutboundPacket pingRequest = new NetOutboundPacket(NetDeliveryMethod.Unreliable);
            pingRequest.SendImmediately = true; // We need accurate results
            pingRequest.Type = NetPacketType.PingRequest;

            // Send it
            SendPacket(pingRequest);
        }

        internal void SendPingResponse()
        {
            if (ignorePing)
                return;

            // Create the response
            NetOutboundPacket pingResponse = new NetOutboundPacket(NetDeliveryMethod.Unreliable);
            pingResponse.SendImmediately = true; // We need accurate results
            pingResponse.Type = NetPacketType.PingResponse;

            // Send it
            SendPacket(pingResponse);
        }

        internal void ReceivedPingResponse()
        {
            // Reset the timer
            lastPingResponse = NetTime.Now;
            int ping = lastPingResponse - lastPingRequest;
            Stats.Ping = ping;

            if (ping < Stats.LowestPing)
                Stats.LowestPing = ping;
            if (ping > Stats.HighestPing)
                Stats.HighestPing = ping;

            AddRecentPing(ping);
        }

        internal bool Heartbeat(int now)
        {
            if (ignorePing)
                lastPingResponse = now;

            // Check if any reliable non-ordered packet is needed to be resent
            foreach (ReliablePacket packet in reliableOutboundPacketQueue.Values)
            {
                if (now - packet.LastSendAttempt >= config.AckAwaitDelay + Stats.Ping)
                {
                    Stats.PacketsLost++;
                    if (NetLogger.LogReliableResends)
                        NetLogger.LogWarning("[Resending Packet] [{0}:{1}:{2}]",
                            packet.Packet.Id, packet.Packet.Type, packet.Packet.DeliveryMethod);
                    AddPacketToChunkQueue(packet.Packet); // Assume packet was lost and resend
                }
            }

            // Check if the current ordered reliable packet is needed to be resent
            ReliablePacket orderedPacket;
            if (reliableOrderedOutboundPacketQueue.TryPeek(out orderedPacket)
                && now - orderedPacket.LastSendAttempt >= config.AckAwaitDelay + Stats.Ping)
            {
                if (orderedPacket.LastSendAttempt != 0 && NetLogger.LogReliableResends)
                {
                    Stats.PacketsLost++;
                    NetLogger.LogWarning("[Resending Packet] [{0}:{1}:{2}]",
                        orderedPacket.Packet.Id, orderedPacket.Packet.Type, 
                        orderedPacket.Packet.DeliveryMethod);
                }

                // If the the next packet exists, and it took too long for an ack, 
                // assume the packet was lost and resend
                AddPacketToChunkQueue(orderedPacket.Packet);
            }

            // Ping
            // Check for timeout
            if (now - lastPingResponse >= config.ConnectionTimeout)
                // We lost connection so return false to indicate the heartbeat failed
                return false;

            // Ping connection
            if (now - lastPingRequest >= config.PingDelay)
                Ping();

            // Run flow control checks
            FlowControlHeartbeat(NetTime.Now);

            // Check if its time to send the chunk
            if ((now - lastChunkSend >= chunkSendDelay || currentChunkSize >= maxChunkSize) 
                && physicalPacketsSentInLastSecond < PacketSendRate)
                SendChunk();

            // Log warning if this connection gets overflowed
            if (physicalPacketsSentInLastSecond >= PacketSendRate)
                NetLogger.LogWarning("[OVERFLOW:{2}] Packets Sent Last Second: {0}, SendRate: {1}",
                    physicalPacketsSentInLastSecond, PacketSendRate, EndPoint);

            // Reset packets per second calculation
            if (now - packetPerSecondLastReset >= 1000)
            {
                Stats.PhysicalPacketsSentPerSecond = physicalPacketsSentInLastSecond;
                Stats.PacketsSentPerSecond = packetsSentInLastSecond;
                physicalPacketsSentInLastSecond = 0;
                packetsSentInLastSecond = 0;
                packetPerSecondLastReset = now;
            }
            if (now - packetsRecPerSecondLastReset >= 1000)
            {
                Stats.PhysicalPacketsReceivedPerSecond = physicalPacketsReceivedInLastSecond;
                Stats.PacketsReceivedPerSecond = packetsReceivedInLastSecond;
                packetsReceivedInLastSecond = 0;
                physicalPacketsReceivedInLastSecond = 0;
                packetsRecPerSecondLastReset = now;
            }

            // Heartbeat was successful
            return true;
        }

        public override string ToString()
        {
            return EndPoint.ToString();
        }
    }
}
