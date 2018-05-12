using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using Dash.Net;
using LibNoise;
using System;

/* Chunk.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public enum ChunkState
    {
        Unpopulated = 0,
        Unshaped = 1,
        Unlit = 2,
        Unbuilt = 3,
        MeshReady = 4,
        Renderable = 5
    }

    public class Chunk : VoxelObject
    {
        struct ChunkBlock
        {
            public bool Left { get { return neighborFlags.Get(0); } }
            public bool Right { get { return neighborFlags.Get(1); } }
            public bool Up { get { return neighborFlags.Get(2); } }
            public bool Down { get { return neighborFlags.Get(3); } }
            public bool Front { get { return neighborFlags.Get(4); } }
            public bool Back { get { return neighborFlags.Get(5); } }

            public byte NeighborCount;
            ByteFlag neighborFlags;

            public void SetNeighbors(bool above, bool below, bool left, bool right, bool forward, bool backward)
            {
                ByteFlag newFlag = new ByteFlag();
                newFlag.Set(0, left);
                newFlag.Set(1, right);
                newFlag.Set(2, above);
                newFlag.Set(3, below);
                newFlag.Set(4, forward);
                newFlag.Set(5, backward);

                byte nCount = 0;
                if (above) nCount++;
                if (below) nCount++;
                if (left) nCount++;
                if (right) nCount++;
                if (forward) nCount++;
                if (backward) nCount++;

                neighborFlags = newFlag;
                NeighborCount = nCount;
            }
        }

        public const int HSIZE = 32;
        public const int VSIZE = 32;
        public const float UNIT_HSIZE = HSIZE * Block.CUBE_SIZE;
        public const float UNIT_VSIZE = VSIZE * Block.CUBE_SIZE;
        const float MIN_BLOCK_DENSITY = 0.1f;

        public static readonly IndexPosition SIZE = new IndexPosition(HSIZE, VSIZE, HSIZE);
        public static readonly Vector3 UNIT_SIZE = new Vector3(UNIT_HSIZE, UNIT_VSIZE, UNIT_HSIZE);
        public static readonly Vector3 HALF_UNIT_SIZE = UNIT_SIZE / 2f;

        public Vector3 RenderOffset;
        public Vector3 Position { get; private set; }
        public IndexPosition IndexPosition { get; private set; }
        public Vector3 CenterWorldPosition { get { return Position + HALF_UNIT_SIZE; } }
        public AxisAlignedBoundingBox BoundingBox { get; private set; }
        public bool BeingWorkedOn;
        public ChunkState State = ChunkState.Unpopulated;
        public bool IsEmpty { get; set; }
        public bool IsDirty;
        public bool Culled;
        public ChunkLightingContainer Lighting { get; private set; }

        static Perlin p = new Perlin();

        bool[,,] dirtyMask;
        ChunkBlock[,,] blockData;

        IndexPosition blockWorldOffset;
        Terrain terrain;

        VoxelMeshBuilder frontMeshBuilder;
        VoxelMeshBuilder backMeshBuilder;

        public static float rand;

        public Chunk(Terrain terrain, IndexPosition indexPosition, Vector3 worldPosition)
            : base(Block.CUBE_SIZE)
        {
            this.terrain = terrain;
            Position = worldPosition;
            IndexPosition = indexPosition;

            Lighting = new ChunkLightingContainer(this, terrain);

            blockWorldOffset = IndexPosition * SIZE;
            BoundingBox = new AxisAlignedBoundingBox(Position, Position + UNIT_SIZE);
            IsEmpty = true;

            if (!GlobalNetwork.IsServer)
            {
                frontMeshBuilder.SetupForDynamic(128);
                backMeshBuilder.SetupForDynamic(128);

                frontMeshBuilder.LightingContainer = Lighting;
                backMeshBuilder.LightingContainer = Lighting;
            }
        }

        protected override void CreateMeshBuilders(float cubeSize)
        {
            frontMeshBuilder = new VoxelMeshBuilder(this, Block.CUBE_SIZE, 0.5f);
            backMeshBuilder = new VoxelMeshBuilder(this, Block.CUBE_SIZE, 0.5f);
        }

        public Block this[IndexPosition index]
        {
            get { return Blocks[index.Z, index.Y, index.X]; }
        }

        public bool IsBlockAt(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= HSIZE || y >= VSIZE || z >= HSIZE)
            {
                int dx = (int)Math.Floor((float)x / HSIZE);
                int dy = (int)Math.Floor((float)y / VSIZE);
                int dz = (int)Math.Floor((float)z / HSIZE);

                Chunk otherChunk;
                if (terrain.TryGetChunk(IndexPosition + new IndexPosition(dx, dy, dz), out otherChunk))
                {
                    int bx = x < 0 ? x + HSIZE : x % HSIZE;
                    int by = y < 0 ? y + VSIZE : y % VSIZE;
                    int bz = z < 0 ? z + HSIZE : z % HSIZE;

                    return otherChunk.Blocks[bz, by, bx].Material != Block.AIR.Material;
                }
                else
                    return false;
            }
            else
                return Blocks[z, y, x].Material != Block.AIR.Material;
        }

        public void BlockDirtyAt(IndexPosition at, bool carryToOtherChunks)
        {
            if (GlobalNetwork.IsServer)
                return;

            for (int x = at.X - 1; x <= at.X + 1; x++)
                for (int y = at.Y - 1; y <= at.Y + 1; y++)
                    for (int z = at.Z - 1; z <= at.Z + 1; z++)
                    {
                        if (x < 0 || y < 0 || z < 0 || x >= Width || y >= Height || z >= Depth)
                        {
                            if (carryToOtherChunks)
                            {
                                int nx = x;
                                int ny = y;
                                int nz = z;

                                IndexPosition otherChunkI = WrapBlockCoords(ref nx, ref ny, ref nz);
                                Chunk other;
                                if (terrain.Chunks.TryGetValue(otherChunkI, out other))
                                {
                                    other.dirtyMask[nz, ny, nx] = true;
                                    other.IsDirty = true;
                                }
                            }

                            continue;
                        }

                        dirtyMask[z, y, x] = true;
                    }

            IsDirty = true;
        }

        public void RemoveBlock(IndexPosition at)
        {
            if (terrain.LockBottomLayer && IndexPosition.Y == 0 && at.Y == 0)
                return;

            Lighting.RequestSunlightRefill(at.X, at.Y, at.Z);

            Blocks[at.Z, at.Y, at.X] = Block.AIR;
            BlockDirtyAt(at, true);
            terrain.AddChange(this, Block.AIR, at);
        }

        public Block SetBlock(Block block, IndexPosition at)
        {
            Block before = Blocks[at.Z, at.Y, at.X];

            if (terrain.LockBottomLayer && IndexPosition.Y == 0 && at.Y == 0)
                return before;

            Lighting.RequestSunlightRemoval(at.X, at.Y, at.Z);

            Blocks[at.Z, at.Y, at.X] = block;
            BlockDirtyAt(at, true);
            terrain.AddChange(this, block, at);

            return before;
        }

        public void DamageBlock(IndexPosition at, int damage)
        {
            if (terrain.LockBottomLayer && IndexPosition.Y == 0 && at.Y == 0)
                return;

            Block c = Blocks[at.Z, at.Y, at.X];
            byte newHealth = (byte)(Math.Max(c.Health - damage, 0));
            if (newHealth > 0)
            {
                Block newBlock = new Block(c.Material, newHealth, c.R, c.G, c.B);
                Blocks[at.Z, at.Y, at.X] = newBlock;
                BlockDirtyAt(at, false);
                terrain.AddChange(this, newBlock, at);
            }
            else
                RemoveBlock(at);
        }

        #region Conversions
        public static IndexPosition WorldToBlockCoords(Vector3 worldCoords)
        {
            return new IndexPosition(
                Maths.NegativeRound(worldCoords.X / Block.CUBE_SIZE),
                Maths.NegativeRound(worldCoords.Y / Block.CUBE_SIZE),
                Maths.NegativeRound(worldCoords.Z / Block.CUBE_SIZE));
        }

        public static Vector3 ChunkBlockToWorldCoords(IndexPosition chunkIndex, IndexPosition blockIndex)
        {
            return ChunkBlockToWorldCoords(chunkIndex * UNIT_SIZE, blockIndex);
        }

        public static Vector3 ChunkBlockToWorldCoords(Vector3 chunkWorld, IndexPosition blockIndex)
        {
            return chunkWorld + (blockIndex * Block.CUBE_3D_SIZE);
        }

        public static IndexPosition BlockToChunkBlockCoords(IndexPosition chunkIndex, IndexPosition blockCoords)
        {
            return blockCoords - (chunkIndex * SIZE);
        }

        public IndexPosition BlockToChunkBlockCoords(IndexPosition blockCoords)
        {
            return BlockToChunkBlockCoords(IndexPosition, blockCoords);
        }
        #endregion

        #region Block Location
        public override Block GetBlockSafe(int x, int y, int z)
        {
            if (!IsBlockCoordInRange(x, y, z))
            {
                IndexPosition ipos = WrapBlockCoords(ref x, ref y, ref z);
                if (ipos == IndexPosition)
                    return Block.STONE;
                else
                    return terrain.GetBlockInChunkFast(ipos, x, y, z);
            }
            else
                return Blocks[z, y, x];
        }

        public Block GetBlockSafeFull(int x, int y, int z, out IndexPosition ipos)
        {
            ipos = IndexPosition;
            if (!IsBlockCoordInRange(x, y, z))
            {
                ipos = WrapBlockCoords(ref x, ref y, ref z);
                if (ipos == IndexPosition)
                    return Block.STONE;
                else
                    return terrain.GetBlockInChunkFast(ipos, x, y, z);
            }
            else
                return Blocks[z, y, x];
        }

        public IndexPosition WrapBlockCoords(ref int bx, ref int by, ref int bz)
        {
            IndexPosition ipos = IndexPosition;
            if (bx < 0) { bx = Width + bx; ipos.X--; }
            else if (bx >= Width) { bx = bx - Width; ipos.X++; }

            if (by < 0) { by = Height + by; ipos.Y--; }
            else if (by >= Height) { by = by - Height; ipos.Y++; }

            if (bz < 0) { bz = Depth + bz; ipos.Z--; }
            else if (bz >= Depth) { bz = bz - Depth; ipos.Z++; }

            return ipos;
        }

        public static void WrapBlockCoords(int bx, int by, int bz, IndexPosition chunkIndex,
            out IndexPosition newBlockIndex, out IndexPosition newChunkIndex)
        {
            IndexPosition ipos = chunkIndex;
            if (bx < 0) { bx = HSIZE + bx; ipos.X--; }
            else if (bx >= HSIZE) { bx = bx - HSIZE; ipos.X++; }

            if (by < 0) { by = VSIZE + by; ipos.Y--; }
            else if (by >= VSIZE) { by = by - VSIZE; ipos.Y++; }

            if (bz < 0) { bz = HSIZE + bz; ipos.Z--; }
            else if (bz >= HSIZE) { bz = bz - HSIZE; ipos.Z++; }

            newBlockIndex = new IndexPosition(bx, by, bz);
            newChunkIndex = ipos;
        }
        #endregion

        public static NoiseWaves GetNoiseWaves(float ax, float ay, float az)
        {
            double px = ax / 256d;
            double py = ay / 256d;
            double pz = az / 256d;

            double sx = ax / 160d;
            double sy = ay / 160d;
            double sz = az / 160d;

            double primaryWave = p.GetValue(px, py, pz);
            double secondaryWave = p.GetValue(sz, sy, sx);

            return new NoiseWaves(primaryWave, secondaryWave);
        }

        public static double GetDensity(NoiseWaves waves, float y)
        {
            double yd = (double)y / 48; // [0, 2]
            double yd2 = (double)y / (48 * 2 * rand); // [0, 1]
            double invYd = 1 - yd; // [0, 1]
            
            double mixedWave = Maths.Mix(waves.PrimaryWave, waves.PrimaryWave * waves.SecondaryWave, yd2);

            double density = mixedWave - (yd / 2d);
            density = Maths.ReScale(density, -1, 1, -0.8 + invYd, 0.9);
            
            return density;
        }

        public override void InitBlocks(int width, int height, int depth)
        {
            dirtyMask = new bool[HSIZE, VSIZE, HSIZE];
            blockData = new ChunkBlock[HSIZE, VSIZE, HSIZE];
            base.InitBlocks(width, height, depth);
        }

        public void BuildTerrain()
        {
            InitBlocks(HSIZE, VSIZE, HSIZE);

            for (int x = 0; x < HSIZE; x++)
                for (int z = 0; z < HSIZE; z++)
                {
                    Block lastType = Block.AIR;

                    for (int y = VSIZE - 1; y >= 0; y--)
                    {
                        NoiseWaves waves = GetNoiseWaves(x + blockWorldOffset.X, 0, z + blockWorldOffset.Z);
                        float blockHeight = y + blockWorldOffset.Y;

                        double density = GetDensity(waves, blockHeight);

                        if (density >= MIN_BLOCK_DENSITY)
                        {
                            if (lastType == Block.AIR)
                                Blocks[z, y, x] = lastType = Block.GRASS;
                            else
                                Blocks[z, y, x] = lastType = Block.STONE;
                        }
                    }
                }

            //for (int x = 0; x < HSIZE; x++)
            //{
            //    float xf = x + Position.X / Block.CUBE_SIZE;
            //    for (int z = 0; z < HSIZE; z++)
            //    {
            //        float zf = z + Position.Z / Block.CUBE_SIZE;
            //        Block lastType = Block.AIR;

            //        for (int y = VSIZE - 1; y >= 0; y--)
            //        {
            //            float yf = y + Position.Y / Block.CUBE_SIZE;

            //            double density = p.GetValue(xf / 48, yf / 48, zf / 48);

            //            if (density >= MIN_BLOCK_DENSITY)
            //            {
            //                if (lastType == Block.AIR)
            //                    Blocks[z, y, x] = lastType = Block.GRASS;
            //                else
            //                    Blocks[z, y, x] = lastType = Block.STONE;
            //            }
            //        }
            //    }
            //}
        }

        public void CalculateLighting()
        {
            Lighting.Process();
        }

        public void ShapeTerrain()
        {
            BakeColors();
        }

        public void BakeColors()
        {
            if (GlobalNetwork.IsServer)
                return;

            ColorGradient grassGradient = new ColorGradient(
                new Vector3(-512, 0, -512), new Vector3(512, 256, 512),
                Maths.RGBToVector3(0, 135, 16), Maths.RGBToVector3(33, 156, 22));
            
            ColorGradient dirtGradient = new ColorGradient(
                new Vector3(0, -64, 0), new Vector3(0, 1024, 0),
                Maths.RGBToVector3(209, 123, 38), Maths.RGBToVector3(133, 76, 19));

            ColorGradient stoneGradient = new ColorGradient(
                new Vector3(0, -64, 0), new Vector3(0, 512, 0),
                Maths.RGBToVector3(122, 122, 122), Maths.RGBToVector3(200, 200, 200));

            ColorGradient waterGradient = new ColorGradient(
                new Vector3(0, 0, 0), new Vector3(1000, 0, 1000),
                Maths.RGBToVector3(31, 122, 219), Maths.RGBToVector3(78, 160, 255));

            Color waterColor = new Color(31, 122, 230, 200);

            Random rnd = new Random();

            // Bake final voxel colors
            for (int x = HSIZE - 1; x >= 0; x--)
                for (int y = VSIZE - 1; y >= 0; y--)
                    for (int z = HSIZE - 1; z >= 0; z--)
                    {
                        Block block = Blocks[z, y, x];
                        if (block != Block.AIR)
                        {
                            IndexPosition blockPos = new IndexPosition(x, y, z);
                            Vector3 coloroff = (blockPos + blockWorldOffset) * Block.CUBE_SIZE;
                            Color voxelColor;

                            if (block == Block.STONE)
                                voxelColor = stoneGradient.GetColor(coloroff);
                            else if (block == Block.GRASS)
                                voxelColor = grassGradient.GetColor(coloroff);
                            else if (block == Block.WATER)
                                voxelColor = waterColor;
                            else if (block == Block.DIRT)
                                voxelColor = dirtGradient.GetColor(coloroff);
                            else if (block != Block.CUSTOM)
                                voxelColor = Color.White;
                            else
                                voxelColor = block.GetColor();

                            if (block != Block.CUSTOM)
                                voxelColor = new Color(
                                    (byte)MathHelper.Clamp(voxelColor.R + rnd.Next(-3, 3), 0, 255),
                                    (byte)MathHelper.Clamp(voxelColor.G + rnd.Next(-3, 3), 0, 255),
                                    (byte)MathHelper.Clamp(voxelColor.B + rnd.Next(-3, 3), 0, 255));

                            Blocks[z, y, x] = new Block(block.Data, voxelColor.R, voxelColor.G, voxelColor.B);
                        }
                    }
        }

        public override void BuildMesh(BufferUsageHint bufferUsage)
        {
            throw new NotSupportedException("Use Chunk.BuildMesh(void) instead.");
        }

        public void BuildMesh()
        {
            if (Mesh != null)
                frontMeshBuilder.Clear();

            // We use a temporary empty var here in the case of this being ran on a seperate thread.
            // If this chunk was previously renderable, and is being updated, we don't want it to "flicker" 
            // on the main thread while it is being updated. The main thread should just render the old mesh
            // until this is completed.
            bool isEmpty = true;

            for (int x = 0; x < HSIZE; x++)
                for (int y = 0; y < VSIZE; y++)
                    for (int z = 0; z < HSIZE; z++)
                    {
                        // Grab data about the current block
                        ChunkBlock data = blockData[z, y, x];
                        Block block = Blocks[z, y, x];

                        // If it's not air, lets draw!
                        if (block != Block.AIR)
                        {
                            // Calculate the color of the block
                            Color4 voxelColor = block.GetColor4();

                            // Determine which builder we'll be using
                            VoxelMeshBuilder useBuilder = frontMeshBuilder;

                            // Calculate the position of the block for local and global space.
                            IndexPosition blockPos = new IndexPosition(x, y, z);
                            Vector3 off = (blockPos * Block.CUBE_SIZE);

                            // Get information about this block's neighbors.
                            bool blockAbove, blockBelow, blockLeft,
                                blockForward, blockBackward, blockRight;
                            if (Mesh != null && !dirtyMask[z, y, x])
                            {
                                // If this block was already calculated and isn't dirty,
                                // we can simply pull the neighbor data from the chunkblock data.
                                blockAbove = data.Up;
                                blockBelow = data.Down;
                                blockLeft = data.Left;
                                blockForward = data.Front;
                                blockBackward = data.Back;
                                blockRight = data.Right;
                            }
                            else
                            {
                                // Search for each block in all 6 directions
                                blockAbove = GetBlockSafe(x, y + 1, z).IsOpaqueTo(block);
                                blockBelow = GetBlockSafe(x, y - 1, z).IsOpaqueTo(block);
                                blockLeft = GetBlockSafe(x - 1, y, z).IsOpaqueTo(block);
                                blockForward = GetBlockSafe(x, y, z + 1).IsOpaqueTo(block);
                                blockBackward = GetBlockSafe(x, y, z - 1).IsOpaqueTo(block);
                                blockRight = GetBlockSafe(x + 1, y, z).IsOpaqueTo(block);

                                // Update the chunkblock data
                                data.SetNeighbors(blockAbove, blockBelow, blockLeft, blockRight, blockForward, blockBackward);
                            }

                            // Only set this chunk to being non-empty if we are actually rendering something
                            if (data.NeighborCount != 6)
                                isEmpty = false;

                            // Add each cube face
                            if (!blockLeft) useBuilder.AddLeft(block, blockPos, off, voxelColor);
                            if (!blockRight) useBuilder.AddRight(block, blockPos, off, voxelColor);
                            if (!blockBackward) useBuilder.AddBack(block, blockPos, off, voxelColor);
                            if (!blockForward) useBuilder.AddFront(block, blockPos, off, voxelColor);
                            if (!blockAbove) useBuilder.AddTop(block, blockPos, off, voxelColor);
                            if (!blockBelow) useBuilder.AddBottom(block, blockPos, off, voxelColor);
                        }
                        else
                            // Default air-blocks to zero so that they are
                            // correctly updated when a block is place there later.
                            data.NeighborCount = 0;

                        // Update the block data
                        dirtyMask[z, y, x] = false;
                        blockData[z, y, x] = data;
                    }

            IsEmpty = isEmpty;

            // Swap Buffers
            VoxelMeshBuilder temp = frontMeshBuilder;
            frontMeshBuilder = backMeshBuilder;
            backMeshBuilder = temp;
        }

        public override void CreateOrUpdateMesh(BufferUsageHint bufferUsage)
        {
            if (Mesh == null)
                Mesh = new VoxelMesh(bufferUsage, backMeshBuilder);
            else
                Mesh.Update(backMeshBuilder);
        }

        public override string ToString()
        {
            return string.Format("IPos: {0}, State: {1}", IndexPosition, State);
        }
    }
}
