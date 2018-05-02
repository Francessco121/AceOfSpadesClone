using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics.OpenGL;
using Dash.Engine.Physics;

namespace AceOfSpades
{
    public class MelonEntity : GameObject
    {
        public bool IsDed { get; private set; }

        World world;
        Player owner;
        VoxelRenderComponent renderer;
        PhysicsBodyComponent physicsBody;

        public MelonEntity(Player owner, Vector3 position, Vector3 velocity, World world) 
            : base(position - new Vector3(1))
        {
            this.owner = owner;
            this.world = world;

            // Setup physics
            physicsBody = new PhysicsBodyComponent(new Vector3(2f), 0.0001f);
            AddComponent(physicsBody);

            physicsBody.Velocity = velocity * 400;

            physicsBody.CanCollideWithSoft = false;
            physicsBody.IsAffectedByGravity = true;

            physicsBody.OnCollision += PhysicsBody_OnCollision;

            // Setup renderer
            if (GlobalNetwork.IsClient)
            {
                renderer = new VoxelRenderComponent();
                AddComponent(renderer);

                renderer.VoxelObject = AssetManager.LoadVoxelObject("Models/melon.aosm", BufferUsageHint.StaticDraw);
            }
        }

        private void PhysicsBody_OnCollision(object sender, PhysicsBodyComponent e)
        {
            if (!IsDed && e.GameObject is PhysicsBlock)
            {
                physicsBody.OnCollision -= PhysicsBody_OnCollision;
                IsDed = true;
                world.Explode(new Explosion(owner, Transform.Position, 30, 50, 300, 0.5f));

                Dispose();
            }
        }

        protected override void Update(float deltaTime)
        {
            if (!IsDed && Transform.Position.Y < -50)
                IsDed = true;

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            renderer.WorldMatrix = Transform.Matrix;
            base.Draw();
        }
    }
}
