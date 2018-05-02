using Dash.Engine.Graphics.OpenGL;
using System;

namespace Dash.Engine.Graphics
{
    public class PostProcessBuffer : RenderTarget
    {
        public FramebufferTexture ColorTexture { get; private set; }

        uint depthBuffer;

        public PostProcessBuffer(int width, int height) 
            : base(width, height)
        {
            GLError.Begin();

            // Bind the FBO
            Bind();

            // Create the textures
            TextureParamPack texParams = new TextureParamPack(TextureMagFilter.Nearest, TextureMinFilter.Nearest, TextureWrapMode.ClampToEdge);
            ColorTexture = new FramebufferTexture(width, height, texParams, FramebufferAttachment.ColorAttachment0,
                PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.HalfFloat);
            
            // Create the depth buffer
            depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, 
                RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            // Set and enable the draw buffers
            SetDrawBuffers(DrawBuffersEnum.ColorAttachment0);
            EnableDrawBuffers();

            // Check for errors
            CheckForErrors();

            // Unbind
            Unbind();

            ErrorCode err = GLError.End();
            if (err != ErrorCode.NoError)
                throw new Exception(string.Format("Failed to create PostProcessBuffer. OpenGL Error: {0}", err));
        }

        public override void Resize(int width, int height)
        {
            base.Resize(width, height);

            ColorTexture.Resize(Width, Height);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }

        public override void Dispose()
        {
            ColorTexture.Dispose();
            GL.DeleteRenderbuffer(depthBuffer);
            base.Dispose();
        }
    }
}
