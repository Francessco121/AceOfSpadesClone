using AceOfSpades.Net;
using Dash.Net;
using System;

/* NetworkedScreen.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public abstract class NetworkedScreen : GameScreen
    {
        protected AOSClient client { get; private set; }
        protected SnapshotNetComponent snapshotComponent { get; private set; }
        protected ObjectNetComponent objectComponent { get; private set; }
        protected NetPlayerComponent netPlayerComponent { get; private set; }
        protected static RemoteChannel channel { get; private set; }

        public NetworkedScreen(MainWindow window, string name) 
            : base(window, name)
        { }

        public override void Load(object[] args)
        {
            if (client == null)
            {
                // Attempt to start AOSClient if not already started
                if (AOSClient.Instance == null && !AOSClient.Initialize())
                    throw new Exception("Failed to initialize AOSClient!");

                // Setup some common shortcuts
                client = AOSClient.Instance;
                snapshotComponent = client.GetComponent<SnapshotNetComponent>();
                objectComponent = client.GetComponent<ObjectNetComponent>();
                netPlayerComponent = client.GetComponent<NetPlayerComponent>();

                // Grab the screen channel
                channel = client.GetChannel(AOSChannelType.Screen);

                // Make sure to move on the initialization to specific screens.
                OnClientInitialized();
            }

            base.Load(args);
        }

        public override void Unload()
        {
            // Make sure to disconnect before we fully unload the screen
            if (client.IsConnected)
                client.Disconnect("User leaving...");

            base.Unload();
        }

        protected virtual void OnClientInitialized() { }
    }
}
