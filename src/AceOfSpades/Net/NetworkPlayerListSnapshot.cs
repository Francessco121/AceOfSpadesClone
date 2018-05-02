using Dash.Net;
using System.Collections.Generic;

namespace AceOfSpades.Net
{
    public class NetworkPlayerListSnapshot : Snapshot
    {
        Dictionary<ushort, DynamicSnapshotField> snapshotFields;

        public NetworkPlayerListSnapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection, 
            bool dontAllocateId = false, bool dontAwait = false) 
            : base(snapshotSystem, otherConnection, dontAllocateId, dontAwait)
        {
            snapshotFields = new Dictionary<ushort, DynamicSnapshotField>();
            Setup();
        }

        public override string GetUniqueId()
        {
            return "NetworkPlayerList";
        }

        public void AddNetPlayer(NetworkPlayer netPlayer, bool isOwner)
        {
            DynamicSnapshotField field = AddNestedField(netPlayer.Id, 
                new NetworkPlayerSnapshot(snapshotSystem, OtherConnection, netPlayer, !isOwner));
            snapshotFields.Add(netPlayer.Id, field);
        }

        public bool TryGetNetPlayer(ushort id, out NetworkPlayerSnapshot snapshot)
        {
            DynamicSnapshotField field;
            if (snapshotFields.TryGetValue(id, out field))
            {
                snapshot = (NetworkPlayerSnapshot)field.Value;
                return true;
            }
            else
            {
                snapshot = null;
                return false;
            }
        }

        public bool RemoveNetPlayer(NetworkPlayer netPlayer)
        {
            DynamicSnapshotField field;
            if (snapshotFields.TryGetValue(netPlayer.Id, out field))
                return RemoveDynamicField(field);
            else
                return false;
        }
    }
}
