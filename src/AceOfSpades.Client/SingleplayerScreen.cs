using AceOfSpades.Characters;
using AceOfSpades.Client.Gui;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

/* SingleplayerScreen.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public class SingleplayerScreen : GameScreen
    {
        SPWorld world;
        SingleplayerMenu menu;

        public SingleplayerScreen(MainWindow window)
            : base(window, "Singleplayer")
        {
            GUITheme theme = AssetManager.CreateDefaultGameTheme();
            theme.SetField("Font", AssetManager.LoadFont("arial-14"));

            menu = new SingleplayerMenu(GUISystem, theme, Window);
            menu.OnClosed += Menu_OnClosed;

            GUISystem.Add(menu);
            Windows.Add(menu);
        }

        private void Window_OnFocusChanged(GameWindow window, bool focused)
        {
            if (!focused)
            {
                menu.Visible = true;
                if (world.Player != null)
                    world.Player.AllowUserInput = !menu.Visible;

                Input.IsCursorLocked = !menu.Visible;
                Input.IsCursorVisible = menu.Visible;
                Camera.Active.AllowUserControl = !menu.Visible;
                Camera.Active.HoldM2ToLook = menu.Visible;
                Camera.Active.SmoothCamera = menu.Visible;
            }
        }

        private void Menu_OnClosed(GUIWindowBase e)
        {
            Input.IsCursorLocked = true;
            Input.IsCursorVisible = false;
            Camera.Active.AllowUserControl = true;
            Camera.Active.HoldM2ToLook = false;
            Camera.Active.SmoothCamera = false;

            if (world.Player != null)
                world.Player.AllowUserInput = true;
        }

        protected override void OnLoad(object[] args)
        {
            StaticGui.ShowBackground = false;
            Window.OnFocusChanged += Window_OnFocusChanged;
            world = new SPWorld(Renderer);
            base.OnLoad(args);
        }

        protected override void OnUnload()
        {
            Renderer.GlobalWireframe = false;
            Window.OnFocusChanged -= Window_OnFocusChanged;
            world.Dispose();
            base.OnUnload();
        }

        public override void OnScreenResized(int width, int height)
        {
            world.OnScreenResized(width, height);
            base.OnScreenResized(width, height);
        }

        public override void Update(float deltaTime)
        {
            if (Input.GetControlDown("ToggleMenu"))
            {
                menu.Visible = !menu.Visible;
                if (world.Player != null)
                    world.Player.AllowUserInput = !menu.Visible;

                Input.IsCursorLocked = !menu.Visible;
                Input.IsCursorVisible = menu.Visible;
                Camera.Active.AllowUserControl = !menu.Visible;
                Camera.Active.HoldM2ToLook = menu.Visible;
                Camera.Active.SmoothCamera = menu.Visible;
            }

            if (Input.GetKeyDown(Key.F1))
            {
                Renderer.GlobalWireframe = !Renderer.GlobalWireframe;
                if (Renderer.GlobalWireframe)
                    StateManager.UsePointWireframe = Input.IsControlHeld;
            }

            if (Input.GetKeyDown(Key.F2))
                Renderer.DebugRenderShadowMap = !Renderer.DebugRenderShadowMap;
            if (Input.GetKeyDown(Key.F3))
                Player.DrawCollider = !Player.DrawCollider;

            float sunSpeed = Input.IsControlHeld ? 0.5f : Input.IsShiftHeld ? 4 : 2;
            if (Input.GetKey(Key.PageUp))
                Renderer.Sky.currentHour += sunSpeed * deltaTime;
            else if (Input.GetKey(Key.PageDown))
                Renderer.Sky.currentHour -= sunSpeed * deltaTime;

            world.Update(deltaTime);
        }

        public override void Draw()
        {
            world.Draw();
        }
    }
}
