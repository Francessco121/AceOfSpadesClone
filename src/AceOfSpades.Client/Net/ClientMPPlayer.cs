using AceOfSpades.Characters;
using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
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
                        flashlight.Visible = !flashlight.Visible;
                }

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

                ClientSnapshot.IsCrouching = CharacterController.IsCrouching;
                ClientSnapshot.IsFlashlightVisible = flashlight.Visible;
                ClientSnapshot.Reload = inputReload || ClientSnapshot.Reload;
                ClientSnapshot.DropIntel = inputDropIntel || ClientSnapshot.DropIntel;

                ClientSnapshot.SelectedItem = (byte)ItemManager.SelectedItemIndex;

                if (ItemManager.SelectedItem is BlockItem blockItem)
                {
                    ClientSnapshot.ColorR = blockItem.BlockColor.R;
                    ClientSnapshot.ColorG = blockItem.BlockColor.G;
                    ClientSnapshot.ColorB = blockItem.BlockColor.B;
                }
            }

            base.Update(deltaTime);
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

            if (HitFeedbackPositions.Count > 0 && !camfx.IsShaking)
                camfx.ShakeCamera(0.2f, 0.05f);
        }

        public void OnClientOutbound(float rtt) { }

        public void OnPostClientOutbound() { }
    }
}
