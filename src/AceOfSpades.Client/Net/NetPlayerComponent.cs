using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System;
using System.Collections.Generic;

namespace AceOfSpades.Client.Net
{
    public class NetPlayerComponent : NetComponent
    {
        public event EventHandler<NetworkPlayer> OnNetPlayerAdded;
        public event EventHandler<NetworkPlayer> OnNetPlayerRemoved;
        public event EventHandler OnNetPlayersInitialized;

        public ushort? OurNetPlayerId { get; private set; }
        public IEnumerable<NetworkPlayer> NetPlayers
        {
            get { return netPlayers.Values; }
        }

        RemoteChannel channel;
        SnapshotNetComponent snapshotComponent;

        Dictionary<ushort, NetworkPlayer> netPlayers;

        public NetPlayerComponent(AOSClient client)
            : base(client)
        {
            netPlayers  = new Dictionary<ushort, NetworkPlayer>();

            channel           = client.GetChannel(AOSChannelType.NetInterface);
            snapshotComponent = client.GetComponent<SnapshotNetComponent>();

            channel.AddRemoteEvent("Client_AddNetPlayer", R_AddNetPlayer);
            channel.AddRemoteEvent("Client_RemoveNetPlayer", R_RemoveNetPlayer);
            channel.AddRemoteEvent("Client_AddInitialNetPlayers", R_AddInitialNetPlayers);
        }

        public override void OnDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            OurNetPlayerId = null;
            netPlayers.Clear();

            base.OnDisconnected(connection, reason, lostConnection);
        }

        public bool HasNetPlayer(ushort id)
        {
            return netPlayers.ContainsKey(id);
        }

        public bool TryGetNetPlayer(ushort id, out NetworkPlayer netPlayer)
        {
            return netPlayers.TryGetValue(id, out netPlayer);
        }

        public void SendClientInfo(string playerName)
        {
            DashCMD.WriteStandard("[NPM] Sending client info...");

            NetBuffer data = new NetBuffer();
            data.Write(playerName);
            GameVersion.Current.Serialize(data);

            channel.FireEvent("Server_ClientInfo", client.ServerConnection, 
                RemoteFlag.None, NetDeliveryMethod.ReliableOrdered, data);
        }

        void R_AddNetPlayer(NetConnection server, NetBuffer data, ushort numArgs)
        {
            ushort playerId = data.ReadUInt16();
            string playerName = data.ReadString();

            DashCMD.WriteLine("[NPM] Got NetPlayer[{1}] '{0}'", playerName, playerId);

            NetworkPlayer netPlayer = new NetworkPlayer(playerName, playerId);
            netPlayers.Add(playerId, netPlayer);

            snapshotComponent.WorldSnapshot.NetworkPlayerListSnapshot.AddNetPlayer(netPlayer, false);

            if (OnNetPlayerAdded != null)
                OnNetPlayerAdded(this, netPlayer);
        }

        void R_RemoveNetPlayer(NetConnection server, NetBuffer data, ushort numArgs)
        {
            ushort playerId = data.ReadUInt16();
            NetworkPlayer netPlayer;
            if (netPlayers.TryGetValue(playerId, out netPlayer))
            {
                snapshotComponent.WorldSnapshot.NetworkPlayerListSnapshot.RemoveNetPlayer(netPlayer);
                netPlayers.Remove(playerId);
                DashCMD.WriteLine("[NPM] Removed NetPlayer[{0}]", playerId);

                if (OnNetPlayerRemoved != null)
                    OnNetPlayerRemoved(this, netPlayer);
            }
        }

        void R_AddInitialNetPlayers(NetConnection server, NetBuffer data, ushort numArgs)
        {
            OurNetPlayerId = data.ReadUInt16();
            ushort numPlayers = data.ReadUInt16();
            for (int i = 0; i < numPlayers; i++)
            {
                ushort playerId = data.ReadUInt16();
                string playerName = data.ReadString();
                Team team = (Team)data.ReadByte();
                ushort? charId = null;
                if (data.ReadBool()) // Only read charId if the player has a character
                    charId = data.ReadUInt16();

                DashCMD.WriteLine("[NPM] Got NetPlayer[{1}] '{0}' on team {2}", playerName, playerId, team);

                NetworkPlayer player = new NetworkPlayer(playerName, playerId);
                player.Team = team;
                player.CharacterId = charId;

                if (!netPlayers.ContainsKey(playerId))
                {
                    snapshotComponent.WorldSnapshot.NetworkPlayerListSnapshot.AddNetPlayer(player, false);
                    netPlayers.Add(playerId, player);
                }
                else
                    DashCMD.WriteError("[NPM] Got NetPlayer[{1}] '{0}', but we already added this player!",
                        playerName, playerId);
            }

            if (OnNetPlayersInitialized != null)
                OnNetPlayersInitialized(this, EventArgs.Empty);
        }
    }
}
