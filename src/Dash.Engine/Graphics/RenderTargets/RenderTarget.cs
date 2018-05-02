using System;
using Dash.Engine.Graphics.OpenGL;

/* RenderTarget.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public abstract class RenderTarget : IDisposable
    {
        public uint FBO { get; protected set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        protected DrawBuffersEnum[] DrawBuffers { get; private set; }

        public RenderTarget(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            // Initialize FBO
            FBO = GManager.GenFramebuffer();
        }

        public virtual void Resize(int width, int height)
        {
            Width = Math.Max(width, 1);
            Height = Math.Max(height, 1);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        protected void CheckForErrors()
        {
            FramebufferErrorCode ec = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (ec != FramebufferErrorCode.FramebufferComplete)
                throw new GPUResourceException("Failed to create FBO. Reason: " + ec.ToString());
        }

        public void SetDrawBuffers(params DrawBuffersEnum[] drawBuffers)
        {
            DrawBuffers = drawBuffers;
        }

        public void EnableDrawBuffers()
        {
            GL.DrawBuffers(DrawBuffers.Length, DrawBuffers);
        }

        public void SetReadBuffer(ReadBufferMode mode)
        {
            GL.ReadBuffer(mode);
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        }

        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public virtual void Dispose()
        {
            GManager.DeleteFramebuffer(FBO);
        }
    }
}
