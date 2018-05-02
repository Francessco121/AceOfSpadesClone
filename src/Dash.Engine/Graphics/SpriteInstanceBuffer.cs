using Dash.Engine.Graphics.OpenGL;
using System;

namespace Dash.Engine.Graphics
{
    public class SpriteInstanceBuffer
    {
        public const int MaxSprites = 4096;

        uint VAO;
        uint VertexBuffer;
        uint PositionBuffer;
        uint OrientationBuffer;
        uint ColorBuffer;

        int numSprites;

        public SpriteInstanceBuffer()
        {
            VAO = GManager.GenVertexArray();
            GL.BindVertexArray(VAO);

            float[] vertexData = new float[]
            {
                -0.5f, -0.5f,
                 0.5f, -0.5f,
                 -0.5f, 0.5f,
                 0.5f, 0.5f
            };

            int floatSize = sizeof(float);

            // Setup buffers
            VertexBuffer = GManager.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * floatSize, vertexData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            PositionBuffer = GManager.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(MaxSprites * 2 * floatSize), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            ColorBuffer = GManager.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(MaxSprites * 4 * sizeof(byte)), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, 0, IntPtr.Zero);

            OrientationBuffer = GManager.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, OrientationBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(MaxSprites * 3 * floatSize), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindVertexArray(0);
        }

        public void UpdateBuffers(int numSprites, float[] positions, byte[] colors, float[] orientations)
        {
            this.numSprites = numSprites;

            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(MaxSprites * 2 * sizeof(float)), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, numSprites * sizeof(float) * 2, positions);

            GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(MaxSprites * 4 * sizeof(byte)), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, numSprites * 4 * sizeof(byte), colors);

            GL.BindBuffer(BufferTarget.ArrayBuffer, OrientationBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(MaxSprites * 3 * sizeof(float)), IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, numSprites * 3 * sizeof(float), orientations);
        }

        public void Begin()
        {
            GL.BindVertexArray(VAO);
        }

        public void Render()
        {
            GL.VertexAttribDivisor(0, 0);
            GL.VertexAttribDivisor(1, 1);
            GL.VertexAttribDivisor(2, 1);
            GL.VertexAttribDivisor(3, 1);

            GL.DrawArraysInstanced(BeginMode.TriangleStrip, 0, 4, numSprites);
        }

        public void End()
        {
            GL.BindVertexArray(0);
        }
    }
}
