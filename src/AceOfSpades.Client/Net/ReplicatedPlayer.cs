using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Audio;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;

/* ReplicatedPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class ReplicatedPlayer : ClientPlayer
    {
        Vector3Anim interpPos;
        FloatAnim yawAnim;
        FloatAnim pitchAnim;
        SimpleCamera camera;

        bool reload;
        bool lastGrounded;
        bool isMoving;
        bool isGrounded;
        bool jump;
        bool land;

        bool isDisposed;

        readonly AudioSource jumpAudioSource;
        readonly AudioSource landAudioSource;
        readonly CyclicAudioSource walkingAudioSource;
        readonly CyclicAudioSource runningAudioSource;

        public ReplicatedPlayer(MasterRenderer renderer, World world, SimpleCamera camera, Vector3 position, Team team)
            : base(renderer, world, camera, position, team)
        {
            this.camera = camera;

            interpPos = new Vector3Anim();
            interpPos.SnapTo(position);

            yawAnim = new FloatAnim();
            pitchAnim = new FloatAnim();

            // This is fully server controlled
            ItemManager.DontUpdateItems = true;
            ItemManager.IsReplicated = true;
            CharacterController.IsEnabled = false;

            CreateStarterBackpack();

            AudioBuffer jumpAudioBuffer = AssetManager.LoadSound("Player/jump.wav");

            if (jumpAudioBuffer != null)
            {
                jumpAudioSource = new AudioSource(jumpAudioBuffer);
                jumpAudioSource.Gain = 0.2f;
                jumpAudioSource.MaxDistance = 100f;
            }

            AudioBuffer landAudioBuffer = AssetManager.LoadSound("Player/land.wav");

            if (landAudioBuffer != null)
            {
                landAudioSource = new AudioSource(landAudioBuffer);
                landAudioSource.Gain = 0.2f;
                landAudioSource.MaxDistance = 120f;
            }

            walkingAudioSource = new CyclicAudioSource("Player/footstep.wav", 1, 0f,
                relative: false, maxDistance: 100f);

            runningAudioSource = new CyclicAudioSource("Player/run.wav", 1, 0f,
                relative: false, maxDistance: 200f);
        }

        protected override void Update(float deltaTime)
        {
            if (StateInfo != null)
            {
                // Update viewbob
                Viewbob.UpdateReplicated(deltaTime);

                // Step each value animation
                float interpStep = DashCMD.GetCVar<float>("cl_interp_rep") * deltaTime;
                interpPos.Step(interpStep);
                yawAnim.Step(interpStep);
                pitchAnim.Step(interpStep);

                // Update each property affected by interpolation
                Transform.Position = interpPos.Value;

                camera.Yaw = yawAnim.Value;
                camera.Pitch = pitchAnim.Value;

                // Update our "fake" camera
                camera.Position = Transform.Position + new Vector3(0, Size.Y / 2f - 1.1f, 0);
                camera.Update(deltaTime);

                // Update flashlight
                flashlight.Position = camera.Position;
                flashlight.Direction = -camera.LookVector;

                // Update the item manager
                ItemManager.UpdateReplicated(deltaTime);

                // Handle jump
                jumpAudioSource.Position = Transform.Position;

                if (jump)
                {
                    jump = false;
                    jumpAudioSource?.Play();
                }

                // Handle landing
                landAudioSource.Position = Transform.Position;

                if (land)
                {
                    land = false;
                    landAudioSource?.Play();
                }

                // Handle walking/running
                walkingAudioSource.IsPlaying = isGrounded && isMoving && !IsSprinting;
                walkingAudioSource.IterationLength = (1f / GetFootstepIterationSpeed()) * 2f;
                walkingAudioSource.Update(deltaTime, Transform.Position);

                runningAudioSource.IsPlaying = isGrounded && isMoving && IsSprinting;
                runningAudioSource.IterationLength = (1f / GetFootstepIterationSpeed()) * 2f;
                runningAudioSource.Update(deltaTime, Transform.Position);

                // Handle reload
                if (reload)
                {
                    reload = false;

                    if (ItemManager.SelectedItem is Gun gun)
                    {
                        gun.OnReplicatedReload();
                    }
                }
            }

            base.Update(deltaTime);
        }

        float GetFootstepIterationSpeed()
        {
            if (IsAiming)
                return ItemViewbob.SPEED_WALK;
            else if (CharacterController.IsCrouching)
                return ItemViewbob.SPEED_CROUCH;
            else if (IsAiming)
                return ItemViewbob.SPEED_WALK;
            else if (IsSprinting)
                return ItemViewbob.SPEED_SPRINT;
            else
                return ItemViewbob.SPEED_NORMAL;
        }

        protected override void Draw()
        {
            if (StateInfo == null)
                return;

            CharacterController.Size.Y = CharacterController.IsCrouching 
                ? CharacterController.CrouchHeight 
                : CharacterController.NormalHeight;

            // Setup world matrix
            Renderer.WorldMatrix = Matrix4.CreateScale(VoxelObject.MeshScale)
                * Matrix4.CreateRotationY(MathHelper.ToRadians(-camera.Yaw))
                * Matrix4.CreateTranslation(Transform.Position);

            if (DrawCollider)
                debugRenderer.Batch(CharacterController.GetCollider());

            ItemManager.Draw(entRenderer, Viewbob);

            base.Draw();
        }

        public override void OnClientInbound(PlayerSnapshot snapshot)
        {
            ItemManager.Equip(snapshot.SelectedItem, forceEquip: true);
            ItemManager.MuzzleFlashIterations += snapshot.TimesShot;

            yawAnim.SetTarget(snapshot.CamYaw);
            pitchAnim.SetTarget(snapshot.CamPitch);
            interpPos.SetTarget(new Vector3(snapshot.X, snapshot.Y, snapshot.Z));

            flashlight.Visible = snapshot.IsFlashlightOn;
            reload = snapshot.IsReloading;

            CharacterController.IsCrouching = snapshot.IsCrouching;
            IsSprinting = snapshot.IsSprinting;
            isMoving = snapshot.IsMoving;
            IsAiming = snapshot.IsAiming;
            isGrounded = snapshot.IsGrounded;
            jump = snapshot.IsJumping;

            if (isGrounded && !lastGrounded)
                land = true;

            lastGrounded = snapshot.IsGrounded;
        }

        public override void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                jumpAudioSource?.Dispose();
                landAudioSource?.Dispose();
                walkingAudioSource.Dispose();
                runningAudioSource.Dispose();
            }

            base.Dispose();
        }
    }
}
