using AceOfSpades.Net;
using Dash.Net;

/* (Client)NetComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public abstract class NetComponent
    {
        protected AOSClient client { get; }

        public NetComponent(AOSClient client)
        {
            this.client = client;
        }

        public virtual void Initialize() { }

        public virtual void OnConnected(NetConnection connection) { }
        public virtual void OnDisconnected(NetConnection connection, string reason, bool lostConnection) { }

        /// <summary>
        /// Attempts to process packet with this component.
        /// Returns true if handled, false is unhandled.
        /// </summary>
        /// <returns>true if handled, false is unhandled</returns>
        public virtual bool HandlePacket(NetInboundPacket packet, CustomPacketType type)
        {
            return false;
        }

        /// <summary>
        /// Called once every frame on main thread.
        /// </summary>
        public virtual void Update(float deltaTime) { }
    }
}
