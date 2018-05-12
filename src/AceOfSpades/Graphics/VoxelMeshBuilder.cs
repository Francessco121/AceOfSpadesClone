using Dash.Engine;
using Dash.Engine.Graphics;
using System;
using System.Collections.Generic;

/* VoxelMeshBuilder.cs
 * Ethan Lafrenais
 * Creates a mesh for a voxel object, 
 * including AO calculations.
*/

namespace AceOfSpades.Graphics
{
    public class VoxelMeshBuilder
    {
        public float CubeSize { get; private set; }

        public int TriangleCount { get; private set; }
        public List<Vector4> Vertices { get; private set; }
        public List<Color4> Colors { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<uint> Indexes { get; private set; }

        public ILightingContainer LightingContainer { get; set; }

        float[] cVertices;
        float[] cColors;
        float[] cNormals;
        uint[] cIndexes;

        readonly Vector3 frontNormal, backNormal, leftNormal, rightNormal, topNormal, bottomNormal;
        Vector3 frontTopLeft, frontTopRight, frontBottomLeft, frontBottomRight,
                backTopLeft, backTopRight, backBottomLeft, backBottomRight;

        float vertexOff;

        bool dynamicResizing;
        int dynamicBlockCount;

        VoxelObject vo;
        float ao;

        public VoxelMeshBuilder(VoxelObject vo, float cubeSize, float ao) 
        {
            this.vo = vo;
            this.ao = ao;

            SetCubeSize(cubeSize);

            frontNormal = Vector3.UnitZ;
            backNormal = -Vector3.UnitZ;
            leftNormal = -Vector3.UnitX;
            rightNormal = Vector3.UnitX;
            topNormal = Vector3.UnitY;
            bottomNormal = -Vector3.UnitY;
        }

        /// <summary>
        /// Lets the mesh builder know that this voxel object
        /// is going to be updated dynamically.
        /// Lets the mesh builder start with smaller arrays,
        /// and increase them as it's rebuilt.
        /// </summary>
        /// <param name="initialBlockCount">Initial blocks rendered in mesh. Can be estimate.</param>
        public void SetupForDynamic(int initialBlockCount)
        {
            dynamicResizing = true;
            dynamicBlockCount = initialBlockCount;

            SetupDynamicArrays();
        }

        void IncreaseDynamicArrays()
        {
            int neededSize = Vertices.Count / 4 / 6;

            dynamicBlockCount = neededSize + (int)(dynamicBlockCount * 0.1f);
            SetupDynamicArrays();
        }

        void SetupDynamicArrays()
        {
            int vertexBufferSize = dynamicBlockCount * 4 * 4 * 6; // Components * QuadVertCount * CubeFaceCount
            int normalBufferSize = dynamicBlockCount * 3 * 4 * 6;
            int colorBufferSize = dynamicBlockCount * 4 * 4 * 6;
            int indexBufferSize = dynamicBlockCount * 6 * 4 * 6;

            if (cVertices == null)
            {
                cVertices = new float[vertexBufferSize];
                cColors = new float[colorBufferSize];
                cNormals = new float[normalBufferSize];
                cIndexes = new uint[indexBufferSize];
            }
            else
            {
                Array.Resize(ref cVertices, vertexBufferSize);
                Array.Resize(ref cColors, colorBufferSize);
                Array.Resize(ref cNormals, normalBufferSize);
                Array.Resize(ref cIndexes, indexBufferSize);
            }
        }

        public ushort CopyFromOther(int at, int count, VoxelMeshBuilder other)
        {
            ushort newIndex = (ushort)(Vertices.Count / 4);

            for (int i = 0; i < count; i++, at++)
            {
                int tvi = at * 4;
                int tni = at * 4;
                int tci = at * 4;
                int tii = at * 6;

                AddQuad(other.Vertices[tvi], other.Vertices[tvi + 1], other.Vertices[tvi + 2], other.Vertices[tvi + 3], other.Normals[tni], other.Colors[tci]);
            }

            return newIndex;
        }

        public void SetCubeSize(float cubeSize)
        {
            CubeSize = cubeSize;
            vertexOff = cubeSize / 2f;

            frontTopLeft = new Vector3(-vertexOff, vertexOff, vertexOff);
            frontTopRight = new Vector3(vertexOff, vertexOff, vertexOff);
            frontBottomLeft = new Vector3(-vertexOff, -vertexOff, vertexOff);
            frontBottomRight = new Vector3(vertexOff, -vertexOff, vertexOff);
            backTopLeft = new Vector3(-vertexOff, vertexOff, -vertexOff);
            backTopRight = new Vector3(vertexOff, vertexOff, -vertexOff);
            backBottomLeft = new Vector3(-vertexOff, -vertexOff, -vertexOff);
            backBottomRight = new Vector3(vertexOff, -vertexOff, -vertexOff);

            Vertices = new List<Vector4>();
            Colors = new List<Color4>();
            Normals = new List<Vector3>();
            Indexes = new List<uint>();
        }

        public void Finalize(out float[] vertices, out float[] colors, out float[] normals, out uint[] indexes, out int indexCount)
        {
            if (!dynamicResizing)
            {
                vertices = new float[Vertices.Count * 4];
                normals = new float[Normals.Count * 3];
                colors = new float[Colors.Count * 4];
                indexes = Indexes.ToArray();
                indexCount = indexes.Length;
            }
            else
            {
                // If this mesh is dynamically updated,
                // we may need to increase the size of the arrays.
                if (Vertices.Count * 4 >= cVertices.Length)
                    IncreaseDynamicArrays();

                vertices = cVertices;
                normals = cNormals;
                colors = cColors;

                for (int k = 0; k < Indexes.Count; k++)
                    cIndexes[k] = Indexes[k];

                indexes = cIndexes;

                indexCount = Indexes.Count;
            }

            int i = 0;
            foreach (Vector4 vertex in Vertices)
            {
                vertices[i++] = vertex.X;
                vertices[i++] = vertex.Y;
                vertices[i++] = vertex.Z;
                vertices[i++] = vertex.W;
            }
            i = 0;
            foreach (Vector3 normal in Normals)
            {
                normals[i++] = normal.X;
                normals[i++] = normal.Y;
                normals[i++] = normal.Z;
            }
            i = 0;
            foreach (Color4 color in Colors)
            {
                colors[i++] = color.R;
                colors[i++] = color.G;
                colors[i++] = color.B;
                colors[i++] = color.A;
            }

            TriangleCount = indexCount / 3;
        }

        void AddVertex(Vector4 vertex)
        {
            Vertices.Add(vertex);
        }

        void AddNormal(Vector3 normal)
        {
            Normals.Add(normal);
        }

        void AddIndex(uint index, uint offset)
        {
            Indexes.Add((uint)Vertices.Count + (index - offset));
        }

        void AddVertexColor(Color4 color)
        {
            Colors.Add(color);
        }

        void AddQuad(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4, Vector3 normal, Color4 color)
        {
            AddVertex(v1);
            AddVertex(v2);
            AddVertex(v3);
            AddVertex(v4);

            AddNormal(normal);
            AddNormal(normal);
            AddNormal(normal);
            AddNormal(normal);

            /*
                Index setup:
                Counter-Clockwise

                vertex(local index)

                v1(0)      v2(1)
                    +------+
                    |    / |
                    |  /   |
                    |/     |
                    +------+
                v3(2)      v4(3)
            */
            AddIndex(0, 4); // v1
            AddIndex(2, 4); // v3
            AddIndex(1, 4); // v2
            AddIndex(1, 4); // v2
            AddIndex(2, 4); // v3
            AddIndex(3, 4); // v4

            AddVertexColor(color);
            AddVertexColor(color);
            AddVertexColor(color);
            AddVertexColor(color);
        }

        void AddBlockQuad(Block block, IndexPosition blockIndex, 
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, 
            Vector3 normal, Vector3 offset, Color4 color)
        {
            // Calculate AO for each vertex
            Vector4 ao = new Vector4(
                CalculateAOFor(block, blockIndex, v1, normal),
                CalculateAOFor(block, blockIndex, v2, normal),
                CalculateAOFor(block, blockIndex, v3, normal),
                CalculateAOFor(block, blockIndex, v4, normal));

            // Calculate world lighting
            if (LightingContainer != null)
            {
                //float lighting1 = LightingAt(blockIndex.X, blockIndex.Y, blockIndex.Z, normal, v1);
                //float lighting2 = LightingAt(blockIndex.X, blockIndex.Y, blockIndex.Z, normal, v2);
                //float lighting3 = LightingAt(blockIndex.X, blockIndex.Y, blockIndex.Z, normal, v3);
                //float lighting4 = LightingAt(blockIndex.X, blockIndex.Y, blockIndex.Z, normal, v4);

                //// Blend by taking the darkest per vertex
                //ao.X = Math.Max(ao.X, 1f - lighting1);
                //ao.Y = Math.Max(ao.Y, 1f - lighting2);
                //ao.Z = Math.Max(ao.Z, 1f - lighting3);
                //ao.W = Math.Max(ao.W, 1f - lighting4);

                int lightingBlockX = blockIndex.X + (int)normal.X;
                int lightingBlockY = blockIndex.Y + (int)normal.Y;
                int lightingBlockZ = blockIndex.Z + (int)normal.Z;

                float lighting = 1f - LightingContainer.LightingAt(lightingBlockX, lightingBlockY, lightingBlockZ);

                ao.X = Math.Max(ao.X, lighting);
                ao.Y = Math.Max(ao.Y, lighting);
                ao.Z = Math.Max(ao.Z, lighting);
                ao.W = Math.Max(ao.W, lighting);
            }

            // Apply the local offset of the quad
            v1 += offset;
            v2 += offset;
            v3 += offset;
            v4 += offset;

            // Fix anisotropy issue by flipping quad when AO is mostly 
            // on corners adjacent to the seam in quad (from two triangles).
            if (ao.Y + ao.Z > ao.X + ao.W)
                // Flipped
                AddQuad(new Vector4(v2, ao.Y), new Vector4(v4, ao.W), new Vector4(v1, ao.X), new Vector4(v3, ao.Z), normal, color);
            else
                // Normal
                AddQuad(new Vector4(v1, ao.X), new Vector4(v2, ao.Y), new Vector4(v3, ao.Z), new Vector4(v4, ao.W), normal, color);
        }

        float CalculateAOFor(Block block, IndexPosition blockIndex, Vector3 vertexOffset, Vector3 faceNormal)
        {
            int xShift = 0, yShift = 0, zShift = 0;

            // Depending on the normal, the shift values need to be
            // rotated to be in perspective of that normal.
            // Basically for the 3 blocks located, one shift value
            // will always be calculated, for the other two one will
            // be zero (with the exception of the corner block which
            // is unaffected).

            // Reference (concept from here, not so much implementation):
            // http://0fps.net/2013/07/03/ambient-occlusion-for-minecraft-like-worlds/

            // Locate side block 1
            if (faceNormal.Y != 0)
            {
                xShift = Math.Sign(vertexOffset.X);
                yShift = Math.Sign(vertexOffset.Y);
                zShift = 0;
            }
            else if (faceNormal.X != 0)
            {
                xShift = Math.Sign(vertexOffset.X);
                yShift = Math.Sign(vertexOffset.Y);
                zShift = 0;
            }
            else if (faceNormal.Z != 0)
            {
                xShift = 0;
                yShift = Math.Sign(vertexOffset.Y);
                zShift = Math.Sign(vertexOffset.Z);
            }

            Block side1b = vo.GetBlockSafe(blockIndex.X + xShift, blockIndex.Y + yShift, blockIndex.Z + zShift);

            // Locate side block 2
            if (faceNormal.Y != 0)
            {
                xShift = 0;
                yShift = Math.Sign(vertexOffset.Y);
                zShift = Math.Sign(vertexOffset.Z);
            }
            else if (faceNormal.X != 0)
            {
                xShift = Math.Sign(vertexOffset.X);
                yShift = 0;
                zShift = Math.Sign(vertexOffset.Z);
            }
            else if (faceNormal.Z != 0)
            {
                xShift = Math.Sign(vertexOffset.X);
                yShift = 0;
                zShift = Math.Sign(vertexOffset.Z);
            }

            Block side2b = vo.GetBlockSafe(blockIndex.X + xShift, blockIndex.Y + yShift, blockIndex.Z + zShift);

            // Locate corner block
            xShift = Math.Sign(vertexOffset.X);
            yShift = Math.Sign(vertexOffset.Y);
            zShift = Math.Sign(vertexOffset.Z);

            Block cornerb = vo.GetBlockSafe(blockIndex.X + xShift, blockIndex.Y + yShift, blockIndex.Z + zShift);

            // Calculate the amount of AO for the vertex
            int side1 = side1b.IsOpaque() ? 1 : 0;
            int side2 = side2b.IsOpaque() ? 1 : 0;
            int corner = cornerb.IsOpaque() ? 1 : 0;

            if (side1 + side2 + corner == 3)
                // If all blocks are present, full AO is applied
                // Having full-corner blocks use double AO than others is important,
                // because it is used for the weight test to determine
                // when a quad needs to be flipped for the anistropy fix.
                return ao;
            else
                // If one or two blocks are missing, half AO is applied
                return (corner + side1 + side2 >= 1) ? (ao / 2f) : 0;
        }

        float LightingAt(int x, int y, int z, Vector3 normal, Vector3 vertexOffset)
        {
            if (LightingContainer == null)
                return 1f;

            int minx, maxx, miny, maxy, minz, maxz;

            if (normal.X == 0)
            {
                int x1 = (int)vertexOffset.X;
                minx = Math.Min(0, x1);
                maxx = Math.Max(0, x1);
            }
            else
                minx = maxx = (int)normal.X;

            if (normal.Y == 0)
            {
                int y1 = (int)vertexOffset.Y;
                miny = Math.Min(0, y1);
                maxy = Math.Max(0, y1);
            }
            else
                miny = maxy = (int)normal.Y;

            if (normal.Z == 0)
            {
                int z1 = (int)vertexOffset.Z;
                minz = Math.Min(0, z1);
                maxz = Math.Max(0, z1);
            }
            else
                minz = maxz = (int)normal.Z;

            bool onlydiag = true;
            int i = 0;
            float lighting = 0;
            float ligtingAtOrigin = 0;
            for (int dx = minx; dx <= maxx; dx++)
                for (int dy = miny; dy <= maxy; dy++)
                    for (int dz = minz; dz <= maxz; dz++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        int nz = z + dz;

                        if (vo.IsBlockTransparent(nx, ny, nz))
                        {
                            float deltaSum = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                            float light = LightingContainer.LightingAt(nx, ny, nz);

                            if (deltaSum == 2)
                                onlydiag = false;
                            else if (deltaSum == 1)
                                ligtingAtOrigin = light;

                            lighting += light;
                            i++;
                        }
                    }

            return onlydiag ? ligtingAtOrigin : lighting / i;
        }

        #region Add*()
        public void AddFront(Block block, IndexPosition blockIndex, Vector3 offset, Color4 color)
        {
            AddBlockQuad(block, blockIndex, frontTopLeft, frontTopRight, frontBottomLeft, frontBottomRight,
                frontNormal, offset, color);
        }

        public void AddBack(Block block, IndexPosition blockIndex, Vector3 offset, Color4 color)
        {
            AddBlockQuad(block, blockIndex, backTopRight, backTopLeft, backBottomRight, backBottomLeft,
                backNormal, offset, color);
        }

        public void AddLeft(Block block, IndexPosition blockIndex, Vector3 offset, Color4 color)
        {
            AddBlockQuad(block, blockIndex, backTopLeft, frontTopLeft, backBottomLeft, frontBottomLeft,
                leftNormal, offset, color);
        }

        public void AddRight(Block block, IndexPosition blockIndex, Vector3 offset, Color4 color)
        {
            AddBlockQuad(block, blockIndex, frontTopRight, backTopRight, frontBottomRight, backBottomRight,
                rightNormal, offset, color);
        }

        public void AddTop(Block block, IndexPosition blockIndex, Vector3 offset, Color4 color)
        {
            AddBlockQuad(block, blockIndex, backTopLeft, backTopRight, frontTopLeft, frontTopRight,
                topNormal, offset, color);
        }

        public void AddBottom(Block block, IndexPosition blockIndex, Vector3 offset, Color4 color)
        {
            AddBlockQuad(block, blockIndex, frontBottomLeft, frontBottomRight, backBottomLeft, backBottomRight,
                bottomNormal, offset, color);
        }
        #endregion

        public virtual void Clear()
        {
            Vertices.Clear();
            Normals.Clear();
            Colors.Clear();
            Indexes.Clear();
        }
    }
}
