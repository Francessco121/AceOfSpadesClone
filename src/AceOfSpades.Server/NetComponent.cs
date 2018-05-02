using AceOfSpades.Net;
using Dash.Net;

/* (Server)NetComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public abstract class NetComponent
    {
        protected AOSServer server { get; }

        public NetComponent(AOSServer server)
        {
            this.server = server;
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
