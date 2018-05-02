using AceOfSpades.Characters;
using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Net;
using System;
using System.Collections.Generic;
using System.IO;

/* MatchScreen.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class MatchScreen : GameScreen
    {
        public ServerWorld World { get; private set; }

        const float GAMEMODE_RESTART_DELAY = 10f;

        HandshakeComponent handshakeComponent;

        Dictionary<GamemodeType, NetworkedGamemode> gamemodes;
        NetworkedGamemode currentGamemode;

        bool processNewConnections;
        float restartTime;
        bool gameRestarting;

        public MatchScreen(ServerGame game) 
            : base(game, "Match")
        {
            gamemodes = new Dictionary<GamemodeType, NetworkedGamemode>()
            {
                { GamemodeType.TDM, new TDMGamemode(this) },
                { GamemodeType.CTF, new CTFGamemode(this) }
            };

            // Setup default multiplayer cvars
            DashCMD.SetCVar("ch_infammo", false);
            DashCMD.SetCVar("ch_infhealth", false);
            DashCMD.SetCVar("mp_friendlyfire", false);

            DashCMD.SetCVar("sv_impacts", false);
            DashCMD.SetCVar("sv_hitboxes", false);

            DashCMD.SetCVar("rp_rollback_constant", false);
            DashCMD.SetCVar("rp_rollback_factor", 0.5f);
            DashCMD.SetCVar("rp_rollback_offset", 0);
            DashCMD.SetCVar("rp_usetargetping", false);

            DashCMD.SetCVar("gm_neverend", false);

            DashCMD.AddCommand("world", "Changes the world", "world [filename | *]",
                (args) =>
                {
                    if (args.Length != 1)
                        DashCMD.WriteImportant("Current World: {0}", World.CurrentWorldName);
                    else
                    {
                        string worldFile = args[0];
                        ChangeWorld(worldFile);
                    }
                });

            DashCMD.AddCommand("worlds", "Lists all worlds", "worlds",
                (args) =>
                {
                    string[] worlds = Directory.GetFiles("Content/Worlds");
                    DashCMD.WriteImportant("Available Worlds ({0}):", worlds.Length);
                    for (int i = 0; i < worlds.Length; i++)
                        DashCMD.WriteStandard("  {0}", Path.GetFileNameWithoutExtension(worlds[i]));
                });

            DashCMD.AddCommand("gamemode", "Changes the gamemode", "gamemode [mode]",
                (args) =>
                {
                    if (args.Length != 1)
                        DashCMD.WriteImportant("Current Gamemode: {0}", 
                            currentGamemode != null ? currentGamemode.Type.ToString() : "None");
                    else
                    {
                        GamemodeType type;
                        if (Enum.TryParse(args[0], true, out type))
                            ChangeWorld(World.CurrentWorldName, type);
                        else
                            DashCMD.WriteError("Gamemode '{0}' does not exist!", type);
                    }
                });

            DashCMD.AddCommand("say", "Announces a global message", "say <message>",
                (args) =>
                {
                    if (args.Length == 0)
                        DashCMD.ShowSyntax("say");
                    else
                        Announce(DashCMD.CombineArgs(args), 5);
                });

            DashCMD.AddCommand("chat", "Sends a chat message from the user 'SERVER'", "chat <message>",
                (args) =>
                {
                    if (args.Length == 0)
                        DashCMD.ShowSyntax("chat");
                    else
                        Chat(DashCMD.CombineArgs(args));
                });
        }

        protected override void OnServerInitialized()
        {
            NetLogger.LogObjectStateChanges = true;
            NetLogger.LogVerboses = true;

            base.OnServerInitialized();
        }

        protected override void OnLoad(object[] args)
        {
            // Create the world and hook into it's events
            World = new ServerWorld();
            World.OnPlayerKilled += World_OnPlayerKilled;

            // Hook into the component events
            snapshotComponent.OnWorldSnapshotOutbound += SnapshotComponent_OnWorldSnapshotOutbound;
            netPlayerComponent.OnClientInfoReceived   += NetPlayerComponent_OnClientInfoReceived;
            netPlayerComponent.OnClientLeave          += NetPlayerComponent_OnClientLeave;

            // Create the handshake component so we can send players world data
            handshakeComponent = new HandshakeComponent(this, World, server);

            // Add remotes
            channel.AddRemoteEvent("Server_ChatItem", R_ChatItem);

            // If the world loaded a default map
            // hook into it's events and let clients download it
            if (World.Terrain != null)
            {
                World.Terrain.OnModified += Terrain_OnModified;
                processNewConnections = true;
            }

            // Hook into the custom packet handler
            server.AddPacketHook(OnCustomPacket);

            // Hook into the user connection events
            server.OnUserConnected    += Server_OnUserConnected;
            server.OnUserDisconnected += Server_OnUserDisconnected;

            SwitchGamemode(GamemodeType.CTF);
        }

        protected override void OnUnload()
        {
            // Stop sending world data to players
            processNewConnections = false;
            
            // Dipose of the world
            World.OnPlayerKilled -= World_OnPlayerKilled;
            World.Dispose();

            // Unhook from user connection events
            server.OnUserConnected    -= Server_OnUserConnected;
            server.OnUserDisconnected -= Server_OnUserDisconnected;

            // Unhook from the custom packet handler
            server.RemovePacketHook(OnCustomPacket);

            // Remove remotes
            channel.RemoveRemoteEvent("Server_ChatItem");

            // Unhook component events
            snapshotComponent.OnWorldSnapshotOutbound -= SnapshotComponent_OnWorldSnapshotOutbound;
            netPlayerComponent.OnClientInfoReceived   -= NetPlayerComponent_OnClientInfoReceived;
            netPlayerComponent.OnClientLeave          -= NetPlayerComponent_OnClientLeave;

            // Stop the current gamemode
            if (currentGamemode != null)
            {
                currentGamemode.Stop();
                currentGamemode = null;
            }

            base.OnUnload();
        }

        public void TeamWon(Team winner)
        {
            if (DashCMD.GetCVar<bool>("gm_neverend"))
                return;

            channel.FireEventForAllConnections("Client_TeamWon", (byte)winner);
            Chat("Game Over, restarting in " + GAMEMODE_RESTART_DELAY + " seconds...");
            restartTime = GAMEMODE_RESTART_DELAY;
            gameRestarting = true;
        }

        void SwitchGamemode(GamemodeType to)
        {
            if (currentGamemode != null)
                currentGamemode.Stop();

            currentGamemode = null;

            NetworkedGamemode gamemode;
            if (gamemodes.TryGetValue(to, out gamemode))
            {
                currentGamemode = gamemode;
                currentGamemode.Start();

                channel.FireEventForAllConnections("Client_SwitchGamemode", (byte)to);
            }
            else
                DashCMD.WriteError("[MatchScreen] Gamemode type '{0}' is not defined!", to);
        }

        private void SnapshotComponent_OnWorldSnapshotOutbound(object sender, WorldSnapshot e)
        {
            
        }

        private void World_OnPlayerKilled(PlayerDamage damageEvent)
        {
            ServerMPPlayer killer = (ServerMPPlayer)damageEvent.Attacker;
            ServerMPPlayer assistant = (ServerMPPlayer)damageEvent.AttackerAssistant;
            ServerMPPlayer killed = (ServerMPPlayer)damageEvent.Attacked;
            string item = damageEvent.Cause;

            if (killer == killed)
                // Kill was actually suicide
                killer = null;

            // Killer only gets credit within 6.5s of players death
            if (Environment.TickCount - damageEvent.DamagedAt >= 6500)
                killer = null;
            // Assistant only gets credit within 8s of players death
            if (Environment.TickCount - damageEvent.AttackerAssistedAt >= 8000)
                assistant = null;

            // Get network players
            string leftName = "", rightName = "", assistantName = "";
            NetworkPlayer killerNetPlayer = null, killedNetPlayer = null, assistantNetPlayer = null;
            if (killer != null && netPlayerComponent.TryGetPlayer(killer.StateInfo.Owner, out killerNetPlayer))
                leftName = killerNetPlayer.Name;
            if (killed != null && netPlayerComponent.TryGetPlayer(killed.StateInfo.Owner, out killedNetPlayer))
                rightName = killedNetPlayer.Name;
            if (assistant != null && netPlayerComponent.TryGetPlayer(assistant.StateInfo.Owner, out assistantNetPlayer))
                assistantName = assistantNetPlayer.Name;

            // Announce feed item
            AddFeedItem(
                leftName, assistantName,
                killer != null ? World.GetTeamColor(killer.Team) : Color.White, 
                item, 
                rightName, killed != null ? World.GetTeamColor(killed.Team) : Color.White);

            // Notify gamemode
            currentGamemode.OnPlayerKilled(killer, killerNetPlayer, assistant, assistantNetPlayer, killed, killedNetPlayer, item);

            // Debug
            if (killer != null)
            {
                if (assistant != null)
                    DashCMD.WriteLine("[MatchScreen] '{0} + {1} [ {2} ] {3}'", leftName, assistantName, item, rightName);
                else
                    DashCMD.WriteLine("[MatchScreen] '{0} [ {1} ] {2}'", leftName, item, rightName);
            }
            else
                DashCMD.WriteLine("[MatchScreen] '[ {0} ] {1}'", item, rightName);
        }

        public void Announce(string message, float duration)
        {
            channel.FireEventForAllConnections("Client_Announcement", message, duration);
        }

        public void AddFeedItem(string left, string leftAssist, Color leftColor, string middle, string right, Color rightColor)
        {
            channel.FireEventForAllConnections("Client_AddFeedItem", 
                left, leftAssist, leftColor.R, leftColor.G, leftColor.B,
                middle, 
                right, rightColor.R, rightColor.G, rightColor.B);
        }

        void R_ChatItem(NetConnection client, NetBuffer data, ushort numArgs)
        {
            string message = data.ReadString();
            channel.FireEventForAllConnections("Client_ChatItem", message);
        }

        public void Chat(string message)
        {
            string fullMessage = string.Format("<SERVER> {0}", message);
            channel.FireEventForAllConnections("Client_ChatItem", fullMessage);
        }

        void ChangeWorld(string worldFile, GamemodeType? newGamemode = null)
        {
            // Disable this in the case of a forced world change
            // while the count down is going.
            gameRestarting = false;

            // Reset all the scores
            ResetPlayerScores();

            // Disable client downloading of worlds temporarily
            processNewConnections = false;
            if (World.Terrain != null)
                World.Terrain.OnModified -= Terrain_OnModified;

            // Stop the gamemode
            currentGamemode.Stop();

            // Notify each client we are changing worlds
            channel.FireEventForAllConnections("Client_UnloadWorld");

            // Attempt to load the world
            if (World.LoadFromFile(worldFile))
            {
                // Re-enable client world downloading
                World.Terrain.OnModified += Terrain_OnModified;
                processNewConnections = true;

                // Start the gamemode
                if (newGamemode.HasValue)
                    SwitchGamemode(newGamemode.Value);
                else
                    currentGamemode.Start();

                channel.FireEventForAllConnections("Client_SwitchGamemode", (byte)currentGamemode.Type);

                // Initiate a download handshake with each client
                foreach (NetConnection conn in server.Connections.Values)
                    if (!handshakeComponent.Initiate(conn))
                        conn.Disconnect("Failed to initiate handshake!");
            }
        }

        private void Terrain_OnModified(object sender, BlockChange e)
        {
            // Relay terrain modification to each client
            foreach (NetConnectionSnapshotState state in snapshotComponent.ConnectionStates.Values)
                state.WorldSnapshot.TerrainSnapshot.AddChange(e);
        }

        private void NetPlayerComponent_OnClientInfoReceived(NetConnection connection, NetworkPlayer player)
        {
            DashCMD.WriteImportant("[MatchScreen] '{0}' has joined!", player.Name);
            Chat(string.Format("'{0}' has joined!", player.Name));

            // Initate handshake with this new connection
            if (processNewConnections && !handshakeComponent.Initiate(connection))
                connection.Disconnect("Failed to initiate handshake!");
        }

        bool OnCustomPacket(NetInboundPacket packet, CustomPacketType type)
        {
            if (type == CustomPacketType.HandshakeComplete || type == CustomPacketType.WorldSectionAck)
            {
                handshakeComponent.OnPacketInbound(packet, type);
                return true;
            }

            return false;
        }

        private void NetPlayerComponent_OnClientLeave(NetConnection connection, NetworkPlayer player)
        {
            DashCMD.WriteImportant("[MatchScreen] '{0}' has left!", player.Name);
            Chat(string.Format("'{0}' has left!", player.Name));
        }

        private void Server_OnUserDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            // Just in case
            handshakeComponent.TryCancel(connection);
        }

        private void Server_OnUserConnected(NetConnection conn)
        {
            if (currentGamemode != null)
                channel.FireEvent("Client_SwitchGamemode", conn, (byte)currentGamemode.Type);

            // Send existing instantiations to make sure the client is up to date
            objectComponent.SendInstantiationPackets(conn);
        }

        public void OnHandshakeComplete(Handshake h)
        {
            // Notify gamemode
            currentGamemode.OnConnectionReady(h.With);

            // Add late changes in terrain to players snapshot
            NetConnectionSnapshotState state = snapshotComponent.ConnectionStates[h.With];
            foreach (BlockChange change in h.TerrainChanges)
                state.WorldSnapshot.TerrainSnapshot.AddChange(change);

            // And...were good.
            state.Ready = true;
        }
        
        void ResetPlayerScores()
        {
            foreach (NetworkPlayer netPlayer in netPlayerComponent.NetPlayers)
                netPlayer.Score = 0;
        }

        public override void Update(float deltaTime)
        {
            // Update the gamemode
            if (currentGamemode != null && currentGamemode.IsActive)
                currentGamemode.Update(deltaTime);

            // Update the world
            if (World != null)
                World.Update(deltaTime);

            // Handle gamemode restarts
            if (gameRestarting)
            {
                if (restartTime > 0)
                    restartTime -= deltaTime;
                else
                {
                    gameRestarting = false;
                    ChangeWorld(World.CurrentWorldName);
                }
            }
        }
    }
}
