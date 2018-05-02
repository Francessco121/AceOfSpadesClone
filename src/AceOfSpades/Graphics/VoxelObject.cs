using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using System;

/* VoxelObject.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Graphics
{
    public class VoxelObject : IDisposable
    {
        public VoxelMesh Mesh { get; protected set; }
        public VoxelMesh AlphaMesh { get; protected set; }

        public Vector3 MeshScale { get; set; }
        public Vector3 MeshRotation { get; set; }

        public Block[, ,] Blocks { get; protected set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int Depth { get; protected set; }
        public float UnitWidth { get { return Width * meshBuilder.CubeSize; } }
        public float UnitHeight { get { return Height * meshBuilder.CubeSize; } }
        public float UnitDepth { get { return Depth * meshBuilder.CubeSize; } }
        public Vector3 UnitSize { get { return new Vector3(UnitWidth, UnitHeight, UnitDepth); } }
        public Vector3 UnitCenter { get { return UnitSize / 2f; } }

        public bool CleanedUp { get; private set; }
        public float CubeSize { get; protected set; }

        protected VoxelMeshBuilder meshBuilder;
        protected VoxelMeshBuilder alphaBuilder;

        public VoxelObject(float cubeSize)
        {
            CubeSize = cubeSize;
            MeshScale = new Vector3(1, 1, 1);

            if (!GlobalNetwork.IsServer)
                CreateMeshBuilders(cubeSize);
        }

        protected virtual void CreateMeshBuilders(float cubeSize)
        {
            meshBuilder = new VoxelMeshBuilder(this, cubeSize, 0.6f);
            alphaBuilder = new VoxelMeshBuilder(this, cubeSize, 0.6f);
        }

        public virtual void InitBlocks(int width, int height, int depth)
        {
            if (!CleanedUp)
            {
                Blocks = new Block[depth, height, width];
                Width = width;
                Height = height;
                Depth = depth;
            }
            else
                throw new InvalidOperationException("Cannot initialize voxel object, it is already cleaned up!");
        }

        public virtual void BuildMesh(BufferUsageHint bufferUsage)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        Block block = Blocks[z, y, x];
                        if (block != Block.AIR)
                        {
                            Color4 voxelColor = block.GetColor4();
                            VoxelMeshBuilder useBuilder = voxelColor.A < 1f ? alphaBuilder : meshBuilder;

                            IndexPosition blockIndex = new IndexPosition(x, y, z);
                            Vector3 blockPos = new Vector3(x, y, z);
                            Vector3 off = blockPos * useBuilder.CubeSize;

                            bool blockAbove = !IsBlockTransparent(x, y + 1, z);
                            bool blockBelow = !IsBlockTransparent(x, y - 1, z);
                            bool blockLeft = !IsBlockTransparent(x - 1, y, z);
                            bool blockForward = !IsBlockTransparent(x, y, z + 1);
                            bool blockBackward = !IsBlockTransparent(x, y, z - 1);
                            bool blockRight = !IsBlockTransparent(x + 1, y, z);

                            if (blockAbove && blockBelow && blockLeft
                                && blockForward && blockBackward && blockRight)
                                continue;

                            //VoxelAO ao = aoBuilder.Calculate(block, x, y, z, off, useBuilder);

                            if (!blockLeft) useBuilder.AddLeft(block, blockIndex, off, voxelColor);
                            if (!blockRight) useBuilder.AddRight(block, blockIndex, off, voxelColor);
                            if (!blockBackward) useBuilder.AddBack(block, blockIndex, off, voxelColor);
                            if (!blockForward) useBuilder.AddFront(block, blockIndex, off, voxelColor);
                            if (!blockAbove) useBuilder.AddTop(block, blockIndex, off, voxelColor);
                            if (!blockBelow) useBuilder.AddBottom(block, blockIndex, off, voxelColor);
                        }
                    }

            this.CreateOrUpdateMesh(bufferUsage);
        }

        public bool IsBlockTransparent(int x, int y, int z)
        {
            Block block = GetBlockSafe(x, y, z);
            return !block.IsOpaque();
        }

        public virtual void CreateOrUpdateMesh(BufferUsageHint bufferUsage)
        {
            if (AlphaMesh != null || alphaBuilder.Colors.Count > 0)
            {
                if (AlphaMesh == null)
                    AlphaMesh = new VoxelMesh(bufferUsage, alphaBuilder);
                else
                    AlphaMesh.Update(alphaBuilder);

                alphaBuilder.Clear();
            }

            if (Mesh == null)
                Mesh = new VoxelMesh(bufferUsage, meshBuilder);
            else
                Mesh.Update(meshBuilder);

            meshBuilder.Clear();
        }

        public bool IsBlockCoordInRange(IndexPosition blockCoords)
        {
            return IsBlockCoordInRange(blockCoords.X, blockCoords.Y, blockCoords.Z);
        }

        public bool IsBlockCoordInRange(int x, int y, int z)
        {
            return x >= 0 && y >= 0 && z >= 0 && x < Width && y < Height && z < Depth;
        }

        public virtual Block GetBlockSafe(int x, int y, int z)
        {
            if (!IsBlockCoordInRange(x, y, z))
                return Block.AIR;

            return Blocks[z, y, x];
        }

        public virtual void Dispose()
        {
            if (!CleanedUp)
            {
                CleanedUp = true;

                if (Mesh != null)
                    Mesh.Dispose();
                if (meshBuilder != null)
                    meshBuilder.Clear();

                if (AlphaMesh != null)
                    AlphaMesh.Dispose();
                if (alphaBuilder != null)
                    alphaBuilder.Clear();

                Blocks = null;
            }
            else
                throw new InvalidOperationException("VoxelObject already cleaned up!");
        }        
    }
}
