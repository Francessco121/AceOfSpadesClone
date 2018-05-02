using AceOfSpades.Net;
using Dash.Net;

/* (Server)GameScreen.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public abstract class GameScreen
    {
        public string Name { get; }

        protected ServerGame Game { get; }

        protected AOSServer server { get; private set; }
        protected SnapshotNetComponent snapshotComponent { get; private set; }
        protected ObjectNetComponent objectComponent { get; private set; }
        protected NetPlayerComponent netPlayerComponent { get; private set; }
        protected static RemoteChannel channel { get; private set; }

        public GameScreen(ServerGame game, string name)
        {
            Game = game;
            Name = name;
        }

        public virtual void Load(object[] args)
        {
            if (server == null)
            {
                // Setup common shortcuts
                server             = AOSServer.Instance;
                snapshotComponent  = server.GetComponent<SnapshotNetComponent>();
                objectComponent    = server.GetComponent<ObjectNetComponent>();
                netPlayerComponent = server.GetComponent<NetPlayerComponent>();
                // Grab the channel
                channel            = server.GetChannel(AOSChannelType.Screen);

                // Pass on the one-time initialization
                OnServerInitialized();
            }

            OnLoad(args);
        }

        public virtual void Unload()
        {
            OnUnload();
        }

        protected virtual void OnServerInitialized() { }

        protected abstract void OnLoad(object[] args);
        protected virtual void OnUnload() { }

        public abstract void Update(float deltaTime);
    }
}
