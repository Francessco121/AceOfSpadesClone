using System;
using System.Collections;
using System.Collections.Generic;

/* LightList.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class LightList : IList<Light>
    {
        public int Maximum { get { return MasterRenderer.MAX_LIGHTS; } }

        List<Light> list;

        public LightList()
        {
            list = new List<Light>();
        }

        public Light this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(Light light)
        {
            if (list.Count >= MasterRenderer.MAX_LIGHTS)
                throw new InvalidOperationException("Maximum lights exceeded!");
            else
                list.Add(light);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(Light light)
        {
            return list.Contains(light);
        }

        public void CopyTo(Light[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Light> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(Light light)
        {
            return list.IndexOf(light);
        }

        public void Insert(int index, Light light)
        {
            list.Insert(index, light);
        }

        public bool Remove(Light light)
        {
            return list.Remove(light);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
