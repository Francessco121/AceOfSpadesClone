using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics.OpenGL;
using System.Collections.Generic;
using System.IO;

/* OBJLoader.cs
   Loads an obj file to a mesh.
*/

namespace Dash.Engine.Graphics.IO
{
    public static class OBJLoader
    {
        class OBJVertex
        {
            public Vector3 Position;
            public int UVIndex = -1;
            public int NormalIndex = -1;
            public OBJVertex DuplicateVertex;
            public int Index;
            public float Length;

            public OBJVertex(int index, Vector3 pos)
            {
                this.Index = index;
                this.Position = pos;
                this.Length = pos.Length;
            }

            public bool IsSet()
            {
                return UVIndex != -1 && NormalIndex != -1;
            }

            public bool HasSameTextureAndNormal(int texIndexOther, int normalIndexOther)
            {
                return UVIndex == texIndexOther && NormalIndex == normalIndexOther;
            }
        }

        public static TextureMesh LoadOBJ(string filePath, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
        {
            string finalPath = GLoader.GetContentRelativePath(filePath);

            string line;
            List<OBJVertex> vertices = new List<OBJVertex>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<uint> indexes = new List<uint>();

            try
            {
                using (StreamReader sr = new StreamReader(File.Open(finalPath, FileMode.Open, FileAccess.Read)))
                {
                    // Read Part one of the OBJ file (till the f's start)
                    int i = 0;
                    while (true)
                    {
                        // Get the current line
                        line = sr.ReadLine();

                        // Split it by spaces
                        string[] currentLine = line.Split(' ');

                        // Handle Vertex
                        if (line.StartsWith("v "))
                        {
                            float x, y, z;
                            if (!float.TryParse(currentLine[1], out x)) LogFloatParseError(i, line);
                            if (!float.TryParse(currentLine[2], out y)) LogFloatParseError(i, line);
                            if (!float.TryParse(currentLine[3], out z)) LogFloatParseError(i, line);

                            vertices.Add(new OBJVertex(vertices.Count, new Vector3(x, y, z)));
                        }
                        // Handle UV
                        else if (line.StartsWith("vt "))
                        {
                            float x, y;
                            if (!float.TryParse(currentLine[1], out x)) LogFloatParseError(i, line);
                            if (!float.TryParse(currentLine[2], out y)) LogFloatParseError(i, line);

                            uvs.Add(new Vector2(x, y));
                        }
                        // Handle Normal
                        else if (line.StartsWith("vn "))
                        {
                            float x, y, z;
                            if (!float.TryParse(currentLine[1], out x)) LogFloatParseError(i, line);
                            if (!float.TryParse(currentLine[2], out y)) LogFloatParseError(i, line);
                            if (!float.TryParse(currentLine[3], out z)) LogFloatParseError(i, line);

                            normals.Add(new Vector3(x, y, z));
                        }
                        // Handle Index Start
                        else if (line.StartsWith("f "))
                            break;
                        else if (!line.StartsWith("# "))
                            DashCMD.WriteWarning("Unrecognized OBJ line: {0}", line);

                        i++;
                    }

                    // Read the indexes
                    while (line != null)
                    {
                        // Skip non-index lines
                        if (!line.StartsWith("f "))
                        {
                            line = sr.ReadLine();
                            continue;
                        }

                        // Split the current line by spaces
                        string[] currentLine = line.Split(' ');
                        // Get each vertex
                        string[] vertex1 = currentLine[1].Split('/');
                        string[] vertex2 = currentLine[2].Split('/');
                        string[] vertex3 = currentLine[3].Split('/');

                        // Process each vertex to the arrays
                        ProcessVertex(vertex1, vertices, indexes);
                        ProcessVertex(vertex2, vertices, indexes);
                        ProcessVertex(vertex3, vertices, indexes);

                        // Move to the next line
                        // This is at the end because the first line this loop will read,
                        // will be the previous one that broke out of the part 1 loop.
                        line = sr.ReadLine();
                    }
                }
            }
            catch (IOException)
            {
                // TEMP:
                throw;
            }

            // Remove and unused vertices
            RemoveUnusedVertices(vertices);

            // Finalize
            float[] finalVertices = new float[vertices.Count * 3];
            float[] finalUvs = new float[vertices.Count * 2];
            float[] finalNormals = new float[vertices.Count * 3];

            float furthest = ConvertDataToArrays(vertices, uvs, normals, finalVertices, finalUvs, finalNormals);

            uint[] finalIndexes = indexes.ToArray();

            // Load mesh to a VAO
            return new TextureMesh(usageHint, finalVertices, finalIndexes, finalUvs, finalNormals);
        }

        static float ConvertDataToArrays(List<OBJVertex> vertices, List<Vector2> uvs, List<Vector3> normals,
            float[] finalVertices, float[] finalUVs, float[] finalNormals)
        {
            float furthestPoint = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                OBJVertex vertex = vertices[i];
                // Handle furthest point
                if (vertex.Length > furthestPoint)
                    furthestPoint = vertex.Length;

                // Get position, uv, and normal
                Vector3 position = vertex.Position;
                Vector2 uv = uvs[vertex.UVIndex];
                Vector3 normal = normals[vertex.NormalIndex];

                // Set final vertices
                finalVertices[i * 3] = position.X;
                finalVertices[i * 3 + 1] = position.Y;
                finalVertices[i * 3 + 2] = position.Z;

                // Set final uvs
                finalUVs[i * 2] = uv.X;
                finalUVs[i * 2 + 1] = 1 - uv.Y;

                // Set final normals
                finalNormals[i * 3] = normal.X;
                finalNormals[i * 3 + 1] = normal.Y;
                finalNormals[i * 3 + 2] = normal.Z;
            }

            return furthestPoint;
        }

