using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System;
using System.Collections.Generic;

/* NetPlayerComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public delegate void NetworkPlayerConnectionCallback(NetConnection connection, NetworkPlayer player);

    /// <summary>
    /// Manages NetworkPlayers and synchronizes 
    /// them with each client.
    /// </summary>
    public class NetPlayerComponent : NetComponent
    {
        class ClientInfo
        {
            public string PlayerName { get; }

            public ClientInfo(string playerName)
            {
                PlayerName = playerName;
            }
        }

        public event NetworkPlayerConnectionCallback OnClientInfoReceived;
        public event NetworkPlayerConnectionCallback OnClientLeave;

        public ICollection<KeyValuePair<NetConnection, NetworkPlayer>> NetPlayersInPairs
        {
            get { return netPlayers.Pairs; }
        }
        public ICollection<NetworkPlayer> NetPlayers
        {
            get { return netPlayers.Values; }
        }

        RemoteChannel channel;
        SnapshotNetComponent snapshotComponent;

        Dictionary<NetConnection, ClientInfo> stashedClientInfo;
        BiDictionary<NetConnection, NetworkPlayer> netPlayers;
        ushort netPlayerId;

        public NetPlayerComponent(AOSServer server)
            : base(server)
        {
            netPlayers        = new BiDictionary<NetConnection, NetworkPlayer>();
            stashedClientInfo = new Dictionary<NetConnection, ClientInfo>();
            channel           = server.GetChannel(AOSChannelType.NetInterface);
            snapshotComponent = server.GetComponent<SnapshotNetComponent>();

            channel.AddRemoteEvent("Server_ClientInfo", R_ClientInfo);
        }

        public NetworkPlayer GetPlayer(NetConnection connection)
        {
            return netPlayers[connection];
        }

        public bool TryGetPlayer(NetConnection connection, out NetworkPlayer player)
        {
            return netPlayers.TryGetValue(connection, out player);
        }

        public NetConnection GetConnection(NetworkPlayer player)
        {
            return netPlayers[player];
        }

        public bool TryGetConnection(NetworkPlayer player, out NetConnection connection)
        {
            return netPlayers.TryGetValue(player, out connection);
        }

        public override void OnConnected(NetConnection connection)
        {
            // Add existing netplayers to the new connections world snapshot
            NetworkPlayerListSnapshot list =
                snapshotComponent.ConnectionStates[connection].WorldSnapshot.NetworkPlayerListSnapshot;
            foreach (NetworkPlayer existingNetPlayer in netPlayers.Values)
                list.AddNetPlayer(existingNetPlayer, true);

            // Add the new netplayer
            NetworkPlayer netPlayer = new NetworkPlayer(netPlayerId++);
            netPlayers.Add(connection, netPlayer);

            // Attempt to handle early client info
            ClientInfo stashedCi;
            if (stashedClientInfo.TryGetValue(connection, out stashedCi))
                ProcessClientInfo(netPlayer, connection, stashedCi);

            base.OnConnected(connection);
        }

        public override void OnDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            RemoveNetPlayer(connection);
            stashedClientInfo.Remove(connection);
            base.OnDisconnected(connection, reason, lostConnection);
        }

        void DisconnectClientForGameVersion(NetConnection client)
        {
            client.Disconnect("Invalid game version. Server version: " + GameVersion.Current.ToString());
        }

        void R_ClientInfo(NetConnection client, NetBuffer data, ushort numArgs)
        {
            try
            {
                if (numArgs == 1)
                    DisconnectClientForGameVersion(client);

                string playerName = data.ReadString();
                GameVersion clientVersion = GameVersion.Deserialize(data);

                if (!GameVersion.Current.Equals(clientVersion))
                    DisconnectClientForGameVersion(client);

                ClientInfo ci = new ClientInfo(playerName);

                NetworkPlayer netPlayer;
                if (netPlayers.TryGetValue(client, out netPlayer))
                    // Process information from client info
                    ProcessClientInfo(netPlayer, client, ci);
                else
                    // Just incase the client got here first, just stash it for later
                    stashedClientInfo.Add(client, ci);
            }
            catch (Exception)
            {
                DashCMD.WriteError("[NPM] Client {0} send invalid client info!", client);
                client.Disconnect("Invalid client info.");
            }
        }

        void ProcessClientInfo(NetworkPlayer netPlayer, NetConnection client, ClientInfo info)
        {
            netPlayer.Name = info.PlayerName;

            // Add netplayer to every players world snapshot
            foreach (NetConnectionSnapshotState state in snapshotComponent.ConnectionStates.Values)
                state.WorldSnapshot.NetworkPlayerListSnapshot.AddNetPlayer(netPlayer, true);

            // Tell other clients
            Client_AddNetPlayer(netPlayer, client);

            // Initialize netplayers for client
            Client_AddInitialPlayers(client);

            if (OnClientInfoReceived != null)
                OnClientInfoReceived(client, netPlayer);
        }

        void Client_AddNetPlayer(NetworkPlayer netPlayer, NetConnection owner)
        {
            NetBuffer buffer = new NetBuffer();
            buffer.Write(netPlayer.Id);
            buffer.Write(netPlayer.Name);
            foreach (NetConnection conn in server.Connections.Values)
                if (conn != owner)
                    channel.FireEvent("Client_AddNetPlayer", conn, buffer);
        }

        void RemoveNetPlayer(NetConnection connection)
        {
            NetworkPlayer netPlayer;
            if (netPlayers.TryGetValue(connection, out netPlayer))
            {
                netPlayers.Remove(connection);

                // Remove netplayer from every players world snapshot
                foreach (NetConnectionSnapshotState state in snapshotComponent.ConnectionStates.Values)
                    state.WorldSnapshot.NetworkPlayerListSnapshot.RemoveNetPlayer(netPlayer);

                // Tell each client this player has left
                channel.FireEventForAllConnections("Client_RemoveNetPlayer", netPlayer.Id);

                if (OnClientLeave != null)
                    OnClientLeave(connection, netPlayer);
            }
        }

        /// <summary>
        /// Sends initialization information to the client
        /// netplayer component. Contains all existing netplayers
        /// and the client's netPlayerId.
        /// </summary>
        void Client_AddInitialPlayers(NetConnection to)
        {
            NetBuffer buffer = new NetBuffer();
            buffer.Write(netPlayers[to].Id);
            buffer.Write((ushort)netPlayers.Count);
            foreach (NetworkPlayer player in netPlayers.Values)
            {
                buffer.Write(player.Id);
                buffer.Write(player.Name);
                buffer.Write((byte)player.Team);
                buffer.Write(player.CharacterId.HasValue);
                if (player.CharacterId.HasValue)
                    buffer.Write(player.CharacterId.Value);
            }

            channel.FireEvent("Client_AddInitialNetPlayers", to,
                RemoteFlag.None, NetDeliveryMethod.ReliableOrdered, buffer);
        }

        public override void Update(float deltaTime)
        {
            foreach (KeyValuePair<NetConnection, NetworkPlayer> pair in netPlayers.Pairs)
                pair.Value.Ping = pair.Key.Stats.Ping;

            base.Update(deltaTime);
        }
    }
}
