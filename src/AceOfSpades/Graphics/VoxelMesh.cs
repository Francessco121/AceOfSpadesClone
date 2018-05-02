using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;

/* VoxelMesh.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Graphics
{
    public class VoxelMesh : Mesh
    {
        public BufferObject VertexBuffer { get { return ArrayBufferObjects[0]; } }
        public BufferObject ColorBuffer { get { return ArrayBufferObjects[1]; } }
        public BufferObject NormalBuffer { get { return ArrayBufferObjects[2]; } }

        public VoxelMesh(BufferUsageHint usageHint)
            : base(usageHint, 3)
        {
            CreateBuffers();
        }

        public VoxelMesh(BufferUsageHint usageHint, VoxelMeshBuilder builder)
            : base(usageHint, 3)
        {
            CreateBuffers();
            Update(builder);
        }

        public VoxelMesh(BufferUsageHint usageHint, float[] vertices, uint[] indexes, float[] colors, float[] normals)
            : base(usageHint, 3)
        {
            CreateBuffers();
            Update(vertices, indexes, colors, normals, indexes.Length);
        }

        void CreateBuffers()
        {
            Bind();

            // Vertex Buffer
            InitializeArrayBuffer(0, 4, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);
            // Color Buffer
            InitializeArrayBuffer(1, 4, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);
            // Normal Buffer
            InitializeArrayBuffer(2, 3, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);

            InitializeElementBuffer(BufferUsage);

            Unbind();
        }

        public void Update(VoxelMeshBuilder builder)
        {
            float[] vertices, colors, normals;
            uint[] indexes;
            int indexCount;
            builder.Finalize(out vertices, out colors, out normals, out indexes, out indexCount);

            Update(vertices, indexes, colors, normals, indexCount);
        }

        public void Update(float[] vertices, uint[] indexes, float[] colors, float[] normals, int vertexCount)
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

            VertexCount = vertexCount;

            Unbind();
        }
    }
}
