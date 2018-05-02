using System.Net;

/* NetConnectionRequest.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public class NetConnectionRequest
    {
        public IPEndPoint EndPoint;
        public string Password;

        internal NetConnectionRequest(NetConnectionlessInboundPacket packet)
        {
            EndPoint = packet.SenderIP;

            // If the packet contains a password
            if (packet.ReadBool())
                Password = packet.ReadString();
        }
    }
}
