using AceOfSpades.Characters;
using AceOfSpades.IO;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Physics;
using Dash.Net;

namespace AceOfSpades.Server
{
    public sealed class CTFGamemode : NetworkedGamemode
    {
        int teamAScore;
        int teamBScore;

        const float RESPAWN_TIME = 5f;
        const int SCORE_SUICIDE_PENALTY = -2;
        const int SCORE_TEAMKILL_PENALTY = -4;
        const int SCORE_ASSIST_TEAMKILL_PENALTY = -2;
        const int SCORE_KILL = 2;
        const int SCORE_ASSIST = 1;
        const int SCORE_CAPTURE = 10;
        const int SCORE_CAP = 3;

        BiDictionary<NetConnection, NetworkPlayer> teamA;
        BiDictionary<NetConnection, NetworkPlayer> teamB;

        CommandPost redPost, bluePost;
        Intel redIntel, blueIntel;

        public CTFGamemode(MatchScreen screen) 
            : base(screen, GamemodeType.CTF)
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
            if (redIntel.Holder != null)
                NetChannel.FireEvent("Client_IntelPickedUp", client,
                    (byte)Team.A, ((ServerMPPlayer)redIntel.Holder).StateInfo.Id);
            if (blueIntel.Holder != null)
                NetChannel.FireEvent("Client_IntelPickedUp", client,
                    (byte)Team.B, ((ServerMPPlayer)blueIntel.Holder).StateInfo.Id);

            // Spawn their player
            AddRespawn(client, 0);
        }

        protected override void OnPlayerRespawn(NetConnection client)
        {
            // Find the connections networkplayer
            NetworkPlayer netPlayer = NetPlayerComponent.GetPlayer(client);
            Team team = netPlayer.Team;

            if (team == Team.None)
                DashCMD.WriteError("[CTFGamemode] Failed to respawn player, they do not have a team!");
            else
            {
                Vector3 spawnLocation = team == Team.A
                    ? redPost.Transform.Position
                    : bluePost.Transform.Position;

                // Create the character
                SpawnPlayer(client, spawnLocation, team);
            }
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
            }

            base.OnPlayerKilled(killer, killerNetPlayer, assistant, assistantNetPlayer, killed, killedNetPlayer, item);

