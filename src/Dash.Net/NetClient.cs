using System.Net;
using System.Threading;

/* NetClient.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public class NetClient : NetMessenger
    {
        public NetConnection ServerConnection { get; private set; }
        public bool IsConnected
        {
            get { return ServerConnection != null; }
        }

        public event ConnectionChangedHandler OnConnected;
        public event DisconnectedHandler OnDisconnected;

        bool awaitingConnectionResponse;
        bool lastConnectionApproved;
        NetDenialReason lastDenialReason;

        NetClientConfig config;

        bool disconnected;
        string lastDisconnectedReason;
        bool lastDisconnectWasTimeout;

        public NetClient(NetClientConfig config)
            : base(config)
        {
            this.config = config;
        }

        public bool SendPacket(NetOutboundPacket packet)
        {
            if (IsConnected)
            {
                ServerConnection.SendPacket(packet);
                return true;
            }
            else
                return false;
               // throw new NetException("Cannot send packet, this client is not connected!");
        }

        public virtual void Disconnect(string reason)
        {
            if (IsConnected)
            {
                ServerConnection.Disconnect(reason);
                ServerConnection = null;
            }
        }

        public virtual bool Connect(IPEndPoint serverAddress, out NetDenialReason? denialReason)
        {
            return Connect(serverAddress, null, out denialReason);
        }

        public virtual bool Connect(IPEndPoint serverAddress, string password, out NetDenialReason? denialReason)
        {
            if (IsConnected)
                throw new NetException("Client is already connected to a server!");
            if (awaitingConnectionResponse)
                throw new NetException("Cannot connect to server, client is already in the middle of connecting!");

            denialReason = null;
            awaitingConnectionResponse = true;

            NetOutboundPacket request = new NetOutboundPacket(NetDeliveryMethod.Unreliable,
                NetPacketType.ConnectionRequest);
            request.Write(password != null);
            if (password != null)
                request.Write(password);

            int tries = 0;
            while (awaitingConnectionResponse && tries < config.MaxConnectionAttempts)
            {
                tries++;
                SendInternalPacket(request.Clone(), serverAddress);

                // Block thread while awaiting connection
                int timeoutAt = NetTime.Now + config.ConnectionAttemptTimeout;
                while (awaitingConnectionResponse && NetTime.Now < timeoutAt)
                    Thread.Sleep(1);
            }

            if (awaitingConnectionResponse)
            {
                awaitingConnectionResponse = false;
                denialReason = NetDenialReason.ConnectionTimedOut;
                return false;
            }
            else
            {
                if (!lastConnectionApproved)
                    denialReason = lastDenialReason;

                return lastConnectionApproved;
            }
        }

        protected override void HandleConnectionDisconnected(NetConnection connection, string reason, bool connectionLost)
        {
            lastDisconnectedReason = reason;
            lastDisconnectWasTimeout = connectionLost;
            disconnected = true;
        }

        protected override void HandleConnectionRequest(NetConnectionRequest request)
        {
            AddWatchedConnection(request.EndPoint, "Client attempted to connect to this client");
        }

        protected override void HandleConnectionReady(IPEndPoint from)
        {
            AddWatchedConnection(from, "ConnectionReady sent to this client");
            base.HandleConnectionReady(from);
        }

        protected override void HandleConnectionApproved(IPEndPoint from)
        {
            if (awaitingConnectionResponse)
            {
                lastConnectionApproved = true;
                awaitingConnectionResponse = false;

                // Add server NetConnection
                ServerConnection = new NetConnection(from, this);
                Connections.TryAdd(from, ServerConnection);

                if (OnConnected != null)
                    OnConnected(ServerConnection);

                // Tell the server we are ready
                NetOutboundPacket readyPacket = new NetOutboundPacket(NetDeliveryMethod.Unreliable, 
                    NetPacketType.ConnectionReady);
                //SendInternalPacket(readyPacket, from);

                ServerConnection.SendPacket(readyPacket);
            }
            else
                AddWatchedConnection(from, "ConnectionApproved received, but this client is not connecting!");

            base.HandleConnectionApproved(from);
        }

        protected override void HandleConnectionDenied(IPEndPoint from, NetDenialReason reason)
        {
            if (awaitingConnectionResponse)
            {
                lastConnectionApproved = false;
                lastDenialReason = reason;
                awaitingConnectionResponse = false;
            }
            else
                AddWatchedConnection(from, "ConnectionDenied received, but this client is not connecting!");

            base.HandleConnectionDenied(from, reason);
        }

        public override void Update()
        {
            base.Update();

            if (disconnected)
            {
                disconnected = false;

                if (OnDisconnected != null)
                    OnDisconnected(ServerConnection, lastDisconnectedReason, lastDisconnectWasTimeout);

                ServerConnection = null;
            }
        }
    }
}