        static void ProcessVertex(string[] vertexData, List<OBJVertex> vertices, List<uint> indexes)
        {
            // Get vertex
            uint index;
            if (!uint.TryParse(vertexData[0], out index)) LogFloatParseError(-1, "");
            index--;
            // Get the ObjVertex
            OBJVertex currentVertex = vertices[(int)index];
            // Get UV
            int texId;
            if (!int.TryParse(vertexData[1], out texId)) LogFloatParseError(-1, "");
            texId--;
            // Get normal
            int normalId;
            if (!int.TryParse(vertexData[2], out normalId)) LogFloatParseError(-1, "");
            normalId--;

            // Setup current vertex if it is not set
            if (!currentVertex.IsSet())
            {
                currentVertex.UVIndex = texId;
                currentVertex.NormalIndex = normalId;
                indexes.Add(index);
            }
            else
                // Deal with the already processed vertex
                DealWithAlreadyProcessedVertex(currentVertex, texId, normalId, indexes, vertices);
        }

        static void DealWithAlreadyProcessedVertex(OBJVertex vertex, int newTexId, int newNormalId,
            List<uint> indexes, List<OBJVertex> vertices)
        {
            // If it is the same, just add it's index
            if (vertex.HasSameTextureAndNormal(newTexId, newNormalId))
                indexes.Add((uint)vertex.Index);
            else
            {
                OBJVertex prevDupeVertex = vertex.DuplicateVertex;
                if (prevDupeVertex != null)
                    // Handle that vertex's duplicate
                    DealWithAlreadyProcessedVertex(prevDupeVertex, newTexId, newNormalId, indexes, vertices);
                else
                {
                    // Create the duplicated vertex of the previous vertex
                    OBJVertex dupeVertex = new OBJVertex(vertices.Count, vertex.Position);
                    dupeVertex.UVIndex = newTexId;
                    dupeVertex.NormalIndex = newNormalId;

                    vertex.DuplicateVertex = dupeVertex;

                    vertices.Add(dupeVertex);
                    indexes.Add((uint)dupeVertex.Index);
                }
            }
        }

        static void RemoveUnusedVertices(List<OBJVertex> vertices)
        {
            foreach (OBJVertex vertex in vertices)
            {
                if (!vertex.IsSet())
                {
                    vertex.UVIndex = 0;
                    vertex.NormalIndex = 0;
                }
            }
        }

        static void LogFloatParseError(int lineNumber, string line)
        {
            DashCMD.WriteError("OBJ Syntax error on line {0}! \"{1}\"", lineNumber, line);
        }
    }
}
