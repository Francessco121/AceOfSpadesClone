using Dash.Engine;
using Dash.Engine.Graphics.Gui;
using System;

namespace AceOfSpades.Client.Gui
{
    public delegate void ConnectWindowConnectPressedCallback(string endPoint, string name);

    public class ConnectWindow : GUIWindow
    {
        public event ConnectWindowConnectPressedCallback OnConnectPressed;

        GUIButton connectBtn;
        GUITextField endPointField;
        GUITextField playerField;

        public ConnectWindow(GUISystem system, GUITheme theme, UDim2 size) 
            : base(system, size, "Connect to server", theme)
        {
            GUILabel endPointLabel = new GUILabel(new UDim2(0, 10, 0, 50), UDim2.Zero, "Server Address:",
                TextAlign.Left, theme);
            Vector2 labelSize = endPointLabel.Font.MeasureString(endPointLabel.Text);
            endPointField = new GUITextField(new UDim2(0, labelSize.X + 20, 0, 35),
                new UDim2(1f, -labelSize.X - 30, 0, 30), "", TextAlign.Left, theme);

            GUILabel nameLabel = new GUILabel(new UDim2(0, 10, 0, 100), UDim2.Zero, "Player Name:",
                TextAlign.Left, theme);
            labelSize = nameLabel.Font.MeasureString(nameLabel.Text);
            playerField = new GUITextField(new UDim2(0, labelSize.X + 20, 0, 85),
                new UDim2(1f, -labelSize.X - 30, 0, 30), "Player", TextAlign.Left, theme);
            playerField.MaxLength = 60;

            connectBtn = new GUIButton(new UDim2(0, 10, 1f, -40), new UDim2(0, 100, 0, 30), "Connect", theme);
            connectBtn.OnMouseClick += (btn, mbtn) => { TryConnect(); };

            AddTopLevel(endPointLabel, endPointField, nameLabel, playerField, connectBtn);
        }

        void TryConnect()
        {
            if (OnConnectPressed != null)
                OnConnectPressed(endPointField.Text, playerField.Text);
        }

        public override void Update(float deltaTime)
        {
            if (Visible && Input.GetKeyDown(Key.Enter))
                TryConnect();
                
            base.Update(deltaTime);
        }
    }
}
