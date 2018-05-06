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

/* ClientMPPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class ClientMPPlayer : ClientPlayer
    {
        class PositionSnapshot
        {
            public NetworkClientMovement Movement { get; }
            public Vector3 ExpectedPosition { get; }

            public PositionSnapshot(NetworkClientMovement movement, Vector3 expectedPosition)
            {
                Movement = movement;
                ExpectedPosition = expectedPosition;
            }
        }

        public ClientInputSnapshot ClientInput { get; private set; }

        Vector3 lastServerPos;

        FloatAnim yawAnim;
        FloatAnim pitchAnim;

        SimpleCamera camera;
        CameraFX camfx;

        FakeServerPlayer serverTestPlayer;

        ushort latestServerClientSequence;
        ushort currentSequence;

        Queue<PositionSnapshot> positionSnapshots;

        public ClientMPPlayer(MasterRenderer renderer, World world, Camera camera, Vector3 position, Team team)
            : base(renderer, world, camera, position, team)
        {
            this.camera = camera;
            camfx = new CameraFX(this, camera);

            yawAnim = new FloatAnim();
            pitchAnim = new FloatAnim();

            positionSnapshots = new Queue<PositionSnapshot>();

            // Setup ClientInput Snapshot
            AOSClient client = AOSClient.Instance;
            SnapshotNetComponent snc = client.GetComponent<SnapshotNetComponent>();
            SnapshotSystem ss = snc.SnapshotSystem;
            ClientInput = new ClientInputSnapshot(ss, client.ServerConnection);

            lastServerPos = position;

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

                CharacterController.MovementSmoothingInterp = DashCMD.GetCVar<float>("cl_interp_movement_smooth");
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

                // Determine if we've drifted to far from the server based on our predictions and server position
                bool forcedPosition = false;
                if (positionSnapshots.Count > 0 && positionSnapshots.Peek().Movement.Sequence <= latestServerClientSequence)
                {
                    PositionSnapshot predictedSnapshot = null;
                    while (positionSnapshots.Count > 0
                        && (predictedSnapshot == null || predictedSnapshot.Movement.Sequence != latestServerClientSequence))
                    {
                        predictedSnapshot = positionSnapshots.Dequeue();
                    }

                    if (predictedSnapshot != null)
                    {
                        float xDiff = Math.Abs(predictedSnapshot.ExpectedPosition.X - lastServerPos.X);
                        float yDiff = Math.Abs(predictedSnapshot.ExpectedPosition.Y - lastServerPos.Y);
                        float zDiff = Math.Abs(predictedSnapshot.ExpectedPosition.Z - lastServerPos.Z);

                        float distance = (new Vector3(xDiff, yDiff, zDiff)).Length;

                        float maxErrorDistance = DashCMD.GetCVar<float>("cl_max_error_dist");

                        if (distance > maxErrorDistance)
                        {
                            forcedPosition = true;

                            Transform.Position = lastServerPos;
                        }
                    }
                }

                if (!forcedPosition && move != Vector3.Zero)
                {
                    Vector3 interpolation = new Vector3(lastServerPos.X - Transform.Position.X, 0, lastServerPos.Z - Transform.Position.Z);

                    float interp = DashCMD.GetCVar<float>("cl_interp");

                    CharacterController.MoveVectorOffset = interpolation;
                    CharacterController.MoveVectorOffsetFactor = interp;
                }
                else
                {
                    CharacterController.MoveVectorOffset = Vector3.Zero;
                    CharacterController.MoveVectorOffsetFactor = 0;
                }

                // Update the input snapshot
                ClientInput.SelectedItem = (byte)ItemManager.SelectedItemIndex;
                ClientInput.IsFlashlightVisible = flashlight.Visible;

                ClientInput.Reload = inputReload || ClientInput.Reload;
                ClientInput.IsAiming = IsAiming;
                ClientInput.DropIntel = inputDropIntel || ClientInput.DropIntel;

                SnapshotMovement(new NetworkClientMovement
                {
                    MoveForward = move.Z == -1,
                    MoveBackward = move.Z == 1,
                    MoveLeft = move.X == 1,
                    MoveRight = move.X == -1,
                    Crouch = inputCrouch,
                    Walk = inputWalk,
                    Sprint = inputSprint,
                    Jump = inputJump,
                    CameraPitch = camera.Pitch,
                    CameraYaw = camera.Yaw,
                    Sequence = currentSequence++
                }, deltaTime);
            }

            base.Update(deltaTime);
        }

        void SnapshotMovement(NetworkClientMovement movement, float deltaTime)
        {
            NetworkClientMovement actuallyAddedMovement = ClientInput.MovementSnapshot.EnqueueMovement(movement, deltaTime);

            // Movements are always buffered so that the latest isn't actually sent.
            // The actuallyAddedMovement variable contains the previously enqueued movement,
            // which now correctly has the Length property set.
            if (actuallyAddedMovement != null)
            {
                positionSnapshots.Enqueue(new PositionSnapshot(movement, PredictServer(actuallyAddedMovement)));

                // Ensure the queue doesn't grow insanely large since it's only read from
                // when the server responds
                if (positionSnapshots.Count > 100)
                {
                    while (positionSnapshots.Count > 100)
                        positionSnapshots.Dequeue();
                }
            }
        }

        Vector3 PredictServer(NetworkClientMovement movement)
        {
            Vector3 move = Vector3.Zero;
            if (movement.MoveForward) move.Z -= 1;
            if (movement.MoveBackward) move.Z += 1;
            if (movement.MoveLeft) move.X += 1;
            if (movement.MoveRight) move.X -= 1;

            // Predict the direction
            float speed = GetSpeed(movement.Sprint, movement.Walk || ClientInput.IsAiming, movement.Crouch);
            Vector3 predictedDir = CalculateMoveVector(move, movement.Jump, false, false, speed);

            // Simulate one physics step for our "fake" server player
            // Time to simulate is equal to half the snapshot round-trip time
            serverTestPlayer.PhysicsBody.Size = Size;
            serverTestPlayer.Transform.Position = Transform.Position;
            serverTestPlayer.PhysicsBody.Velocity = predictedDir;
            World.Physics.SimulateSingle(serverTestPlayer.PhysicsBody, movement.Length);

            // return the predicted position
            return serverTestPlayer.Transform.Position;
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
            latestServerClientSequence = snapshot.Sequence;

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

        public void OnClientOutbound(float rtt) { }

        public void OnPostClientOutbound() { }
    }
}
