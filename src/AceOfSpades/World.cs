using AceOfSpades.Characters;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Physics;
using System;
using System.Collections.Generic;
using System.Runtime;

/* World.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public abstract class World : Scene
    {
        public PhysicsEngine Physics { get { return physics; } }
        public TerrainPhysicsExtension TerrainPhysics { get { return terrainPhysics; } }
        public FixedTerrain Terrain { get; private set; }

        public virtual float TimeOfDay { get; set; }

        public Color TeamAColor = new Color(200, 10, 10);
        public Color TeamBColor = new Color(10, 10, 200);

        PhysicsEngine physics;
        TerrainPhysicsExtension terrainPhysics;

        protected List<GrenadeEntity> grenades { get; } = new List<GrenadeEntity>();
        protected List<GrenadeEntity> grenadesToRemove { get; } = new List<GrenadeEntity>();
        protected List<MelonEntity> melons { get; } = new List<MelonEntity>();
        protected List<MelonEntity> melonsToRemove { get; } = new List<MelonEntity>();

        protected int? rollbackTime { get; private set; }

        List<WorldAudioSource> audioSources = new List<WorldAudioSource>();
        List<WorldAudioSource> audioSourcesToRemove = new List<WorldAudioSource>();

        public World()
        {
            PhysicsEngine.GlobalGravity = new Vector3(0, -9.81f * 8, 0);
            physics = new PhysicsEngine(60);
            terrainPhysics = new TerrainPhysicsExtension();
            physics.AddExtension(terrainPhysics);

            AddComponent(physics);
        }

        public virtual void BeginRollback(int timeFrame)
        {
            rollbackTime = timeFrame;
        }

        public virtual void EndRollback()
        {
            rollbackTime = null;
        }

        public virtual WorldRaycastResult Raycast(Ray ray, bool ignoreTerrainNonColliders, float maxDist = float.MaxValue, params Player[] ignorePlayers)
        {
            TerrainRaycastResult tResult = RaycastTerrain(ray, ignoreTerrainNonColliders, maxDist);
            PlayerRaycastResult pResult = RaycastPlayers(ray, maxDist, ignorePlayers);

            if (tResult.Intersects && (!pResult.Intersects || (pResult.IntersectionDistance.Value > tResult.IntersectionDistance.Value)))
                return new WorldRaycastResult(tResult);
            else if (pResult.Intersects && pResult.Player.Health > 0)
                return new WorldRaycastResult(pResult);
            else
                return new WorldRaycastResult(ray);
        }
        public abstract PlayerRaycastResult RaycastPlayers(Ray ray, float maxDist = float.MaxValue, params Player[] ignore);
        public virtual TerrainRaycastResult RaycastTerrain(Ray ray, bool ignoreNonColliders = true, float maxDist = float.MaxValue)
        {
            if (terrainPhysics != null && terrainPhysics.Terrain != null)
                return terrainPhysics.Raycast(ray, ignoreNonColliders, maxDist);
            else
                return new TerrainRaycastResult(ray);
        }

        public Color GetTeamColor(Team team)
        {
            if (team == Team.A)
                return TeamAColor;
            else if (team == Team.B)
                return TeamBColor;
            else
                return Color.White; 
        }

        public abstract void FireBullet(Player player, Vector3 origin, Vector3 dir, Vector3 recoil, 
            int blockDamage, float playerDamage, float maxDist = float.MaxValue);
        public virtual void GunFired(float verticalRecoil, float horizontalRecoil, float kickback) { }
        public abstract Block SetBlock(IndexPosition chunkIndex, IndexPosition blockPos, Block block, bool placement);
        public abstract void ThrowGrenade(Player owner, Vector3 origin, Vector3 dir, float power);
        public abstract void ShootMelon(Player owner, Vector3 origin, Vector3 dir);
        public abstract void Explode(Explosion explosion);
        public virtual void BulletFired(Gun gun) { }

        public void PlayWorldAudio(WorldAudioSource source)
        {
            audioSources.Add(source);
        }

        public PlayerRaycastResult RaycastPlayer(Vector3 origin, Player player, float maxDist = 2000f)
        {
            Vector3 dir = (player.Transform.Position - origin).Normalize();
            Ray ray = new Ray(origin, dir);

            TerrainRaycastResult tResult = TerrainPhysics.Raycast(ray, true, maxDist);
            float? dist;
            PhysicsBodyComponent playerPhysics = player.GetComponent<PhysicsBodyComponent>();
            bool hitPlayer = ray.Intersects(playerPhysics.GetCollider(), out dist);

            if (hitPlayer && dist.Value <= maxDist && (!tResult.Intersects || dist.Value < tResult.IntersectionDistance.Value))
                return new PlayerRaycastResult(ray, true, ray.Origin + ray.Direction * dist.Value, dist, player);
            else
                return new PlayerRaycastResult(ray);
        }

        public void SetTerrain(FixedTerrain terrain)
        {
            if (Terrain != null)
                Terrain.Dispose();

            Terrain = terrain;
            terrainPhysics.Terrain = terrain;
        }

        public override void Update(float deltaTime)
        {
            foreach (GrenadeEntity g in grenades)
                if (g.IsDed)
                    grenadesToRemove.Add(g);

            foreach (GrenadeEntity g in grenadesToRemove)
            {
                grenades.Remove(g);
                g.Dispose();
            }

            grenadesToRemove.Clear();

            foreach (MelonEntity m in melons)
                if (m.IsDed)
                    melonsToRemove.Add(m);

            foreach (MelonEntity m in melonsToRemove)
            {
                melons.Remove(m);
                m.Dispose();
            }

            melonsToRemove.Clear();

            foreach (WorldAudioSource source in audioSources)
            {
                if (source.IsDone())
                    audioSourcesToRemove.Add(source);
            }

            foreach (WorldAudioSource source in audioSourcesToRemove)
            {
                audioSources.Remove(source);
                source.Dispose();
            }

            audioSourcesToRemove.Clear();

            if (Terrain != null)
            {
                if (Camera.Active != null)
                    Terrain.CullingFrustum = Camera.Active.ViewFrustum;

                Terrain.Update(1f / 60f);
            }

            base.Update(deltaTime);
        }

        public override void Dispose()
        {
            if (Terrain != null)
            {
                TerrainPhysics.Terrain = null;
                Terrain.Dispose();
                Terrain = null;

                // Clean up LOH
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            }

            if (audioSources.Count > 0)
            {
                foreach (WorldAudioSource source in audioSources)
                    source.Dispose();

                audioSources.Clear();
            }

            base.Dispose();
        }
    }
}
