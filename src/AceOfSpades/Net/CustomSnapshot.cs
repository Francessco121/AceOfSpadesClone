using Dash.Net;

/* CustomSnapshot.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// A snapshot that has custom serialize and deserialize methods.
    /// This allows complex data to be nested in a normal snapshot.
    /// </summary>
    public abstract class CustomSnapshot
    {
        protected abstract void OnSerialize(NetBuffer buffer);
        protected abstract void OnDeserialize(NetBuffer buffer);

        public void Serialize(NetBuffer buffer)
        {
            NetBuffer t = new NetBuffer();
            OnSerialize(t);

            // When the parent snapshot goes to deserialize,
            // it needs the length of the sent data incase it needs
            // to skip this.
            buffer.Write((ushort)t.Length);
            buffer.WriteBytes(t.Data, 0, t.Length);
        }

        public void Deserialize(NetBuffer buffer)
        {
            OnDeserialize(buffer);
        }
    }
}
