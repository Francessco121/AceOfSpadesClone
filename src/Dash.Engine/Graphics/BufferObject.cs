using Dash.Engine.Graphics.OpenGL;
using System;

namespace Dash.Engine.Graphics
{
    public class BufferObject : IGraphicsObject
    {
        public readonly uint Id;
        public readonly BufferTarget Target;
        public readonly BufferUsageHint UsageHint;

        public int Size { get; private set; }

        internal BufferObject(BufferTarget target, BufferUsageHint hint)
        {
            Id = GManager.GenBuffer();
            Target = target;
            UsageHint = hint;
            Size = -1;
        }

        public void Bind()
        {
            GL.BindBuffer(Target, Id);
        }

        public void Unbind()
        {
            GL.BindBuffer(Target, 0);
        }

        public void Invalidate()
        {
            if (MasterRenderer.GLVersion < 4.3)
            {
                CheckUpdate();
                GL.BufferData(Target, new IntPtr(Size), IntPtr.Zero, UsageHint);
            }
            else
                GL.InvalidateBufferData(Id);
        }

        #region SetData
        public void SetData(IntPtr data)
        {
            CheckUpdate();
            SetData(Size, data);
        }

        public void SetData<T>(ref T data)
            where T : struct
        {
            CheckUpdate();
            SetData(Size, ref data);
        }

        public void SetData<T>(T[] data)
            where T : struct
        {
            CheckUpdate();
            SetData(Size, data);
        }

        public void SetData<T>(T[,] data)
            where T : struct
        {
            CheckUpdate();
            SetData(Size, data);
        }

        public void SetData<T>(T[,,] data)
            where T : struct
        {
            CheckUpdate();
            SetData(Size, data);
        }

        public void SetData(int size)
        {
            SetData(size, IntPtr.Zero);
        }

        public void SetData<T>(int size, T[] data)
            where T : struct
        {
            Size = size;
            GL.BufferData(Target, size, data, UsageHint);
        }

        public void SetData<T>(int size, T[,] data)
            where T : struct
        {
            Size = size;
            GL.BufferData(Target, size, data, UsageHint);
        }

        public void SetData<T>(int size, T[,,] data)
            where T : struct
        {
            Size = size;
            GL.BufferData(Target, size, data, UsageHint);
        }

        public void SetData<T>(int size, ref T data)
            where T : struct
        {
            Size = size;
            GL.BufferData(Target, size, ref data, UsageHint);
        }

        public void SetData(int size, IntPtr data)
        {
            Size = size;
            GL.BufferData(Target, new IntPtr(size), data, UsageHint);
        }
        #endregion

        #region UpdateData
        void CheckUpdate()
        {
            if (Size == -1)
                throw new InvalidOperationException("Attempt to update an unintialized buffer object!");
        }

        public void UpdateData(IntPtr data)
        {
            CheckUpdate();
            SetData(Size, data);
        }

        public void UpdateData<T>(T[] data)
            where T : struct
        {
            CheckUpdate();
            SetData(Size, data);
        }

        public void UpdateData<T>(T[,] data)
            where T : struct
        {
            CheckUpdate();
            SetData(Size, data);
        }

        public void UpdateData<T>(T[,,] data)
            where T : struct
        {
            CheckUpdate();
            SetData(Size, data);
        }

        public void UpdateData<T>(ref T data)
            where T : struct
        {
            CheckUpdate();
            SetData(Size, ref data);
        }
        #endregion

        #region SetSubData
        public void SetSubData(int size, int offset, IntPtr data)
        {
            GL.BufferSubData(Target, new IntPtr(offset), size, data);
        }

        public void SetSubData<T>(int size, int offset, T[] data)
            where T : struct
        {
            GL.BufferSubData(Target, new IntPtr(offset), size, data);
        }

        public void SetSubData<T>(int size, int offset, T[,] data)
            where T : struct
        {
            GL.BufferSubData(Target, new IntPtr(offset), size, data);
        }

        public void SetSubData<T>(int size, int offset, T[,,] data)
            where T : struct
        {
            GL.BufferSubData(Target, new IntPtr(offset), size, data);
        }

        public void SetSubData<T>(int size, int offset, ref T data)
            where T : struct
        {
            GL.BufferSubData(Target, new IntPtr(offset), size, ref data);
        }
        #endregion

        public void Dispose()
        {
            GManager.DeleteBuffer(Id);
        }
    }
}
