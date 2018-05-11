using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using Dash.Engine.Graphics.OpenGL;

namespace AceOfSpades
{
    public class CommandPost : GameObject, INetEntity
    {
        public Team Team { get; }
        public NetCreatableInfo CreatableInfo { get; private set; }
        public VoxelPhysicsBody PhysicsBody { get; }

        Vector3Anim positionAnim;
        Vector3 startPosition;

        VoxelRenderComponent renderer;

        static Texture commandpostIconTex;

        public CommandPost(Vector3 position, Team team)
            : base(position)
        {
            startPosition = position;
            Team = team;

            positionAnim = new Vector3Anim();

            // Setup physics body
            float cubeSize = 1.5f; // blockSize * dims
            Vector3 size = new Vector3(cubeSize * 9, cubeSize * 10, cubeSize * 7);
            PhysicsBody = new VoxelPhysicsBody(size, 2f, cubeSize);
            AddComponent(PhysicsBody);

            PhysicsBody.CanCollideWithSoft = true;
            PhysicsBody.CanBePushedBySoft = false;
            PhysicsBody.IsAffectedByGravity = true;
            PhysicsBody.CanBeSteppedOn = false;

            if (!GlobalNetwork.IsServer)
            {
                // Setup renderer
                renderer = new VoxelRenderComponent();
                AddComponent(renderer);

                string modelPath = team == Team.A ? "Models/commandpost-red.aosm" : "Models/commandpost-blue.aosm";
                renderer.VoxelObject = AssetManager.LoadVoxelObject(modelPath, BufferUsageHint.StaticDraw);

                if (commandpostIconTex == null)
                    commandpostIconTex = GLoader.LoadTexture("Textures/Gui/commandpost.png");

                // Setup icon
                IconRenderer icon = new IconRenderer();
                AddComponent(icon);

                icon.Size = new Vector2(16, 16);
                icon.Image = new Image(commandpostIconTex, team == Team.A ? new Color(255, 0, 0, 128) : new Color(0, 0, 255, 128));
                icon.Offset = renderer.VoxelObject.UnitSize / 2f;
            }
        }

        public void OnNetworkInstantiated(NetCreatableInfo info)
        {
            CreatableInfo = info;
        }

        public void OnNetworkDestroy() { }

        public NetEntitySnapshot CreateSnapshot(NetCreatableInfo info, SnapshotSystem snapshotSystem)
        {
            return new CommandPostEntitySnapshot(this, info, snapshotSystem);
        }

        protected override void Update(float deltaTime)
        {
            if (GlobalNetwork.IsClient && GlobalNetwork.IsConnected && CreatableInfo != null)
            {
                positionAnim.Step(DashCMD.GetCVar<float>("cl_interp_rep") * deltaTime);
                Transform.Position = positionAnim.Value;
            }
            else if (!GlobalNetwork.IsConnected || GlobalNetwork.IsServer)
            {
                if (Transform.Position.Y < -200)
                    Transform.Position = startPosition;
            }

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            renderer.WorldMatrix = Transform.Matrix;
            base.Draw();
        }

        public void OnServerOutbound(NetEntitySnapshot _snapshot)
        {
            CommandPostEntitySnapshot snapshot = (CommandPostEntitySnapshot)_snapshot;
            snapshot.X = Transform.Position.X;
            snapshot.Y = Transform.Position.Y;
            snapshot.Z = Transform.Position.Z;
        }

        public void OnClientInbound(NetEntitySnapshot _snapshot)
        {
            CommandPostEntitySnapshot snapshot = (CommandPostEntitySnapshot)_snapshot;
            positionAnim.SetTarget(new Vector3(snapshot.X, snapshot.Y, snapshot.Z));

            if ((positionAnim.Target - positionAnim.Value).Length > 20)
                positionAnim.SnapTo(positionAnim.Target);
        }
    }
}
