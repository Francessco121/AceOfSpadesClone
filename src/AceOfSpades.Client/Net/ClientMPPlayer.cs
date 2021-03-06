﻿using AceOfSpades.Characters;
using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Audio;
using Dash.Engine.Graphics;

/* ClientMPPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class ClientMPPlayer : ClientPlayer
    {
        public ClientPlayerSnapshot ClientSnapshot { get; private set; }

        SimpleCamera camera;
        CameraFX camfx;

        bool lastGrounded = true;
        bool jumped = false;
        bool isDisposed;

        readonly AudioSource hitAudioSource;
        readonly AudioSource flashlightAudioSource;
        readonly AudioSource jumpAudioSource;
        readonly AudioSource landAudioSource;
        readonly CyclicAudioSource walkingAudioSource;
        readonly CyclicAudioSource runningAudioSource;

        public ClientMPPlayer(MasterRenderer renderer, World world, Camera camera, Vector3 position, Team team)
            : base(renderer, world, camera, position, team)
        {
            this.camera = camera;
            camfx = new CameraFX(this, camera);

            // Setup ClientInput Snapshot
            AOSClient client = AOSClient.Instance;
            SnapshotNetComponent snc = client.GetComponent<SnapshotNetComponent>();
            SnapshotSystem ss = snc.SnapshotSystem;
            ClientSnapshot = new ClientPlayerSnapshot(ss, client.ServerConnection);

            Camera.Active.FOV = Camera.Active.DefaultFOV;
            Camera.Active.FPSMouseSensitivity = Camera.Active.DefaultFPSMouseSensitivity;
            Camera.Active.ArcBallMouseSensitivity = Camera.Active.DefaultArcBallMouseSensitivity;

            CreateStarterBackpack();

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

        public void OnKilled()
        {
            // Setting the health to zero will allow the HUD to see the change.
            // Once the player is dead on the server it won't send health updates
            // so this gets around it.
            Health = 0;
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

        protected override void Update(float deltaTime)
        {
            if (StateInfo != null)
            {
                // Get user input
                bool inputM1 = AllowUserInput && Input.GetControlDown("PrimaryFire");
                bool inputM1Held = AllowUserInput && Input.GetControl("PrimaryFire");
                bool inputM2 = AllowUserInput && Input.GetControlDown("SecondaryFire");
                bool inputM2Held = AllowUserInput && Input.GetControl("SecondaryFire");
                bool inputReload = AllowUserInput && Input.GetControlDown("Reload");
                bool inputJump = AllowUserInput && Input.GetControl("Jump");
                bool inputSprint = AllowUserInput && Input.GetControl("Sprint");
                bool inputWalk = AllowUserInput && Input.GetControl("Walk");
                bool inputCrouch = AllowUserInput && Input.GetControl("Crouch");
                bool inputCrouchUp = AllowUserInput && Input.GetControlUp("Crouch");
                bool inputDropIntel = AllowUserInput && Input.GetControlDown("DropIntel");

                // Check if aiming
                IsAiming = ItemManager.SelectedItem != null
                    && (ItemManager.SelectedItem.Type.HasFlag(ItemType.Gun)
                        || ItemManager.SelectedItem.Type.HasFlag(ItemType.MelonLauncher))
                    && ItemManager.SelectedItem.CanSecondaryFire()
                    ? inputM2Held : false;

                IsSprinting = CharacterController.IsMoving && CharacterController.DeltaPosition.Length > 0 && !IsAiming 
                    && !CharacterController.IsCrouching && !inputWalk && inputSprint;

                // Update the selected item
                ItemManager.Update(inputM1, inputM1Held, inputM2, inputM2Held, inputReload, deltaTime);

                // Move the character
                Vector3 move = Vector3.Zero;
                if (AllowUserInput)
                {
                    if (Input.GetControl("MoveForward")) move.Z -= 1;
                    if (Input.GetControl("MoveBackward")) move.Z += 1;
                    if (Input.GetControl("MoveLeft")) move.X -= 1;
                    if (Input.GetControl("MoveRight")) move.X += 1;
                }

                UpdateMoveVector(move, inputJump, inputSprint, IsWalking = inputWalk);

                if (inputCrouch)
                    CharacterController.IsCrouching = true;
                else if (inputCrouchUp || !inputCrouch && CharacterController.IsCrouching)
                    CharacterController.TryUncrouch(World);

                // Toggle the mouse and flashlight
                if (AllowUserInput)
                {
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

                // Update viewbob
                Viewbob.Update(deltaTime);

                // Ensure camera firstperson offset is accurate (changes when crouched)
                Camera.Active.FirstPersonLockOffset = new Vector3(0, Size.Y / 2f - 1.1f, 0);

                // Update the camera effects
                camfx.Update(deltaTime);

                // Update flashlight transformation
                flashlight.Position = IsRenderingThirdperson
                    ? Transform.Position + Camera.Active.FirstPersonLockOffset
                    : Camera.Active.Position;
                flashlight.Direction = -Camera.Active.LookVector;

                // Update the snapshot
                ClientSnapshot.X = Transform.Position.X;
                ClientSnapshot.Y = Transform.Position.Y;
                ClientSnapshot.Z = Transform.Position.Z;
                ClientSnapshot.CamYaw = camera.Yaw;
                ClientSnapshot.CamPitch = camera.Pitch;

                ClientSnapshot.IsFlashlightVisible = flashlight.Visible;
                ClientSnapshot.Reload = inputReload || ClientSnapshot.Reload;
                ClientSnapshot.DropIntel = inputDropIntel || ClientSnapshot.DropIntel;

                ClientSnapshot.IsCrouching = CharacterController.IsCrouching;
                ClientSnapshot.IsSprinting = IsSprinting;
                ClientSnapshot.IsMoving = CharacterController.IsMoving;
                ClientSnapshot.IsAiming = IsAiming;
                ClientSnapshot.IsGrounded = CharacterController.IsGrounded;
                ClientSnapshot.Jump = jumped || ClientSnapshot.Jump;

                ClientSnapshot.SelectedItem = (byte)ItemManager.SelectedItemIndex;

                if (ItemManager.SelectedItem is BlockItem blockItem)
                {
                    ClientSnapshot.ColorR = blockItem.BlockColor.R;
                    ClientSnapshot.ColorG = blockItem.BlockColor.G;
                    ClientSnapshot.ColorB = blockItem.BlockColor.B;
                }
            }

            lastGrounded = CharacterController.IsGrounded;
            jumped = false;

            base.Update(deltaTime);
        }

        protected override void OnJump()
        {
            jumpAudioSource?.Play();
            jumped = true;

            base.OnJump();
        }

        protected override void Draw()
        {
            if (StateInfo == null)
                return;

            // Draw world model for shadow
            Renderer.OnlyRenderFor = RenderPass.Shadow;

            // Setup world matrix
            Renderer.WorldMatrix = Matrix4.CreateTranslation(Transform.Position)
                * Matrix4.CreateRotationY(MathHelper.ToDegrees(-Camera.Active.Yaw));

            // Render the current item
            ItemManager.Draw(entRenderer, Viewbob);

            // Apply camera effects (yes this has to be last)
            // TODO: This trick being last doesn't work with unstable framerates
            camfx.Apply();

            base.Draw();
        }

        public override void OnClientInbound(PlayerSnapshot snapshot)
        {
            if (ItemManager.SelectedItem != null && ItemManager.SelectedItemIndex == snapshot.SelectedItem)
            {
                if (ItemManager.SelectedItem is Gun gun)
                {
                    gun.ServerMag = snapshot.CurrentMag;
                    gun.ServerStoredAmmo = snapshot.StoredAmmo;
                }
            }

            Health = snapshot.Health;
            NumBlocks = snapshot.NumBlocks;
            NumGrenades = snapshot.NumGrenades;
            NumMelons = snapshot.NumMelons;

            HitFeedbackPositions.Clear();
            foreach (Vector3 vec in snapshot.HitFeedbackSnapshot.Hits)
                HitFeedbackPositions.Add(vec);

            HitPlayer = snapshot.HitEnemy > 0;

            if (HitFeedbackPositions.Count > 0)
            {
                hitAudioSource?.Play();

                if (!camfx.IsShaking)
                    camfx.ShakeCamera(0.2f, 0.05f);
            }
        }

        public void OnClientOutbound(float rtt) { }

        public void OnPostClientOutbound() { }

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
