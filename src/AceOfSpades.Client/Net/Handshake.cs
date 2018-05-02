using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Net;

/* (Client)Handshake.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public delegate void HandshakeTerrainProgressCallback(int bytesDownloaded, int bytesTotal);

    public class Handshake
    {
        public event HandshakeTerrainProgressCallback OnTerrainProgressReported;

        MultiplayerScreen screen;
        AOSClient client;

        byte[][] terrainData;
        int terrainDataFullSize;
        int terrainDataI;
        int terrainDataRead;
        int terrainUncompressedSize;

        public Handshake(AOSClient client, MultiplayerScreen screen, NetInboundPacket packet)
        {
            this.client = client;
            this.screen = screen;

            Start(packet);
        }

        void Start(NetInboundPacket packet)
        {
            DashCMD.WriteStandard("[HS] Started handshake with {0}...", client.ServerConnection);

            ushort numTerrainSections = packet.ReadUInt16();
            terrainDataFullSize = packet.ReadInt32();
            terrainUncompressedSize = packet.ReadInt32();

            terrainData = new byte[numTerrainSections][];
            DashCMD.WriteStandard("[HS] Awaiting {0} terrain data sections...", numTerrainSections);
        }

        public void Complete()
        {
            DashCMD.WriteStandard("[HS] Completed handshake with {0}.", client.ServerConnection);

            NetOutboundPacket completePacket = new NetOutboundPacket(NetDeliveryMethod.ReliableOrdered);
            completePacket.Write((byte)CustomPacketType.HandshakeComplete);

            client.SendPacket(completePacket);

            screen.OnHandshakeComplete();
        }

        public void OnLevelChunkInbound(NetInboundPacket packet)
        {
            ushort dataLength = packet.ReadUInt16();

            terrainData[terrainDataI] = packet.ReadBytes(dataLength);

            terrainDataRead += dataLength;

            if (OnTerrainProgressReported != null)
                OnTerrainProgressReported(terrainDataRead, terrainDataFullSize);

            DashCMD.WriteStandard("[HS] Received terrain data {0}/{1} bytes", terrainDataRead, terrainDataFullSize);
            terrainDataI++;

            if (terrainDataI < terrainData.Length)
            {
                // Send terrain ack to ask for next part
                NetOutboundPacket ack = new NetOutboundPacket(NetDeliveryMethod.ReliableOrdered);
                ack.Write((byte)CustomPacketType.WorldSectionAck);

                client.SendPacket(ack);
            }
            else
            {
                if (OnTerrainProgressReported != null)
                    OnTerrainProgressReported(terrainDataFullSize, terrainDataFullSize);

                // Uncompress the data and notify the screen we are done downloading.
                HandshakeTerrainData data = new HandshakeTerrainData(terrainData, terrainUncompressedSize);
                screen.OnHandshakeDoneDownloading(data);
            }
        }
    }
}
