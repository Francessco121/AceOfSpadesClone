using Dash.Net;
using System.Collections.Generic;

namespace AceOfSpades.Net
{
    public class NetEntityListSnapshot : Snapshot
    {
        Dictionary<ushort, DynamicSnapshotField> snapshotFields;

        public NetEntityListSnapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection, 
            bool dontAllocateId = false, bool dontAwait = false) 
            : base(snapshotSystem, otherConnection, dontAllocateId, dontAwait)
        {
            snapshotFields = new Dictionary<ushort, DynamicSnapshotField>();
        }

        public override string GetUniqueId()
        {
            return "NetEntityList";
        }

        public void AddNetEntity(NetCreatableInfo info, INetEntity entity)
        {
            NetEntitySnapshot snapshot = entity.CreateSnapshot(info, snapshotSystem);
            DynamicSnapshotField field = AddNestedField(info.Id, snapshot);
            snapshotFields.Add(info.Id, field);
        }

        public bool TryGetEntitySnapshot(ushort id, out NetEntitySnapshot snapshot)
        {
            DynamicSnapshotField field;
            if (snapshotFields.TryGetValue(id, out field))
            {
                snapshot = (NetEntitySnapshot)field.Value;
                return true;
            }
            else
            {
                snapshot = null;
                return false;
            }
        }

        public bool RemoveNetEntitiy(ushort id)
        {
            DynamicSnapshotField field;
            if (snapshotFields.TryGetValue(id, out field))
                return RemoveDynamicField(field);
            else
                return false;
        }
    }
}
