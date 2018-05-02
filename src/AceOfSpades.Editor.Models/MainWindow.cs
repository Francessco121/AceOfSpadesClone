using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;

/* ToolBar.cs
 * Ethan Lafrenais
 * Tristan Smith
*/

namespace AceOfSpades.Editor.Models
{
    public class MainWindow : GameWindow
    {
        EditorScreen screen;

        public MainWindow()
            : base("Ace of Spades Model Editor", 1280, 720)
        { }

        protected override void Load()
        {
            SetVSync(true);

            Renderer.GFXSettings.RenderShadows = false;
            Renderer.GFXSettings.FogQuality = FogQuality.Off;

            Renderer.AddRenderer(new EntityRenderer(Renderer));
            Renderer.AddRenderer(new DebugRenderer(Renderer));
            Renderer.AddRenderer(new ChunkRenderer(Renderer));

            Renderer.Sun = new Light(Vector3.Zero, LightType.Directional, 1, Color.White);
            Renderer.Lights.Add(Renderer.Sun);

            Camera.Active.SetMode(CameraMode.ArcBall);
            Camera.Active.SmoothCamera = true;
            Camera.Active.Speeds[0] = 0.1f;
            Camera.Active.Speeds[1] = 0.25f;
            Camera.Active.ArcBallScrollSpeed = 0.75f;
            Camera.Active.ArcBallMinimumRadius = 1f;

            screen = new EditorScreen(this, Renderer);
        }

        protected override void Draw(float deltaTime)
        {
            screen.Draw();
        }

        protected override void Resized(int width, int height) { }

        protected override void Update(float deltaTime)
        {
            screen.Update(deltaTime);
        }

        public void UpdateTitle(string fileName)
        {
            Title = $"Ace of Spades Model Editor - {fileName ?? "<untitled>"}";
        }
    }
}
