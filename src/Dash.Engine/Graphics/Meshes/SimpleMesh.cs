using Dash.Engine.Graphics.OpenGL;

/* SimpleMesh.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class SimpleMesh : Mesh
    {
        public BufferObject VertexBuffer { get { return ArrayBufferObjects[0]; } }
        public int Dimensions { get; private set; }

        public SimpleMesh(BufferUsageHint usageHint, int dimensions)
            : base(usageHint, 1)
        {
            Dimensions = dimensions;
            CreateBuffers();
        }

        public SimpleMesh(BufferUsageHint usageHint, int dimensions, float[] vertices)
            : base(usageHint, 1)
        {
            Dimensions = dimensions;
            CreateBuffers();
            Update(vertices);
        }

        void CreateBuffers()
        {
            Bind();
            InitializeArrayBuffer(0, Dimensions, VertexAttribPointerType.Float, false, 0, 0, BufferUsage);
            Unbind();
        }

        public void Update(float[] vertices)
        {
            Bind();

            VertexBuffer.Bind();
            VertexBuffer.SetData(sizeof(float) * vertices.Length, vertices);

            VertexCount = vertices.Length / Dimensions;

            Unbind();
        }
    }
}
