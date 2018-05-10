using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Audio;
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
            : base(position)
        {
            this.owner = owner;
            this.world = world;

            // Setup physics
            physicsBody = new PhysicsBodyComponent(new Vector3(2f), 0.0001f);
            AddComponent(physicsBody);

            physicsBody.Velocity = velocity * 300;

            physicsBody.CanCollideWithSoft = true;
            physicsBody.CanBePushedBySoft = false;
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
            if (!IsDed && (e.GameObject is PhysicsBlock || (owner != null && e.GameObject is Player player && owner.Team != player.Team)))
            {
                physicsBody.OnCollision -= PhysicsBody_OnCollision;
                IsDed = true;
                world.Explode(new Explosion(owner, Transform.Position, 30, 40, 200, 0.35f, "Melon"));

                if (!GlobalNetwork.IsServer)
                {
                    AudioSource explodeAudioSource = new AudioSource(AssetManager.LoadSound("Weapons/Grenade/Explode.wav"));
                    explodeAudioSource.MaxDistance = 1000;
                    explodeAudioSource.Position = Transform.Position;
                    explodeAudioSource.Pitch = 2f;

                    world.PlayWorldAudio(new WorldAudioSource(explodeAudioSource));
                }

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
            renderer.WorldMatrix = Matrix4.LookAt(Vector3.Zero, physicsBody.Velocity, Vector3.Up).ClearTranslation().Inverse()
                * Matrix4.CreateTranslation(Transform.Position);

            base.Draw();
        }
    }
}
