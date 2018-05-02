using System.Collections.Generic;

/* ColorMeshBuilder.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    /// <summary>
    /// Designed to build an optimal vertex-color mesh.
    /// </summary>
    public class ColorMeshBuilder
    {
        public int TriangleCount { get; private set; }
        public List<Vector3> Vertices;
        public List<Color4> Colors;
        public List<Vector3> Normals;
        public List<uint> Indexes;

        public ColorMeshBuilder()
        {
            Vertices = new List<Vector3>();
            Colors = new List<Color4>();
            Normals = new List<Vector3>();
            Indexes = new List<uint>();
        }

        public void Finalize(out float[] vertices, out float[] colors, out float[] normals, out uint[] indexes)
        {
            vertices = new float[Vertices.Count * 3];
            normals = new float[Normals.Count * 3];
            colors = new float[Colors.Count * 4];
            indexes = Indexes.ToArray();

            int i = 0;
            foreach (Vector3 vertex in Vertices)
            {
                vertices[i++] = vertex.X;
                vertices[i++] = vertex.Y;
                vertices[i++] = vertex.Z;
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

            TriangleCount = indexes.Length / 3;
        }

        public void AddVertex(Vector3 vertex)
        {
            Vertices.Add(vertex);
        }

        public void AddNormal(Vector3 normal)
        {
            Normals.Add(normal);
        }

        public void AddIndex(uint index, uint offset)
        {
            Indexes.Add((uint)Vertices.Count + (index - offset));
        }

        public void AddVertexColor(Color4 color)
        {
            Colors.Add(color);
        }

        public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 normal, Color4 color)
        {
            AddVertex(v1); // 0     0       1
            AddVertex(v2); // 1     2       3
            AddVertex(v3); // 2
            AddVertex(v4); // 3

            AddNormal(normal);
            AddNormal(normal);
            AddNormal(normal);
            AddNormal(normal);

            AddIndex(0, 4); // 4 + (0 - 4) 4 + -4 = 0
            AddIndex(2, 4);
            AddIndex(1, 4);
            AddIndex(1, 4);
            AddIndex(2, 4);
            AddIndex(3, 4);

            AddVertexColor(color);
            AddVertexColor(color);
            AddVertexColor(color);
            AddVertexColor(color);
        }

        public virtual void Clear()
        {
            Vertices.Clear();
            Normals.Clear();
            Colors.Clear();
            Indexes.Clear();
        }
    }
}
