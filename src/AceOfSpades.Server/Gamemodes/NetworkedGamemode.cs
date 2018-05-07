using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/* (Server)NetworkedGamemode.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public abstract class NetworkedGamemode : Gamemode
    {
        class RespawnToken
        {
            public NetConnection Client { get; }
            public float TimeLeft;

            public RespawnToken(NetConnection client, float time)
            {
                Client = client;
                TimeLeft = time;
            }
        }

        protected MatchScreen Screen { get; }
        protected ServerWorld World { get { return Screen.World; } }
        protected RemoteChannel NetChannel { get; private set; }
        protected AOSServer Server { get; private set; }
        protected Dictionary<NetConnection, ServerMPPlayer> Players { get; }
        protected NetPlayerComponent NetPlayerComponent { get; private set; }
        protected bool AllowMelonLaunchers { get; set; } = true;

        protected ObjectNetComponent objectComponent { get; private set; }
        ConcurrentDictionary<NetConnection, RespawnToken> respawns;

        public NetworkedGamemode(MatchScreen screen, GamemodeType type)
            : base(type)
        {
            Screen = screen;
            Players = new Dictionary<NetConnection, ServerMPPlayer>();
            respawns = new ConcurrentDictionary<NetConnection, RespawnToken>();
        }

        public abstract void OnConnectionReady(NetConnection client);
        public virtual void OnPlayerKilled(ServerMPPlayer killer, NetworkPlayer killerNetPlayer,
            ServerMPPlayer assistant, NetworkPlayer assistantNetPlayer,
            ServerMPPlayer killed, NetworkPlayer killedNetPlayer, string item)
        {
            if (killer != null)
                HandleKillStreak(killer, killerNetPlayer, 1);

            if (assistant != null)
                HandleKillStreak(assistant, assistantNetPlayer, 0.5f);

            killed.KillStreak = 0;
            DespawnPlayer(killed);
        }
        
        protected Vector3 GetSpawnLocation(float x, float z, float yOffset)
        {
            float y = World.Terrain.GetGlobalYAt(
                (int)(x / Block.CUBE_SIZE),
                (int)(z / Block.CUBE_SIZE)) * Block.CUBE_SIZE + yOffset;
            return new Vector3(x, y, z);
        }

        protected void EndGame(Team winner)
        {
            if (DashCMD.GetCVar<bool>("gm_neverend"))
                return;

            IsActive = false;
            DespawnAllPlayers();
            Screen.TeamWon(winner);
        }

        protected void DespawnAllPlayers()
        {
            // Despawn all players
            ServerMPPlayer[] players = new ServerMPPlayer[Players.Count];
            Players.Values.CopyTo(players, 0);
            foreach (ServerMPPlayer player in players)
                DespawnPlayer(player);

            // Clean up
            respawns.Clear();
            Players.Clear();
        }

        protected void AddRespawn(NetConnection client, float respawnTime)
        {
            if (respawns.ContainsKey(client))
                throw new InvalidOperationException("Cannot add respawn, one already exists for this player!");
            if (!IsActive)
                return;

            if (respawnTime > 0)
            {
                RespawnToken token = new RespawnToken(client, respawnTime);
                respawns.TryAdd(client, token);
            }
            else if(IsActive)
                // This method can also be used to simply defer spawn logic
                // to the OnPlayerRespawn method.
                OnPlayerRespawn(client);
        }

        protected bool CancelRespawn(NetConnection client)
        {
            RespawnToken token;
            return respawns.TryRemove(client, out token);
        }

        protected ServerMPPlayer SpawnPlayer(NetConnection client, Vector3 position, Team team)
        {
            // Create the player
            ServerMPPlayer player = new ServerMPPlayer(World, position, team);

            // Instantiate over the network
            objectComponent.NetworkInstantiate(player, "Client_CreatePlayer", client,
                position.X, position.Y, position.Z, (byte)team);

            return player;
        }

        protected void DespawnPlayer(ServerMPPlayer player)
        {
            if (Players.Remove(player.StateInfo.Owner))
            {
                // Network destroy the player
                objectComponent.NetworkDestroy(player.StateInfo.Id);

                // Inform gamemode
                OnPlayerRemoved(player.StateInfo.Owner, player);
            }
        }

        protected override void OnStarted()
        {
            IsActive = true;
            objectComponent.OnCreatableInstantiated += ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed    += ObjectComponent_OnCreatableDestroyed;
            base.OnStarted();
        }

        private void ObjectComponent_OnCreatableDestroyed(object sender, NetCreatableInfo e)
        {
            ServerMPPlayer player = e.Creatable as ServerMPPlayer;
            if (player != null)
            {
                // Update network player
                NetworkPlayer netPlayer;
                if (NetPlayerComponent.TryGetPlayer(e.Owner, out netPlayer))
                    netPlayer.CharacterId = null;
            }
        }

        private void ObjectComponent_OnCreatableInstantiated(object sender, NetCreatableInfo e)
        {
            ServerMPPlayer player = e.Creatable as ServerMPPlayer;
            if (player != null)
            {
                // Update network player
                NetworkPlayer netPlayer = NetPlayerComponent.GetPlayer(e.Owner);
                netPlayer.CharacterId = player.StateInfo.Id;

                // Player is all set
                Players.Add(player.StateInfo.Owner, player);

                // Inform the rest of the gamemode
                OnPlayerAdded(player.StateInfo.Owner, player);
            }
        }

        protected override void OnStopped()
        {
            objectComponent.OnCreatableInstantiated -= ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed    -= ObjectComponent_OnCreatableDestroyed;

            // Despawn all players
            DespawnAllPlayers();

            base.OnStopped();
        }

        protected virtual void OnPlayerAdded(NetConnection client, ServerMPPlayer player) { }
        protected virtual void OnPlayerRemoved(NetConnection client, ServerMPPlayer player) { }
        protected abstract void OnPlayerRespawn(NetConnection client);

        public override void Start()
        {
            if (AOSServer.Instance == null)
                throw new InvalidOperationException("Cannot start networked gamemode, no net server has been created!");

            Server             = AOSServer.Instance;
            objectComponent    = Server.GetComponent<ObjectNetComponent>();
            NetPlayerComponent = Server.GetComponent<NetPlayerComponent>();
            NetChannel         = Server.GetChannel(AOSChannelType.Gamemode);

            base.Start();
        }

        public override void Update(float deltaTime)
        {
            // Process respawns
            foreach (RespawnToken token in respawns.Values)
            {
                token.TimeLeft -= deltaTime;
                
                if (token.TimeLeft <= 0)
                {
                    RespawnToken temp;
                    respawns.TryRemove(token.Client, out temp);

                    if (IsActive)
                        OnPlayerRespawn(token.Client);
                }
            }

            base.Update(deltaTime);
        }

        void HandleKillStreak(ServerMPPlayer player, NetworkPlayer netPlayer, float killCredit)
        {
            float previousKillStreak = player.KillStreak;
            player.KillStreak += killCredit;

            if (AllowMelonLaunchers)
            {
                float previousMod = previousKillStreak % 7f;
                float newMod = player.KillStreak % 7f;

                if (newMod < previousMod)
                {
                    player.NumMelons = Characters.Player.MAX_MELONS;
                    Screen.Chat($"{netPlayer.Name} has acquired the melon launcher!");
                }
            }
        }
    }
}
