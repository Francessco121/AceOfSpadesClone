using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using AceOfSpades.IO;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using System;

/* ClientWorld.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public abstract class ClientWorld : World, IDisposable
    {
        public MasterRenderer Renderer { get; private set; }

        protected DebugRenderer debugRenderer;
        protected EntityRenderer entRenderer;

        public ClientWorld(MasterRenderer renderer)
        {
            Renderer = renderer;

            debugRenderer = Renderer.GetRenderer3D<DebugRenderer>();
            entRenderer = Renderer.GetRenderer3D<EntityRenderer>();
        }

        public virtual void OnScreenResized(int width, int height) { }

        protected WorldDescription LoadFromFile(string fileName)
        {
            WorldDescription desc = WorldIO.Load(fileName);
            SetTerrain(desc.Terrain);
            return desc;
        }

        public override void BeginRollback(int timeFrame)
        {
            throw new NotSupportedException("World.BeginRollback is only available on the server.");
        }

        public override void FireBullet(Player player, Vector3 origin, Vector3 dir, Vector3 recoil,
            int blockDamage, float playerDamage, float maxDist = float.MaxValue)
        {
            Ray ray = new Ray(origin, dir + recoil);
            TerrainRaycastResult result = TerrainPhysics.Raycast(ray, true, maxDist);

            if (result.Intersects)
            {
                result.Chunk.DamageBlock(result.BlockIndex.Value, blockDamage);
                if (DashCMD.GetCVar<bool>("cl_impacts"))
                    debugRenderer.AddBulletImpact(ray.Origin + ray.Direction * result.IntersectionDistance.Value, Color.Red);
            }
        }

        public override Block SetBlock(IndexPosition chunkIndex, IndexPosition blockPos, Block block, bool placement)
        {
            return Terrain.Chunks[chunkIndex].SetBlock(block, blockPos);
        }

        public override void ThrowGrenade(Player owner, Vector3 origin, Vector3 dir, float power)
        {
            GrenadeEntity ent = new GrenadeEntity(owner, origin, dir, this, power);
            grenades.Add(ent);
            AddGameObject(ent);
        }

        public override void ShootMelon(Player owner, Vector3 origin, Vector3 dir)
        {
            MelonEntity ent = new MelonEntity(owner, origin, dir, this);
            melons.Add(ent);
            AddGameObject(ent);
        }

        public override void Explode(Explosion explosion)
        {
            // Destroy terrain
            Vector3 origin = explosion.Origin;
            float radius = explosion.BlockRadius;

            IndexPosition cpos = FixedTerrain.WorldToChunkCoords(origin);
            IndexPosition blockPos = Chunk.WorldToBlockCoords(origin);
            blockPos -= cpos * Chunk.SIZE;

            Chunk chunk;
            if (Terrain.Chunks.TryGetValue(cpos, out chunk))
            {
                int blockRadius = (int)(radius / Block.CUBE_SIZE);
                for (int x = -blockRadius; x <= blockRadius; x++)
                    for (int y = -blockRadius; y <= blockRadius; y++)
                        for (int z = -blockRadius; z <= blockRadius; z++)
                        {
                            int nx = x + blockPos.X;
                            int ny = y + blockPos.Y;
                            int nz = z + blockPos.Z;

                            IndexPosition ncpos = chunk.WrapBlockCoords(ref nx, ref ny, ref nz);

                            if (!chunk.IsBlockCoordInRange(nx, ny, nz))
                                continue;

                            Vector3 apos = Chunk.ChunkBlockToWorldCoords(ncpos, new IndexPosition(nx, ny, nz))
                                - Block.HALF_CUBE_3D_SIZE;

                            float dist = Maths.Distance(apos, origin);
                            if (dist > radius)
                                continue;

                            int damage = (int)(14 * (1f - (dist / radius)));

                            if (ncpos != cpos)
                            {
                                Chunk otherChunk;
                                if (Terrain.Chunks.TryGetValue(ncpos, out otherChunk))
                                    otherChunk.DamageBlock(new IndexPosition(nx, ny, nz), damage);
                            }
                            else
                                chunk.DamageBlock(new IndexPosition(nx, ny, nz), damage);
                        }
            }

            // Fling other grenades
            radius = explosion.PlayerRadius;

            for (int i = 0; i < grenades.Count; i++)
            {
                GrenadeEntity grenade = grenades[i];

                Vector3 dirTo = grenade.Transform.Position - origin;
                float dist = dirTo.Length;
                if (dist > radius)
                    continue;

                dirTo += new Vector3(0, 1, 0);
                dirTo = dirTo.Normalize();

                float kickBack = 60 * (1f - (dist / radius));
                grenade.PhysicsBody.Velocity += dirTo * kickBack;
                grenade.Transform.Position.Y += 0.01f;
            }
        }

        public override void Update(float deltaTime)
        {
            if (Terrain != null && Terrain.Ready)
                base.Update(deltaTime);
        }

        public override void Draw()
        {
            if (Terrain != null)
                Terrain.Render(Renderer);

            base.Draw();
        }
    }
}
