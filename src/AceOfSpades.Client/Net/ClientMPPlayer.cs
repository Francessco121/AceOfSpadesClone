using AceOfSpades.Characters;
using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Physics;
using System;

/* ClientMPPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class ClientMPPlayer : ClientPlayer
    {
        public ClientInputSnapshot ClientInput { get; private set; }
        MovementState lastMovement;

        Vector3 lastServerPos;
        Vector3 predictedDir;

        FloatAnim yawAnim;
        FloatAnim pitchAnim;

        SimpleCamera camera;
        CameraFX camfx;

        bool serverGrounded;

        float snapshotRTT;
        Vector3 expectedServerPos;
        FakeServerPlayer serverTestPlayer;

        public ClientMPPlayer(MasterRenderer renderer, World world, Camera camera, Vector3 position, Team team)
            : base(renderer, world, camera, position, team)
        {
            this.camera = camera;
            camfx = new CameraFX(this, camera);

            yawAnim = new FloatAnim();
            pitchAnim = new FloatAnim();

            // Setup ClientInput Snapshot
            AOSClient client = AOSClient.Instance;
            SnapshotNetComponent snc = client.GetComponent<SnapshotNetComponent>();
            SnapshotSystem ss = snc.SnapshotSystem;
            ClientInput = new ClientInputSnapshot(ss, client.ServerConnection);

            lastServerPos = position;
            expectedServerPos = position;

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

        public override void OnNetworkInstantiated(NetCreatableInfo stateInfo)
        {
            lastMovement = new MovementState();
            serverTestPlayer = new FakeServerPlayer(Transform.Position, Size, CharacterController.Mass);

            base.OnNetworkInstantiated(stateInfo);
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
                    && ItemManager.SelectedItem.Type.HasFlag(ItemType.Gun)
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

                CharacterController.MovementSmoothingInterp = DashCMD.GetCVar<float>("cl_interp_smooth");
                UpdateMoveVector(move, inputJump, inputSprint, IsWalking = inputWalk);

                bool actuallyJumped = inputJump && CharacterController.IsGrounded;

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

                // Interpolate position from the client position vs the server position
                float interp = DashCMD.GetCVar<float>("cl_interp");
                float interp_ysnapoffset = DashCMD.GetCVar<float>("cl_interp_ysnapoffset");
                float interp_ysnapoffset_i = DashCMD.GetCVar<float>("cl_interp_ysnapoffset_i");

                if (!predictedDir.IsNan())
                {
                    // Modify the angle we are moving at to be more like the
                    // direction the server will move us at.
                    // This very smoothly corrects our ending position.
                    CharacterController.MoveVectorOffset = new Vector3(predictedDir.X, 0, predictedDir.Z);
                    CharacterController.MoveVectorOffsetFactor = interp;
                }
                else
                    CharacterController.MoveVectorOffsetFactor = 0;

                // Slowly correct ourselves
                Transform.Position.X = Interpolation.Linear(Transform.Position.X, expectedServerPos.X, interp);
                Transform.Position.Y = Interpolation.Linear(Transform.Position.Y, expectedServerPos.Y, 
                    CharacterController.IsGrounded ? interp : interp / 2f);
                Transform.Position.Z = Interpolation.Linear(Transform.Position.Z, expectedServerPos.Z, interp);

                // If our synchronization drags us onto a block,
                // compensate by snapping to the server y coordinate.
                if (serverGrounded && 
                    ((!CharacterController.IsMoving && Math.Abs(expectedServerPos.Y - Transform.Position.Y) >= interp_ysnapoffset_i) 
                    || (CharacterController.IsMoving && Math.Abs(expectedServerPos.Y - Transform.Position.Y) >= interp_ysnapoffset)))
                    Transform.Position.Y = expectedServerPos.Y;

                // Update the input snapshot
                ClientInput.CameraPitch = camera.Pitch;
                ClientInput.CameraYaw = camera.Yaw;

                ClientInput.SelectedItem = (byte)ItemManager.SelectedItemIndex;
                ClientInput.IsFlashlightVisible = flashlight.Visible;

                ClientInput.Crouch = inputCrouch;
                ClientInput.Walk = inputWalk;
                ClientInput.Sprint = inputSprint;
                ClientInput.Jump = actuallyJumped || ClientInput.Jump;
                if (actuallyJumped)
                    ClientInput.JumpTimeTicks = Environment.TickCount;

                ClientInput.MoveForward = move.Z == -1 || ClientInput.MoveForward;
                ClientInput.MoveBackward = move.Z == 1 || ClientInput.MoveBackward;
                ClientInput.MoveLeft = move.X == 1 || ClientInput.MoveLeft;
                ClientInput.MoveRight = move.X == -1 || ClientInput.MoveRight;

                ClientInput.Reload = inputReload || ClientInput.Reload;
                ClientInput.IsAiming = IsAiming;
                ClientInput.DropIntel = inputDropIntel || ClientInput.DropIntel;
            }

            base.Update(deltaTime);
        }

        void PredictServer()
        {
            Vector3 move = Vector3.Zero;
            if (lastMovement.MoveForward) move.Z -= 1;
            if (lastMovement.MoveBackward) move.Z += 1;
            if (lastMovement.MoveLeft) move.X += 1;
            if (lastMovement.MoveRight) move.X -= 1;

            // Predict the direction
            float speed = GetSpeed(lastMovement.Sprint, lastMovement.Walk || lastMovement.Aiming, lastMovement.Crouch);
            predictedDir = CalculateMoveVector(move, lastMovement.Jump, false, false, speed);

            // Simulate one physics step for our "fake" server player
            // Time to simulate is equal to half the snapshot round-trip time
            serverTestPlayer.PhysicsBody.Size = Size;
            serverTestPlayer.Transform.Position = lastServerPos;
            serverTestPlayer.PhysicsBody.Velocity = predictedDir;
            World.Physics.SimulateSingle(serverTestPlayer.PhysicsBody, snapshotRTT / 2f);

            // Update the predicted position
            expectedServerPos = serverTestPlayer.Transform.Position;
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
            if (ItemManager.SelectedItem != null)
            {
                Gun gun = ItemManager.SelectedItem as Gun;
                if (gun != null)
                {
                    gun.ServerMag = snapshot.CurrentMag;
                    gun.ServerStoredAmmo = snapshot.StoredAmmo;
                }
            }

            lastServerPos = new Vector3(snapshot.X, snapshot.Y, snapshot.Z);
            serverGrounded = snapshot.IsGrounded;
            Health = snapshot.Health;
            NumBlocks = snapshot.NumBlocks;
            NumGrenades = snapshot.NumGrenades;

            HitFeedbackPositions.Clear();
            foreach (Vector3 vec in snapshot.HitFeedbackSnapshot.Hits)
                HitFeedbackPositions.Add(vec);

            HitPlayer = snapshot.HitEnemy > 0;

            if (HitFeedbackPositions.Count > 0 && !camfx.IsShaking)
                camfx.ShakeCamera(0.2f, 0.05f);
        }

        public void OnClientOutbound(float rtt)
        {
            // Update our copy of the snapshot round-trip time
            snapshotRTT = rtt;
        }

        public void OnPostClientOutbound()
        {
            // Retreive the movement state we just sent to the server
            lastMovement.FromByteFlag(ClientInput.GetMovementFlag(), ClientInput.IsAiming);
            // Predict where the server will move us by the time
            // we get the next player snapshot
            PredictServer();
        }
    }
}
