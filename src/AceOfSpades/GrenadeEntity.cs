using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics.OpenGL;
using Dash.Engine.Physics;

/* GrenadeEntity.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class GrenadeEntity : GameObject
    {
        public bool IsDed { get; private set; }
        public PhysicsBodyComponent PhysicsBody { get; }

        const float BLOCK_RADIUS = 22;
        const float PLAYER_RADIUS = 40;
        const float DAMAGE = 150;
        const float DAMAGE_FALLOFF = 0.2f;
        const float KABOOM_DELAY = 2f;

        float timer = KABOOM_DELAY;
        World world;
        Player owner;
        VoxelRenderComponent renderer;

        public GrenadeEntity(Player owner, Vector3 position, Vector3 velocity, World world, float throwPower) 
            : base(position - new Vector3(0.75f))
        {
            this.owner = owner;
            this.world = world;

            // Setup physics
            PhysicsBody = new PhysicsBodyComponent(new Vector3(1.5f), 0.0001f);
            AddComponent(PhysicsBody);

            PhysicsBody.Velocity = velocity * throwPower;

            PhysicsBody.CanCollideWithSoft = false;
            PhysicsBody.BounceOnWallCollision = true;
            PhysicsBody.BounceOnVerticalCollision = true;
            PhysicsBody.VerticalBounceFalloff = 0.8f;
            PhysicsBody.HorizontalBounceFalloff = 0.7f;
            PhysicsBody.Friction = 0.2f;

            // Setup renderer
            if (GlobalNetwork.IsClient)
            {
                renderer = new VoxelRenderComponent();
                AddComponent(renderer);

                renderer.VoxelObject = AssetManager.LoadVoxelObject("Models/grenade.aosm", BufferUsageHint.StaticDraw);
            }
        }

        protected override void Update(float deltaTime)
        {
            if (!IsDed)
            {
                timer -= deltaTime;

                if (timer <= 0)
                {
                    IsDed = true;
                    world.Explode(new Explosion(owner, Transform.Position, BLOCK_RADIUS, PLAYER_RADIUS, DAMAGE, DAMAGE_FALLOFF));

                    Dispose();
                }
            }

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            renderer.WorldMatrix = Transform.Matrix;
            base.Draw();
        }
    }
}
