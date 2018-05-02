using System.Collections.Generic;

/* NetCreatableCollection.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    public class NetCreatableCollection
    {
        public int Count { get { return Creatables.Count + Entities.Count; } }

        public Dictionary<ushort, NetCreatableInfo> Creatables { get; }
        public Dictionary<ushort, NetCreatableInfo> Entities { get; }

        public NetCreatableCollection()
        {
            Creatables = new Dictionary<ushort, NetCreatableInfo>();
            Entities = new Dictionary<ushort, NetCreatableInfo>();
        }

        public void Clear()
        {
            Creatables.Clear();
            Entities.Clear();
        }

        public void Add(ushort key, NetCreatableInfo creatable)
        {
            Creatables.Add(key, creatable);

            if (creatable.Creatable is INetEntity)
                Entities.Add(key, creatable);
        }

        public bool Remove(ushort key)
        {
            Entities.Remove(key);
            return Creatables.Remove(key);
        }
    }
}
