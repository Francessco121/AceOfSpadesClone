using Dash.Engine.Graphics.OpenGL;
using System;

/* SpriteVertexBuffer.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    public class SpriteVertexBuffer : Mesh
    {
        public const int MAX_SPRITES = 4096;

        public BufferObject VertexBuffer { get { return ArrayBufferObjects[0]; } }
        public BufferObject UVBuffer { get { return ArrayBufferObjects[1]; } }
        public BufferObject ColorBuffer { get { return ArrayBufferObjects[2]; } }

        public SpriteVertexBuffer() 
            : base(BufferUsageHint.StreamDraw, 3)
        {
            Bind();

            InitializeArrayBuffer(0, 2, VertexAttribPointerType.Float, false, 0, 0, BufferUsageHint.StreamDraw);
            VertexBuffer.SetData(MAX_SPRITES * 8 * sizeof(float));

            InitializeArrayBuffer(1, 2, VertexAttribPointerType.Float, false, 0, 0, BufferUsageHint.StreamDraw);
            UVBuffer.SetData(MAX_SPRITES * 8 * sizeof(float));

            InitializeArrayBuffer(2, 4, VertexAttribPointerType.UnsignedByte, true, 0, 0, BufferUsageHint.StreamDraw);
            ColorBuffer.SetData(MAX_SPRITES * 16 * sizeof(byte));

            InitializeElementBuffer(BufferUsageHint.StaticDraw);
            ElementBuffer.SetData(MAX_SPRITES * 6 * sizeof(uint), ConstructElementBuffer());
            
            Unbind();
        }

        static uint[] ConstructElementBuffer()
        {
            uint[] indexes = new uint[MAX_SPRITES * 6];
            uint k = 0;
            for (int i = 0, j = 0; j < MAX_SPRITES; i += 6, k += 4, j++)
            {
                indexes[i] = k;
                indexes[i + 1] = k + 1;
                indexes[i + 2] = k + 2;

                indexes[i + 3] = k;
                indexes[i + 4] = k + 2;
                indexes[i + 5] = k + 3;
            }

            return indexes;
        }

        public void UpdateBuffers(int numSprites, float[] vertices, float[] uvs, byte[] colors)
        {
            VertexBuffer.Bind();
            //VertexBuffer.Invalidate();
            VertexBuffer.SetSubData(numSprites * 8 * sizeof(float), 0, vertices);

            UVBuffer.Bind();
            //UVBuffer.Invalidate();
            UVBuffer.SetSubData(numSprites * 8 * sizeof(float), 0, uvs);

            ColorBuffer.Bind();
            //ColorBuffer.Invalidate();
            ColorBuffer.SetSubData(numSprites * 16 * sizeof(byte), 0, colors);
        }

        public void Render(int numSprites)
        {
            ElementBuffer.Bind();
            GL.DrawElements(BeginMode.Triangles, numSprites * 6, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Tell opengl we don't need this data anymore.
            VertexBuffer.Bind();
            VertexBuffer.Invalidate();

            UVBuffer.Bind();
            UVBuffer.Invalidate();

            ColorBuffer.Bind();
            ColorBuffer.Invalidate();
        }
    }
}
