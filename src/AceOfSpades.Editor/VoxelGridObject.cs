using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;

/* VoxelGridObject.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor
{
    public class VoxelGridObject : VoxelObject
    {
        public VoxelGridObject(int width, int height, int depth, float cubeSize)
            : base(cubeSize)
        {
            Width = width;
            Height = height;
            Depth = depth;

            BuildMesh();
        }

        public VoxelGridObject(VoxelEditorObject voxObj)
        : base(voxObj.CubeSize) {
            Width = voxObj.Width;
            Height = voxObj.Height;
            Depth = voxObj.Depth;

            BuildMesh();
        }

        public void ChangeDimensions(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;

            BuildMesh();
        }

        public void ChangeCubeSize(float cubeSize)
        {
            meshBuilder.SetCubeSize(cubeSize);
            BuildMesh();
        }

        public void BuildMesh()
        {
            Color4 gridColor;
            Block b = Block.AIR;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        if (x != 0 && y != 0 && z != 0)
                            continue;

                        IndexPosition blockIndex = new IndexPosition(x, y, z);
                        Vector3 blockPos = new Vector3(x, y, z);
                        Vector3 off = blockPos * meshBuilder.CubeSize;

                        if (x == 0 && z == 0) gridColor = Color4.Red;
                        else if (y == 0 && z == 0) gridColor = Color4.Blue;
                        else if (x == 0 && y == 0) gridColor = Color4.Green;
                        else gridColor = Color4.Lavender;

                        if (x == 0) meshBuilder.AddLeft(b, blockIndex, off, gridColor);
                        if (z == 0) meshBuilder.AddBack(b, blockIndex, off, gridColor);
                        if (y == 0) meshBuilder.AddBottom(b, blockIndex, off, gridColor);
                    }

            CreateOrUpdateMesh(BufferUsageHint.DynamicDraw);
            Mesh.BeginMode = BeginMode.Lines;
        }
    }
}
