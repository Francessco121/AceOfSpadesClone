/* INetCreatable.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// Represents an object that can be 
    /// network instantiated and destroyed.
    /// </summary>
    public interface INetCreatable
    {
        void OnNetworkInstantiated(NetCreatableInfo info);
        void OnNetworkDestroy();
    }
}
