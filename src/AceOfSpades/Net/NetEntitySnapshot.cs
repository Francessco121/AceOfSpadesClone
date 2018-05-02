using System;

namespace AceOfSpades.Net
{
    public abstract class NetEntitySnapshot : Snapshot
    {
        public INetEntity Entity { get; }
        public NetCreatableInfo EntityInfo { get; }

        public NetEntitySnapshot(INetEntity entity, NetCreatableInfo info, SnapshotSystem snapshotSystem) 
            : base(snapshotSystem, info.Owner, !info.IsAppOwner)
        {
            Entity = entity;
            EntityInfo = info;

            if (entity != info.Creatable)
                throw new Exception("Failed to create NetEntitySnapshot, entity does not match the creatable info!");

            EnableDeltaCompression(10);
        }

        public override string GetUniqueId()
        {
            return string.Format("NetEntity_{0}", EntityInfo.Id);
        }
    }
}
