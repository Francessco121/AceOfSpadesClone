using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System.Collections.Generic;

/* HandshakeComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class HandshakeComponent
    {
        Dictionary<NetConnection, Handshake> handshakes;
        AOSServer server;
        MatchScreen screen;
        ServerWorld world;

        public HandshakeComponent(MatchScreen screen, ServerWorld world, AOSServer server)
        {
            this.screen = screen;
            this.server = server;
            this.world = world;
            handshakes = new Dictionary<NetConnection, Handshake>();
        }

        public AOSServer GetServer()
        {
            return server;
        }

        public ServerWorld GetWorld()
        {
            return world;
        }

        public void OnPacketInbound(NetInboundPacket packet, CustomPacketType type)
        {
            Handshake h;
            if (handshakes.TryGetValue(packet.Sender, out h))
                h.OnPacketInbound(packet, type);
            else
                DashCMD.WriteError("[HS] Received handshake completion packet, but connection {0} is not in a handshake!", 
                    packet.Sender);
        }

        public bool Initiate(NetConnection with)
        {
            if (handshakes.ContainsKey(with))
            {
                DashCMD.WriteError("[HS] Failed to initate handshake with {0}! Handshake is already in progress!", with);
                return false;
            }

            handshakes.Add(with, new Handshake(this, with));
            return true;
        }

        public void TryCancel(NetConnection with)
        {
            handshakes.Remove(with);
        }

        public void HandshakeCompleted(Handshake h)
        {
            DashCMD.WriteStandard("[HS] Completed handshake with {0}.", h.With);
            handshakes.Remove(h.With);
            screen.OnHandshakeComplete(h);
        }

        public void OnTerrainModified(BlockChange change)
        {
            foreach (Handshake h in handshakes.Values)
                h.TerrainChanges.Add(change);
        }
    }
}
