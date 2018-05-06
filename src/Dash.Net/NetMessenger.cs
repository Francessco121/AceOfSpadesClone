using Dash.Net.Helpers;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Threading;

/* NetMessenger.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public abstract partial class NetMessenger
    {
        private class PacketState
        {
            public EndPoint Sender;
            public byte[] Buffer;
            public int BytesReceived;

            public PacketState(IPEndPoint sender, byte[] buffer)
            {
                Sender = sender;
                Buffer = buffer;
            }
        }

        internal class PartialPacket
        {
            public NetConnection from;
            public ushort id;
            public int numPartials;
            public int numPartialsReceived;
            public byte[][] packets;
            public ushort size;

            public PartialPacket(NetConnection from, ushort id, int numPartials, ushort size)
            {
                this.from = from;
                this.id = id;
                this.numPartials = numPartials;
                this.packets = new byte[numPartials][];
                this.size = size;
            }

            public bool AddPacket(int index, byte[] data)
            {
                if (packets[index] == null)
                {
                    packets[index] = data;
                    numPartialsReceived++;
                    return true;
                }

                return false;
            }

            public NetInboundPacket Construct(NetInboundPacket original)
            {
                NetBuffer buffer = new NetBuffer(size);
                for (int i = 0; i < packets.Length; i++)
                    buffer.WriteBytes(packets[i]);

                buffer.RemovePadding();
                return new NetInboundPacket(original, buffer.data);
            }
        }

        class IgnoredConnection
        {
            public IPEndPoint EndPoint;
            public int AcknowledgeAt;
            public int IgnoreTime;

            public IgnoredConnection(IPEndPoint endPoint, int ignoreTime)
            {
                EndPoint = endPoint;
                AcknowledgeAt = NetTime.Now + ignoreTime;
                IgnoreTime = ignoreTime;
            }
        }

        class TrackedConnection
        {
            public IPEndPoint EndPoint;
            public int PacketsFrom;
            public int RefreshAt;
            public int StopTrackingAt;
            public int Accumulator;

            public TrackedConnection(IPEndPoint endPoint)
            {
                EndPoint = endPoint;
                PacketsFrom = 1;
                Accumulator = 1;
                RefreshAt = NetTime.Now + 1000;
                StopTrackingAt = NetTime.Now + 10000;
            }

            public bool IsFlooding()
            {
                if (NetTime.Now < RefreshAt)
                    return false;
                else
                {
                    if (PacketsFrom > 50 || Accumulator > 200)
                        return true;
                    else
                    {
                        RefreshAt = NetTime.Now + 1000;

                        if (PacketsFrom > 0)
                            StopTrackingAt = NetTime.Now + 10000;

                        PacketsFrom = 0;
                        return false;
                    }
                }
            }
        }

        class DelayedInboundPacket
        {
            public PacketState Packet { get; }
            public int QueueAfter { get; }

            public DelayedInboundPacket(PacketState packet, int queueAfter)
            {
                Packet = packet;
                QueueAfter = queueAfter;
            }
        }

        public const int MAX_UDP_PACKET_SIZE = 65507;

        public bool IsRunning { get; private set; }

        public IPEndPoint BoundEndPoint { get; private set; }
        public IPEndPoint ReceiveEndPoint { get; private set; }

        public NetConnectionDictionary Connections { get; private set; }

        public int AvailablePackets
        {
            get { return receivedPackets.Count; }
        }

        public int HeartbeatComputionTimeMS { get; private set; }

        const int READ_BUFFER_SIZE = MAX_UDP_PACKET_SIZE;

        NetMessengerConfig config;
        Thread thread;
        Socket socket;
        bool isLoopRunning;
        Random packetLossRandom;

        ConcurrentQueue<NetInboundPacket> receivedPackets;
        ConcurrentQueue<DelayedInboundPacket> delayedReceivedPackets;

        ushort nextPacketId;

        ConcurrentDictionary<IPEndPoint, IgnoredConnection> ignoredConnections;
        ConcurrentDictionary<IPEndPoint, TrackedConnection> trackedConnections;
        ConcurrentDictionary<IPEndPoint, int> lastIgnoreLengths;

        public NetMessenger(NetMessengerConfig config)
        {
            this.config = config;
            receivedPackets = new ConcurrentQueue<NetInboundPacket>();
            delayedReceivedPackets = new ConcurrentQueue<DelayedInboundPacket>();
            ignoredConnections = new ConcurrentDictionary<IPEndPoint, IgnoredConnection>();
            trackedConnections = new ConcurrentDictionary<IPEndPoint, TrackedConnection>();
            lastIgnoreLengths = new ConcurrentDictionary<IPEndPoint, int>();

            Channels = new ConcurrentDictionary<ushort, RemoteChannel>();
            StateChannels = new ConcurrentDictionary<ushort, StateRemoteChannel>();
            inboundRemotes = new ConcurrentQueue<InboundRemote>();

            Connections = new NetConnectionDictionary();

            packetLossRandom = new Random();
            
            GlobalChannel = new CoreRemoteChannel(this, 0);
            HiddenChannel = new CoreRemoteChannel(this, 1);
        }

        public bool Start(IPEndPoint endPoint, IPEndPoint receiveEndPoint = null)
        {
            if (IsRunning)
                throw new NetException(string.Format("{0} is already running!", GetType().Name));

            try
            {
                IsRunning = true;

                // Create the socket and setup endpoints
                CreateSocket(endPoint, receiveEndPoint);

                // Log success
                NetLogger.LogImportant("Started DashNet v3.0");
                //NetLogger.LogImportant("{1} bound to {0}, listening on {2}", BoundEndPoint, GetType().Name, ReceiveEndPoint);
                NetLogger.LogImportant("{1} bound to {0}", BoundEndPoint, GetType().Name);
                NetLogger.LogImportant("{1} receiving on {0}", ReceiveEndPoint, GetType().Name);

                // Start the network loop
                CreateStartThread();
                return true;
            }
            catch (SocketException e)
            {
                IsRunning = false;
                NetLogger.LogError("Could not start Messenger, {0}", e.SocketErrorCode);
                return false;
            }
        }

        public void Shutdown(string reason)
        {
            if (!IsRunning)
                throw new NetException(string.Format("{0} is not running!", GetType().Name));

            IsRunning = false;
            isLoopRunning = false;

            foreach (NetConnection conn in Connections.Values)
                conn.Disconnect(reason);
        }

        void CreateSocket(IPEndPoint endPoint, IPEndPoint receiveEndPoint)
        {
            // Create socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(endPoint);

            //socket = new UdpClient(endPoint);
            //socket.AllowNatTraversal(true);

            // Setup endpoints
            BoundEndPoint = (IPEndPoint)socket.LocalEndPoint;
            if (receiveEndPoint == null)
                receiveEndPoint = new IPEndPoint(IPAddress.Any, BoundEndPoint.Port);
            ReceiveEndPoint = receiveEndPoint;
        }

        void CreateStartThread()
        {
            thread = new Thread(new ThreadStart(NetworkLoop));
            thread.IsBackground = true;
            thread.Name = string.Format("{0} Network Thread", GetType().Name);

            isLoopRunning = true;
            thread.Start();
        }

        /// <summary>
        /// Returns a received packet if any are available.
        /// <para>Returns null if none are available.</para>
        /// </summary>
        public NetInboundPacket ReadPacket()
        {
            NetInboundPacket packet;
            if (receivedPackets.TryDequeue(out packet))
                return packet;
            else
                return null;
        }

        internal NetMessengerConfig GetBaseConfig()
        {
            return config;
        }

        internal ushort AllocatePacketId()
        {
            return nextPacketId++;
        }

        /// <summary>
        /// Sends a packet to all of the connections.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        public void SendPacketToAll(NetOutboundPacket packet)
        {
            foreach (NetConnection conn in Connections.Values)
                conn.SendPacket(packet.Clone());
        }

        /// <summary>
        /// Sends a packet to all of the connections.
        /// Excludes a connection from the packet.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="exclude">Does not send the packet to that connection.</param>
        public void SendPacketToAll(NetOutboundPacket packet, NetConnection exclude)
        {
            foreach (NetConnection conn in Connections.Values)
                if (conn != exclude)
                    conn.SendPacket(packet.Clone());
        }

        internal void SendInternalPacket(NetOutboundPacket packet, IPEndPoint to)
        {
            packet.RemovePadding();
            packet.SetId(AllocatePacketId());

            if (packet.Type == NetPacketType.ConnectionRequest)
                NetEncryption.EncryptPacket(packet);

            packet.PrependHeader();

            if (NetLogger.LogPacketSends && !IsPingPacket(packet.Type))
                NetLogger.LogVerbose("[Outbound:{0}] {1}", to, packet.ToInternalString());

            SendDataToSocket(packet.data, to, false);
        }

        internal bool SendDataToSocket(byte[] data, IPEndPoint receivingEndPoint, bool mtuTest = false)
        {
            try
            {
                //NetLogger.LogVerbose("Sending {0} byte packet to {1}", data.Length, receivingEndPoint);
                // If this is an MTU test we don't want to automatically fragment the packet
                socket.DontFragment = mtuTest;
                // Try and send the data
                socket.SendTo(data, receivingEndPoint);
                //socket.Send(data, data.Length, receivingEndPoint);
                return true;
            }
            catch (SocketException e)
            {
                // Ignore error reporting if this was an MTU test
                if (!mtuTest)
                    NetLogger.LogError("[ERROR] Failed to send packet to {0}! {1}", receivingEndPoint, e);

                return false;
            }
            finally
            {
                // Reset just incase
                socket.DontFragment = false;
            }
        }

        protected internal virtual NetConnection DropConnection(IPEndPoint endpoint, string reason, bool connectionLost)
        {
            // Remove the disconnected connection
            NetConnection conn = RemoveConnection(endpoint);

            // Handle the Disconnection
            if (conn != null)
                HandleConnectionDisconnected(conn, reason, connectionLost);

            return conn;
        }

        NetConnection RemoveConnection(IPEndPoint endpoint)
        {
            // Remove it from the normal connection list
            NetConnection temp;
            if (!Connections.TryRemove(endpoint, out temp))
                NetLogger.LogError("Failed to remove NetConnection from Connections. IP: {0}", endpoint);

            return temp;
        }

        void NetworkLoop()
        {
            int now = Environment.TickCount;
            while (isLoopRunning)
            {
                Heartbeat();
                HeartbeatComputionTimeMS = Environment.TickCount - now;
                now = Environment.TickCount;
                Thread.Sleep(1);
            }

            socket.Close();
            IsRunning = false;
        }

        void Heartbeat()
        {
            if (socket.Available >= NetOutboundPacket.PacketHeaderSize)
            {
                try
                {
                    // Read the packet
                    PacketState state = new PacketState(ReceiveEndPoint, new byte[READ_BUFFER_SIZE]);
                    //socket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None,
                    //    ref state.Sender, PacketReceived, state);

                    //NetLogger.LogImportant("Awaiting data from {0}...", state.Sender);
                    state.BytesReceived = socket.ReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref state.Sender);
                    //byte[] data = socket.Receive(ref state.Sender);
                    //state.Buffer = data;

                    //NetLogger.LogVerbose("Got {0} byte packet from {1}", br, state.Sender);

                    if (!config.SimulatePacketLoss || packetLossRandom.NextDouble() <= 1f - config.SimulatedPacketLossChance)
                    {
                        if (config.SimulateLatency)
                            delayedReceivedPackets.Enqueue(new DelayedInboundPacket(state, NetTime.Now + config.SimulatedLatencyAmount));
                        else
                            PacketReceived(state);
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.ConnectionReset)
                    {
                        NetLogger.LogError("SocketErrorCode: {0}", e.SocketErrorCode.ToString());
                        NetLogger.LogError(e);
                    }
                }
            }

            if (delayedReceivedPackets.Count > 0)
            {
                DelayedInboundPacket delayedPacket;
                while (delayedReceivedPackets.Count > 0 && delayedReceivedPackets.TryPeek(out delayedPacket))
                {
                    if (delayedPacket.QueueAfter > NetTime.Now)
                        break;

                    if (delayedReceivedPackets.TryDequeue(out delayedPacket))
                    {
                        PacketReceived(delayedPacket.Packet);
                    }
                }
            }

            foreach (NetConnection conn in Connections.Values)
            {
                // Attempt to run the connections heartbeat
                if (!conn.Heartbeat(NetTime.Now))
                {
                    NetLogger.Log("Connection {0} timed out!", conn);
                    // The heartbeat failed which means the connection timed out
                    conn.Disconnect("Lost Connection", true);
                }
            }

            foreach (TrackedConnection conn in trackedConnections.Values)
            {
                if (conn.IsFlooding())
                {
                    int ignoreTime;
                    if (lastIgnoreLengths.TryGetValue(conn.EndPoint, out ignoreTime))
                        ignoreTime *= 2;
                    else
                        ignoreTime = 10000;

                    AddIgnoredConnection(conn.EndPoint,
                        string.Format("Sent {0} invalid packets in the last second, {1} total.", 
                        conn.PacketsFrom, conn.Accumulator),
                        ignoreTime);
                }
                else if (NetTime.Now >= conn.StopTrackingAt && conn.PacketsFrom == 0)
                {
                    TrackedConnection temp;
                    if (trackedConnections.TryRemove(conn.EndPoint, out temp))
                        NetLogger.Log("[WATCH] No longer watching {0} for excessive invalid packets", conn.EndPoint);
                }
            }

            foreach (IgnoredConnection conn in ignoredConnections.Values)
            {
                if (NetTime.Now >= conn.AcknowledgeAt)
                {
                    IgnoredConnection temp;
                    if (ignoredConnections.TryRemove(conn.EndPoint, out temp))
                    {
                        NetLogger.Log("[IGNORE] No longer ignoring {0}...", conn.EndPoint);
                        lastIgnoreLengths[conn.EndPoint] = conn.IgnoreTime;
                    }
                }
            }
        }

        public virtual void Update()
        {
            int now = NetTime.Now;

            ProcessInboundRemotes();

            foreach (NetConnection conn in Connections.Values)
            {
                if (conn.MTUEventNeedsCall)
                {
                    conn.MTUEventNeedsCall = false;
                    conn.FireMTUEvent();
                }
            }
        }

        //void PacketReceived(IAsyncResult ar)
        //{
        //    // End async and get data from socket
        //    PacketState state = (PacketState)ar.AsyncState;
        //    int bytesReceived = socket.EndReceiveFrom(ar, ref state.Sender);

        //    PacketReceived(state, bytesReceived);
        //}

        void PacketReceived(PacketState state)
        {
            if (state.BytesReceived == 0)
                return;

            IPEndPoint sender = (IPEndPoint)state.Sender;

            if (ignoredConnections.Count > 0 && ignoredConnections.ContainsKey(sender))
                return;

            byte[] data = new byte[state.BytesReceived];
            Buffer.BlockCopy(state.Buffer, 0, data, 0, state.BytesReceived);

            // Handle packet
            PacketReceived(data, sender);
        }

        void PacketReceived(byte[] data, IPEndPoint sender, bool parentWasChunk = false)
        {
            NetInboundPacketBase packet;
            NetInboundPacket connectionyPacket = null;
            bool connectionless;

            NetConnection senderConn;
            if (Connections.TryGetValue(sender, out senderConn))
            {
                connectionless = false;
                packet = connectionyPacket = new NetInboundPacket(senderConn, data);

                if (!parentWasChunk)
                    senderConn.PhysicalPacketReceived();
                else
                    senderConn.PacketReceived();
            }
            else
            {
                connectionless = true;
                packet = new NetConnectionlessInboundPacket(sender, data);
            }

            if (packet.ReadHeader())
            {
                if (packet.isChunked)
                {
                    byte[][] chunks = packet.GetChunked();
                    for (int i = 0; i < chunks.Length; i++)
                        PacketReceived(chunks[i], sender, true);
                }
                else
                {
                    if (packet.Type == NetPacketType.MTUTest)
                        return;

                    if (ignoredConnections.Count > 0 && ignoredConnections.ContainsKey(sender))
                        return;

                    bool packetAlreadyReceived = connectionless ? false : !connectionyPacket.Sender.TryHandlePacket(packet.Id);

                    if (NetLogger.LogPacketReceives && !IsPingPacket(packet.Type) && !packetAlreadyReceived)
                        NetLogger.LogVerbose("[Inbound:{0}] {1}", sender, packet.ToInternalString());

                    if (packet.isReliable && !connectionless)
                        ReplyAck(connectionyPacket, sender);
                    else if (packet.isReliable && connectionless)
                        AddWatchedConnection(sender, "Sent reliable connectionless packet");

                    if (packetAlreadyReceived)
                    {
                        if (NetLogger.LogAlreadyHandledPackets)
                            NetLogger.LogWarning("[DUPLICATE PACKET:{0}] Ignoring packet {1}", sender, packet.Id);

                        return;
                    }

                    if (packet.isEncrypted)
                        NetEncryption.DecryptPacket(packet);

                    if (packet.isCompressed)
                        NetCompressor.Decompress(packet);

                    if (packet.isPartial)
                    {
                        if (connectionyPacket == null)
                        {
                            AddWatchedConnection(sender, "Sent connectionless partial packet");
                            return;
                        }

                        if (!HandlePartial(ref connectionyPacket))
                            // Partial is not complete yet, so just return
                            return;
                        else
                            // Partial is complete, so overrite the current 
                            // packet with the completed packet.
                            packet = connectionyPacket;

                    }

                    if (connectionless)
                        HandleConnectionlessInboundPacket((NetConnectionlessInboundPacket)packet);
                    else
                        HandleInboundPacket((NetInboundPacket)packet);
                }
            }
            else
                AddWatchedConnection(sender, "Sent packet with invalid signature");
        }

        protected abstract void HandleConnectionRequest(NetConnectionRequest request);
        protected virtual void HandleConnectionApproved(IPEndPoint from) { }
        protected virtual void HandleConnectionDenied(IPEndPoint from, NetDenialReason reason) { }
        protected virtual void HandleConnectionReady(IPEndPoint from) { }
        protected abstract void HandleConnectionDisconnected(NetConnection connection, string reason, bool connectionLost);

        bool IsRemotePacket(NetPacketType type)
        {
            return type == NetPacketType.RemoteEvent
                || type == NetPacketType.RemoteFunction
                || type == NetPacketType.RemoteFunctionResponse;
        }

        bool IsPingPacket(NetPacketType type)
        {
            return type == NetPacketType.PingRequest
                || type == NetPacketType.PingResponse;
        }

        bool IsPacketConnectionless(NetPacketType type)
        {
            return
                type == NetPacketType.ConnectionApproved ||
                type == NetPacketType.ConnectionDenied ||
                type == NetPacketType.ConnectionRequest ||
                type == NetPacketType.ConnectionReady ||
                type == NetPacketType.Disconnected;
        }

        bool HandlePartial(ref NetInboundPacket packet)
        {
            // Read the partials header (6 bytes)
            ushort partialId = packet.ReadUInt16();
            byte index = packet.ReadByte();
            byte numPartials = packet.ReadByte();
            ushort partialSize = packet.ReadUInt16();

            PartialPacket partial;
            // See if the partial already exists
            if (!packet.Sender.Partials.TryGetValue(partialId, out partial))
            {
                // Since the partial doesn't exist, create one.
                if (!packet.Sender.Partials.TryAdd(partialId,
                    partial = new PartialPacket(packet.Sender, partialId, numPartials, partialSize)))
                {
                    //NetLogger.LogError("[Partial:{0}:{1}] Failed to add new partial!", packet.Sender.EndPoint, partialId);
                    //return false;

                    // TODO: See if theres a better way to handle this partial packet concurrency issue

                    // If for some reason, two partials are processed simultaneously,
                    // and it tried adding two packets at once,
                    // we'll just grab the packet created by the other and move on.
                    partial = packet.Sender.Partials[partialId];
                }
                else
                {
                    if (NetLogger.LogPartials)
                        NetLogger.LogVerbose("[Partial:{0}:{1}] Starting new; NumPackets: {2}; PacketSize: {3}b",
                            packet.Sender, partialId, numPartials, partialSize);

                }
            }

            // Add the current partial
            if (partial.AddPacket(index, packet.ReadBytes(partialSize)))
            {
                if (NetLogger.LogPartials)
                    NetLogger.LogVerbose("[Partial:{0}:{1}] Adding index: {2}; PacketsLeft: {3}",
                        packet.Sender, partialId, index, partial.numPartials - partial.numPartialsReceived);

                // Check if the partial is complete
                if (partial.numPartialsReceived >= partial.numPartials)
                {
                    // Remove the partial from the connections queue
                    if (packet.Sender.Partials.TryRemove(partial.id, out partial))
                    {
                        // Save the old sender
                        NetConnection sender = packet.Sender;
                        // Construct the final packet
                        packet = partial.Construct(packet);
                        // Reset the packets parameters
                        packet.ReadOnly = true;

                        if (NetLogger.LogPartials)
                            NetLogger.LogVerbose("[Partial:{0}:{1}] Constructed final partial; FullSize: {2}b",
                                sender, partialId, packet.Length);

                        // Process the partial like a physical packet
                        packet.position = 0;
                        if (packet.ReadHeader())
                        {
                            if (packet.isEncrypted)
                                NetEncryption.DecryptPacket(packet);
                            if (packet.isCompressed)
                                NetCompressor.Decompress(packet);

                            // Update the stats
                            packet.Sender.PartialPacketReceived(numPartials);

                            // Tell the caller this partial is ready!
                            return true;
                        }
                        else
                        {
                            // Bad stuff happened
                            NetLogger.LogWarning("[Partial:{0}:{1}] Constructed partial packet had invalid header!",
                                sender, partialId);
                            return false;
                        }
                    }
                    else
                        NetLogger.LogError("[Partial:{0}:{1}] Failed to remove completed partial!", 
                            packet.Sender, partialId);
                }
            }

            // Tell the caller this partial is not complete
            return false;
        }

        void ReplyAck(NetInboundPacket packet, IPEndPoint sender)
        {
            // Create the ack packet
            NetOutboundPacket ack = new NetOutboundPacket(NetDeliveryMethod.Unreliable, 3);
            ack.Type = NetPacketType.AckResponse;
            ack.Write(packet.Id); // Write the id

            if (NetLogger.LogAcks)
                NetLogger.LogVerbose("[ACK] Replying {0} to {1}", packet.Id, packet.Sender);

            // And send
            packet.Sender.SendPacket(ack);
        }

        void HandleInboundPacket(NetInboundPacket packet)
        {
            if (IsPingPacket(packet.Type))
                HandlePingPacket(packet);
            else if (packet.Type == NetPacketType.AckResponse)
            {
                // Handle ack
                ushort ackid = packet.ReadUInt16();
                if (NetLogger.LogAcks)
                    NetLogger.LogVerbose("[ACK] Received {0} from {1}", ackid, packet.Sender);
                packet.Sender.HandleAck(ackid);
            }
            else if (packet.Type == NetPacketType.Custom)
                receivedPackets.Enqueue(packet);
            else if (packet.Type == NetPacketType.Disconnected)
            {
                string reason = packet.ReadString();
                bool lostConnection = packet.ReadBool();
                packet.Sender.Disconnect(reason, lostConnection);
            }
            else if (IsRemotePacket(packet.Type))
                HandleRemotePacket(packet);
            else
                NetLogger.LogWarning("Invalid packet sent from {0}. Type: {1}", packet.Sender, packet.Type);
        }

        void HandlePingPacket(NetInboundPacket packet)
        {
            if (packet.Type == NetPacketType.PingRequest)
                packet.Sender.SendPingResponse();
            else if (packet.Type == NetPacketType.PingResponse)
                packet.Sender.ReceivedPingResponse();
        }

        void HandleConnectionlessInboundPacket(NetConnectionlessInboundPacket packet)
        {
            if (packet.Type == NetPacketType.ConnectionRequest)
            {
                NetConnectionRequest request = new NetConnectionRequest(packet);
                HandleConnectionRequest(request);
            }
            else if (packet.Type == NetPacketType.ConnectionApproved)
                HandleConnectionApproved(packet.SenderIP);
            else if (packet.Type == NetPacketType.ConnectionDenied)
            {
                NetDenialReason reason = (NetDenialReason)packet.ReadByte();
                HandleConnectionDenied(packet.SenderIP, reason);
            }
            else if (packet.Type == NetPacketType.ConnectionReady)
                HandleConnectionReady(packet.SenderIP);
            else
                AddWatchedConnection(packet.SenderIP, string.Format("Sent invalid connectionless packet ({0})", packet.Type));
        }

        public void AddIgnoredConnection(IPEndPoint endPoint, string reason, int ignoreTimeMilliseconds)
        {
            NetLogger.LogWarning("[IGNORE:{0}:{1}ms] {2}", endPoint, ignoreTimeMilliseconds, reason);

            IgnoredConnection ignored = new IgnoredConnection(endPoint, ignoreTimeMilliseconds);
            ignoredConnections.TryAdd(endPoint, ignored);

            TrackedConnection temp;
            trackedConnections.TryRemove(endPoint, out temp);
        }

        protected void AddWatchedConnection(IPEndPoint endPoint, string reason)
        {
            if (ignoredConnections.ContainsKey(endPoint))
                return;

            TrackedConnection tracked;
            if (trackedConnections.TryGetValue(endPoint, out tracked))
            {
                tracked.PacketsFrom++;
                tracked.Accumulator++;
            }
            else
            {
                tracked = new TrackedConnection(endPoint);
                if (trackedConnections.TryAdd(endPoint, tracked))
                    NetLogger.LogWarning("[WATCH:{0}] {1}", endPoint, reason);
            }
        }
    }
}
