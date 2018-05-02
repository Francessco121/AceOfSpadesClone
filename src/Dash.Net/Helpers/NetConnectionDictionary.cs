using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

/* NetConnectionDictionary.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    public class NetConnectionDictionary : IEnumerable<KeyValuePair<IPEndPoint, NetConnection>>
    {
        ConcurrentDictionary<IPEndPoint, NetConnection> dictionary;

        public NetConnectionDictionary()
        {
            dictionary = new ConcurrentDictionary<IPEndPoint, NetConnection>();
        }

        public NetConnection this[IPEndPoint key]
        {
            get { return dictionary[key]; }
            internal set { dictionary[key] = value; }
        }

        public int Count { get { return dictionary.Count; } }

        public ICollection<IPEndPoint> Keys { get { return dictionary.Keys; } }
        public ICollection<NetConnection> Values { get { return dictionary.Values; } }

        internal bool TryAdd(IPEndPoint key, NetConnection value)
        {
            return dictionary.TryAdd(key, value);
        }

        internal void Clear()
        {
            dictionary.Clear();
        }

        public bool ContainsKey(IPEndPoint key)
        {
            return dictionary.ContainsKey(key);
        }

        internal bool TryRemove(IPEndPoint key)
        {
            NetConnection value;
            return dictionary.TryRemove(key, out value);
        }

        internal bool TryRemove(IPEndPoint key, out NetConnection value)
        {
            return dictionary.TryRemove(key, out value);
        }

        public bool TryGetValue(IPEndPoint key, out NetConnection value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<IPEndPoint, NetConnection>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}
