using Dash.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AceOfSpades.Graphics
{
    // Light calculation uses a Breadth-First Search (BFS)
    // algorithm to set each light value.

    public class ChunkLightingContainer : ILightingContainer
    {
        struct LightRemovalNode
        {
            public IndexPosition Index;
            public short Value;

            public LightRemovalNode(IndexPosition index, short value)
            {
                Index = index;
                Value = value;
            }
        }

        struct LightRequestNode
        {
            public IndexPosition Index;
            public short Value;

            public LightRequestNode(IndexPosition index, short value)
            {
                Index = index;
                Value = value;
            }
        }

        public const int MAX_LIGHT_LEVEL = 31;

        public bool IsInitialSunlightPhaseDone { get; private set; }

        public bool IsDirty
        {
            get
            {
                return !sunlightQueue.IsEmpty
                    || !sunlightRequestQueue.IsEmpty
                    || !sunlightRefillRequestQueue.IsEmpty
                    || !sunlightRemovalRequestQueue.IsEmpty
                    || !sunlightRemovalQueue.IsEmpty;
            }
        }

        readonly ushort[,,] lighting;

        readonly ConcurrentQueue<IndexPosition> sunlightQueue = new ConcurrentQueue<IndexPosition>();
        readonly ConcurrentQueue<LightRequestNode> sunlightRequestQueue = new ConcurrentQueue<LightRequestNode>();
        readonly ConcurrentQueue<IndexPosition> sunlightRefillRequestQueue = new ConcurrentQueue<IndexPosition>();
        readonly ConcurrentQueue<IndexPosition> sunlightRemovalRequestQueue = new ConcurrentQueue<IndexPosition>();
        readonly ConcurrentQueue<LightRemovalNode> sunlightRemovalQueue = new ConcurrentQueue<LightRemovalNode>();

        readonly Chunk chunk;
        readonly Terrain terrain;

        public ChunkLightingContainer(Chunk chunk, Terrain terrain)
        {
            this.chunk = chunk;
            this.terrain = terrain;

            lighting = new ushort[Chunk.HSIZE, Chunk.VSIZE, Chunk.HSIZE];
        }

        public bool IsMeshReady()
        {
            if (IsDirty)
                return false;

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        int numZero = 0;
                        if (dx == 0) numZero++;
                        if (dy == 0) numZero++;
                        if (dz == 0) numZero++;

                        if (numZero == 2)
                        {
                            Chunk neighborChunk;
                            if (terrain.TryGetChunk(chunk.IndexPosition + new IndexPosition(dx, dy, dz), out neighborChunk))
                            {
                                if (neighborChunk.Lighting.IsDirty)
                                    return false;
                            }
                        }
                    }

            return true;
        }

        public bool CanProcess()
        {
            if (IsInitialSunlightPhaseDone)
                return true;

            // We can only process initial sunlight if the chunk above us has already done so,
            // or we are a top-most chunk.
            Chunk aboveChunk;
            if (terrain.TryGetChunk(new IndexPosition(chunk.IndexPosition.X, chunk.IndexPosition.Y + 1, chunk.IndexPosition.Z), out aboveChunk))
            {
                return aboveChunk.Lighting.IsInitialSunlightPhaseDone;
            }

            return true;
        }

        public void Process()
        {
            if (!IsInitialSunlightPhaseDone)
            {
                AddInitialSunlight();
                RunSunlightFill();

                IsInitialSunlightPhaseDone = true;
            }
            else
            {
                ProcessRequestQueues();
            }
        }

        public void RequestSetSunlight(int x, int y, int z, short val)
        {
            sunlightRequestQueue.Enqueue(new LightRequestNode(new IndexPosition(x, y, z), val));
        }

        public void RequestSunlightRefill(int x, int y, int z)
        {
            sunlightRefillRequestQueue.Enqueue(new IndexPosition(x, y, z));
        }

        public void RequestSunlightRemoval(int x, int y, int z)
        {
            sunlightRemovalRequestQueue.Enqueue(new IndexPosition(x, y, z));
        }

        void AddInitialSunlight()
        {
            // Check if there is a chunk above us
            Chunk aboveChunk;
            if (terrain.TryGetChunk(new IndexPosition(chunk.IndexPosition.X, chunk.IndexPosition.Y + 1, chunk.IndexPosition.Z), out aboveChunk))
            {
                ChunkLightingContainer aboveLighting = aboveChunk.Lighting;
                if (!aboveLighting.IsInitialSunlightPhaseDone)
                    aboveLighting.Process();

                // Copy the bottom sunlight values from the above chunk, into the top blocks of this chunk.
                for (int x = 0; x < Chunk.HSIZE; x++)
                    for (int z = 0; z < Chunk.HSIZE; z++)
                    {
                        if (!chunk.IsBlockAt(x, Chunk.VSIZE - 1, z) && !aboveChunk.IsBlockAt(x, 0, z))
                        {
                            SetSunlight(x, Chunk.VSIZE - 1, z, aboveLighting.GetSunlight(x, 0, z));
                            sunlightQueue.Enqueue(new IndexPosition(x, Chunk.VSIZE - 1, z));
                        }
                    }
            }
            else
            {
                // Assume we are the highest chunk in this column, set every air block to max lighting
                // at the very top of the chunk.
                for (int x = 0; x < Chunk.HSIZE; x++)
                    for (int z = 0; z < Chunk.HSIZE; z++)
                    {
                        if (!chunk.IsBlockAt(x, Chunk.VSIZE - 1, z))
                        {
                            SetSunlight(x, Chunk.VSIZE - 1, z, MAX_LIGHT_LEVEL);
                            sunlightQueue.Enqueue(new IndexPosition(x, Chunk.VSIZE - 1, z));
                        }
                    }
            }
        }

        void ProcessRequestQueues()
        {
            while (sunlightRemovalRequestQueue.TryDequeue(out IndexPosition index))
            {
                int x = index.X;
                int y = index.Y;
                int z = index.Z;

                int currentSunlight = GetSunlight(x, y, z);

                if (currentSunlight != 0)
                {
                    sunlightRemovalQueue.Enqueue(new LightRemovalNode(new IndexPosition(x, y, z), (short)currentSunlight));
                    SetSunlight(x, y, z, 0);
                }
            }

            RunSunlightRemove();
            //RunSunlightFill();

            while (sunlightRequestQueue.TryDequeue(out LightRequestNode node))
            {
                int x = node.Index.X;
                int y = node.Index.Y;
                int z = node.Index.Z;

                int currentSunlight = GetSunlight(x, y, z);

                if (currentSunlight < node.Value)
                {
                    SetSunlight(x, y, z, node.Value);
                    sunlightQueue.Enqueue(node.Index);
                }
            }

           // RunSunlightFill();

            while (sunlightRefillRequestQueue.TryDequeue(out IndexPosition index))
            {
                int x = index.X;
                int y = index.Y;
                int z = index.Z;

                int currentSunlight = GetSunlight(x, y, z);

                if (currentSunlight == 0)
                {
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dz = -1; dz <= 1; dz++)
                            {
                                int numZero = 0;
                                if (dx == 0) numZero++;
                                if (dy == 0) numZero++;
                                if (dz == 0) numZero++;

                                if (numZero == 2)
                                {
                                    int nx = x + dx;
                                    int ny = y + dy;
                                    int nz = z + dz;

                                    if (nx < 0 || ny < 0 || nz < 0 || nx >= Chunk.HSIZE || ny >= Chunk.VSIZE || nz >= Chunk.HSIZE)
                                    {
                                        // Indexes are out of this chunk, check for the other chunk and pass on the light value.
                                        int cx = (int)Math.Floor((float)nx / Chunk.HSIZE);
                                        int cy = (int)Math.Floor((float)ny / Chunk.VSIZE);
                                        int cz = (int)Math.Floor((float)nz / Chunk.HSIZE);

                                        Chunk otherChunk;
                                        if (terrain.TryGetChunk(chunk.IndexPosition + new IndexPosition(cx, cy, cz), out otherChunk))
                                        {
                                            ChunkLightingContainer otherLighting = otherChunk.Lighting;

                                            int bx = nx < 0 ? nx + Chunk.HSIZE : nx % Chunk.HSIZE;
                                            int by = ny < 0 ? ny + Chunk.VSIZE : ny % Chunk.VSIZE;
                                            int bz = nz < 0 ? nz + Chunk.HSIZE : nz % Chunk.HSIZE;

                                            // TODO: Should this make a request instead?
                                            otherLighting.sunlightQueue.Enqueue(new IndexPosition(bx, by, bz));
                                        }
                                    }
                                    else
                                    {
                                        sunlightQueue.Enqueue(new IndexPosition(nx, ny, nz));
                                    }
                                }
                            }
                }
            }

            RunSunlightFill();
        }

        void RunSunlightFill()
        {
            // Flood fill sunlight
            while (sunlightQueue.TryDequeue(out IndexPosition index))
            {
                int x = index.X;
                int y = index.Y;
                int z = index.Z;

                int lightLevel = GetSunlight(x, y, z);

                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            int numZero = 0;
                            if (dx == 0) numZero++;
                            if (dy == 0) numZero++;
                            if (dz == 0) numZero++;

                            if (numZero == 2)
                            {
                                int nx = x + dx;
                                int ny = y + dy;
                                int nz = z + dz;

                                if (nx < 0 || ny < 0 || nz < 0 || nx >= Chunk.HSIZE || ny >= Chunk.VSIZE || nz >= Chunk.HSIZE)
                                {
                                    // Indexes are out of this chunk, check for the other chunk and pass on the light value.
                                    int cx = (int)Math.Floor((float)nx / Chunk.HSIZE);
                                    int cy = (int)Math.Floor((float)ny / Chunk.VSIZE);
                                    int cz = (int)Math.Floor((float)nz / Chunk.HSIZE);

                                    Chunk otherChunk;
                                    if (terrain.TryGetChunk(chunk.IndexPosition + new IndexPosition(cx, cy, cz), out otherChunk))
                                    {
                                        ChunkLightingContainer otherLighting = otherChunk.Lighting;

                                        int bx = nx < 0 ? nx + Chunk.HSIZE : nx % Chunk.HSIZE;
                                        int by = ny < 0 ? ny + Chunk.VSIZE : ny % Chunk.VSIZE;
                                        int bz = nz < 0 ? nz + Chunk.HSIZE : nz % Chunk.HSIZE;

                                        if (!otherChunk.IsBlockAt(bx, by, bz) && otherLighting.GetSunlight(bx, by, bz) + 2 <= lightLevel)
                                        {
                                            int newLight = Math.Max(lightLevel - 1, 0);

                                            otherLighting.RequestSetSunlight(bx, by, bz, (short)newLight);
                                        }
                                    }
                                }
                                else if (!chunk.IsBlockAt(nx, ny, nz) && GetSunlight(nx, ny, nz) + 2 <= lightLevel)
                                {
                                    // Update sunlight level in our chunk
                                    int newLight = Math.Max(lightLevel == MAX_LIGHT_LEVEL && dy == -1
                                        ? MAX_LIGHT_LEVEL : lightLevel - 1, 0);

                                    SetSunlight(nx, ny, nz, newLight);
                                    sunlightQueue.Enqueue(new IndexPosition(nx, ny, nz));
                                }
                            }
                        }
            }
        }

        void RunSunlightRemove()
        {
            while (sunlightRemovalQueue.TryDequeue(out LightRemovalNode node))
            {
                int x = node.Index.X;
                int y = node.Index.Y;
                int z = node.Index.Z;
                int lightLevel = node.Value;

                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            int numZero = 0;
                            if (dx == 0) numZero++;
                            if (dy == 0) numZero++;
                            if (dz == 0) numZero++;

                            if (numZero == 2)
                            {
                                int nx = x + dx;
                                int ny = y + dy;
                                int nz = z + dz;

                                if (nx < 0 || ny < 0 || nz < 0 || nx >= Chunk.HSIZE || ny >= Chunk.VSIZE || nz >= Chunk.HSIZE)
                                {
                                    // Indexes are out of this chunk, check for the other chunk and pass on the light value.
                                    int cx = (int)Math.Floor((float)nx / Chunk.HSIZE);
                                    int cy = (int)Math.Floor((float)ny / Chunk.VSIZE);
                                    int cz = (int)Math.Floor((float)nz / Chunk.HSIZE);

                                    Chunk otherChunk;
                                    if (terrain.TryGetChunk(chunk.IndexPosition + new IndexPosition(cx, cy, cz), out otherChunk))
                                    {
                                        ChunkLightingContainer otherLighting = otherChunk.Lighting;

                                        int bx = nx < 0 ? nx + Chunk.HSIZE : nx % Chunk.HSIZE;
                                        int by = ny < 0 ? ny + Chunk.VSIZE : ny % Chunk.VSIZE;
                                        int bz = nz < 0 ? nz + Chunk.HSIZE : nz % Chunk.HSIZE;

                                        int neighborLevel = otherLighting.GetSunlight(bx, by, bz);

                                        if (neighborLevel != 0 && ((dy == -1 && lightLevel == MAX_LIGHT_LEVEL) || neighborLevel < lightLevel))
                                        {
                                            otherLighting.RequestSunlightRemoval(bx, by, bz);
                                        }
                                        else if (neighborLevel >= lightLevel)
                                        {
                                            // TODO: Should this make a request?
                                            otherLighting.sunlightQueue.Enqueue(new IndexPosition(bx, by, bz));
                                        }
                                    }
                                }
                                else
                                {
                                    int neighborLevel = GetSunlight(nx, ny, nz);

                                    if (neighborLevel != 0 && ((dy == -1 && lightLevel == MAX_LIGHT_LEVEL) || neighborLevel < lightLevel))
                                    {
                                        SetSunlight(nx, ny, nz, 0);
                                        sunlightRemovalQueue.Enqueue(new LightRemovalNode(new IndexPosition(nx, ny, nz), (short)neighborLevel));
                                    }
                                    else if (neighborLevel >= lightLevel)
                                    {
                                        sunlightQueue.Enqueue(new IndexPosition(nx, ny, nz));
                                    }
                                }
                            }
                        }
            }
        }

        int GetSunlight(int x, int y, int z)
        {
            return (lighting[z, y, x] >> 8) & 0xFF;
        }

        void SetSunlight(int x, int y, int z, int val)
        {
            lighting[z, y, x] = (ushort)((lighting[z, y, x] & 0xFF) | (val << 8));
        }

        int GetNormalLight(int x, int y, int z)
        {
            return lighting[z, y, x] & 0xFF;
        }

        void SetNormalLight(int x, int y, int z, int val)
        {
            lighting[z, y, x] = (ushort)((lighting[z, y, x] & 0xFF00) | val);
        }

        public float LightingAt(int x, int y, int z)
        {
            int sunlight, normallight;

            if (x < 0 || y < 0 || z < 0 || x >= Chunk.HSIZE || y >= Chunk.VSIZE || z >= Chunk.HSIZE)
            {
                int dx = (int)Math.Floor((float)x / Chunk.HSIZE);
                int dy = (int)Math.Floor((float)y / Chunk.VSIZE);
                int dz = (int)Math.Floor((float)z / Chunk.HSIZE);

                Chunk otherChunk;
                if (terrain.TryGetChunk(chunk.IndexPosition + new IndexPosition(dx, dy, dz), out otherChunk))
                {
                    ChunkLightingContainer otherLighting = otherChunk.Lighting;
                    int bx = x < 0 ? x + Chunk.HSIZE : x % Chunk.HSIZE;
                    int by = y < 0 ? y + Chunk.VSIZE : y % Chunk.VSIZE;
                    int bz = z < 0 ? z + Chunk.HSIZE : z % Chunk.HSIZE;

                    sunlight = otherLighting.GetSunlight(bx, by, bz);
                    normallight = otherLighting.GetNormalLight(bx, by, bz);
                }
                else
                    return 1f;
            }
            else
            {
                sunlight = GetSunlight(x, y, z);
                normallight = GetNormalLight(x, y, z);
            }

            return Math.Max(Math.Max(sunlight, normallight) / 31f, 0f);
        }
    }
}
