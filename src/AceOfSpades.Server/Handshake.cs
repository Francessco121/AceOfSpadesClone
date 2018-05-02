using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System.Collections.Generic;
using System.Threading;

/* Handshake.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class Handshake
    {
        public NetConnection With { get; }
        public HashSet<BlockChange> TerrainChanges { get; }

        AOSServer server;
        ServerWorld world;
        HandshakeComponent component;

        HandshakeTerrainData terrainData;
        int terrainDataI;

        public Handshake(HandshakeComponent component, NetConnection with)
        {
            this.component = component;
            With = with;

            server = component.GetServer();
            world = component.GetWorld();
            TerrainChanges = new HashSet<BlockChange>();

            Start();
        }

        public void OnPacketInbound(NetInboundPacket packet, CustomPacketType type)
        {
            if (type == CustomPacketType.HandshakeComplete)
            {
                // Were all done here
                component.HandshakeCompleted(this);
            }
            else if (type == CustomPacketType.WorldSectionAck)
            {
                SendNextTerrainChunk();
            }
        }

        void Start()
        {
            if (With.Stats.MTU > 0)
                // If we are late and the MTU is already set,
                // don't hook into mtu event
                InitiateTerrainTransfer();
            else
            {
                DashCMD.WriteLine("[HS] Awaiting MTU for {0}...", With);
                With.OnMTUSet += With_OnMTUSet;
            }
        }

        private void With_OnMTUSet(object sender, int e)
        {
            // Don't need the event handler anymore
            With.OnMTUSet -= With_OnMTUSet;

            // Send the terrain
            InitiateTerrainTransfer();
        }

        void InitiateTerrainTransfer()
        {
            DashCMD.WriteStandard("[HS] Initiating handshake with {0}...", With);

            NetOutboundPacket initPacket = new NetOutboundPacket(NetDeliveryMethod.ReliableOrdered);
            initPacket.Write((byte)CustomPacketType.HandshakeInitiate);

            int packetSize = With.Stats.MTU;

            ThreadPool.QueueUserWorkItem((obj) =>
            {
                terrainData = new HandshakeTerrainData(world.Terrain, packetSize);
                initPacket.Write((ushort)terrainData.Sections.Length);
                initPacket.Write(terrainData.TotalPacketSize);
                initPacket.Write(terrainData.UncompressedSize);

                DashCMD.WriteStandard(
                    "[HS] Prepared terrain packet for {0}. Sections: {1} ({2} bytes max each), Total Size: {3} bytes",
                    With, terrainData.Sections.Length, packetSize, terrainData.TotalPacketSize);

                With.SendPacket(initPacket);
                SendNextTerrainChunk();
            });
        }

        bool SendNextTerrainChunk()
        {
            if (terrainDataI == terrainData.Sections.Length)
                // All done
                return true;
            else
            {
                NetOutboundPacket terrainPacket = new NetOutboundPacket(NetDeliveryMethod.ReliableOrdered);
                terrainPacket.Write((byte)CustomPacketType.WorldSection);

                byte[] data = terrainData.Sections[terrainDataI];
                terrainPacket.Write((ushort)data.Length);
                terrainPacket.WriteBytes(data);

                With.SendPacket(terrainPacket);

                terrainDataI++;
                return false;
            }
        }
    }
}
