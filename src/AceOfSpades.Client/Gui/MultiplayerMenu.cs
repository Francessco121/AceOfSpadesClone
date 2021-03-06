﻿using Dash.Engine;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Client.Gui
{
    public class MultiplayerMenu : GUIWindow
    {
        public MultiplayerMenu(GUISystem system, GUITheme theme, MainWindow mainWindow) 
            : base(system, new UDim2(0.35f, 0, 0.3f, 0), "Ace of Spades", theme)
        {
            IsDraggable = false;

            MaxSize = new UDim2(0, 400, 1f, 0);
            MinSize = new UDim2(0, 220, 0, 100);

            GUIButton disconnectBtn = new GUIButton(new UDim2(0, 0, 0, 25), new UDim2(1f, 0, 0, 30),
                "Disconnect", TextAlign.Center, theme);
            disconnectBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                    mainWindow.SwitchScreen("MainMenu");
            };

            GUIButton controlsBtn = new GUIButton(new UDim2(0, 0, 0, 60), new UDim2(1f, 0, 0, 30),
                "View Controls", TextAlign.Center, theme);
            controlsBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                    mainWindow.StaticGui.ToggleControlsWindow(true);
            };

            AddTopLevel(disconnectBtn, controlsBtn);
        }

        protected override void Shown()
        {
            Center();
            base.Shown();
        }
    }
}
