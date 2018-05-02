using System.Collections.Generic;

/* MeshBuilder.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    /// <summary>
    /// Designed to build an optimal mesh.
    /// </summary>
    public class MeshBuilder
    {
        public List<Vector3> Vertices;
        public List<Vector2> UVs;
        public List<Vector3> Normals;
        public List<uint> Indexes;

        public MeshBuilder()
        {
            Vertices = new List<Vector3>();
            UVs = new List<Vector2>();
            Normals = new List<Vector3>();
            Indexes = new List<uint>();
        }

        public void Finalize(out float[] vertices, out float[] uvs, out float[] normals, out uint[] indexes)
        {
            vertices = new float[Vertices.Count * 3];
            normals = new float[Normals.Count * 3];
            uvs = new float[UVs.Count * 2];
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
            foreach (Vector2 uv in UVs)
            {
                uvs[i++] = uv.X;
                uvs[i++] = uv.Y;
            }
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

        public void AddUV(int x, int y)
        {
            UVs.Add(new Vector2(x, y));
        }

        public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 normal)
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

            AddUV(0, 0);
            AddUV(1, 0);
            AddUV(0, 1);
            AddUV(1, 1);
        }

        public void Clear()
        {
            Vertices.Clear();
            Normals.Clear();
            UVs.Clear();
            Indexes.Clear();
        }
    }
}
