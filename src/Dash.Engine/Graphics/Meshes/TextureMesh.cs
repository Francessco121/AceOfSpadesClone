using Dash.Engine.Graphics.OpenGL;

/* TextureMesh.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class TextureMesh : Mesh
    {
        public BufferObject VertexBuffer { get { return ArrayBufferObjects[0]; } }
        public BufferObject UVBuffer { get { return ArrayBufferObjects[1]; } }
        public BufferObject NormalBuffer { get { return ArrayBufferObjects[2]; } }

        public TextureMesh(BufferUsageHint usageHint)
            : base(usageHint, 3)
        { }

        public TextureMesh(BufferUsageHint usageHint, MeshBuilder builder)
            : base(usageHint, 3)
        {
            Update(builder);
        }

        public TextureMesh(BufferUsageHint usageHint, float[] vertices, uint[] indexes, float[] uvs, float[] normals)
            : base(usageHint, 3)
        {
            Update(vertices, indexes, uvs, normals);
        }

        void CreateBuffers()
        {
            Bind();

            // Vertex Buffer
            InitializeArrayBuffer(0, 3, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);
            // UV Buffer
            InitializeArrayBuffer(1, 2, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);
            // Normal Buffer
            InitializeArrayBuffer(2, 3, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);

            InitializeElementBuffer(BufferUsage);

            Unbind();
        }

        public void Update(MeshBuilder builder)
        {
            float[] vertices, uvs, normals;
            uint[] indexes;
            builder.Finalize(out vertices, out uvs, out normals, out indexes);

            Update(vertices, indexes, uvs, normals);
        }

        public void Update(float[] vertices, uint[] indexes, float[] uvs, float[] normals)
        {
            Bind();

            ElementBuffer.Bind();
            ElementBuffer.SetData(sizeof(uint) * indexes.Length, indexes);

            VertexBuffer.Bind();
            VertexBuffer.SetData(sizeof(float) * vertices.Length, vertices);

            UVBuffer.Bind();
            UVBuffer.SetData(sizeof(float) * uvs.Length, uvs);

            NormalBuffer.Bind();
            NormalBuffer.SetData(sizeof(float) * normals.Length, normals);

            VertexCount = indexes.Length;

            Unbind();
        }
    }
}
