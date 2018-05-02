/* CustomPacketType.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    public enum CustomPacketType : byte
    {
        Instantiate = 0,
        Destroy = 1,
        Snapshot = 2,

        HandshakeInitiate,
        HandshakeComplete,
        WorldSection,
        WorldSectionAck,
    }
}
