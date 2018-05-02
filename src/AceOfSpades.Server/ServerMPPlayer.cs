using AceOfSpades.Characters;
using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Physics;
using System;
using System.Collections.Generic;

/* ServerMPPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class PlayerTransform
    {
        public int Ticks { get; }
        public Vector3 Position { get; }
        public float CameraYaw { get; }
        public float CameraPitch { get; }
        public bool IsGrounded { get; }

        public PlayerTransform(Vector3 position, float camYaw, float camPitch, bool grounded)
        {
            Ticks = Environment.TickCount;
            Position = position;
            CameraYaw = camYaw;
            CameraPitch = camPitch;
            IsGrounded = grounded;
        }

        public PlayerTransform(Vector3 position, float camYaw, float camPitch, bool grounded, int tickCount)
        {
            Ticks = tickCount;
            Position = position;
            CameraYaw = camYaw;
            CameraPitch = camPitch;
            IsGrounded = grounded;
        }
    }

    public class ServerMPPlayer : MPPlayer
    {
        public ClientInputSnapshot ClientInput { get; private set; }
        public int LastBulletDeltaTime { get; private set; }
        public bool HasIntel { get { return Intel != null; } }
        public Intel Intel { get; private set; }

        const int MAX_STORED_TRANSFORMS = 100;
        const float REFRESH_COOLDOWN = 5f;

        float refreshCooldown;

        SimpleCamera camera;
        bool serverFlashlight;
        List<PlayerTransform> playerTransforms;

        Queue<NetworkBullet> bulletsToFire;
        bool forceNextJump;

        public ServerMPPlayer(World world, Vector3 position, Team team)
             : base(null, world, new SimpleCamera(), position, team)
        {
            camera = GetCamera();

            playerTransforms = new List<PlayerTransform>();
            bulletsToFire = new Queue<NetworkBullet>();

            CharacterController.OnCollision += CharacterController_OnCollision;
            
            CreateStarterBackpack();
        }

        public void Refresh()
        {
            if (refreshCooldown <= 0)
            {
                if (Health < MAX_HEALTH)
                {
                    refreshCooldown = REFRESH_COOLDOWN;
                    Health = MAX_HEALTH;
                }

                if (ItemManager.RefillAllGuns())
                    refreshCooldown = REFRESH_COOLDOWN;

                if (NumGrenades < MAX_GRENADES)
                {
                    NumGrenades = MAX_GRENADES;
                    refreshCooldown = REFRESH_COOLDOWN;
                }

                if (NumBlocks < MAX_BLOCKS)
                {
                    NumBlocks = MAX_BLOCKS;
                    refreshCooldown = REFRESH_COOLDOWN;
                }
            }
        }

        public void OnKilled()
        {
            IsEnabled = false;
            DropIntel();
        }

        public override void OnNetworkInstantiated(NetCreatableInfo stateInfo)
        {
            base.OnNetworkInstantiated(stateInfo);

            SnapshotNetComponent snc = AOSServer.Instance.GetComponent<SnapshotNetComponent>();
            ClientInput = new ClientInputSnapshot(snc.SnapshotSystem, stateInfo.Owner, true);
        }

        protected override void Update(float deltaTime)
        {
            if (StateInfo != null)
            {
                IsAiming = ClientInput.IsAiming;
                IsSprinting = CharacterController.IsMoving && CharacterController.DeltaPosition.Length > 0 && !IsAiming
                    && !CharacterController.IsCrouching && !ClientInput.Walk && ClientInput.Sprint;

                // Update item in hand
                ItemManager.Update(false, false, false, false, ClientInput.Reload, deltaTime);

                // Little saftey check
                if (Health > 0)
                {
                    while (bulletsToFire.Count > 0)
                    {
                        // Attempt to fire client bullet
                        NetworkBullet bullet = bulletsToFire.Dequeue();

                        camera.Pitch = bullet.CameraPitch;
                        camera.Yaw = bullet.CameraYaw;
                        camera.Position = bullet.Origin;
                        camera.Update(deltaTime);

                        LastBulletDeltaTime = (Environment.TickCount - bullet.CreatedAt) + bullet.Ticks;

                        ItemManager.TryInvokePrimaryFire();
                    }
                }

                // Move the character
                Vector3 move = Vector3.Zero;
                if (ClientInput.MoveForward) move.Z -= 1;
                if (ClientInput.MoveBackward) move.Z += 1;
                if (ClientInput.MoveLeft) move.X += 1;
                if (ClientInput.MoveRight) move.X -= 1;

                UpdateMoveVector(move, ClientInput.Jump, ClientInput.Sprint, IsWalking = ClientInput.Walk);

                if (forceNextJump)
                {
                    forceNextJump = false;
                    CharacterController.MoveVector.SetY(JUMP_POWER);
                }

                if (ClientInput.Crouch)
                    CharacterController.IsCrouching = true;
                else if (CharacterController.IsCrouching)
                    CharacterController.TryUncrouch(World);

                if (ClientInput.DropIntel)
                    DropIntel();

                if (Transform.Position.Y < -200)
                    this.Damage(100, "The Void");

                if (refreshCooldown > 0)
                    refreshCooldown -= deltaTime;

                // Update our "fake" camera
                camera.Position = Transform.Position + new Vector3(0, Size.Y / 2f - 1.1f, 0);
                camera.Update(deltaTime);

                StoreCurrentTransform();
            }

            base.Update(deltaTime);
        }

        private void CharacterController_OnCollision(object sender, PhysicsBodyComponent e)
        {
            if (Intel == null)
            {
                Intel intel = e.GameObject as Intel;

                if (intel != null)
                {
                    if (intel.RequestOwnership(this))
                    {
                        DashCMD.WriteLine("[ServerMPPlayer] Intel has been picked up!", ConsoleColor.Green);
                        Intel = intel;
                    }
                }
            }
        }

        public void DropIntel()
        {
            if (Intel != null)
            {
                Intel.Drop();
                Intel = null;
                DashCMD.WriteLine("[ServerMPPlayer] Intel has been dropped!", ConsoleColor.Green);
            }
        }

        public PlayerTransform RollbackTransform(int timeFrame, bool suppressLog = false)
        {
            PlayerTransform pt1 = null, pt2 = null;
            for (int i = 0; i < playerTransforms.Count; i++)
            {
                PlayerTransform pt = playerTransforms[i];
                int tickOff = Math.Abs(pt.Ticks - timeFrame);

                // Don't process anything more than a second off
                if (tickOff > 1000)
                    continue;

                if (pt1 == null || tickOff < Math.Abs(pt1.Ticks - timeFrame))
                    pt1 = pt;
            }

            for (int i = 0; i < playerTransforms.Count; i++)
            {
                PlayerTransform pt = playerTransforms[i];
                if (pt == pt1)
                    continue;

                int tickOff = Math.Abs(pt.Ticks - timeFrame);

                // Don't process anything more than a second off
                if (tickOff > 1000)
                    continue;

                if (pt2 == null || tickOff < Math.Abs(pt2.Ticks - timeFrame))
                    pt2 = pt;
            }

            if (pt1 != null && pt2 != null)
            {
                if (pt2.Ticks > pt1.Ticks)
                {
                    PlayerTransform temp = pt2;
                    pt2 = pt1;
                    pt1 = temp;
                }

                // Interpolate
                float timeI = pt1.Ticks == pt2.Ticks ? 0f : (float)(timeFrame - pt2.Ticks) / (pt1.Ticks - pt2.Ticks);
                //timeI = MathHelper.Clamp(timeI, 0f, 1f);

                if (DashCMD.GetCVar<bool>("sv_hitboxes") && !suppressLog)
                    DashCMD.WriteImportant("[RB] Rolling back transform by {0}%. [timeFrame: {3}, pt2: {1}, pt1: {2}]", 
                        timeI * 100, pt2.Ticks, pt1.Ticks, timeFrame);

                Vector3 position = Interpolation.Lerp(pt2.Position, pt1.Position, timeI);
                float camPitch = Interpolation.LerpDegrees(pt2.CameraPitch, pt1.CameraPitch, timeI);
                float camYaw = Interpolation.LerpDegrees(pt2.CameraYaw, pt1.CameraYaw, timeI);

                return new PlayerTransform(position, camYaw, camPitch, 
                    timeI < 0.5f ? pt2.IsGrounded : pt1.IsGrounded, timeFrame);
            }
            else if (pt1 != null && pt2 == null)
                // Take pt1
                return pt1;
            else
                // Take current
                return new PlayerTransform(Transform.Position, camera.Yaw, camera.Pitch, 
                    CharacterController.IsGrounded, Environment.TickCount);
        }

        void StoreCurrentTransform()
        {
            PlayerTransform snapshot = new PlayerTransform(Transform.Position, camera.Yaw, camera.Pitch, 
                CharacterController.IsGrounded);
            playerTransforms.Add(snapshot);

            if (playerTransforms.Count > MAX_STORED_TRANSFORMS)
                playerTransforms.RemoveAt(0);
        }

        public void OnServerInbound()
        {
            // Apply immediate changes
            ItemManager.Equip(ClientInput.SelectedItem);

            serverFlashlight = ClientInput.IsFlashlightVisible;
            camera.Yaw = ClientInput.CameraYaw;
            camera.Pitch = ClientInput.CameraPitch;

            NetworkBullet[] clientBullets = ClientInput.BulletSnapshot.GetBullets();
            for (int i = 0; i < clientBullets.Length; i++)
                bulletsToFire.Enqueue(clientBullets[i]);

            // If the client jumped, and we have them in the air already, check to see  
            // if (in the client's time) the jump is valid (according to our records).
            if (ClientInput.Jump && !CharacterController.IsGrounded && ClientInput.JumpTimeDelta != ushort.MaxValue)
            {
                int rollbackTime = StateInfo.Owner.Stats.Ping + ClientInput.JumpTimeDelta;
                PlayerTransform transform = RollbackTransform(Environment.TickCount - rollbackTime, true);
                if (transform.IsGrounded)
                {
                    Transform.Position.Y = transform.Position.Y;
                    CharacterController.Velocity.Y = 0;
                    forceNextJump = true;
                    //DashCMD.WriteLine("Applied fix jump!");
                }
            }
        }

        public void OnServerOutbound(PlayerSnapshot snapshot)
        {
            if (snapshot.NetId != StateInfo.Id)
                throw new Exception(
                    string.Format("PlayerSnapshot initId does not match ServerMPPlayer's netId! initId {0} != netId {1}",
                    snapshot.NetId, StateInfo.Id));

            snapshot.NetId = StateInfo.Id;

            snapshot.X = Transform.Position.X;
            snapshot.Y = Transform.Position.Y;
            snapshot.Z = Transform.Position.Z;

            snapshot.IsCrouching = CharacterController.IsCrouching;
            snapshot.IsGrounded = CharacterController.IsGrounded;
            
            snapshot.IsFlashlightOn = ClientInput.IsFlashlightVisible;

            if (snapshot.IsOwner)
            {
                if (ItemManager.SelectedItem != null)
                {
                    Gun gun = ItemManager.SelectedItem as Gun;
                    if (gun != null)
                    {
                        snapshot.CurrentMag = (byte)gun.CurrentMag;
                        snapshot.StoredAmmo = (ushort)gun.StoredAmmo;
                    }
                }

                snapshot.Health = Health;
                snapshot.NumBlocks = (ushort)NumBlocks;
                snapshot.NumGrenades = (byte)NumGrenades;
            }
            else
            {
                snapshot.CamYaw = ClientInput.CameraYaw;
                snapshot.CamPitch = ClientInput.CameraPitch;

                snapshot.SelectedItem = (byte)ItemManager.SelectedItemIndex;

                snapshot.TimesShot = (byte)ItemManager.MuzzleFlashIterations;
                ItemManager.MuzzleFlashIterations = 0;
            }
        }
    }
}
