/* NetDenialReason.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public enum NetDenialReason : byte
    {
        ServerFull,
        InvalidPassword,
        ConnectionTimedOut
    }
}
