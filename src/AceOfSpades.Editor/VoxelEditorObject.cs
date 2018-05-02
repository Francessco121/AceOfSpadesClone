using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics.OpenGL;
using Dash.Engine.Physics;
using System;

/* VoxelEditorObject.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor
{
    public class VoxelEditorObject : VoxelObject
    {
        public int TriangleCount { get; private set; }
        public int BlockCount { get; set; }

        public Vector3 CenterPosition
        {
            get
            {
                CalculateMinMax();
                return new Vector3(
                    min.X + ((max.X - min.X) / 2f),
                    min.Y + ((max.Y - min.Y) / 2f),
                    min.Z + ((max.Z - min.Z) / 2f)
                    ) * cube3DSize;
            }
        }
        public IndexPosition Max { get { return max; } }
        public IndexPosition Min { get { return min; } }
        IndexPosition max, min;
        Vector3 cube3DSize;

        public VoxelEditorObject(VoxelObject original)
            : this(original.Width, original.Height, original.Depth, original.CubeSize, true)
        {
            Blocks = original.Blocks;

            CalculateMinMax();
            RecountBlocks();
            BuildMesh();
        }

        public VoxelEditorObject(int width, int height, int depth, float cubeSize, bool makeEmpty = false)
            : base(cubeSize)
        {
            InitBlocks(width, height, depth);
            cube3DSize = new Vector3(cubeSize);
            CubeSize = cubeSize;

            if (!makeEmpty)
            {
                Clear();
                BuildMesh();
            }
        }

        public void Clear()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                        Blocks[z, y, x] = Block.AIR;

            // Create default block
            Blocks[Depth / 2, Height / 2, Width / 2] = new Block(1, 255, 255, 255);
            BlockCount = 1;
            CalculateMinMax();
        }

        public void ChangeBlock(IndexPosition pos, Block block)
        {
            if (!IsBlockCoordInRange(pos) || (block == Block.AIR && BlockCount == 1))
                return;

            Block originalType = Blocks[pos.Z, pos.Y, pos.X];
            Blocks[pos.Z, pos.Y, pos.X] = block;

            if (originalType != Block.AIR && block == Block.AIR) BlockCount--;
            else if (originalType == Block.AIR) BlockCount++;

            CalculateMinMax();
            BuildMesh();
        }

        public void ShrinkToFit()
        {
            CalculateMinMax();
            IndexPosition newDim = Max - Min + new IndexPosition(1, 1, 1);
            Block[,,] newData = new Block[newDim.Z, newDim.Y, newDim.X];

            for (int x = 0; x <= Max.X; x++)
                for (int y = 0; y <= Max.Y; y++)
                    for (int z = 0; z <= Max.Z; z++)
                    {
                        if (z < Min.Z || y < Min.Y || x < Min.X)
                            continue;

                        newData[z - Min.Z, y - Min.Y, x - Min.X] = Blocks[z, y, x];
                    }

            Blocks = newData;
            Width = newDim.X;
            Height = newDim.Y;
            Depth = newDim.Z;

            CalculateMinMax();
            BuildMesh();
        }

        public void RotateX(int turns)
        {
            for (int i = 0; i < turns; i++)
            {
                int maxDim = Math.Max(Math.Max(Width, Height), Depth);

                Block[,,] newData = new Block[maxDim, maxDim, maxDim];

                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        for (int z = 0; z < Depth; z++)
                        {
                            int nx = x;
                            int nz = y;
                            int ny = (Depth - 1) - z;

                            newData[nz, ny, nx] = Blocks[z, y, x];
                        }

                Blocks = newData;
                Width = Height = Depth = maxDim;
            }

            CalculateMinMax();
            BuildMesh();
        }

        public void RotateY(int turns)
        {
            for (int i = 0; i < turns; i++)
            {
                int maxDim = Math.Max(Math.Max(Width, Height), Depth);

                Block[,,] newData = new Block[maxDim, maxDim, maxDim];

                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        for (int z = 0; z < Depth; z++)
                        {
                            int nx = z;
                            int ny = y;
                            int nz = (Width - 1) - x;

                            newData[nz, ny, nx] = Blocks[z, y, x];
                        }

                Blocks = newData;
                Width = Height = Depth = maxDim;
            }

            CalculateMinMax();
            BuildMesh();
        }

        public void RotateZ(int turns)
        {
            for (int i = 0; i < turns; i++)
            {
                int maxDim = Math.Max(Math.Max(Width, Height), Depth);

                Block[,,] newData = new Block[maxDim, maxDim, maxDim];

                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        for (int z = 0; z < Depth; z++)
                        {
                            int nx = y;
                            int ny = (Width - 1) - x;
                            int nz = z;

                            newData[nz, ny, nx] = Blocks[z, y, x];
                        }

                Blocks = newData;
                Width = Height = Depth = maxDim;
            }

            CalculateMinMax();
            BuildMesh();
        }

        public void FlipX()
        {
            Block[,,] newData = new Block[Depth, Height, Width];

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        int nx = (Width - 1) - x;
                        newData[z, y, nx] = Blocks[z, y, x];
                    }

            Blocks = newData;

            CalculateMinMax();
            BuildMesh();
        }

        public void FlipY()
        {
            Block[,,] newData = new Block[Depth, Height, Width];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        int ny = (Height - 1) - y;
                        newData[z, ny, x] = Blocks[z, y, x];
                    }

            Blocks = newData;

            CalculateMinMax();
            BuildMesh();
        }

        public void FlipZ()
        {
            Block[,,] newData = new Block[Depth, Height, Width];

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        int nz = (Depth - 1) - z;
                        newData[nz, y, x] = Blocks[z, y, x];
                    }

            Blocks = newData;

            CalculateMinMax();
            BuildMesh();
        }

        public void Translate(IndexPosition delta)
        {
            CalculateMinMax();

            IndexPosition offMin = Min + delta;
            IndexPosition offMax = Max + delta;

            if (offMax.X >= Width) delta.X -= (offMax.X - Width) + 1;
            if (offMin.X < 0) delta.X += 0 - offMin.X;

            if (offMax.Y >= Height) delta.Y -= (offMax.Y - Height) + 1;
            if (offMin.Y < 0) delta.Y += 0 - offMin.Y;

            if (offMax.Z >= Depth) delta.Z -= (offMax.Z - Depth) + 1;
            if (offMin.Z < 0) delta.Z += 0 - offMin.Z;

            if (delta == IndexPosition.Zero) return;

            offMin = Min + delta;
            offMax = Max + delta;

            Block[,,] newData = new Block[Depth, Height, Width];

            for (int x = 0; x <= Max.X; x++)
                for (int y = 0; y <= Max.Y; y++)
                    for (int z = 0; z <= Max.Z; z++)
                    {
                        if (Blocks[z, y, x] != Block.AIR)
                        {
                            newData[z + delta.Z, y + delta.Y, x + delta.X] = Blocks[z, y, x];
                        }
                    }

            Blocks = newData;

            CalculateMinMax();
            BuildMesh();
        }

        public void RecountBlocks()
        {
            BlockCount = 0;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        if (Blocks[z, y, x] != Block.AIR)
                            BlockCount++;
                    }
        }

        public void CalculateMinMax()
        {
            max = IndexPosition.Zero;
            min = new IndexPosition(Width, Height, Depth);

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        if (Blocks[z, y, x] != Block.AIR)
                        {
                            if (x > Max.X) max.X = x;
                            if (x < Min.X) min.X = x;
                            if (y > Max.Y) max.Y = y;
                            if (y < Min.Y) min.Y = y;
                            if (z > Max.Z) max.Z = z;
                            if (z < Min.Z) min.Z = z;
                        }
                    }
        }

        public void ChangeDimensions(int width, int height, int depth)
        {
            Block[,,] oldBlocks = Blocks;

            InitBlocks(width, height, depth);

            BlockCount = 0;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        if (x >= oldBlocks.GetLength(2) || y >= oldBlocks.GetLength(1) || z >= oldBlocks.GetLength(0))
                            continue;

                        Block block = oldBlocks[z, y, x];

                        if (block != Block.AIR)
                        {
                            Blocks[z, y, x] = block;
                            BlockCount++;
                        }
                    }

            if (BlockCount == 0)
            {
                // Create default block
                Blocks[Depth / 2, Height / 2, Width / 2] = new Block(1, 255, 255, 255);
                BlockCount = 1;
            }

            CalculateMinMax();
        }

        public void ChangeCubeSize(float cubeSize)
        {
            meshBuilder.SetCubeSize(cubeSize);
            alphaBuilder.SetCubeSize(cubeSize);
            cube3DSize = new Vector3(cubeSize);
            CubeSize = cubeSize;
            BuildMesh();
        }

        public void BuildMesh()
        {
            base.BuildMesh(BufferUsageHint.DynamicDraw);
            this.TriangleCount = meshBuilder.TriangleCount + alphaBuilder.TriangleCount;
        }

        static IndexPosition WorldToBlockCoords(Vector3 worldCoords, float cubeSize)
        {
            return new IndexPosition(
                Maths.NegativeRound(worldCoords.X / cubeSize),
                Maths.NegativeRound(worldCoords.Y / cubeSize),
                Maths.NegativeRound(worldCoords.Z / cubeSize));
        }

        public VoxelObjectRaycastResult Raycast(Ray ray, float cubeSize = Block.CUBE_SIZE)
        {
            IndexPosition? blockIntersection = null;
            CubeSide? side = null;
            float? intersectionDistance = null;

            IndexPosition? lastBlockIndex = null;
            Vector3 cube3DSize = new Vector3(cubeSize);
            Vector3 half3DCubeSize = cube3DSize / 2f;

            for (int i = 0; i < 1000; i++)
            {
                // Calculate the world position to check
                Vector3 tryWorldPos = ray.Origin + (ray.Direction * i) + half3DCubeSize;
                // Calculate the block coordinate to try
                IndexPosition blockPos = new IndexPosition(
                    Maths.NegativeRound(tryWorldPos.X / cubeSize),
                    Maths.NegativeRound(tryWorldPos.Y / cubeSize),
                    Maths.NegativeRound(tryWorldPos.Z / cubeSize));

                if (blockPos.X < -2 || blockPos.Y < -2 || blockPos.Z < -2
                    || blockPos.X >= Width + 2 || blockPos.Y >= Height + 2 || blockPos.Z >= Depth + 2)
                    continue;

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
                                IndexPosition shiftedBlockPos = new IndexPosition(blockPos.X + x, blockPos.Y + y, blockPos.Z + z);
                                if (!IsBlockCoordInRange(shiftedBlockPos))
                                    continue;

                                Block type = Blocks[shiftedBlockPos.Z, shiftedBlockPos.Y, shiftedBlockPos.X];
                                if (type != Block.AIR)
                                {
                                    // Calculate the new blocks positions
                                    Vector3 cubeWorldPos = (shiftedBlockPos * cube3DSize) - half3DCubeSize;

                                    // If this blocks distance is smaller than the current, continue
                                    float dist = Maths.DistanceSquared(cubeWorldPos, ray.Origin);
                                    if (dist < closestDist)
                                    {
                                        AxisAlignedBoundingBox aabb =
                                            new AxisAlignedBoundingBox(cubeWorldPos, cubeWorldPos + cube3DSize);

                                        // If this block intersects the ray,
                                        // it is the newly intersected block.
                                        float? interDist;
                                        CubeSide interSide;
                                        if (ray.Intersects(aabb, out interDist, out interSide))
                                        {
                                            closestDist = dist;
                                            intersectionDistance = dist;
                                            side = interSide;
                                            blockIntersection = shiftedBlockPos;
                                            blockFound = true;
                                        }
                                    }
                                }
                            }

                    // If any block was found to actually intersect the ray,
                    // return.
                    if (blockFound)
                    {
                        return new VoxelObjectRaycastResult(ray, true, ray.Origin + ray.Direction * intersectionDistance.Value, 
                            intersectionDistance, blockIntersection, side);
                    }
                }

                lastBlockIndex = blockPos;
            }

            // No intersection at this point
            return new VoxelObjectRaycastResult(ray);
        }

        public override void CreateOrUpdateMesh(BufferUsageHint bufferUsage)
        {
            base.CreateOrUpdateMesh(bufferUsage);
        }

        public Block GetBlockSafe(IndexPosition ipos)
        {
            return GetBlockSafe(ipos.X, ipos.Y, ipos.Z);
        }

        public override Block GetBlockSafe(int x, int y, int z)
        {
            if (!IsBlockCoordInRange(x, y, z))
                return Block.AIR;
            else
                return Blocks[z, y, x];
        }
    }
}
