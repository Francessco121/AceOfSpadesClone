using System;

/* RenderPipeline.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public abstract class RenderPipeline : IDisposable
    {
        protected MasterRenderer Renderer { get; private set; }
        protected int ScreenWidth { get { return Renderer.ScreenWidth; } }
        protected int ScreenHeight { get { return Renderer.ScreenHeight; } }
        protected GraphicsOptions GFXSettings { get { return Renderer.GFXSettings; } }

        public RenderPipeline(MasterRenderer renderer)
        {
            Renderer = renderer;
        }

        protected bool IsRenderingEnabled(RendererFlags flag)
        {
            return Renderer.EnabledRendering.HasFlag(flag);
        }

        public abstract void Resize(int width, int height);
        public abstract void PrepareMesh(Mesh mesh, RenderPass pass);
        public abstract void EndMesh();
        public abstract void TakeScreenshot(ScreenshotRequest request);
        public abstract void Render(float deltaTime);
        public abstract void Dispose();
    }
}
