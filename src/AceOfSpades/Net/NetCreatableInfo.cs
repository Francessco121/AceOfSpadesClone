using Dash.Net;

/* NetEntityInfo.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// Contains information describing an INetCreatable.
    /// Gets around the fact that an INetCreatable cannot
    /// be an abstract class.
    /// </summary>
    public class NetCreatableInfo
    {
        public bool IsAppOwner { get; }
        public ushort Id { get; }
        public NetConnection Owner { get; }
        public INetCreatable Creatable { get; }

        public NetCreatableInfo(NetConnection owner, INetCreatable creatable, ushort id, bool isAppOwner)
        {
            Owner = owner;
            Creatable = creatable;
            Id = id;
            IsAppOwner = isAppOwner;
        }
    }
}
