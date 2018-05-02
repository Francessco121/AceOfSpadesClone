using Dash.Engine.Graphics.OpenGL;

/* TexRenderTarget.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class TexRenderTarget : RenderTarget
    {
        public FramebufferTexture Texture { get; private set; }

        public TexRenderTarget(int width, int height)
            : base(width, height)
        {
            // Bind the FBO
            Bind();

            // Create the textures
            TextureParamPack texParams = new TextureParamPack(TextureMagFilter.Nearest, TextureMinFilter.Nearest, TextureWrapMode.ClampToEdge);
            Texture = new FramebufferTexture(width, height, texParams, FramebufferAttachment.ColorAttachment0,
                PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte);

            // Set and enable the draw buffers
            SetDrawBuffers(DrawBuffersEnum.ColorAttachment0);
            EnableDrawBuffers();

            // Check for errors
            CheckForErrors();

            // Unbind
            Unbind();
        }

        public override void Resize(int width, int height)
        {
            base.Resize(width, height);
            Texture.Resize(Width, Height);
        }

        public override void Dispose()
        {
            Texture.Dispose();
            base.Dispose();
        }
    }
}
