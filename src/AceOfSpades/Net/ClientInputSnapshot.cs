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
        public bool Sprint { get; set; }
        public bool Crouch { get; set; }
        public bool Jump { get; set; }
        public bool Walk { get; set; }
        public bool MoveForward { get; set; }
        public bool MoveBackward { get; set; }
        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }

        public bool IsAiming { get; set; }
        public bool Reload { get; set; }
        public bool IsFlashlightVisible { get; set; }
        public bool DropIntel { get; set; }

        public float CameraPitch
        {
            get { return (float)cameraPitch.Value; }
            set { cameraPitch.Value = value; }
        }
        public float CameraYaw
        {
            get { return (float)cameraYaw.Value; }
            set { cameraYaw.Value = value; }
        }

        public byte SelectedItem
        {
            get { return (byte)selectedItem.Value; }
            set { selectedItem.Value = value; }
        }

        public int JumpTimeTicks { get; set; }
        public ushort JumpTimeDelta
        {
            get { return (ushort)jumpTimeDelta.Value; }
            set { jumpTimeDelta.Value = value; }
        }

        public ClientBulletSnapshot BulletSnapshot { get; }

        SnapshotField bulletSnapshot;

        SnapshotField jumpTimeDelta;

        SnapshotField movementFlag;
        SnapshotField actionFlag;
        SnapshotField cameraPitch;
        SnapshotField cameraYaw;
        SnapshotField selectedItem;

        SnapshotField colorR;
        SnapshotField colorG;
        SnapshotField colorB;

        public ClientInputSnapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection,
            bool dontAllocateId = false)
            : base(snapshotSystem, otherConnection, dontAllocateId)
        {
            movementFlag = AddPrimitiveField<ByteFlag>();
            actionFlag = AddPrimitiveField<ByteFlag>();

            cameraPitch = AddPrimitiveField<float>();
            cameraYaw = AddPrimitiveField<float>();

            selectedItem = AddPrimitiveField<byte>();

            colorR = AddPrimitiveField<byte>();
            colorG = AddPrimitiveField<byte>();
            colorB = AddPrimitiveField<byte>();

            bulletSnapshot = AddCustomField(BulletSnapshot = new ClientBulletSnapshot());

            jumpTimeDelta = AddPrimitiveField<ushort>();

            // TODO: Figure out why we can't compress ByteFlag's.
            // They won't always send the latest version.
            movementFlag.NeverCompress = true;
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

        public ByteFlag GetMovementFlag()
        {
            return (ByteFlag)movementFlag.Value;
        }

        public override void Serialize(NetBuffer buffer)
        {
            ByteFlag movementFlag = new ByteFlag();
            ByteFlag actionFlag = new ByteFlag();

            movementFlag.Set(0, Sprint);
            movementFlag.Set(1, Crouch);
            movementFlag.Set(2, Jump);
            movementFlag.Set(3, MoveForward);
            movementFlag.Set(4, MoveBackward);
            movementFlag.Set(5, MoveLeft);
            movementFlag.Set(6, MoveRight);
            movementFlag.Set(7, Walk);

            actionFlag.Set(0, Reload);
            actionFlag.Set(1, IsFlashlightVisible);
            actionFlag.Set(2, IsAiming);
            actionFlag.Set(3, DropIntel);

            this.movementFlag.Value = movementFlag;
            this.actionFlag.Value = actionFlag;

            Reload = false;
            MoveForward = false;
            MoveBackward = false;
            MoveLeft = false;
            MoveRight = false;
            Jump = false;
            DropIntel = false;

            jumpTimeDelta.Value = JumpTimeTicks == int.MinValue ? ushort.MaxValue : (ushort)(Environment.TickCount - JumpTimeTicks);
            JumpTimeTicks = int.MinValue;

            base.Serialize(buffer);
        }

        public override void Deserialize(NetBuffer packet)
        {
            base.Deserialize(packet);

            ByteFlag movementFlag = (ByteFlag)this.movementFlag.Value;
            ByteFlag actionFlag = (ByteFlag)this.actionFlag.Value;

            Sprint = movementFlag.Get(0);
            Crouch = movementFlag.Get(1);
            Jump = movementFlag.Get(2);
            MoveForward = movementFlag.Get(3);
            MoveBackward = movementFlag.Get(4);
            MoveLeft = movementFlag.Get(5);
            MoveRight = movementFlag.Get(6);
            Walk = movementFlag.Get(7);

            Reload = actionFlag.Get(0);
            IsFlashlightVisible = actionFlag.Get(1);
            IsAiming = actionFlag.Get(2);
            DropIntel = actionFlag.Get(3);
        }
    }
}
