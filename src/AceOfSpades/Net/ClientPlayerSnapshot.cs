using Dash.Net;

/* ClientPlayerSnapshot.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// Snapshot containing player information from a client.
    /// </summary>
    public class ClientPlayerSnapshot : Snapshot
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

        public float CamYaw
        {
            get { return (float)camYaw.Value; }
            set { camYaw.Value = value; }
        }

        public float CamPitch
        {
            get { return (float)camPitch.Value; }
            set { camPitch.Value = value; }
        }

        public bool IsCrouching { get; set; }
        public bool IsFlashlightVisible { get; set; }
        public bool Reload { get; set; }
        public bool DropIntel { get; set; }

        public byte SelectedItem
        {
            get { return (byte)selectedItem.Value; }
            set { selectedItem.Value = value; }
        }

        public byte ColorR
        {
            get { return (byte)colorR.Value; }
            set { colorR.Value = value; }
        }

        public byte ColorG
        {
            get { return (byte)colorG.Value; }
            set { colorG.Value = value; }
        }

        public byte ColorB
        {
            get { return (byte)colorB.Value; }
            set { colorB.Value = value; }
        }

        public ClientBulletSnapshot BulletSnapshot { get; }

        SnapshotField x;
        SnapshotField y;
        SnapshotField z;
        SnapshotField camYaw;
        SnapshotField camPitch;

        SnapshotField actionFlag;

        SnapshotField selectedItem;

        SnapshotField colorR;
        SnapshotField colorG;
        SnapshotField colorB;

        SnapshotField bulletSnapshot;

        public ClientPlayerSnapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection,
            bool dontAllocateId = false)
            : base(snapshotSystem, otherConnection, dontAllocateId)
        {
            x = AddPrimitiveField<float>();
            y = AddPrimitiveField<float>();
            z = AddPrimitiveField<float>();
            camYaw = AddPrimitiveField<float>();
            camPitch = AddPrimitiveField<float>();

            actionFlag = AddPrimitiveField<ByteFlag>();

            selectedItem = AddPrimitiveField<byte>();

            colorR = AddPrimitiveField<byte>();
            colorG = AddPrimitiveField<byte>();
            colorB = AddPrimitiveField<byte>();

            bulletSnapshot = AddCustomField(BulletSnapshot = new ClientBulletSnapshot());

            // TODO: Figure out why we can't compress ByteFlag's.
            // They won't always send the latest version.
            actionFlag.NeverCompress = true;

            Setup();

            // This snapshot updates so much that we really don't need
            // to store many delta snapshots.
            EnableDeltaCompression(4);
        }

        public override string GetUniqueId()
        {
            return "ClientInput";
        }

        public override void Serialize(NetBuffer buffer)
        {
            ByteFlag actionFlag = new ByteFlag();

            actionFlag.Set(0, IsCrouching);
            actionFlag.Set(1, IsFlashlightVisible);
            actionFlag.Set(2, Reload);
            actionFlag.Set(3, DropIntel);

            this.actionFlag.Value = actionFlag;

            Reload = false;
            DropIntel = false;

            base.Serialize(buffer);
        }

        public override void Deserialize(NetBuffer packet)
        {
            base.Deserialize(packet);

            ByteFlag actionFlag = (ByteFlag)this.actionFlag.Value;

            IsCrouching = actionFlag.Get(0);
            IsFlashlightVisible = actionFlag.Get(1);
            Reload = actionFlag.Get(2);
            DropIntel = actionFlag.Get(3);
        }
    }
}
