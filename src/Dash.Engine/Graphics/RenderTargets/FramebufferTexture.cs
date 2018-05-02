using System;
using Dash.Engine.Graphics.OpenGL;

/* FramebufferTexture.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class FramebufferTexture : Texture
    {
        public PixelInternalFormat InternalPixelFormat { get; private set; }
        public PixelFormat PixelFormat { get; private set; }
        public PixelType PixelType { get; private set; }
        public FramebufferAttachment AttachmentType { get; private set; }

        public FramebufferTexture(int screenWidth, int screenHeight, TextureParamPack texParams,
            FramebufferAttachment attachmentType, PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
            : base(texParams)
        {
            this.AttachmentType = attachmentType;
            this.InternalPixelFormat = internalFormat;
            this.PixelFormat = format;
            this.PixelType = type;
            this.Width = screenWidth;
            this.Height = screenHeight;

            CreateTexture();
        }

        public void Resize(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            Bind();
            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalPixelFormat, Width, Height, 0,
                PixelFormat, PixelType, IntPtr.Zero);
        }

        void CreateTexture()
        {
            Bind();

            // Create texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalPixelFormat, Width, Height, 0,
                PixelFormat, PixelType, IntPtr.Zero);

            // Attach
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, AttachmentType, TextureTarget.Texture2D, Id, 0);
        }
    }
}
