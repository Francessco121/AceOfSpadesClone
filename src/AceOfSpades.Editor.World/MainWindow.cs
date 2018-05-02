using AceOfSpades.Graphics;
using AceOfSpades.Graphics.Renderers;
using Dash.Engine;
using Dash.Engine.Graphics;

/* MainWindow.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World
{
    public class MainWindow : GameWindow
    {
        EditorScreen screen;

        public MainWindow()
            : base("Ace of Spades World Editor", 1280, 720)
        {
            Net.GlobalNetwork.IsClient = true;
        }

        protected override void Load()
        {
            SetVSync(false);
            TargetFrameRate = 120;

            Renderer.GFXSettings.ShadowResolution = 4096;
            Renderer.FogEnabled = false;
            //Renderer.GFXSettings.ApplyFXAA = true;

            Renderer.AddRenderer(new VoxelRenderer(Renderer));
            Renderer.AddRenderer(new ChunkRenderer(Renderer));
            Renderer.AddRenderer(new EntityRenderer(Renderer));
            Renderer.AddRenderer(new DebugRenderer(Renderer));

            Renderer.Sun = new Light(Vector3.Zero, LightType.Directional, 1, Color.White);
            Renderer.Lights.Add(Renderer.Sun);

            Camera.Active.SetMode(CameraMode.ArcBall);
            Camera.Active.SmoothCamera = true;

            screen = new EditorScreen(this, Renderer);
        }

        public void UpdateTitle(string fileName)
        {
            Title = string.Format("Ace of Spades World Editor - {0}", fileName != null ? fileName : "<untitled>");
        }

        protected override void Resized(int width, int height) { }

        protected override void Update(float deltaTime)
        {
            screen.Update(deltaTime);
        }

        protected override void Draw(float deltaTime)
        {
            screen.Draw();
        }
    }
}
