using Dash.Engine;
using Dash.Engine.Physics;
using System;
using System.Collections.Generic;

/* TerrainPhysicsExtension.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class TerrainPhysicsExtension : IPhysicsEngineExtension
    {
        public Terrain Terrain;

        public bool IsActive
        {
            get { return Terrain != null; }
        }

        public const int MAX_TERRAIN_RAY_CHECKS = 10000;
        public const int MAX_TERRAIN_CHUNK_MISSES = 100;

        Queue<PhysicsBlock> unusedPhysBlocks;
        List<PhysicsBlock> _terrainBlockStorage;
        List<PhysicsBodyComponent> _terrainBlockCache;

        public TerrainPhysicsExtension()
        {
            _terrainBlockStorage = new List<PhysicsBlock>();
            _terrainBlockCache = new List<PhysicsBodyComponent>();
            unusedPhysBlocks = new Queue<PhysicsBlock>();
        }

        public bool CanCheck(IntersectionType intersectType, bool objectIsStatic)
        {
            return intersectType != IntersectionType.Soft && !objectIsStatic;
        }

        /// <summary>
        /// WARNING: Not finalized!
        /// </summary>
        public bool AABBIntersectsTerrain(AxisAlignedBoundingBox aabb, out float highestY)
        {
            highestY = float.MinValue;
            bool intersects = false;

            for (int x = -2; x <= 2; x++)
                for (int y = -2; y <= 2; y++)
                    for (int z = -2; z <= 2; z++)
                    {
                        Vector3 off = new Vector3(Block.CUBE_SIZE * x, Block.CUBE_SIZE * y, Block.CUBE_SIZE * z);
                        IndexPosition bpos = Chunk.WorldToBlockCoords(aabb.Center + off);
                        IndexPosition cpos = Terrain.WorldToChunkCoords(aabb.Center + off);
                        bpos = Chunk.BlockToChunkBlockCoords(cpos, bpos);

                        Chunk chunk;
                        if (Terrain.Chunks.TryGetValue(cpos, out chunk))
                        {
                            if (chunk.Blocks[bpos.Z, bpos.Y, bpos.X].HasCollision())
                            {
                                Vector3 cubeWorldPos = chunk.Position + (bpos * Block.CUBE_3D_SIZE) - Block.HALF_CUBE_3D_SIZE;
                                AxisAlignedBoundingBox AABoundingBox = 
                                    new AxisAlignedBoundingBox(cubeWorldPos, cubeWorldPos + Block.CUBE_3D_SIZE);

                                if (aabb.Intersects(AABoundingBox))
                                {
                                    intersects = true;
                                    highestY = Math.Max(highestY, AABoundingBox.Max.Y);
                                }
                            }
                        }
                    }

            return intersects;
        }

        public TerrainRaycastResult Raycast(Ray ray, bool ignoreNonColliders, float maxDist = float.MaxValue)
        {
            IndexPosition? blockIntersection = null;
            CubeSide? side = null;
            float? intersectDist = null;
            Block? interBlock = null;

            bool rayInTheVoidOfSpace = true;
            int chunkMisses = 0;

            IndexPosition? lastChunkIndex = null;
            IndexPosition? lastBlockIndex = null;
            Chunk lastChunk = null;

            for (int i = 0; i < MAX_TERRAIN_RAY_CHECKS && i < maxDist; i++)
            {
                // Calculate the world position to check
                Vector3 tryWorldPos = ray.Origin + (ray.Direction * i) + Block.HALF_CUBE_3D_SIZE;
                IndexPosition chunkIndex = Terrain.WorldToChunkCoords(tryWorldPos);

                Chunk inChunk;

                // Check the chunk the ray is in.
                // Only try to get the chunk if the rays chunkIndex moved, or its the first pass
                if (!lastChunkIndex.HasValue || (lastChunk != null && chunkIndex != lastChunkIndex))
                {
                    lastBlockIndex = null;
                    Terrain.IsChunkShaped(Terrain.WorldToChunkCoords(tryWorldPos), out inChunk);
                }
                else
                    inChunk = lastChunk;

                // If we can check this chunk
                if (inChunk != null)
                {
                    rayInTheVoidOfSpace = false;

                    // Calculate the block coordinate to try
                    IndexPosition blockPos = inChunk.BlockToChunkBlockCoords(Chunk.WorldToBlockCoords(tryWorldPos));

                    // If this is the first block checked for this chunk, or the index changed, continue
                    if (!lastBlockIndex.HasValue || blockPos != lastBlockIndex)
                    {
                        bool blockFound = false;
                        float closestDist = float.MaxValue;

                        // For a 1 block radius around the block found, see if any
                        // surrounding blocks are intersecting the ray, and are closer
                        // to the ray origin. This prevents the mild error in getting the first
                        // intersecting block, since we are just using block coordinates.
                        for (int x = -1; x <= 1; x++)
                            for (int y = -1; y <= 1; y++)
                                for (int z = -1; z <= 1; z++)
                                {
                                    IndexPosition cpos;
                                    Block type = inChunk.GetBlockSafeFull(blockPos.X + x, blockPos.Y + y, blockPos.Z + z, 
                                        out cpos);

                                    if (cpos != inChunk.IndexPosition)
                                        continue;

                                    if (type != Block.AIR && !ignoreNonColliders
                                        || type.HasCollision())
                                    {
                                        // Calculate the new blocks positions
                                        IndexPosition newIndexPos = blockPos + new IndexPosition(x, y, z);
                                        Vector3 cubeWorldPos = inChunk.Position
                                            + (newIndexPos * Block.CUBE_3D_SIZE) - Block.HALF_CUBE_3D_SIZE;

                                        // If this blocks distance is smaller than the current, continue
                                        float dist = Maths.DistanceSquared(cubeWorldPos, ray.Origin);
                                        if (dist < closestDist)
                                        {
                                            AxisAlignedBoundingBox aabb = 
                                                new AxisAlignedBoundingBox(cubeWorldPos, cubeWorldPos + Block.CUBE_3D_SIZE);

                                            // If this block intersects the ray,
                                            // it is the newly intersected block.
                                            float? interDist;
                                            CubeSide interSide;
                                            if (ray.Intersects(aabb, out interDist, out interSide))
                                            {
                                                closestDist = dist;
                                                side = interSide;
                                                blockFound = true;
                                                blockIntersection = newIndexPos;
                                                intersectDist = interDist;
                                                interBlock = type;
                                            }
                                        }
                                    }
                                }

                        // If any block was found to actually intersect the ray
                        // by here, the closest block was set so just set the chunk
                        // and return.
                        if (blockFound)
                        {
                            Vector3 interPosition = ray.Origin + ray.Direction * intersectDist.Value;
                            return new TerrainRaycastResult(ray, true, interPosition, intersectDist, 
                                inChunk, blockIntersection, interBlock, side);
                        }
                    }

                    lastBlockIndex = blockPos;
                }
                // If the ray missed to many chunks then stop checking because this means the rest of the ray
                // is shooting out into empty space
                else if (!rayInTheVoidOfSpace && chunkMisses++ > MAX_TERRAIN_CHUNK_MISSES)
                    break;

                lastChunk = inChunk;
                if (lastChunk != null)
                    lastChunkIndex = chunkIndex;
                else
                    lastChunkIndex = null;
            }

            // No intersection at this point
            return new TerrainRaycastResult(ray);
        }

        PhysicsBlock GetNewPhysicsBlock(Block block, Vector3 blockWorldPosition, IndexPosition blockIPos, Chunk chunk)
        {
            if (unusedPhysBlocks.Count > 0)
            {
                PhysicsBlock b = unusedPhysBlocks.Dequeue() as PhysicsBlock;
                b.Block = block;
                b.Transform.Position = blockWorldPosition;
                b.BlockPos = blockIPos;
                b.Chunk = chunk;

                return b;
            }
            else
                return new PhysicsBlock(block, blockWorldPosition, blockIPos, chunk);
        }

        public void RecyclePhysicsObjects()
        {
            for (int i = 0; i < _terrainBlockStorage.Count; i++)
                unusedPhysBlocks.Enqueue(_terrainBlockStorage[i]);

            _terrainBlockStorage.Clear();
        }

        public IEnumerable<PhysicsBodyComponent> GetBroadphaseIntersections(AxisAlignedBoundingBox broad)
        {
            _terrainBlockCache.Clear();

            // Convert the broad AABB to an IndexPosition AABB
            IndexPosition min = new IndexPosition(
                Maths.NegativeRound(broad.Min.X / Block.CUBE_SIZE),
                Maths.NegativeRound(broad.Min.Y / Block.CUBE_SIZE),
                Maths.NegativeRound(broad.Min.Z / Block.CUBE_SIZE));

            IndexPosition max = new IndexPosition(
                (int)Math.Ceiling(broad.Max.X / Block.CUBE_SIZE),
                (int)Math.Ceiling(broad.Max.Y / Block.CUBE_SIZE),
                (int)Math.Ceiling(broad.Max.Z / Block.CUBE_SIZE));

            // Calculate the chunk index to use as reference
            IndexPosition chunkIndex = Terrain.WorldToChunkCoords(broad.Center);

            // Try each block
            for (int x = min.X; x <= max.X; x++)
                for (int y = min.Y; y <= max.Y; y++)
                    for (int z = min.Z; z <= max.Z; z++)
                    {
                        // Calculate the index positions for the current block
                        IndexPosition blockIndexWorld = new IndexPosition(x, y, z);
                        IndexPosition blockChunkIndex = Chunk.BlockToChunkBlockCoords(chunkIndex, blockIndexWorld);

                        // Find the block
                        Chunk chunk;
                        int fx, fy, fz;
                        Block block = Terrain.FindBlock(chunkIndex, blockChunkIndex.X, blockChunkIndex.Y, blockChunkIndex.Z,
                            out fx, out fy, out fz, out chunk);

                        // If this block has collision, process it
                        if (block.HasCollision())
                        {
                            IndexPosition blockIPos = new IndexPosition(fx, fy, fz);
                            // Calculate the blocks world position and create a PhyicsBlock from it
                            Vector3 blockWorldPosition = Chunk.ChunkBlockToWorldCoords(chunk.Position, blockIPos);
                            PhysicsBlock physBlock = GetNewPhysicsBlock(block, blockWorldPosition, blockIPos, chunk);

                            // Grab its collider
                            PhysicsBodyComponent physicsBody = physBlock.GetComponent<PhysicsBodyComponent>();
                            AxisAlignedBoundingBox physBlockCollider = physicsBody.GetCollider();
                            //DebugAABBs.Add(physBlockCollider as AABoundingBox);

                            // Check if the block intersects the broad, 
                            // if it does this block is valid for collision response
                            // TODO: Might be able to remove the intersect check
                            if (broad.Intersects(physBlockCollider))
                            {
                                _terrainBlockStorage.Add(physBlock);
                                _terrainBlockCache.Add(physicsBody);
                            }
                            else
                                unusedPhysBlocks.Enqueue(physBlock);
                        }
                    }

            return _terrainBlockCache;
        }
    }
}
