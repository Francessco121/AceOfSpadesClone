using AceOfSpades.Graphics;
using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Graphics;
using System;
using System.Collections.Generic;

/* Player.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Characters
{
    public abstract class Player : Character, IDisposable
    {
        public static bool DrawCollider;

        public const float SPEED_CROUCH = 20;
        public const float SPEED_NORMAL = 40;
        public const float SPEED_WALK = 20;
        public const float SPEED_SPRINT = 60;
        public const float JUMP_POWER = 40;

        public const float MAX_HEALTH = 100;
        public const int MAX_GRENADES = 3;
        public const int MAX_BLOCKS = 200;

        public bool AllowUserInput { get; set; } = true;
        public abstract bool IsRenderingThirdperson { get; set; }
        public bool IsAiming { get; protected set; }
        public bool IsWalking { get; protected set; }
        public bool IsSprinting { get; protected set; }

        public bool IsStrafing { get; protected set; }
        public int StrafeDir { get; protected set; }

        public PlayerDamage LastDamage { get; protected set; }

        public List<Vector3> HitFeedbackPositions { get; }
        public bool HitPlayer { get; set; }

        public ItemManager ItemManager { get; }
        protected ItemViewbob Viewbob { get; }

        public Team Team { get; }

        public int NumBlocks
        {
            get { return numBlocks; }
            set { numBlocks = MathHelper.Clamp(value, 0, MAX_BLOCKS); }
        }
        int numBlocks = MAX_BLOCKS;

        public int NumGrenades
        {
            get { return numGrenades; }
            set { numGrenades = MathHelper.Clamp(value, 0, MAX_GRENADES); }
        }
        int numGrenades = MAX_GRENADES;

        public virtual float Health { get; protected set; } = MAX_HEALTH;

        public World World { get; private set; }

        float lastYaw;
        
        protected DebugRenderer debugRenderer;
        protected EntityRenderer entRenderer;

        protected MasterRenderer masterRenderer;
        protected Light flashlight;

        SimpleCamera camera;

        public Player(MasterRenderer renderer, World world, SimpleCamera camera, Vector3 position, Team team) 
            : base(position, 11, 5, 2.5f)
        {
            this.masterRenderer = renderer;
            this.World = world;
            this.camera = camera;
            Team = team;

            if (!GlobalNetwork.IsServer)
            {
                debugRenderer = renderer.GetRenderer3D<DebugRenderer>();
                entRenderer = renderer.GetRenderer3D<EntityRenderer>();
                
                base.Renderer.VoxelObject = new DebugVOCube(world.GetTeamColor(team).ToColor4(), 1);
                
                flashlight = new Light(Vector3.Zero, LightType.Spot, 2, Color.White, new Vector3(1, 0, 0.0025f))
                    { Radius = MathHelper.ToRadians(35), Visible = false };
                renderer.Lights.Add(flashlight);
            }

            HitFeedbackPositions = new List<Vector3>();

            Viewbob = GlobalNetwork.IsClient ? new ItemViewbob(this) : null;
            ItemManager = new ItemManager(renderer, this, world, Viewbob);
        }

        public void Damage(float damage, string cause)
        {
            if (LastDamage == null)
                LastDamage = new PlayerDamage(this, damage, cause);
            else
                LastDamage = new PlayerDamage(LastDamage, damage, cause);

            Health -= damage;
        }

        public void Damage(Player attacker, float damage, string cause)
        {
            if (LastDamage == null)
                LastDamage = new PlayerDamage(attacker, this, damage, cause);
            else
                LastDamage = new PlayerDamage(LastDamage, attacker, this, damage, cause);

            Health -= damage;
        }

        public OrientatedBoundingBox GetOrientatedBoundingBox()
        {
            return GetOrientatedBoundingBox(camera.Yaw);
        }

        public OrientatedBoundingBox GetOrientatedBoundingBox(float yaw)
        {
            Vector3 halfSize = Size / 2f;
            OrientatedBoundingBox obb = new OrientatedBoundingBox(-halfSize, halfSize);
            obb.Matrix = Matrix4.CreateRotationY(MathHelper.ToRadians(-yaw));
            return obb;
        }

        public SimpleCamera GetCamera()
        {
            return camera;
        }

        protected void UpdateMoveVector(Vector3 inputMove, bool tryJump, bool sprint, bool walk, float speedMultiplier = 1f)
        {
            // Update strafing properties
            IsStrafing = inputMove.X != 0;
            StrafeDir = (int)inputMove.X;

            // Update MoveVector
            CharacterController.MoveVector = CalculateMoveVector(inputMove, tryJump, sprint, walk) * speedMultiplier;
        }

        protected Vector3 CalculateMoveVector(Vector3 inputMove, bool tryJump, bool sprint, bool walk,
            float? customSpeed = null)
        {
            // Transform inputMove to be in the direction of the camera
            if (inputMove != Vector3.Zero)
            {
                inputMove = camera.TransformNormalY(inputMove);

                // Apply the movement speed
                inputMove.Y = 0;
                inputMove = inputMove.Normalize();
                inputMove *= customSpeed.HasValue ? customSpeed.Value : GetSpeed(sprint, walk);
            }

            // Apply jump power if player is grounded
            if (tryJump && CharacterController.IsGrounded)
                inputMove.Y = JUMP_POWER;

            return inputMove;
        }

        protected float GetSpeed(bool sprinting, bool walk)
        {
            return CharacterController.IsCrouching ? SPEED_CROUCH
                : (IsAiming || walk) ? SPEED_WALK
                : sprinting ? SPEED_SPRINT
                : SPEED_NORMAL;
        }

        protected float GetSpeed(bool sprinting, bool walk, bool crouching)
        {
            return crouching ? SPEED_CROUCH
                : walk ? SPEED_WALK
                : sprinting ? SPEED_SPRINT
                : SPEED_NORMAL;
        }

        protected override void Update(float deltaTime)
        {
            CharacterController.CanStep = !CharacterController.IsCrouching && !IsWalking;

            if (!GlobalNetwork.IsServer)
                UpdateWorldModel();

            base.Update(deltaTime);
        }

        void UpdateWorldModel()
        {
            VoxelObject.MeshScale = Size;

            float characterYaw;
            if (Camera.Active.Mode == CameraMode.ArcBall)
            {
                characterYaw = Maths.VectorToAngle(CharacterController.LastNonZeroMoveVector.X, 
                    CharacterController.LastNonZeroMoveVector.Z);
                lastYaw = Interpolation.LerpRadians(lastYaw, characterYaw, 0.2f);
            }
            else
            {
                characterYaw = MathHelper.ToRadians(-Camera.Active.Yaw + 180);
                lastYaw = characterYaw;
            }
        }

        public override void Dispose()
        {
            ItemManager.Dispose();

            if (GlobalNetwork.IsClient)
            {
                masterRenderer.Lights.Remove(flashlight);

                if (Camera.Active != null)
                {
                    Camera.Active.FPSMouseSensitivity = Camera.Active.DefaultFPSMouseSensitivity;
                    Camera.Active.ArcBallMouseSensitivity = Camera.Active.DefaultArcBallMouseSensitivity;
                    Camera.Active.FOV = Camera.Active.DefaultFOV;
                }
            }

            base.Dispose();
        }
    }
}
