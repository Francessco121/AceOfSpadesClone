using AceOfSpades.Editor.World.WorldObjects;
using AceOfSpades.Graphics;
using AceOfSpades.IO;
using Dash.Engine;
using Dash.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime;

namespace AceOfSpades.Editor.World
{
    public class EditorWorld : Scene
    {
        public FixedTerrain Terrain { get; private set; }
        public bool ShowChunkBorders;

        MasterRenderer renderer;
        TerrainPhysicsExtension terrainPhys;
        DebugRenderer debugRenderer;
        EditorScreen screen;

        public EditorWorld(EditorScreen screen)
        {
            this.screen = screen;

            terrainPhys = new TerrainPhysicsExtension();
            renderer = MasterRenderer.Instance;
            debugRenderer = renderer.GetRenderer3D<DebugRenderer>();
        }

        public WorldDescription CreateDescription()
        {
            List<WorldObjectDescription> objectDescriptions = new List<WorldObjectDescription>();
            foreach (GameObject go in gameObjects)
            {
                WorldObject wo = go as WorldObject;
                if (wo != null)
                    objectDescriptions.Add(wo.CreateIODescription());
            }

            return new WorldDescription(Terrain, objectDescriptions);
        }

        public void AddNewCommandPost()
        {
            CommandPostObject post = new CommandPostObject(Camera.Active.Position + Camera.Active.LookVector * 50);
            AddGameObject(post);
        }

        public void AddNewIntel()
        {
            IntelObject intel = new IntelObject(Camera.Active.Position + Camera.Active.LookVector * 50);
            AddGameObject(intel);
        }

        public void UnloadTerrain()
        {
            if (Terrain != null)
            {
                Terrain.Dispose();
                Terrain = null;
                terrainPhys.Terrain = null;
                GC.Collect(1, GCCollectionMode.Forced, true, true);
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            }
        }

        public void SetTerrain(FixedTerrain terrain)
        {
            UnloadTerrain();
            Terrain = terrain;
            terrainPhys.Terrain = terrain;

            gameObjects.Clear();
        }

        public void TranslateTerrain(IndexPosition delta)
        {
            for (int x = 0; x < Terrain.Width * Chunk.HSIZE; x++)
                for (int y = 0; y < Terrain.Height * Chunk.VSIZE; y++)
                    for (int z = 0; z < Terrain.Depth * Chunk.HSIZE; z++)
                    {
                        IndexPosition blockPos = new IndexPosition(x, y, z);
                        IndexPosition newBlockPos = blockPos + delta;

                        IndexPosition originChunkPos, originBlockPos;
                        IndexPosition destChunkPos, destBlockPos;

                        AceOfSpades.Terrain.GetLocalBlockCoords(blockPos, out originChunkPos, out originBlockPos);
                        AceOfSpades.Terrain.GetLocalBlockCoords(newBlockPos, out destChunkPos, out destBlockPos);

                        Chunk originChunk, destChunk;

                        Terrain.Chunks.TryGetValue(originChunkPos, out originChunk);
                        Terrain.Chunks.TryGetValue(destChunkPos, out destChunk);

                        if (originChunk != null && destChunk != null && originChunk.IsBlockCoordInRange(originBlockPos))
                            screen.WorldEditor.TerrainEditor.SetBlock(destChunk, originChunk[originBlockPos], destBlockPos);
                    }
        }

        public EditorWorldRaycastResult Raycast()
        {
            return Raycast(Camera.Active.MouseRay);
        }

        public EditorWorldRaycastResult Raycast(Ray ray)
        {
            EditorObjectRaycastResult objectResult = RaycastEditorObjects(ray);
            TerrainRaycastResult terrainResult = RaycastTerrain(ray);

            if (objectResult.Intersects)
                return new EditorWorldRaycastResult(objectResult);
            else if (terrainResult.Intersects)
                return new EditorWorldRaycastResult(terrainResult);
            else
                return new EditorWorldRaycastResult(ray);
        }

        EditorObjectRaycastResult RaycastEditorObjects(Ray ray)
        {
            float closest = float.MaxValue;
            EditorObject closestObject = null;

            for (int i = 0; i < gameObjects.Count; i++)
            {
                EditorObject eo = gameObjects[i] as EditorObject;

                if (eo != null && eo.IsDrawable)
                {
                    float? dist;
                    bool intersects = ray.Intersects(eo.GetCollider(), out dist);

                    if (intersects && dist < closest)
                    {
                        closest = dist.Value;
                        closestObject = eo;
                    }
                }
            }

            if (closestObject != null)
                return new EditorObjectRaycastResult(closestObject, ray, true, ray.GetPoint(closest), closest);
            else
                return new EditorObjectRaycastResult(ray);
        }

        TerrainRaycastResult RaycastTerrain(Ray ray)
        {
            if (Terrain != null)
                return terrainPhys.Raycast(ray, false);
            else
                return new TerrainRaycastResult(ray);
        }

        public override void Update(float deltaTime)
        {
            if (Terrain != null)
            {
                Terrain.CullingFrustum = Camera.Active.ViewFrustum;
                Terrain.Update(deltaTime);
            }

            base.Update(deltaTime);
        }

        public override void Draw()
        {
            if (Terrain != null)
                Terrain.Render(renderer);

            if (ShowChunkBorders)
            {
                foreach (Chunk chunk in Terrain.Chunks.Values)
                    debugRenderer.Batch(chunk.BoundingBox);
            }

            base.Draw();
        }
    }
}
