namespace AceOfSpades.Net
{
    public class CommandPostEntitySnapshot : NetEntitySnapshot
    {
        public float X
        {
            get { return (float)x.Value; }
            set { x.Value = value; }
        }
        public float Y
        {
            get { return (float)y.Value; }
            set { y.Value = value; }
        }
        public float Z
        {
            get { return (float)z.Value; }
            set { z.Value = value; }
        }

        SnapshotField x;
        SnapshotField y;
        SnapshotField z;

        public CommandPostEntitySnapshot(INetEntity entity, NetCreatableInfo info, SnapshotSystem snapshotSystem) 
            : base(entity, info, snapshotSystem)
        {
            x = AddPrimitiveField<float>();
            y = AddPrimitiveField<float>();
            z = AddPrimitiveField<float>();
        }
    }
}
