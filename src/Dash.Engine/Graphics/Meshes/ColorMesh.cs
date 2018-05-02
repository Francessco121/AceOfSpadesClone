using Dash.Engine.Graphics.OpenGL;

/* ColorMesh.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class ColorMesh : Mesh
    {
        public BufferObject VertexBuffer { get { return ArrayBufferObjects[0]; } }
        public BufferObject ColorBuffer { get { return ArrayBufferObjects[1]; } }
        public BufferObject NormalBuffer { get { return ArrayBufferObjects[2]; } }

        public ColorMesh(BufferUsageHint usageHint)
            : base(usageHint, 3)
        {
            CreateBuffers();
        }

        public ColorMesh(BufferUsageHint usageHint, ColorMeshBuilder builder)
            : base(usageHint, 3)
        {
            CreateBuffers();
            Update(builder);
        }

        public ColorMesh(BufferUsageHint usageHint, float[] vertices, uint[] indexes, float[] colors, float[] normals)
            : base(usageHint, 3)
        {
            CreateBuffers();
            Update(vertices, indexes, colors, normals);
        }

        void CreateBuffers()
        {
            Bind();

            // Vertex Buffer
            InitializeArrayBuffer(0, 3, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);
            // Color Buffer
            InitializeArrayBuffer(1, 4, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);
            // Normal Buffer
            InitializeArrayBuffer(2, 3, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);

            InitializeElementBuffer(BufferUsage);

            Unbind();
        }

        public void Update(ColorMeshBuilder builder)
        {
            float[] vertices, colors, normals;
            uint[] indexes;
            builder.Finalize(out vertices, out colors, out normals, out indexes);

            Update(vertices, indexes, colors, normals);
        }

        public void Update(float[] vertices, uint[] indexes, float[] colors, float[] normals)
        {
            Bind();

            ElementBuffer.Bind();
            ElementBuffer.SetData(sizeof(uint) * indexes.Length, indexes);

            VertexBuffer.Bind();
            VertexBuffer.SetData(sizeof(float) * vertices.Length, vertices);

            ColorBuffer.Bind();
            ColorBuffer.SetData(sizeof(float) * colors.Length, colors);

            NormalBuffer.Bind();
            NormalBuffer.SetData(sizeof(float) * normals.Length, normals);

            VertexCount = indexes.Length;

            Unbind();
        }
    }
}
