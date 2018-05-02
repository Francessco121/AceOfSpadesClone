using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

/* NetServer.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public class NetServer : NetMessenger
    {
        struct LostConnection
        {
            public NetConnection Connection;
            public string Reason;
            public bool ConnectionLost;

            public LostConnection(NetConnection conn, string reason, bool connLost)
            {
                Connection = conn;
                Reason = reason;
                ConnectionLost = connLost;
            }
        }

        public event ConnectionChangedHandler OnUserConnected;
        public event DisconnectedHandler OnUserDisconnected;

        HashSet<IPEndPoint> connectingMessengers;
        NetServerConfig config;

        ConcurrentQueue<NetConnection> newConnections;
        ConcurrentQueue<LostConnection> newConnectionsLost;

        public NetServer(NetServerConfig config)
            : base(config)
        {
            this.config = config;
            connectingMessengers = new HashSet<IPEndPoint>();
            newConnections = new ConcurrentQueue<NetConnection>();
            newConnectionsLost = new ConcurrentQueue<LostConnection>();
        }

        protected override void HandleConnectionDisconnected(NetConnection connection, string reason, bool connectionLost)
        {
            NetLogger.Log("Client disconnected: {0}", reason);
            newConnectionsLost.Enqueue(new LostConnection(connection, reason, connectionLost));
        }

        protected internal override NetConnection DropConnection(IPEndPoint endpoint, string reason, bool connectionLost)
        {
            connectingMessengers.Remove(endpoint);
            return base.DropConnection(endpoint, reason, connectionLost);
        }

        protected override void HandleConnectionRequest(NetConnectionRequest request)
        {
            // Check if the password is correct and if there is room
            if (Connections.Count < config.MaxConnections && (config.Password == null || config.Password == request.Password))
            {
                if (!connectingMessengers.Add(request.EndPoint))
                    NetLogger.LogWarning("Connection request from already connecting client @ {0}! (or an error occured)", request.EndPoint);
                else
                {
                    // Send approval
                    NetOutboundPacket approvalPacket = new NetOutboundPacket(NetDeliveryMethod.Unreliable,
                        NetPacketType.ConnectionApproved);
                    SendInternalPacket(approvalPacket, request.EndPoint);
                }
            }
            else
            {
                // Send denial
                NetOutboundPacket denialPacket = new NetOutboundPacket(NetDeliveryMethod.Unreliable,
                    NetPacketType.ConnectionDenied);
                denialPacket.Write((byte)(Connections.Count < config.MaxConnections ? NetDenialReason.InvalidPassword : NetDenialReason.ServerFull));
                SendInternalPacket(denialPacket, request.EndPoint);
            }
        }

        protected override void HandleConnectionReady(IPEndPoint from)
        {
            if (connectingMessengers.Remove(from))
            {
                // Complete the connection process
                NetConnection conn = new NetConnection(from, this);

                if (!Connections.TryAdd(from, conn))
                    NetLogger.LogError("An error occured trying to add a NetConnection! IP: {0}", from);
                else
                {
                    NetLogger.LogImportant("Client successfully connected from {0}", from);
                    newConnections.Enqueue(conn);
                }
            }
            else
            {
                NetLogger.LogWarning("ConnectionReady sent from non-connecting client @ {0}!", from);
                AddWatchedConnection(from, "connection ready from non-connecting client");
            }

            base.HandleConnectionReady(from);
        }

        public override void Update()
        {
            base.Update();

            if (OnUserConnected != null)
            {
                while (newConnections.Count > 0)
                {
                    NetConnection conn;
                    if (newConnections.TryDequeue(out conn))
                        OnUserConnected(conn);
                }
            }

            if (OnUserDisconnected != null)
            {
                while (newConnectionsLost.Count > 0)
                {
                    LostConnection conn;
                    if (newConnectionsLost.TryDequeue(out conn))
                        OnUserDisconnected(conn.Connection, conn.Reason, conn.ConnectionLost);
                }
            }
        }
    }
}
