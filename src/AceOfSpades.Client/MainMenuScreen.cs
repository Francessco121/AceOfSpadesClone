using AceOfSpades.Client.Gui;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using Dash.Net;
using System.Net;

/* MainMenuScreen.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public class MainMenuScreen : GameScreen
    {
        GUITheme theme;

        MessageWindow popup;
        ConnectWindow connectWindow;

        public MainMenuScreen(MainWindow mainWindow) 
            : base(mainWindow, "MainMenu")
        {
            theme = AssetManager.CreateDefaultGameTheme();
            theme.SetField("SmallFont", AssetManager.LoadFont("arial-bold-14"));
            theme.SetField("Font", AssetManager.LoadFont("arial-16"));
            theme.SetField("BigFont", AssetManager.LoadFont("arial-20"));

            popup = new MessageWindow(GUISystem, theme, new UDim2(0.1f, 0, 0.3f, 0), "Alert!");
            popup.MinSize = new UDim2(0, 215, 0, 200);
            popup.MaxSize = new UDim2(1f, 0, 0, 275);

            connectWindow = new ConnectWindow(GUISystem, theme, new UDim2(1f, 0, 1f, 0));
            connectWindow.MinSize = new UDim2(0, 375, 0, 200);
            connectWindow.MaxSize = new UDim2(0, 700, 0, 200);
            connectWindow.OnConnectPressed += ConnectWindow_OnConnectPressed;

            GUIFrame title = new GUIFrame(new UDim2(0.5f, -260, 0.2f, -40), new UDim2(0, 520, 0, 80), 
                new Image(GLoader.LoadTexture("Textures/title.png")));

            GUIFrame btnFrame = new GUIFrame(new UDim2(0.5f, -200, 0.5f, -50), new UDim2(0, 400, 0, 110), theme);
            btnFrame.Image = null;

            GUIButton connectBtn = new GUIButton(new UDim2(0, 0, 0, 0), new UDim2(1f, 0, 0, 30), "Connect to a Server",
                TextAlign.Center, theme)
            { Parent = btnFrame };
            connectBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                    connectWindow.Visible = true;
            };

            GUIButton controlsBtn = new GUIButton(new UDim2(0, 0, 0, 40), new UDim2(1f, 0, 0, 30), "View Controls",
                TextAlign.Center, theme)
            { Parent = btnFrame };
            controlsBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                    Window.StaticGui.ToggleControlsWindow(true);
            };

            GUIButton spBtn = new GUIButton(new UDim2(0, 0, 0, 80), new UDim2(1f, 0, 0, 30), "Start Singleplayer Test",
                TextAlign.Center, theme)
            { Parent = btnFrame };
            spBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                    Window.SwitchScreen("Singleplayer");
            };

            GUIButton randomImageButton = new GUIButton(new UDim2(1f, -160, 1f, -40), new UDim2(0, 150, 0, 30), 
                "Random Image", theme);
            randomImageButton.OnMouseClick += (btn, mbtn) => { Window.StaticGui.ShowRandomBackgroundImage(); };

            GUIArea.AddTopLevel(title, randomImageButton, btnFrame);
            GUISystem.Add(connectWindow, popup);
            Windows.Add(connectWindow);
            Windows.Add(popup);
        }

        private void ConnectWindow_OnConnectPressed(string e, string name)
        {
            if (string.IsNullOrWhiteSpace(e))
                e = "auto:auto";

            if (string.IsNullOrWhiteSpace(name))
            {
                popup.Show("Please enter a valid name.");
                return;
            }

            string[] parts = e.Split(':');
            if (parts.Length != 2)
            {
                popup.Show(string.Format("Invalid address '{0}'", e));
                return;
            }

            IPAddress ip;
            if (!NetHelper.TryParseIP(parts[0], out ip))
            {
                popup.Show(string.Format("Invalid ip address '{0}'", parts[0]));
                return;
            }

            int port;
            if (parts[1] == "auto")
                port = 12123;
            else if (!int.TryParse(parts[1], out port))
            {
                popup.Show(string.Format("Invalid port '{0}'", parts[1]));
                return;
            }

            IPEndPoint ep = new IPEndPoint(ip, port);
            popup.Show(string.Format("Connecting to '{0}'...", ep.ToString()));
            Window.SwitchScreen("Multiplayer", ep, name);
        }

        protected override void OnLoad(object[] args)
        {
            Window.StaticGui.ShowRandomBackgroundImage();
            StaticGui.ShowBackground = true;

            connectWindow.Visible = false;
            popup.Visible = false;

            StaticGui.IsVisible = false;
            Camera.Active.AllowUserControl = false;

            Input.IsCursorLocked = false;
            Input.IsCursorVisible = true;

            if (args.Length > 0 && args[0] is string)
                popup.Show((string)args[0]);

            base.OnLoad(args);
        }

        protected override void OnUnload()
        {
            StaticGui.IsVisible = true;
            Camera.Active.AllowUserControl = true;
            base.OnUnload();
        }

        public override void Update(float deltaTime)
        {
            
        }

        public override void Draw()
        {
            
        }
    }
}
