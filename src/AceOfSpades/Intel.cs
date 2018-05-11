using System;
using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using Dash.Engine.Physics;
using Dash.Engine.Animation;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades
{
    public class Intel : GameObject, INetEntity
    {
        const float PICKUP_COOLDOWN = 4f;

        public event EventHandler<Player> OnPickedUp;
        public event EventHandler<Player> OnDropped;
        public event EventHandler<Player> OnReturned;

        public Team Team { get; }
        public NetCreatableInfo CreatableInfo { get; private set; }
        public Player Holder { get; private set; }
        public Player LastHolder => lastHolder;

        public bool IsIconVisible
        {
            get { return icon.IsDrawable; }
            set { icon.IsDrawable = value; }
        }

        public VoxelPhysicsBody PhysicsBody { get; }
        VoxelRenderComponent renderer;
        IconRenderer icon;

        Player lastHolder;
        float lastHolderCooldown;
        Vector3Anim positionAnim;
        Vector3 startPosition;
        bool wasPickedUp;
        bool resetPosition;

        static Texture intelIconTex;

        public Intel(Vector3 position, Team team) 
            : base(position)
        {
            startPosition = position;
            Team = team;

            positionAnim = new Vector3Anim();

            // Setup physics body
            float cubeSize = 0.6f; // blockSize * dims
            Vector3 size = new Vector3(cubeSize * 10, cubeSize * 8, cubeSize * 3);
            PhysicsBody = new VoxelPhysicsBody(size, 0.1f, cubeSize);
            AddComponent(PhysicsBody);

            PhysicsBody.CanCollideWithSoft = true;
            PhysicsBody.CanBePushedBySoft = false;
            PhysicsBody.IsAffectedByGravity = true;
            PhysicsBody.CanBeSteppedOn = false;

            if (!GlobalNetwork.IsServer)
            {
                // Setup renderer
                AddComponent(renderer = new VoxelRenderComponent());

                string modelPath = team == Team.A ? "Models/intel-red.aosm" : "Models/intel-blue.aosm";
                renderer.VoxelObject = AssetManager.LoadVoxelObject(modelPath, BufferUsageHint.StaticDraw);

                if (intelIconTex == null)
                    intelIconTex = GLoader.LoadTexture("Textures/Gui/intel.png");

                // Setup icon
                icon = new IconRenderer();
                AddComponent(icon);

                icon.Size = new Vector2(16, 16);
                icon.Image = new Image(intelIconTex, team == Team.A ? new Color(255, 0, 0, 128) : new Color(0, 0, 255, 128));
                icon.Offset = renderer.VoxelObject.UnitSize / 2f;
            }
        }

        public void Return()
        {
            resetPosition = true;
            Drop(true);
        }

        public void OnNetworkInstantiated(NetCreatableInfo info)
        {
            CreatableInfo = info;
        }

        public void OnNetworkDestroy() { }

        public NetEntitySnapshot CreateSnapshot(NetCreatableInfo info, SnapshotSystem snapshotSystem)
        {
            return new IntelEntitySnapshot(this, info, snapshotSystem);
        }

        public void ForcePickup(Player holder)
        {
            Holder = holder;
            PhysicsBody.CanCollide = false;
            PhysicsBody.CanCollideWithSoft = false;
            PhysicsBody.IsStatic = true;
            wasPickedUp = true;

            if (OnPickedUp != null)
                OnPickedUp(this, holder);
        }

        public bool RequestOwnership(Player requestee)
        {
            if (Holder != null)
                return false;
            else if (requestee.Team != Team)
            {
                if (requestee == lastHolder && lastHolderCooldown > 0)
                    return false;
                else
                {
                    Holder = requestee;
                    PhysicsBody.CanCollide = false;
                    PhysicsBody.CanCollideWithSoft = false;
                    PhysicsBody.IsStatic = true;
                    wasPickedUp = true;

                    if (OnPickedUp != null)
                        OnPickedUp(this, requestee);

                    return true;
                }
            }
            else
            {
                if (wasPickedUp)
                {
                    if (OnReturned != null)
                        OnReturned(this, requestee);

                    wasPickedUp = false;
                    resetPosition = true;
                }

                return false;
            }
        }

        public void Drop(bool returning = false, bool yeet = false)
        {
            if (Holder != null)
            {
                PhysicsBody.CanCollide = true;
                PhysicsBody.CanCollideWithSoft = true;
                PhysicsBody.IsStatic = false;

                Transform.Position = Holder.Transform.Position + new Vector3(0, 4, 0);

                if (yeet)
                {
                    PhysicsBody.Velocity = Holder.GetCamera().LookVector * 65f;
                }

                lastHolder = Holder;
                lastHolderCooldown = PICKUP_COOLDOWN;

                if (!returning && OnDropped != null)
                    OnDropped(this, Holder);

                Holder = null;
            }
        }

        protected override void Update(float deltaTime)
        {
            if (!GlobalNetwork.IsConnected || (CreatableInfo != null && CreatableInfo.IsAppOwner))
            {
                if (lastHolderCooldown > 0)
                    lastHolderCooldown -= deltaTime;

                if (Holder != null)
                    Transform.Position = Holder.Transform.Position;

                if (Transform.Position.Y < -200)
                {
                    Drop();
                    resetPosition = true;
                }

                if (resetPosition)
                {
                    Transform.Position = startPosition;
                    resetPosition = false;
                }
            }
            else if (GlobalNetwork.IsClient && GlobalNetwork.IsConnected && CreatableInfo != null)
            {
                positionAnim.Step(DashCMD.GetCVar<float>("cl_interp_rep") * deltaTime);
                Transform.Position = positionAnim.Value;
            }

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            if (Holder == null)
                renderer.WorldMatrix = Transform.Matrix;
            else
            {
                SimpleCamera holderCam = Holder.GetCamera();

                Matrix4 matrix = Matrix4.CreateTranslation(
                        -PhysicsBody.Size.X / 2f + 0.25f,
                        0,
                        -(Holder.Size.Z / 2f + PhysicsBody.Size.Z - 0.25f))
                    * Matrix4.CreateRotationY(MathHelper.ToRadians(-holderCam.Yaw) + MathHelper.Pi)
                    * Matrix4.CreateTranslation(Holder.Transform.Position);

                renderer.WorldMatrix = matrix;
            }

            base.Draw();
        }

        public void OnServerOutbound(NetEntitySnapshot _snapshot)
        {
            IntelEntitySnapshot snapshot = (IntelEntitySnapshot)_snapshot;
            snapshot.X = Transform.Position.X;
            snapshot.Y = Transform.Position.Y;
            snapshot.Z = Transform.Position.Z;
        }

        public void OnClientInbound(NetEntitySnapshot _snapshot)
        {
            IntelEntitySnapshot snapshot = (IntelEntitySnapshot)_snapshot;
            positionAnim.SetTarget(new Vector3(snapshot.X, snapshot.Y, snapshot.Z));

            if ((positionAnim.Target - positionAnim.Value).Length > 20)
                positionAnim.SnapTo(positionAnim.Target);
        }
    }
}