            // Add player to the respawn list
            AddRespawn(killed.StateInfo.Owner, RESPAWN_TIME);
        }

        protected override void OnStarted()
        {
            Server.OnUserDisconnected += Server_OnUserDisconnected;

            var commandposts = World.Description.GetObjectsByTag("CommandPost");
            var intels = World.Description.GetObjectsByTag("Intel");

            if (commandposts.Count == 2 && intels.Count == 2)
            {
                // Load intel
                foreach (WorldObjectDescription ob in intels)
                {
                    Vector3 position = ob.GetVector3("Position");
                    Team team = (Team)(ob.GetField<byte>("Team") ?? 0);

                    Intel intel = new Intel(position, team);
                    if (team == Team.A) redIntel = intel;
                    else blueIntel = intel;

                    intel.OnPickedUp += Intel_OnPickedUp;
                    intel.OnDropped += Intel_OnDropped;
                    intel.OnReturned += Intel_OnReturned;

                    objectComponent.NetworkInstantiate(intel, "Client_CreateIntel", null,
                        position.X, position.Y, position.Z, (byte)team);
                }

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
                DashCMD.WriteWarning("[CTFGamemode] Current world does not have a proper gameobject setup! Falling back to default.");
                LoadFallbackGameObjects();
            }

            base.OnStarted();
        }

        void LoadFallbackGameObjects()
        {
            Vector3 blueTeamOrigin =
                World.Terrain.Chunks[new IndexPosition(World.Terrain.Width - 1, 0, World.Terrain.Depth - 1)].Position;

            redIntel = new Intel(GetSpawnLocation(200, 200, Block.CUBE_SIZE * 1.5f), Team.A);
            blueIntel = new Intel(GetSpawnLocation(blueTeamOrigin.X - 200, blueTeamOrigin.Z - 200,
                Block.CUBE_SIZE * 1.5f), Team.B);

            redPost = new CommandPost(GetSpawnLocation(100, 100, Block.CUBE_SIZE * 3), Team.A);
            bluePost = new CommandPost(GetSpawnLocation(blueTeamOrigin.X - 75, blueTeamOrigin.Z - 75,
                Block.CUBE_SIZE * 3), Team.B);

            redIntel.OnPickedUp += Intel_OnPickedUp;
            redIntel.OnDropped += Intel_OnDropped;
            redIntel.OnReturned += Intel_OnReturned;
            blueIntel.OnPickedUp += Intel_OnPickedUp;
            blueIntel.OnDropped += Intel_OnDropped;
            blueIntel.OnReturned += Intel_OnReturned;

            redPost.PhysicsBody.OnCollision += Post_OnCollision;
            bluePost.PhysicsBody.OnCollision += Post_OnCollision;

            // Instantiate intels over network
            objectComponent.NetworkInstantiate(redIntel, "Client_CreateIntel", null,
                redIntel.Transform.Position.X, redIntel.Transform.Position.Y, redIntel.Transform.Position.Z, (byte)redIntel.Team);
            objectComponent.NetworkInstantiate(blueIntel, "Client_CreateIntel", null,
                blueIntel.Transform.Position.X, blueIntel.Transform.Position.Y, blueIntel.Transform.Position.Z, (byte)blueIntel.Team);
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
                {
                    player.Refresh();

                    if (player.HasIntel)
                    {
                        Intel intel = player.Intel;
                        intel.Return();
                        player.DropIntel();

                        string team = intel.Team == Team.A ? "Red" : "Blue";
                        Screen.Chat(string.Format("The {0} intel has been captured!", team));

                        NetworkPlayer netPlayer;
                        if (NetPlayerComponent.TryGetPlayer(player.StateInfo.Owner, out netPlayer))
                        {
                            netPlayer.Score += SCORE_CAPTURE;

                            Screen.AddFeedItem(netPlayer.Name, "", World.GetTeamColor(player.Team),
                                "Captured", "Intel", World.GetTeamColor(player.Team == Team.A ? Team.B : Team.A));
                        }

                        if (player.Team == Team.A) teamAScore++;
                        else teamBScore++;

                        if (teamAScore < SCORE_CAP && teamBScore < SCORE_CAP)
                            NetChannel.FireEventForAllConnections("Client_IntelCaptured", (byte)intel.Team);

                        NetChannel.FireEventForAllConnections("Client_UpdateScores",
                            (short)teamAScore, (short)teamBScore);
                    }
                }
            }
        }

        protected override void OnStopped()
        {
            Server.OnUserDisconnected -= Server_OnUserDisconnected;

            if (redIntel != null && redIntel.CreatableInfo != null)
            {
                objectComponent.NetworkDestroy(redIntel.CreatableInfo.Id);
                objectComponent.NetworkDestroy(blueIntel.CreatableInfo.Id);
                objectComponent.NetworkDestroy(redPost.CreatableInfo.Id);
                objectComponent.NetworkDestroy(bluePost.CreatableInfo.Id);

                redIntel.OnPickedUp -= Intel_OnPickedUp;
                redIntel.OnDropped -= Intel_OnDropped;
                redIntel.OnReturned -= Intel_OnReturned;
                blueIntel.OnPickedUp -= Intel_OnPickedUp;
                blueIntel.OnDropped -= Intel_OnDropped;
                blueIntel.OnReturned -= Intel_OnReturned;

                redPost.PhysicsBody.OnCollision -= Post_OnCollision;
                bluePost.PhysicsBody.OnCollision -= Post_OnCollision;
            }

            redIntel.Dispose();
            blueIntel.Dispose();
            redPost.Dispose();
            blueIntel.Dispose();

            teamA.Clear();
            teamB.Clear();

            teamAScore = 0;
            teamBScore = 0;

            base.OnStopped();
        }

        private void Intel_OnReturned(object sender, Player _player)
        {
            Intel intel = (Intel)sender;
            ServerMPPlayer player = (ServerMPPlayer)_player;

            NetworkPlayer netPlayer;
            if (NetPlayerComponent.TryGetPlayer(player.StateInfo.Owner, out netPlayer))
                Screen.AddFeedItem(netPlayer.Name, "", World.GetTeamColor(player.Team),
                    "Returned", "Intel", World.GetTeamColor(player.Team));

            string team = intel.Team == Team.A ? "Red" : "Blue";
            Screen.Chat(string.Format("The {0} intel has been returned to their base!", team));

            NetChannel.FireEventForAllConnections("Client_IntelReturned", (byte)intel.Team);
        }

        private void Intel_OnPickedUp(object sender, Player _player)
        {
            Intel intel = (Intel)sender;
            ServerMPPlayer player = (ServerMPPlayer)_player;

            string team = intel.Team == Team.A ? "Red" : "Blue";
            Screen.Chat(string.Format("The {0} intel has been picked up!", team));

            NetworkPlayer netPlayer;
            if (NetPlayerComponent.TryGetPlayer(player.StateInfo.Owner, out netPlayer))
                Screen.AddFeedItem(netPlayer.Name, "", World.GetTeamColor(player.Team),
                    "Picked Up", "Intel", World.GetTeamColor(intel.Team));

            NetChannel.FireEventForAllConnections("Client_IntelPickedUp",
                (byte)intel.Team, ((ServerMPPlayer)_player).StateInfo.Id);
        }

        private void Intel_OnDropped(object sender, Player _player)
        {
            Intel intel = (Intel)sender;
            ServerMPPlayer player = (ServerMPPlayer)_player;

            string team = intel.Team == Team.A ? "Red" : "Blue";
            Screen.Chat(string.Format("The {0} intel has been dropped!", team));

            NetworkPlayer netPlayer;
            if (NetPlayerComponent.TryGetPlayer(player.StateInfo.Owner, out netPlayer))
                Screen.AddFeedItem(netPlayer.Name, "", World.GetTeamColor(player.Team),
                    "Dropped", "Intel", World.GetTeamColor(intel.Team));

            NetChannel.FireEventForAllConnections("Client_IntelDropped", (byte)intel.Team);
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
            {
                player.DropIntel();
                DespawnPlayer(player);
            }

            teamA.TryRemove(connection);
            teamB.TryRemove(connection);
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
