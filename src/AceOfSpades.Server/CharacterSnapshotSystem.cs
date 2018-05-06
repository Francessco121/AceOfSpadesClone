using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System.Collections.Generic;

/* (Server)CharacterSnapshotSystem.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class CharacterSnapshotSystem
    {
        Dictionary<ushort, ServerMPPlayer> players;
        Dictionary<NetConnection, ServerMPPlayer> playersFromConnection;

        SnapshotNetComponent snapshotComponent;
        SnapshotSystem deltaSnapshotSystem;

        public CharacterSnapshotSystem(SnapshotNetComponent snapshotComponent, SnapshotSystem deltaSnapshotSystem)
        {
            this.snapshotComponent = snapshotComponent;
            this.deltaSnapshotSystem = deltaSnapshotSystem;

            players               = new Dictionary<ushort, ServerMPPlayer>();
            playersFromConnection = new Dictionary<NetConnection, ServerMPPlayer>();

            DashCMD.SetCVar("log_css", false);
        }

        void WriteDebug(string message, params object[] args)
        {
            if (DashCMD.GetCVar<bool>("log_css"))
                DashCMD.WriteLine(message, args);
        }

        public void OnCreatableInstantiated(NetCreatableInfo info, NetConnectionSnapshotState state)
        {
            ServerMPPlayer player = info.Creatable as ServerMPPlayer;
            if (player != null)
            {
                NetConnection owner = state.Connection;
                WriteDebug("[CSS] Created MPPlayer for {0}.", owner);

                // Create the worldsnapshot for the the player
                WorldSnapshot ws = state.WorldSnapshot;

                // Add the current players to the snapshot
                foreach (ServerMPPlayer plr in players.Values)
                    if (!ws.IsPlayerAdded(plr.StateInfo.Id))
                        ws.AddPlayer(plr.StateInfo.Id, false, true);

                // Add the new player
                players.Add(info.Id, player);
                playersFromConnection.Add(player.StateInfo.Owner, player);

                // Add the new player to each players state (including the new player's state)
                foreach (NetConnectionSnapshotState otherState in snapshotComponent.ConnectionStates.Values)
                    otherState.WorldSnapshot.AddPlayer(info.Id, state == otherState, true);
            }
        }

        public void OnCreatableDestroyed(ushort id)
        {
            ServerMPPlayer player;
            if (players.TryGetValue(id, out player))
            {
                players.Remove(id);
                playersFromConnection.Remove(player.StateInfo.Owner);
                foreach (NetConnectionSnapshotState otherState in snapshotComponent.ConnectionStates.Values)
                    otherState.WorldSnapshot.RemovePlayer(id);

                WriteDebug("[CSS] Removed MPPlayer for {0}.", player.StateInfo.Owner);
            }
        }

        public void OnServerInbound(NetInboundPacket packet, NetConnectionSnapshotState state)
        {
            bool playerSentData = packet.ReadBool();
            if (!playerSentData)
                return;

            ushort entId = packet.ReadUInt16();

            ServerMPPlayer player;
            if (players.TryGetValue(entId, out player))
            {
                // Read packet to snapshot
                player.ClientSnapshot.Deserialize(packet);
                // Update player
                player.OnServerInbound();
            }
            else if (DashCMD.GetCVar<bool>("log_css"))
                DashCMD.WriteError("[CSS] Received client snapshot for player with id {0}, which does not exist!", entId);
        }

        public void OnServerOutbound(NetOutboundPacket packet, NetConnectionSnapshotState state)
        {
            // Write each player to the snapshot
            WorldSnapshot worldSnapshot = state.WorldSnapshot;
            foreach (ServerMPPlayer plr in players.Values)
            {
                PlayerSnapshot playerSnapshot;
                if (worldSnapshot.TryGetPlayer(plr.StateInfo.Id, out playerSnapshot))
                    plr.OnServerOutbound(playerSnapshot);
            }
        }
    }
}
