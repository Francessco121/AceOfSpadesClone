using AceOfSpades.Characters;
using AceOfSpades.Tools;
using Dash.Engine;
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

        CameraFX camfx;
        Camera camera;
        bool thirdPerson;

        Vector3 startLocation;
        Intel intel;

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
                new MelonLauncher(ItemManager, renderer, 9999)
            }, 5);

            CharacterController.OnCollision += CharacterController_OnCollision;
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
                    }
                }
            }
        }

        void DropIntel()
        {
            if (intel != null)
            {
                intel.Drop();
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

        protected override void Update(float deltaTime)
        {
            if (Input.GetControlDown("DropIntel"))
                DropIntel();

            // Check if aiming
            IsAiming = AllowUserInput && ItemManager.SelectedItem != null
                && ItemManager.SelectedItem.Type.HasFlag(ItemType.Gun)
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
                    flashlight.Visible = !flashlight.Visible;
            }

            // Ensure the firstperson offset is correct (this changes when crouching)
            camera.FirstPersonLockOffset = new Vector3(0, Size.Y / 2f - 1.1f, 0);

            // Update the flashlight
            flashlight.Position = IsRenderingThirdperson 
                ? Transform.Position + Camera.Active.FirstPersonLockOffset 
                : camera.Position;
            flashlight.Direction = -camera.LookVector;

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
    }
}
