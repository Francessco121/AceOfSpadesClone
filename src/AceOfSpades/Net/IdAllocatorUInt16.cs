using System;
using System.Collections.Generic;

namespace AceOfSpades.Net
{
    public class IdAllocatorUInt16
    {
        public int AllocationCount
        {
            get { return inUse.Count; }
        }

        ushort currentId;
        HashSet<ushort> inUse;

        public IdAllocatorUInt16()
        {
            inUse = new HashSet<ushort>();
        }

        public ushort Allocate()
        {
            ushort id = currentId++;
            ushort start = id;
            while (inUse.Contains(id))
            {
                id = currentId++;

                if (id == start)
                    throw new Exception("Failed to allocate id, max id's have been reached!");
            }

            return id;
        }

        public bool Deallocate(ushort id)
        {
            return inUse.Remove(id);
        }
    }
}
