using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Frustum = Dash.Engine.Frustum;

namespace AceOfSpades
{
    public abstract class Terrain : IDisposable
    {
        public ConcurrentDictionary<IndexPosition, Chunk> Chunks { get; private set; }
        public Frustum CullingFrustum;

        public int CulledChunks { get; protected set; }
        public int RenderableChunks { get; protected set; }

        public event EventHandler<BlockChange> OnModified;

        public List<BlockChange> AllChanges { get; private set; }
        public bool TrackChanges;

        public bool LockBottomLayer;

        internal ConcurrentQueue<Chunk> MeshReadyChunks { get; private set; }

        protected EntityRenderer entRenderer;
        protected ChunkRenderer chunkRenderer;

        public Terrain(MasterRenderer renderer)
        {
            if (!GlobalNetwork.IsServer)
            {
                entRenderer = renderer.GetRenderer3D<EntityRenderer>();
                chunkRenderer = renderer.GetRenderer3D<ChunkRenderer>();
                MeshReadyChunks = new ConcurrentQueue<Chunk>();
            }

            Chunks = new ConcurrentDictionary<IndexPosition, Chunk>();
            AllChanges = new List<BlockChange>();

            TrackChanges = GlobalNetwork.IsServer;
        }

        public void AddChange(Chunk chunk, Block block, IndexPosition ipos)
        {
            if (TrackChanges)
            {
                BlockChange change = new BlockChange(chunk, block, ipos);
                AllChanges.Add(change);

                if (OnModified != null)
                    OnModified(this, change);
            }
        }

        public static IndexPosition GetGlobalBlockCoords(IndexPosition chunkPos, IndexPosition blockPos)
        {
            return blockPos
                + new IndexPosition(
                    chunkPos.X * Chunk.HSIZE,
                    chunkPos.Y * Chunk.VSIZE,
                    chunkPos.Z * Chunk.HSIZE);
        }

        public static void GetLocalBlockCoords(IndexPosition global, 
            out IndexPosition chunkPos, out IndexPosition blockPos)
        {
            int cx = global.X / Chunk.HSIZE;
            int cy = global.Y / Chunk.VSIZE;
            int cz = global.Z / Chunk.HSIZE;

            chunkPos = new IndexPosition(cx, cy, cz);
            blockPos = global
                - new IndexPosition(
                    cx * Chunk.HSIZE,
                    cy * Chunk.VSIZE,
                    cz * Chunk.HSIZE);
        }

        #region Conversions
        public static IndexPosition WorldToChunkCoords(Vector3 worldCoords)
        {
            return new IndexPosition(
                Maths.NegativeRound(worldCoords.X / Chunk.UNIT_HSIZE),
                Maths.NegativeRound(worldCoords.Y / Chunk.UNIT_VSIZE),
                Maths.NegativeRound(worldCoords.Z / Chunk.UNIT_HSIZE));
        }

        public static Vector3 ChunkToWorldCoords(IndexPosition pos)
        {
            return ChunkToWorldCoords(pos.X, pos.Y, pos.Z);
        }

        public static Vector3 ChunkToWorldCoords(int x, int y, int z)
        {
            return new Vector3(x * Chunk.UNIT_HSIZE, y * Chunk.UNIT_VSIZE, z * Chunk.UNIT_HSIZE);
        }
        #endregion

        #region Block Location
        public Block FindBlock(IndexPosition tryChunkIndex, int bx, int by, int bz,
            out int fx, out int fy, out int fz, out Chunk chunk)
        {
            IndexPosition fbi;
            IndexPosition fci;
            Chunk.WrapBlockCoords(bx, by, bz, tryChunkIndex, out fbi, out fci);

            bx = fbi.X;
            by = fbi.Y;
            bz = fbi.Z;

            fx = bx;
            fy = by;
            fz = bz;

            if (Chunks.TryGetValue(fci, out chunk) && chunk.State > ChunkState.Unshaped)
            {
                if (chunk.IsBlockCoordInRange(bx, by, bz))
                    return chunk.Blocks[bz, by, bx];
                else
                {
                    IndexPosition wrappedChunk = chunk.WrapBlockCoords(ref bx, ref by, ref bz);
                    if (wrappedChunk == tryChunkIndex) return Block.AIR;
                    return FindBlock(wrappedChunk, bx, by, bz, out fx, out fy, out fz, out chunk);
                }
            }
            else
                return Block.AIR;
        }

        public Block GetBlockInChunk(IndexPosition chunkIndex, int bx, int by, int bz, out Chunk chunk)
        {
            if (Chunks.TryGetValue(chunkIndex, out chunk) && chunk.State > ChunkState.Unshaped)
                return chunk.GetBlockSafe(bx, by, bz);
            else
                return Block.STONE;
        }

        public Block GetBlockInChunkFast(IndexPosition chunkIndex, int bx, int by, int bz)
        {
            Chunk chunk;
            if (Chunks.TryGetValue(chunkIndex, out chunk) && chunk.State > ChunkState.Unshaped)
                return chunk.GetBlockSafe(bx, by, bz);
            else
                return Block.STONE;
        }
        #endregion

        public bool IsChunkPopulated(IndexPosition pos)
        {
            Chunk chunk;
            if (Chunks.TryGetValue(pos, out chunk))
                return chunk.State > ChunkState.Unpopulated;
            else
                return false;
        }

        public bool IsChunkShaped(IndexPosition pos)
        {
            Chunk chunk;
            if (Chunks.TryGetValue(pos, out chunk))
                return chunk.State > ChunkState.Unshaped;
            else
                return false;
        }

        public bool IsChunkShaped(IndexPosition pos, out Chunk chunk)
        {
            if (Chunks.TryGetValue(pos, out chunk))
                return chunk.State > ChunkState.Unshaped;
            else
                return false;
        }

        public abstract void Update(float deltaTime);
        public virtual void Render(MasterRenderer renderer)
        {
            RenderableChunks = 0;
            CulledChunks = 0;

            foreach (Chunk chunk in Chunks.Values)
            {
                if (chunk.State == ChunkState.Renderable && !chunk.IsEmpty
                    && CullingFrustum.Intersects(chunk.BoundingBox))
                {
                    RenderableChunks++;
                    chunkRenderer.Batch(chunk);
                }
                else
                    CulledChunks++;
            }
        }

        public virtual void Dispose()
        {
            foreach (Chunk chunk in Chunks.Values)
                chunk.Dispose();
        }
    }
}
