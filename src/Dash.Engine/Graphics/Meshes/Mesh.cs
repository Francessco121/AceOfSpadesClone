using System;
using Dash.Engine.Graphics.OpenGL;

/* Mesh.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public abstract class Mesh : IGraphicsObject
    {
        public uint VAO { get; private set; }
        public int VertexCount { get; protected set; }

        public BufferObject[] ArrayBufferObjects { get; private set; }
        public BufferObject ElementBuffer { get; private set; }

        public BufferUsageHint BufferUsage { get; set; }
        public BeginMode BeginMode { get; set; }

        public bool RenderAsWireframe;

        protected bool cleanedUp { get; private set; }

        public Mesh(BufferUsageHint usageHint, int numArrayBuffers)
        {
            BufferUsage = usageHint;
            BeginMode = BeginMode.Triangles;

            VAO = GManager.GenVertexArray();
            ArrayBufferObjects = new BufferObject[numArrayBuffers];
        }

        public void Bind()
        {
            if (cleanedUp)
                throw new InvalidOperationException("This Mesh was already cleaned up!");

            GL.BindVertexArray(VAO);
        }

        public void Unbind()
        {
            GL.BindVertexArray(0);
        }

        protected BufferObject InitializeArrayBuffer(uint index, int components, VertexAttribPointerType type, bool normalized,
            int stride, int offset, BufferUsageHint hint)
        {
            if (index < 0 || index >= ArrayBufferObjects.Length)
                throw new IndexOutOfRangeException(
                    "Index must be within range of the number of array buffer objects for this vertex buffer!");

            if (ArrayBufferObjects[index] != null)
                throw new InvalidOperationException(
                    string.Format("ArrayBuffer {0} was already initialized for this vertex buffer!", index));

            GLError.Begin();

            BufferObject buffer = new BufferObject(BufferTarget.ArrayBuffer, hint);
            buffer.Bind();
            GL.VertexAttribPointer(index, components, type, normalized, stride, new IntPtr(offset));
            GL.EnableVertexAttribArray(index);

            ErrorCode err = GLError.End();
            if (err != ErrorCode.NoError)
                throw new Exception(string.Format("Failed to initialize array buffer: {0}", err));

            ArrayBufferObjects[index] = buffer;
            return buffer;
        }

        protected BufferObject InitializeElementBuffer(BufferUsageHint hint)
        {
            if (ElementBuffer != null)
                throw new InvalidOperationException("This vertex buffer already has an element buffer!");

            ElementBuffer = new BufferObject(BufferTarget.ElementArrayBuffer, hint);
            ElementBuffer.Bind();
            return ElementBuffer;
        }

        public virtual void Dispose()
        {
            if (!cleanedUp)
            {
                cleanedUp = true;

                for (int i = 0; i < ArrayBufferObjects.Length; i++)
                    ArrayBufferObjects[i].Dispose();

                if (ElementBuffer != null)
                    ElementBuffer.Dispose();
            }
        }
    }
}
