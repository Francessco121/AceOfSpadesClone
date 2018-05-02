using AceOfSpades.Client.Net;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;

namespace AceOfSpades.Client.Gui
{
    public class ChatBox : GUIFrame
    {
        class ChatItem : GUIFrame
        {
            public float TimeLeft;

            GUILabel label;

            public ChatItem(GUITheme theme, float height, int numFeed,
                string text, BMPFont font)
                : base(UDim2.Zero, UDim2.Zero, image: null)
            {
                Position = new UDim2(0, 0, 0, 20 + (height * numFeed));
                Size = new UDim2(0, 1f, 0, height);

                label = new GUILabel(UDim2.Zero, new UDim2(0, 1, 1f, 0), text, TextAlign.Left, Color.White, theme)
                { Parent = this };

                label.Font = font;

                TimeLeft = 60f;
            }

            public void ShiftY(float offset)
            {
                label.Position.Y.Offset += offset;
            }
        }

        public bool HasFocus { get { return textField.HasFocus; } }

        const int MAX_LINES = 10;
        const float LINE_HEIGHT = 12;

        List<ChatItem> items;
        BMPFont font;
        GUITextField textField;
        MultiplayerScreen screen;

        public ChatBox(UDim2 position, UDim2 size, GUITheme theme, MultiplayerScreen screen)
            : base(position, size, theme)
        {
            this.screen = screen;
            items = new List<ChatItem>();
            Image = null;
            font = AssetManager.LoadFont("arial-bold-11");

            textField = new GUITextField(new UDim2(0, 0, 1f, -15), new UDim2(1f, 0, 0, 30), theme)
            { Parent = this };

            textField.OnEnterPressed += TextField_OnEnterPressed;
        }

        private void TextField_OnEnterPressed(GUITextField field, string text)
        {
            field.Text = "";
            screen.ChatOut(text);
        }

        public void Clear()
        {
            foreach (ChatItem item in items)
                item.Parent = null;
            items.Clear();
        }

        public void Focus()
        {
            textField.HasFocus = true;
        }

        public void Unfocus()
        {
            textField.HasFocus = false;
        }

        public void AddLine(string text)
        {
            Vector2 textSize = font.MeasureString(text);
            int maxXCharacters = (int)Math.Max(CalculatedRectangle.Width / textSize.X * text.Length, 1);

            foreach (string line in text.SplitByLength(maxXCharacters))
            {
                ChatItem item = new ChatItem(Theme, LINE_HEIGHT, items.Count, line, font)
                { Parent = this };
                items.Add(item);

                if (items.Count > MAX_LINES)
                {
                    items[0].Parent = null;
                    items.RemoveAt(0);

                    for (int k = 0; k < items.Count; k++)
                        items[k].ShiftY(-LINE_HEIGHT);
                }
            }
        }

        public override void Update(float deltaTime)
        {
            textField.Visible = textField.HasFocus;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                ChatItem item = items[i];
                item.TimeLeft -= deltaTime;

                if (item.TimeLeft < 0)
                {
                    items.RemoveAt(i);
                    item.Parent = null;

                    for (int k = 0; k < items.Count; k++)
                        items[k].ShiftY(-LINE_HEIGHT);
                }
            }
        }
    }
}
