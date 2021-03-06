﻿using Dash.Net;
using System;

/* PlayerSnapshot.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// Contains information describing a character from
    /// the server's point of view.
    /// </summary>
    public class PlayerSnapshot : Snapshot
    {
        public ushort NetId
        {
            get { return (ushort)netId.Value; }
            set { netId.Value = value; }
        }

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

        public bool IsFlashlightOn { get; set; }
        public bool IsReloading { get; set; }

        public bool IsCrouching { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsMoving { get; set; }
        public bool IsAiming { get; set; }
        public bool IsGrounded { get; set; }
        public bool IsJumping { get; set; }

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

        public byte SelectedItem
        {
            get { return (byte)selectedItem.Value; }
            set { selectedItem.Value = value; }
        }

        public byte CurrentMag
        {
            get { return (byte)currentMag.Value; }
            set { currentMag.Value = value; }
        }

        public ushort StoredAmmo
        {
            get { return (ushort)storedAmmo.Value; }
            set { storedAmmo.Value = value; }
        }

        public byte TimesShot
        {
            get { return timesShot.Iterations; }
            set { timesShot.Activate(value); }
        }

        public float Health
        {
            get { return (float)health.Value; }
            set { health.Value = value; }
        }

        public ushort NumBlocks
        {
            get { return (ushort)numBlocks.Value; }
            set { numBlocks.Value = value; }
        }

        public byte NumGrenades
        {
            get { return (byte)numGrenades.Value; }
            set { numGrenades.Value = value; }
        }

        public byte NumMelons
        {
            get { return (byte)numMelons.Value; }
            set { numMelons.Value = value; }
        }

        public byte HitEnemy
        {
            get { return hitEnemy.Iterations; }
            set { hitEnemy.Activate(value); }
        }

        public bool IsOwner { get; }
        public HitFeedbackSnapshot HitFeedbackSnapshot { get; }

        SnapshotField netId;
        SnapshotField x;
        SnapshotField y;
        SnapshotField z;
        SnapshotField camYaw;
        SnapshotField camPitch;
        SnapshotField selectedItem;

        SnapshotField stateFlag;
        SnapshotField movementFlag;

        SnapshotField health;
        SnapshotField numBlocks;
        SnapshotField numGrenades;
        SnapshotField numMelons;

        SnapshotField currentMag;
        SnapshotField storedAmmo;
        Trigger timesShot;
        Trigger hitEnemy;

        ushort initId;

        public PlayerSnapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection, bool isOwner, ushort id, 
            bool dontAllocateId = false)
            : base(snapshotSystem, otherConnection, dontAllocateId)
        {
            initId = id;
            IsOwner = isOwner;

            netId = AddPrimitiveField(id);

            selectedItem = AddPrimitiveField<byte>();

            if (isOwner)
            {
                currentMag = AddPrimitiveField<byte>();
                storedAmmo = AddPrimitiveField<ushort>();

                health = AddPrimitiveField<float>();

                numBlocks = AddPrimitiveField<ushort>();
                numGrenades = AddPrimitiveField<byte>();
                numMelons = AddPrimitiveField<byte>();

                AddCustomField(HitFeedbackSnapshot = new HitFeedbackSnapshot());

                hitEnemy = (Trigger)AddTrigger().Value;
            }
            else
            {
                x = AddPrimitiveField<float>();
                y = AddPrimitiveField<float>();
                z = AddPrimitiveField<float>();

                stateFlag = AddPrimitiveField<ByteFlag>();
                stateFlag.NeverCompress = true;

                movementFlag = AddPrimitiveField<ByteFlag>();
                movementFlag.NeverCompress = true;

                camYaw = AddPrimitiveField<float>();
                camPitch = AddPrimitiveField<float>();

                timesShot = (Trigger)AddTrigger().Value;
            }

            Setup();
            EnableDeltaCompression(32);
        }

        public override string GetUniqueId()
        {
            return string.Format("PlayerData_{0}", initId);
        }

        public override void Serialize(NetBuffer buffer)
        {
            if (this.stateFlag != null)
            {
                ByteFlag stateFlag = new ByteFlag();
                stateFlag.Set(0, IsFlashlightOn);
                stateFlag.Set(1, IsReloading);

                this.stateFlag.Value = stateFlag;
            }

            if (this.movementFlag != null)
            {
                ByteFlag movementFlag = new ByteFlag();

                movementFlag.Set(0, IsCrouching);
                movementFlag.Set(1, IsSprinting);
                movementFlag.Set(2, IsMoving);
                movementFlag.Set(3, IsAiming);
                movementFlag.Set(4, IsGrounded);
                movementFlag.Set(5, IsJumping);

                this.movementFlag.Value = movementFlag;
            }

            base.Serialize(buffer);
        }

        public override void Deserialize(NetBuffer buffer)
        {
            base.Deserialize(buffer);

            if (this.stateFlag != null)
            {
                ByteFlag stateFlag = (ByteFlag)this.stateFlag.Value;
                IsFlashlightOn = stateFlag.Get(0);
                IsReloading = stateFlag.Get(1);
            }

            if (this.movementFlag != null)
            {
                ByteFlag movementFlag = (ByteFlag)this.movementFlag.Value;

                IsCrouching = movementFlag.Get(0);
                IsSprinting = movementFlag.Get(1);
                IsMoving = movementFlag.Get(2);
                IsAiming = movementFlag.Get(3);
                IsGrounded = movementFlag.Get(4);
                IsJumping = movementFlag.Get(5);
            }

            if (NetId != initId)
            {
                throw new Exception(string.Format(
                    "PlayerSnapshot id mismatch! Server had different id than client! (NetId, initId) {0} != {1}", NetId, initId));
            }
        }
    }
}
