using AceOfSpades.Characters;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Audio;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Physics;
using System;

/* SPPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public class SPPlayer : Player
    {
        public override bool IsRenderingThirdperson
        {
            get { return thirdPerson; }
            set
            {
                if (value == thirdPerson)
                    return;

                if (value)
                    camera.SetMode(CameraMode.ArcBall);
                else
                    camera.SetMode(CameraMode.FPS);

                thirdPerson = value;
            }
        }

        const float REFRESH_COOLDOWN = 1f;

        float refreshCooldown;

        CameraFX camfx;
        Camera camera;
        bool thirdPerson;
        bool lastGrounded = true;

        Vector3 startLocation;
        Intel intel;

        bool isDisposed;

        readonly AudioSource hitAudioSource;
        readonly AudioSource flashlightAudioSource;
        readonly AudioSource jumpAudioSource;
        readonly AudioSource landAudioSource;
        readonly CyclicAudioSource walkingAudioSource;
        readonly CyclicAudioSource runningAudioSource;

        public SPPlayer(MasterRenderer renderer, World world, Camera camera, Vector3 position, Team team) 
            : base(renderer, world, camera, position, team)
        {
            this.camera = camera;
            camfx = new CameraFX(this, camera);
            camera.SmoothCamera = true;

            startLocation = position;

            ItemManager.SetItems(new Item[]
            {
                new Rifle(ItemManager, renderer),
                new SMG(ItemManager, renderer),
                new Shotgun(ItemManager, renderer),
                new Grenade(ItemManager, renderer),
                new Spade(ItemManager, renderer),
                new BlockItem(ItemManager, renderer),
                new MelonLauncher(ItemManager, renderer)
            }, 5);

            NumMelons = MAX_MELONS;

            CharacterController.OnCollision += CharacterController_OnCollision;

            AudioBuffer hitAudioBuffer = AssetManager.LoadSound("Impacts/hit-player-local.wav");

            if (hitAudioBuffer != null)
            {
                hitAudioSource = new AudioSource(hitAudioBuffer);
                hitAudioSource.IsSourceRelative = true;
                hitAudioSource.Gain = 0.2f;
            }

            AudioBuffer flashlightAudioBuffer = AssetManager.LoadSound("Player/flashlight.wav");

            if (flashlightAudioBuffer != null)
            {
                flashlightAudioSource = new AudioSource(flashlightAudioBuffer);
                flashlightAudioSource.IsSourceRelative = true;
                flashlightAudioSource.Gain = 0.2f;
            }

            AudioBuffer jumpAudioBuffer = AssetManager.LoadSound("Player/jump.wav");

            if (jumpAudioBuffer != null)
            {
                jumpAudioSource = new AudioSource(jumpAudioBuffer);
                jumpAudioSource.IsSourceRelative = true;
                jumpAudioSource.Gain = 0.2f;
            }

            AudioBuffer landAudioBuffer = AssetManager.LoadSound("Player/land.wav");

            if (landAudioBuffer != null)
            {
                landAudioSource = new AudioSource(landAudioBuffer);
                landAudioSource.IsSourceRelative = true;
                landAudioSource.Gain = 0.2f;
            }

            walkingAudioSource = new CyclicAudioSource("Player/footstep.wav", 8, 0f);
            runningAudioSource = new CyclicAudioSource("Player/run.wav", 12, 0f);
        }

        private void CharacterController_OnCollision(object sender, PhysicsBodyComponent e)
        {
            if (this.intel == null)
            {
                Intel intel = e.GameObject as Intel;

                if (intel != null)
                {
                    if (intel.RequestOwnership(this))
                    {
                        DashCMD.WriteLine("[SPPlayer] Picked up the intel", ConsoleColor.Green);
                        this.intel = intel;
                        intel.IsIconVisible = false;
                    }
                }
            }

            CommandPost commandPost = e.GameObject as CommandPost;

            if (commandPost != null)
            {
                if (commandPost.Team == Team)
                {
                    Refresh();

                    if (intel != null)
                    {
                        intel.Return();
                        intel.IsIconVisible = true;
                        intel = null;
                    }
                }
            }
        }

        /// <summary>
        /// Refills the player's ammo and blocks if the refresh cooldown has passed.
        /// </summary>
        void Refresh()
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

                if (NumMelons < MAX_MELONS)
                {
                    NumMelons = MAX_MELONS;
                    refreshCooldown = REFRESH_COOLDOWN;
                }
            }
        }

        void DropIntel()
        {
            if (intel != null)
            {
                intel.Drop(yeet: true);
                intel.IsIconVisible = true;
                intel = null;
                DashCMD.WriteLine("[SPPlayer] Dropped the intel", ConsoleColor.Green);
            }
        }

        public void ApplyRecoil(float verticalRecoil, float horizontalRecoil, float modelKickback)
        {
            camfx.Recoil(horizontalRecoil, verticalRecoil);
            Viewbob.ApplyKickback(modelKickback);
        }

        public void ShakeCamera(float duration, float factor)
        {
            camfx.ShakeCamera(duration, factor);
        }

        public void AttachCamera()
        {
            camera.SetMode(thirdPerson ? CameraMode.ArcBall : CameraMode.FPS);
            camera.LockedToTransform = Transform;

            Camera.Active.HoldM2ToLook = false;
            Camera.Active.SmoothCamera = false;
            Input.IsCursorLocked = true;
            Input.IsCursorVisible = false;
        }

        protected override void OnDamaged(float damage)
        {
            hitAudioSource?.Play();

            if (!camfx.IsShaking)
                camfx.ShakeCamera(0.2f, 0.05f);

            if (Health <= 0)
                DropIntel();

            base.OnDamaged(damage);
        }

        protected override void OnJump()
        {
            jumpAudioSource?.Play();

            base.OnJump();
        }

        protected override void Update(float deltaTime)
        {
            if (Input.GetControlDown("DropIntel"))
                DropIntel();

            // Check if aiming
            IsAiming = AllowUserInput && ItemManager.SelectedItem != null
                && (ItemManager.SelectedItem.Type.HasFlag(ItemType.Gun)
                    || ItemManager.SelectedItem.Type.HasFlag(ItemType.MelonLauncher))
                && ItemManager.SelectedItem.CanSecondaryFire()
                ? Input.GetControl("SecondaryFire") : false;

            IsSprinting = AllowUserInput && CharacterController.IsMoving && CharacterController.DeltaPosition.Length > 0 
                && !IsAiming && !CharacterController.IsCrouching && !Input.GetControl("Walk") && Input.GetControl("Sprint");

            // Update the currently selected item
            if (AllowUserInput)
                ItemManager.Update(
                    Input.GetControlDown("PrimaryFire"), Input.GetControl("PrimaryFire"),
                    Input.GetControlDown("SecondaryFire"), Input.GetControl("SecondaryFire"),
                    Input.GetControlDown("Reload"), deltaTime);
            else
                ItemManager.Update(false, false, false, false, false, deltaTime);

            // Move the player
            Vector3 move = Vector3.Zero;
            if (AllowUserInput)
            {
                if (Input.GetControl("MoveForward")) move.Z -= 1;
                if (Input.GetControl("MoveBackward")) move.Z += 1;
                if (Input.GetControl("MoveLeft")) move.X -= 1;
                if (Input.GetControl("MoveRight")) move.X += 1;

                UpdateMoveVector(move, Input.GetControl("Jump"), Input.GetControl("Sprint"), IsWalking = Input.GetControl("Walk"));
            }
            else
                UpdateMoveVector(move, false, false, false);

            if (Input.GetControlDown("Crouch") && AllowUserInput)
                CharacterController.IsCrouching = true;
            else if (Input.GetControlUp("Crouch")
                || (!Input.GetControl("Crouch") && AllowUserInput) && CharacterController.IsCrouching)
                CharacterController.TryUncrouch(World);

            // Toggle thirdperson, mouse, and flashlight
            if (AllowUserInput)
            {
                if (Input.GetKeyDown(Key.C))
                    IsRenderingThirdperson = !IsRenderingThirdperson;

                if (Input.GetControlDown("ToggleFlashlight"))
                {
                    flashlight.Visible = !flashlight.Visible;
                    flashlightAudioSource?.Play();
                }
            }

            // Handle landing
            if (CharacterController.IsGrounded && !lastGrounded)
            {
                landAudioSource?.Play();
            }

            // Handle walking/running
            walkingAudioSource.IsPlaying = CharacterController.IsGrounded && CharacterController.IsMoving && !IsSprinting;
            walkingAudioSource.IterationLength = (1f / Viewbob.GetSpeed()) * 2f;
            walkingAudioSource.Update(deltaTime);

            runningAudioSource.IsPlaying = CharacterController.IsGrounded && CharacterController.IsMoving && IsSprinting;
            runningAudioSource.IterationLength = (1f / Viewbob.GetSpeed()) * 2f;
            runningAudioSource.Update(deltaTime);

            // Ensure the firstperson offset is correct (this changes when crouching)
            camera.FirstPersonLockOffset = new Vector3(0, Size.Y / 2f - 1.1f, 0);

            // Update the flashlight
            flashlight.Position = IsRenderingThirdperson 
                ? Transform.Position + Camera.Active.FirstPersonLockOffset 
                : camera.Position;
            flashlight.Direction = -camera.LookVector;

            // Store grounded state
            lastGrounded = CharacterController.IsGrounded;

            // Process refresh cooldown
            if (refreshCooldown > 0)
                refreshCooldown -= deltaTime;

            base.Update(deltaTime);

            // Update camera effects
            camfx.Update(deltaTime);

            // Update the viewbob
            Viewbob.Update(deltaTime);

            if (Transform.Position.Y < -200)
                Transform.Position = startLocation;
        }

        protected override void Draw()
        {
            // Render world-model
            if (!IsRenderingThirdperson)
                Renderer.OnlyRenderFor = RenderPass.Shadow;
            else
                Renderer.OnlyRenderFor = null;

            // Setup world matrix
            Renderer.WorldMatrix = Matrix4.CreateScale(VoxelObject.MeshScale)
                * Matrix4.CreateRotationY(MathHelper.ToRadians(-Camera.Active.Yaw))
                * Matrix4.CreateTranslation(Transform.Position);

            // Draw model
            if (DrawCollider)
                debugRenderer.Batch(CharacterController.GetCollider());

            // Render item in hand
            ItemManager.Draw(entRenderer, Viewbob);

            // Apply camera effects (yes this has to be last)
            camfx.Apply();
            base.Draw();
        }

        public override void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                hitAudioSource?.Dispose();
                flashlightAudioSource?.Dispose();
                jumpAudioSource?.Dispose();
                landAudioSource?.Dispose();
                walkingAudioSource.Dispose();
                runningAudioSource.Dispose();
            }

            base.Dispose();
        }
    }
}
