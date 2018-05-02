using Dash.Engine.Graphics.OpenGL;
using System;

/* ShadowMap.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class ShadowMap : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        uint fbo;
        uint depthTex;

        public ShadowMap(int width, int height) 
        {
            Width = width;
            Height = height;

            GLError.Begin();

            fbo = GL.GenFramebuffer();
            depthTex = GL.GenTexture();

            BindTex();

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0,
                PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameterfv(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[] { 1, 1, 1, 1 });

            // Bind the FBO
            Bind();

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, 
                TextureTarget.Texture2D, depthTex, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            // Check for errors
            FramebufferErrorCode ec = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (ec != FramebufferErrorCode.FramebufferComplete)
                throw new GPUResourceException("Failed to create FBO. Reason: " + ec.ToString());

            // Unbind
            Unbind();

            ErrorCode err = GLError.End();
            if (err != ErrorCode.NoError)
                throw new Exception(string.Format("Failed to initialize shadow map: OpenGL error: {0}", err));
        }

        public void Resize(int width, int height)
        {
            Width = Math.Max(width, 1);
            Height = Math.Max(height, 1);

            BindTex();

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, Width, Height, 0,
                PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            UnbindTex();
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        }

        public void BindTex()
        {
            GL.BindTexture(TextureTarget.Texture2D, depthTex);
        }

        public void UnbindTex()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(fbo);
            GL.DeleteTexture(depthTex);
        }
    }
}
