using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Net;
using System;
using System.Collections.Generic;

/* (Client)NetworkedGamemode.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public abstract class NetworkedGamemode : Gamemode
    {
        protected MultiplayerScreen Screen { get; }
        protected MPWorld World { get { return Screen.World; } }
        protected RemoteChannel NetChannel { get; private set; }
        protected AOSClient Client { get; private set; }
        protected Dictionary<ushort, ClientPlayer> Players { get; }
        protected ClientMPPlayer OurPlayer { get; private set; }

        protected MasterRenderer renderer { get; }
        protected ObjectNetComponent objectComponent { get; private set; }

        public NetworkedGamemode(MultiplayerScreen screen, GamemodeType type)
            : base(type)
        {
            Screen = screen;
            Players = new Dictionary<ushort, ClientPlayer>();
            renderer = MasterRenderer.Instance;
        }

        protected virtual void OnPlayerAdded(ushort netId, ClientPlayer player) { }
        protected virtual void OnPlayerRemoved(ushort netId, ClientPlayer player) { }

        public override void Start()
        {
            if (AOSClient.Instance == null)
                throw new InvalidOperationException("Cannot start networked gamemode, no net client has been created!");

            Client          = AOSClient.Instance;
            NetChannel      = Client.GetChannel(AOSChannelType.Gamemode);
            objectComponent = Client.GetComponent<ObjectNetComponent>();

            base.Start();
        }

        protected override void OnStarted()
        {
            IsActive = true;
            objectComponent.AddInstantiationEvent("Client_CreatePlayer", I_CreatePlayer);
            objectComponent.OnCreatableInstantiated += ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed    += ObjectComponent_OnCreatableDestroyed;
            base.OnStarted();
        }
        protected override void OnStopped()
        {
            IsActive = false;
            Players.Clear();
            OurPlayer = null;

            objectComponent.RemoveInstantiationEvent("Client_CreatePlayer");
            objectComponent.OnCreatableInstantiated -= ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed    -= ObjectComponent_OnCreatableDestroyed;
            base.OnStopped();
        }

        INetCreatable I_CreatePlayer(ushort id, bool isAppOwner, NetBuffer data)
        {
            // Read the packet
            float x = data.ReadFloat();
            float y = data.ReadFloat();
            float z = data.ReadFloat();
            Team team = (Team)data.ReadByte();

            ClientPlayer player;
            if (isAppOwner)
            {
                // It's our player, so create a little differently
                OurPlayer = new ClientMPPlayer(renderer, World, Camera.Active, new Vector3(x, y, z), team);
                player = OurPlayer;
            }
            else
                // Someone else's player, create normally
                player = new ReplicatedPlayer(renderer, World, new SimpleCamera(), new Vector3(x, y, z), team);

            return player;
        }

        private void ObjectComponent_OnCreatableInstantiated(object sender, NetCreatableInfo e)
        {
            ClientPlayer player = e.Creatable as ClientPlayer;
            if (player != null)
            {
                // Add to our list
                Players.Add(e.Id, player);
                // Inform gamemode
                OnPlayerAdded(e.Id, player);
            }
        }

        private void ObjectComponent_OnCreatableDestroyed(object sender, NetCreatableInfo e)
        {
            ClientPlayer player = e.Creatable as ClientPlayer;
            if (player == null)
                // We are only concerned with ClientPlayers here
                return;

            // If a player was destroyed we need to remove 
            // anything associated with them.
            Players.Remove(e.Id);

            if (player == OurPlayer)
            {
                // Notify our player
                OurPlayer.OnKilled();
                OurPlayer = null;
            }
        }
    }
}
