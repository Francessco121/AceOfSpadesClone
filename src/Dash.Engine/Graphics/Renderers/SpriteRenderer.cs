using Dash.Engine.Graphics.Gui;
using Dash.Engine.Graphics.OpenGL;

namespace Dash.Engine.Graphics
{
    public class SpriteRenderer : Renderer2D
    {
        public SpriteBatch SpriteBatch { get; }
        public GUISystem GUISystem { get; }

        SpriteShader shader;

        public SpriteRenderer(MasterRenderer master) 
            : base(master)
        {
            SpriteBatch = new SpriteBatch(master.ScreenWidth, master.ScreenHeight);
            GUISystem = new GUISystem(SpriteBatch);
            shader = new SpriteShader();
        }

        public void Add(GUIArea area)
        {
            GUISystem.Add(area);
        }

        public void Remove(GUIArea area)
        {
            GUISystem.Remove(area);
        }

        public override void Resize(int width, int height)
        {
            SpriteBatch.Resize(width, height);
            GUISystem.OnScreenResized(width, height);
        }

        public override void Update(float deltaTime)
        {
            GUISystem.Update(deltaTime);
            base.Update(deltaTime);
        }

        public override void Prepare()
        {
            if (Master.Gui.Hide)
                return;

            GUISystem.Draw();
        }

        public override void Render()
        {
            StateManager.Disable(EnableCap.CullFace);
            StateManager.Disable(EnableCap.DepthTest);
            StateManager.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            shader.Start();
            shader.LoadMatrix4("projectionMatrix", Matrix4.CreateOrthographic(Master.ScreenWidth, Master.ScreenHeight, 1, -1));
            shader.LoadMatrix4("viewMatrix", Matrix4.Identity);
            shader.LoadMatrix4("transformationMatrix", Matrix4.Identity);

            SpriteBatch.Render(shader);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            shader.Stop();

            StateManager.Enable(EnableCap.CullFace);
        }

        public override void Dispose()
        {
            shader.Dispose();
            SpriteBatch.Dispose();
        }
    }
}
