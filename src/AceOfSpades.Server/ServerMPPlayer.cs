using AceOfSpades.Characters;
using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Animation;
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
    /// <summary>
    /// A snapshot of a player's transform.
    /// </summary>
    public class PlayerTransform
    {
        public int Ticks { get; }
        public Vector3 Position { get; }
        public float CameraYaw { get; }
        public float CameraPitch { get; }

        public PlayerTransform(Vector3 position, float camYaw, float camPitch)
        {
            Ticks = Environment.TickCount;
            Position = position;
            CameraYaw = camYaw;
            CameraPitch = camPitch;
        }

        public PlayerTransform(Vector3 position, float camYaw, float camPitch, int tickCount)
        {
            Ticks = tickCount;
            Position = position;
            CameraYaw = camYaw;
            CameraPitch = camPitch;
        }
    }

    public class ServerMPPlayer : MPPlayer
    {
        public ClientPlayerSnapshot ClientSnapshot { get; private set; }
        public int LastBulletDeltaTime { get; private set; }
        public bool HasIntel { get { return Intel != null; } }
        public Intel Intel { get; private set; }

        public float KillStreak;

        const int MAX_STORED_TRANSFORMS = 100;
        const float REFRESH_COOLDOWN = 5f;

        float refreshCooldown;

        SimpleCamera camera;
        List<PlayerTransform> playerTransforms;

        Queue<NetworkBullet> bulletsToFire;

        Vector3Anim movementAnim;

        public ServerMPPlayer(World world, Vector3 position, Team team)
             : base(null, world, new SimpleCamera(), position, team)
        {
            camera = GetCamera();

            playerTransforms = new List<PlayerTransform>();
            bulletsToFire = new Queue<NetworkBullet>();
            movementAnim = new Vector3Anim();

            // Let client's handle movement. 
            // - We don't need to bother with terrain collision, only entity collision.
            // - Gravity shouldn't be bother with either.
            CharacterController.CanCollideWithTerrain = false;
            CharacterController.IsAffectedByGravity = false;

            CharacterController.OnCollision += CharacterController_OnCollision;
            
            CreateStarterBackpack();
        }

        /// <summary>
        /// Refills the player's ammo and blocks if the refresh cooldown has passed.
        /// </summary>
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
            ClientSnapshot = new ClientPlayerSnapshot(snc.SnapshotSystem, stateInfo.Owner, true);
        }

        protected override void Update(float deltaTime)
        {
            if (StateInfo != null)
            {
                // Sync our server transform with the client's reported position
                Vector3 clientPosition = new Vector3(ClientSnapshot.X, ClientSnapshot.Y, ClientSnapshot.Z);
                if (movementAnim.Target != clientPosition)
                    movementAnim.SetTarget(clientPosition);

                movementAnim.Step(deltaTime * 30f);

                Transform.Position = movementAnim.Value;

                // Sync our server camera with the client's
                camera.Yaw = ClientSnapshot.CamYaw;
                camera.Pitch = ClientSnapshot.CamPitch;

                // Update item in hand 
                ItemManager.Update(false, false, false, false, ClientSnapshot.Reload, deltaTime);

                // Only process bullets if the player is alive
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

                // Handle the client dropping intel
                if (ClientSnapshot.DropIntel)
                    DropIntel();

                // Handle the player falling off the map
                if (Transform.Position.Y < -200)
                    this.Damage(100, "The Void");

                // Process refresh cooldown
                if (refreshCooldown > 0)
                    refreshCooldown -= deltaTime;

                // Update our "fake" camera
                camera.Position = Transform.Position + new Vector3(0, Size.Y / 2f - 1.1f, 0);
                camera.Update(deltaTime);

                // Save the current transform for future rollbacks
                StoreCurrentTransform();
            }

            base.Update(deltaTime);
        }

        private void CharacterController_OnCollision(object sender, PhysicsBodyComponent e)
        {
            if (Intel == null)
            {
                if (e.GameObject is Intel intel)
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
                Intel.Drop(yeet: ClientSnapshot.DropIntel);
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

                return new PlayerTransform(position, camYaw, camPitch, timeFrame);
            }
            else if (pt1 != null && pt2 == null)
                // Take pt1
                return pt1;
            else
                // Take current
                return new PlayerTransform(Transform.Position, camera.Yaw, camera.Pitch, Environment.TickCount);
        }

        void StoreCurrentTransform()
        {
            PlayerTransform snapshot = new PlayerTransform(Transform.Position, camera.Yaw, camera.Pitch);
            playerTransforms.Add(snapshot);

            if (playerTransforms.Count > MAX_STORED_TRANSFORMS)
                playerTransforms.RemoveAt(0);
        }

        public void OnServerInbound()
        {
            // Equip the weapon the client has equipped
            ItemManager.Equip(ClientSnapshot.SelectedItem);

            // Queue all bullets fired client-side
            NetworkBullet[] clientBullets = ClientSnapshot.BulletSnapshot.GetBullets();
            for (int i = 0; i < clientBullets.Length; i++)
                bulletsToFire.Enqueue(clientBullets[i]);
        }

        public void OnServerOutbound(PlayerSnapshot snapshot)
        {
            if (snapshot.NetId != StateInfo.Id)
            {
                throw new Exception(
                    string.Format("PlayerSnapshot initId does not match ServerMPPlayer's netId! initId {0} != netId {1}",
                    snapshot.NetId, StateInfo.Id));
            }

            snapshot.NetId = StateInfo.Id;

            snapshot.X = Transform.Position.X;
            snapshot.Y = Transform.Position.Y;
            snapshot.Z = Transform.Position.Z;

            snapshot.IsCrouching = ClientSnapshot.IsCrouching; 
            snapshot.IsFlashlightOn = ClientSnapshot.IsFlashlightVisible;

            snapshot.SelectedItem = (byte)ItemManager.SelectedItemIndex;

            if (snapshot.IsOwner)
            {
                // Owner-only data
                if (ItemManager.SelectedItem != null)
                {
                    if (ItemManager.SelectedItem is Gun gun)
                    {
                        snapshot.CurrentMag = (byte)gun.CurrentMag;
                        snapshot.StoredAmmo = (ushort)gun.StoredAmmo;
                    }
                }

                snapshot.Health = Health;
                snapshot.NumBlocks = (ushort)NumBlocks;
                snapshot.NumGrenades = (byte)NumGrenades;
                snapshot.NumMelons = (byte)NumMelons;
            }
            else
            {
                // Replicated-only data
                snapshot.CamYaw = ClientSnapshot.CamYaw;
                snapshot.CamPitch = ClientSnapshot.CamPitch;

                snapshot.TimesShot = (byte)ItemManager.MuzzleFlashIterations;
            }

            ItemManager.MuzzleFlashIterations = 0;
        }
    }
}
