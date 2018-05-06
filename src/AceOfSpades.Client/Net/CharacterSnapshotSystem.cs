using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System.Collections.Generic;

/* (Client)CharacterSnapshotSystem.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class CharacterSnapshotSystem
    {
        Dictionary<ushort, ClientPlayer> players;
        ClientMPPlayer ourPlayer;

        SnapshotSystem deltaSnapshotSystem;
        SnapshotNetComponent snapshotComponent;

        public CharacterSnapshotSystem(SnapshotNetComponent snapshotComponent, SnapshotSystem deltaSnapshotSystem)
        {
            this.snapshotComponent = snapshotComponent;
            this.deltaSnapshotSystem = deltaSnapshotSystem;
            
            players = new Dictionary<ushort, ClientPlayer>();

            DashCMD.SetCVar("log_css", false);
        }

        public void Clear()
        {
            players.Clear();
            ourPlayer = null;
        }

        public void OnCreatableInstantiated(NetCreatableInfo info, WorldSnapshot ws)
        {
            ClientPlayer player = info.Creatable as ClientPlayer;

            // Add the new player
            if (player != null)
            {
                players.Add(info.Id, player);

                if (info.IsAppOwner)
                {
                    // Setup player as our own
                    if (ourPlayer != null)
                        DashCMD.WriteError("[CSS] Received client player instantiation twice!");
                    else
                    {
                        // Setup our gamestate
                        // Copy each existing player to the worldsnapshot
                        foreach (ClientPlayer plr in players.Values)
                        {
                            if (plr == player)
                                // The new player doesn't have stateinfo setup yet
                                ws.AddPlayer(info.Id, true, false);
                            else if (!ws.PlayerFieldExists(plr.StateInfo.Id))
                                ws.AddPlayer(plr.StateInfo.Id, false, false);
                        }

                        // Set our player and our new world snapshot
                        ourPlayer = (ClientMPPlayer)player;
                    }
                }
                else
                    ws.AddPlayer(info.Id, false, false);
            }
        }

        public void OnCreatableDestroyed(NetCreatableInfo info, WorldSnapshot ws)
        {
            ClientPlayer player;
            if (players.TryGetValue(info.Id, out player))
            {
                players.Remove(info.Id);

                if (ws != null)
                    ws.RemovePlayer(player);

                if (ourPlayer != null && ourPlayer.StateInfo.Id == info.Id)
                    ourPlayer = null;
            }
        }

        public void OnClientInbound(WorldSnapshot worldSnapshot)
        {
            // Update players
            foreach (SnapshotField field in worldSnapshot.Players)
            {
                PlayerSnapshot snapshot = (PlayerSnapshot)field.Value;

                ClientPlayer player;
                if (players.TryGetValue(snapshot.NetId, out player))
                {
                    player.OnClientInbound(snapshot);
                }
                else if (DashCMD.GetCVar<bool>("log_css"))
                    DashCMD.WriteError("[CSS] Received player state for non-existant player! Id: {0}", snapshot.NetId);
            }
        }

        public void OnClientOutbound(NetOutboundPacket packet)
        {
            if (ourPlayer != null)
            {
                packet.Write(true);
                packet.Write(ourPlayer.StateInfo.Id);

                // Grab current client snapshot
                ourPlayer.OnClientOutbound(snapshotComponent.SnapshotRoundTripTime);
                ourPlayer.ClientSnapshot.Serialize(packet);
                ourPlayer.OnPostClientOutbound();
            }
            else
                // Notify the server that we don't have any player information
                // to send.
                packet.Write(false);
        }
    }
}
