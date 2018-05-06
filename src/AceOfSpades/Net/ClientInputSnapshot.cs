using Dash.Net;
using System;

/* ClientInputSnapshot.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// Snapshot containing all input from a client.
    /// </summary>
    public class ClientInputSnapshot : Snapshot
    {
        public bool IsAiming { get; set; }
        public bool Reload { get; set; }
        public bool IsFlashlightVisible { get; set; }
        public bool DropIntel { get; set; }

        public byte SelectedItem
        {
            get { return (byte)selectedItem.Value; }
            set { selectedItem.Value = value; }
        }

        public ClientBulletSnapshot BulletSnapshot { get; }
        public ClientMovementSnapshot MovementSnapshot { get; }

        SnapshotField bulletSnapshot;
        SnapshotField movementSnapshot;

        SnapshotField actionFlag;
        SnapshotField selectedItem;

        SnapshotField colorR;
        SnapshotField colorG;
        SnapshotField colorB;

        public ClientInputSnapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection,
            bool dontAllocateId = false)
            : base(snapshotSystem, otherConnection, dontAllocateId)
        {
            actionFlag = AddPrimitiveField<ByteFlag>();

            selectedItem = AddPrimitiveField<byte>();

            colorR = AddPrimitiveField<byte>();
            colorG = AddPrimitiveField<byte>();
            colorB = AddPrimitiveField<byte>();

            bulletSnapshot = AddCustomField(BulletSnapshot = new ClientBulletSnapshot());
            movementSnapshot = AddCustomField(MovementSnapshot = new ClientMovementSnapshot());

            // TODO: Figure out why we can't compress ByteFlag's.
            // They won't always send the latest version.
            //movementFlag.NeverCompress = true;
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

            actionFlag.Set(0, Reload);
            actionFlag.Set(1, IsFlashlightVisible);
            actionFlag.Set(2, IsAiming);
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

            Reload = actionFlag.Get(0);
            IsFlashlightVisible = actionFlag.Get(1);
            IsAiming = actionFlag.Get(2);
            DropIntel = actionFlag.Get(3);
        }
    }
}
