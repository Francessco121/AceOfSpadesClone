using AceOfSpades.IO;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Physics;
using Dash.Net;
using System;
using System.Collections.Generic;
using System.Linq;

/* (Server)TDMGamemode.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public sealed class TDMGamemode : NetworkedGamemode
    {
        int teamAScore;
        int teamBScore;

        const float RESPAWN_TIME = 5f;
        const int SCORE_SUICIDE_PENALTY = -2;
        const int SCORE_TEAMKILL_PENALTY = -4;
        const int SCORE_ASSIST_TEAMKILL_PENALTY = -2;
        const int SCORE_KILL = 2;
        const int SCORE_ASSIST = 1;
        const int SCORE_CAP = 100;

        BiDictionary<NetConnection, NetworkPlayer> teamA;
        BiDictionary<NetConnection, NetworkPlayer> teamB;

        CommandPost redPost, bluePost;

        public TDMGamemode(MatchScreen screen)
            : base(screen, GamemodeType.TDM)
        {
            teamA = new BiDictionary<NetConnection, NetworkPlayer>();
            teamB = new BiDictionary<NetConnection, NetworkPlayer>();
        }

        public override void OnConnectionReady(NetConnection client)
        {
            // Allocate the player to a team
            AddPlayerToTeam(client, NetPlayerComponent.GetPlayer(client));

            // Get client up to date
            NetChannel.FireEvent("Client_GamemodeInfo", client, (ushort)SCORE_CAP);
            NetChannel.FireEvent("Client_UpdateScores", client, RemoteFlag.None,
                NetDeliveryMethod.ReliableOrdered, (short)teamAScore, (short)teamBScore);

            // Spawn their player
            AddRespawn(client, 0);
        }

        public override void OnPlayerKilled(ServerMPPlayer killer, NetworkPlayer killerNetPlayer,
            ServerMPPlayer assistant, NetworkPlayer assistantNetPlayer,
            ServerMPPlayer killed, NetworkPlayer killedNetPlayer, string item)
        {
            // Update scores
            if (killer == null)
                // Update killed score
                killedNetPlayer.Score += SCORE_SUICIDE_PENALTY;
            else
            {
                // Update killer score
                if (killer.Team == killed.Team)
                    killerNetPlayer.Score += SCORE_TEAMKILL_PENALTY;
                else
                    killerNetPlayer.Score += SCORE_KILL;

                // Update assistant score
                if (assistant != null)
                {
                    if (assistant.Team == killed.Team)
                        assistantNetPlayer.Score += SCORE_ASSIST_TEAMKILL_PENALTY;
                    else
                        assistantNetPlayer.Score += SCORE_ASSIST;
                }

                // Update team scores
                if (killer.Team != killed.Team)
                {
                    if (killer.Team == Team.A) teamAScore++;
                    else if (killer.Team == Team.B) teamBScore++;
                }

                // Announce score change
                NetChannel.FireEventForAllConnections("Client_UpdateScores", RemoteFlag.None,
                    NetDeliveryMethod.ReliableOrdered, (short)teamAScore, (short)teamBScore);
            }

            base.OnPlayerKilled(killer, killerNetPlayer, assistant, assistantNetPlayer, 
                killed, killedNetPlayer, item);

            // Add player to the respawn list
            AddRespawn(killed.StateInfo.Owner, RESPAWN_TIME);
        }

        protected override void OnStarted()
        {
            Server.OnUserDisconnected += Server_OnUserDisconnected;

            // Send gamemode info
            NetChannel.FireEventForAllConnections("Client_GamemodeInfo", (ushort)SCORE_CAP);

            var commandposts = World.Description.GetObjectsByTag("CommandPost");

            if (commandposts.Count == 2)
            {
                // Load command posts
                foreach (WorldObjectDescription ob in commandposts)
                {
                    Vector3 position = ob.GetVector3("Position");
                    Team team = (Team)(ob.GetField<byte>("Team") ?? 0);

                    CommandPost post = new CommandPost(position, team);
                    if (team == Team.A) redPost = post;
                    else bluePost = post;

                    post.PhysicsBody.OnCollision += Post_OnCollision;

                    objectComponent.NetworkInstantiate(post, "Client_CreateCommandPost", null,
                        position.X, position.Y, position.Z, (byte)team);
                }
            }
            else
            {
                DashCMD.WriteWarning("[TDMGamemode] Current world does not have a proper gameobject setup! Falling back to default.");
                LoadFallbackGameObjects();
            }

            base.OnStarted();
        }

        void LoadFallbackGameObjects()
        {
            Vector3 blueTeamOrigin =
               World.Terrain.Chunks[new IndexPosition(World.Terrain.Width - 1, 0, World.Terrain.Depth - 1)].Position;

            redPost = new CommandPost(GetSpawnLocation(100, 100, Block.CUBE_SIZE * 3), Team.A);
            bluePost = new CommandPost(GetSpawnLocation(blueTeamOrigin.X - 75, blueTeamOrigin.Z - 75,
                Block.CUBE_SIZE * 3), Team.B);

            redPost.PhysicsBody.OnCollision += Post_OnCollision;
            bluePost.PhysicsBody.OnCollision += Post_OnCollision;

            objectComponent.NetworkInstantiate(redPost, "Client_CreateCommandPost", null,
                redPost.Transform.Position.X, redPost.Transform.Position.Y, redPost.Transform.Position.Z, (byte)redPost.Team);
            objectComponent.NetworkInstantiate(bluePost, "Client_CreateCommandPost", null,
                bluePost.Transform.Position.X, bluePost.Transform.Position.Y, bluePost.Transform.Position.Z, (byte)bluePost.Team);
        }

        private void Post_OnCollision(object sender, PhysicsBodyComponent e)
        {
            ServerMPPlayer player = e.GameObject as ServerMPPlayer;
            if (player != null)
            {
                CommandPost post = ((PhysicsBodyComponent)sender).GameObject as CommandPost;
                if (post.Team == player.Team)
                    player.Refresh();
            }
        }

        protected override void OnStopped()
        {
            Server.OnUserDisconnected -= Server_OnUserDisconnected;

            if (redPost != null && redPost.CreatableInfo != null)
            {
                objectComponent.NetworkDestroy(redPost.CreatableInfo.Id);
                objectComponent.NetworkDestroy(bluePost.CreatableInfo.Id);

                redPost.PhysicsBody.OnCollision -= Post_OnCollision;
                bluePost.PhysicsBody.OnCollision -= Post_OnCollision;
            }

            redPost.Dispose();
            bluePost.Dispose();

            teamA.Clear();
            teamB.Clear();

            teamAScore = 0;
            teamBScore = 0;

            base.OnStopped();
        }

        void CreateNewTeams()
        {
            teamA.Clear();
            teamB.Clear();

            List<KeyValuePair<NetConnection, NetworkPlayer>> netPlayers = 
                NetPlayerComponent.NetPlayersInPairs.ToList();

            Random rnd = new Random();
            while (netPlayers.Count > 0)
            {
                int i = rnd.Next(0, netPlayers.Count);
                KeyValuePair<NetConnection, NetworkPlayer> pair = netPlayers[i];

                AddPlayerToTeam(pair.Key, pair.Value);
                netPlayers.RemoveAt(i);
            }
        }

        void AddPlayerToTeam(NetConnection client, NetworkPlayer player)
        {
            Team team = teamA.Count > teamB.Count ? Team.B : Team.A;
            player.Team = team;

            if (team == Team.A) teamA.TryAdd(client, player);
            else teamB.TryAdd(client, player);
        }

        private void Server_OnUserDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            CancelRespawn(connection);

            ServerMPPlayer player;
            if (Players.TryGetValue(connection, out player))
                DespawnPlayer(player);

            teamA.TryRemove(connection);
            teamB.TryRemove(connection);
        }

        protected override void OnPlayerRespawn(NetConnection client)
        {
            // Find the connections networkplayer
            NetworkPlayer netPlayer = NetPlayerComponent.GetPlayer(client);
            Team team = netPlayer.Team;

            if (team == Team.None)
                DashCMD.WriteError("[TDMGamemode] Failed to respawn player, they do not have a team!");
            else
            {
                Vector3 spawnLocation = team == Team.A 
                    ? redPost.Transform.Position 
                    : bluePost.Transform.Position;

                // Create the character
                SpawnPlayer(client, spawnLocation, team);
            }
        }

        public override void Update(float deltaTime)
        {
            if (teamAScore >= SCORE_CAP)
                EndGame(Team.A);
            else if (teamBScore >= SCORE_CAP)
                EndGame(Team.B);

            base.Update(deltaTime);
        }
    }
}
