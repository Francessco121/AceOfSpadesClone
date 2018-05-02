using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System.Collections.Generic;

namespace AceOfSpades.Client.Gui
{
    public class ControlsWindow : GUIWindow
    {
        List<GUILabel> labels;
        BMPFont font;

        public ControlsWindow(GUISystem system, GUITheme theme) 
            : base(system, UDim2.Zero, "Controls", theme)
        {
            labels = new List<GUILabel>();
            Setup();
            ZIndex = 100;

            font = theme.GetField<BMPFont>(null, "SmallFont");
        }

        public void Show()
        {
            Setup();
            Visible = true;
        }

        void Setup()
        {
            foreach (GUILabel label in labels)
                RemoveTopLevel(label);
            labels.Clear();

            Image backImage1 = Image.CreateBlank(new Color(80, 80, 80, 128));
            Image backImage2 = Image.CreateBlank(new Color(40, 40, 40, 128));

            float y = 20;
            int i = 0;
            foreach (KeyValuePair<string, Input.InputBind> bind in Input.Binds)
            {
                Image image = i % 2 == 0 ? backImage1 : backImage2;

                GUILabel label1 = new GUILabel(new UDim2(0, 0, 0, y), new UDim2(0.5f, 0, 0, 22),
                    bind.Key, TextAlign.Left, Theme)
                { Font = font, BackgroundImage = image };
                GUILabel label2 = new GUILabel(new UDim2(0.5f, 0, 0, y), new UDim2(0.5f, 0, 0, 22),
                    bind.Value.ToString(), TextAlign.Left, Theme)
                { Font = font, BackgroundImage = image };
                label1.ZIndex = -1;
                label2.ZIndex = 1;
                labels.Add(label1);
                labels.Add(label2);
                y += label1.Size.Y.Offset;
                i++;

                AddTopLevel(label1, label2);
            }

            Size = new UDim2(0.5f, 0, 0, y);
            MinSize = new UDim2(0, 300, 0, 0);
            MaxSize = new UDim2(0, 500, 1f, 0);

            Center();
        }
    }
}
