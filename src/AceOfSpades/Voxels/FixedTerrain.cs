using AceOfSpades.Net;
using AceOfSpades.TerrainThreading;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using System;

/* FixedTerrain.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class FixedTerrain : Terrain
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        public bool Ready { get; private set; }

        public int UnfinishedChunks { get; private set; }

        static TerrainWorker[] workers;

        TerrainWorkerAction currentPreAction;
        bool preGenerating;

        const float applyWorkDelay = 0;
        float applyWorkTime = applyWorkDelay;

        public FixedTerrain(MasterRenderer renderer)
            : base(renderer)
        {
            CreateWorkers();
        }

        public int GetGlobalYAt(int globalBlockX, int globalBlockZ)
        {
            IndexPosition cIndex, bIndex;
            GetLocalBlockCoords(new IndexPosition(globalBlockX, 0, globalBlockZ), out cIndex, out bIndex);
            Chunk.WrapBlockCoords(bIndex.X, 0, bIndex.Z, cIndex, out bIndex, out cIndex);

            for (int yChunk = Height - 1; yChunk >= 0; yChunk--)
            {
                Chunk chunk;
                if (Chunks.TryGetValue(new IndexPosition(cIndex.X, yChunk, cIndex.Z), out chunk))
                {
                    for (int y = Chunk.VSIZE - 1; y >= 0; y--)
                    {
                        if (chunk.Blocks[bIndex.Z, y, bIndex.X] != Block.AIR)
                            return y + yChunk * Chunk.VSIZE;
                    }
                }
            }

            return 0;
        }

        public void GenerateFlat(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    for (int z = 0; z < depth; z++)
                    {
                        IndexPosition ipos = new IndexPosition(x, y, z);
                        Chunk chunk = new Chunk(this, ipos, ChunkToWorldCoords(ipos));
                        chunk.InitBlocks(Chunk.HSIZE, Chunk.VSIZE, Chunk.HSIZE);
                        chunk.State = ChunkState.Unbuilt;

                        if (y == 0)
                        {
                            for (int bx = 0; bx < chunk.Width; bx++)
                                for (int by = 0; by < chunk.Height; by++)
                                    for (int bz = 0; bz < chunk.Depth; bz++)
                                    {
                                        chunk.Blocks[bz, by, bx] = by == chunk.Height - 1 ? Block.GRASS : Block.STONE; 
                                    }

                            //chunk.BakeColors();
                            //AllocateWork(chunk, TerrainWorkerAction.BuildMesh);
                        }

                        Chunks.TryAdd(ipos, chunk);
                    }

           // Ready = true;
            preGenerating = true;
        }

        public void CreatedFromFile()
        {
            Ready = true;
            preGenerating = false;

            int sx = 0, sy = 0, sz = 0;
            foreach (IndexPosition chunk in Chunks.Keys)
            {
                sx = Math.Max(sx, chunk.X + 1);
                sy = Math.Max(sy, chunk.Y + 1);
                sz = Math.Max(sz, chunk.Z + 1);
            }

            UnfinishedChunks = sx * sy * sz;

            Width = sx;
            Height = sy;
            Depth = sz;
        }

        void CreateWorkers()
        {
            if (workers == null)
            {
                workers = new TerrainWorker[Math.Max(Environment.ProcessorCount - (GlobalNetwork.IsServer ? 2 : 1), 2)];
                for (int i = 0; i < workers.Length; i++)
                    workers[i] = new TerrainWorker();

                DashCMD.WriteImportant("[FixedTerrain] Created {0} background threads.", workers.Length);
            }

            for (int i = 0; i < workers.Length; i++)
                workers[i].SetTerrain(this);
        }

        public void Generate(int width, int height, int depth)
        {
            Chunk.rand = Maths.RandomRange(-1, 1);

            Width = width;
            Height = height;
            Depth = depth;

            currentPreAction = TerrainWorkerAction.Populate;
            preGenerating = true;
            PopulateChunks();
        }

        public void UpdateChunk(Chunk chunk)
        {
            if (Ready)
                AllocateWork(chunk, TerrainWorkerAction.BuildMesh);
        }

        void AllocateWork(Chunk chunk, TerrainWorkerAction action)
        {
            if (GlobalNetwork.IsServer && action != TerrainWorkerAction.Populate)
                return;

            chunk.BeingWorkedOn = true;

            // Find the first worker that isnt busy, or the worker with the smallest 
            // work count and add the action to it's queue.
            TerrainWorker worker = null;
            for (int i = 0; i < workers.Length; i++)
            {
                TerrainWorker _worker = workers[i];
                if (!_worker.IsBusy && (worker == null || _worker.WorkCount < worker.WorkCount))
                    worker = _worker;
                else if (i == workers.Length - 1 && worker == null)
                    worker = _worker;
            }

            worker.Enqueue(chunk, action);
        }

        void PopulateChunks()
        {
            int i = 0;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        IndexPosition ipos = new IndexPosition(x, y, z);

                        Chunk chunk = new Chunk(this, ipos, ChunkToWorldCoords(ipos));
                        Chunks.TryAdd(ipos, chunk);

                        workers[i].Enqueue(chunk, TerrainWorkerAction.Populate);

                        i++;
                        if (i == workers.Length)
                            i = 0;
                    }
        }

        void ShapeChunks()
        {
            if (GlobalNetwork.IsServer)
                return;

            int i = 0;

            foreach (Chunk chunk in Chunks.Values)
            {
                workers[i].Enqueue(chunk, TerrainWorkerAction.Shape);

                i++;
                if (i == workers.Length)
                    i = 0;
            }
        }

        void BuildChunkMeshes()
        {
            if (GlobalNetwork.IsServer)
                return;

            int i = 0;

            foreach (Chunk chunk in Chunks.Values)
            {
                workers[i].Enqueue(chunk, TerrainWorkerAction.BuildMesh);

                i++;
                if (i == workers.Length)
                    i = 0;
            }
        }

        bool IsWorkersDone()
        {
            for (int i = 0; i < workers.Length; i++)
                if (workers[i].IsBusy)
                    return false;

            return true;
        }

        public override void Update(float deltaTime)
        {
            if (!Ready && preGenerating)
            {
                // Handle pre-generation
                if (IsWorkersDone())
                {
                    if (currentPreAction == TerrainWorkerAction.Populate)
                    {
                        currentPreAction = TerrainWorkerAction.Shape;
                        ShapeChunks();
                    }
                    else if (currentPreAction == TerrainWorkerAction.Shape && !GlobalNetwork.IsServer)
                    {
                        currentPreAction = TerrainWorkerAction.BuildMesh;
                        BuildChunkMeshes();
                    }
                    else if (currentPreAction == TerrainWorkerAction.BuildMesh && !GlobalNetwork.IsServer)
                    {
                        while (MeshReadyChunks.Count > 0)
                        {
                            Chunk chunk;
                            if (MeshReadyChunks.TryDequeue(out chunk))
                            {
                                chunk.CreateOrUpdateMesh(BufferUsageHint.DynamicDraw);
                                chunk.State = ChunkState.Renderable;
                                chunk.BeingWorkedOn = false;
                            }
                        }

                        Ready = true;
                    }
                    else if (currentPreAction != TerrainWorkerAction.Populate && GlobalNetwork.IsServer)
                        Ready = true;
                }
            }
            else if (!GlobalNetwork.IsServer)
            {
                // Every so often, check if any chunks need an update.
                if (applyWorkTime > 0)
                    applyWorkTime -= deltaTime;
                else
                {
                    applyWorkTime = applyWorkDelay;
                    foreach (Chunk chunk in Chunks.Values)
                    {
                        if (!chunk.BeingWorkedOn && chunk.IsDirty)
                        {
                            chunk.State = ChunkState.Unbuilt;
                            AllocateWork(chunk, TerrainWorkerAction.BuildMesh);
                        }
                    }
                }

                // Finish mesh ready chunks
                while (MeshReadyChunks.Count > 0)
                {
                    Chunk chunk;
                    if (MeshReadyChunks.TryDequeue(out chunk))
                    {
                        chunk.CreateOrUpdateMesh(BufferUsageHint.DynamicDraw);
                        chunk.State = ChunkState.Renderable;
                        chunk.BeingWorkedOn = false;
                        chunk.IsDirty = false;
                    }
                }
            }
        }

        public override void Render(MasterRenderer renderer)
        {
            RenderableChunks = 0;
            CulledChunks = 0;
            int unfinished = 0;

            foreach (Chunk chunk in Chunks.Values)
            {
                if (chunk.State != ChunkState.Renderable)
                    unfinished++;

                if ((chunk.State == ChunkState.Renderable || chunk.State > ChunkState.Unshaped) && !chunk.IsEmpty
                    && CullingFrustum.Intersects(chunk.BoundingBox))
                {
                    RenderableChunks++;
                    chunk.Culled = false;
                }
                else
                {
                    CulledChunks++;
                    chunk.Culled = true;
                }

                chunkRenderer.Batch(chunk);
            }

            UnfinishedChunks = unfinished;
        }
    }
}
