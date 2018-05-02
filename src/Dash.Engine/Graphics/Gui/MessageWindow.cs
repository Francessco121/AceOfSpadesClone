using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dash.Engine.Graphics.Gui
{
    public class MessageWindow : GUIWindow
    {
        GUILabel msgLabel;

        public MessageWindow(GUISystem system, GUITheme theme, UDim2 size, string title) 
            : base(system, size, title, theme)
        {
            msgLabel = new GUILabel(UDim2.Zero, new UDim2(1f, 0, 1f, -30), "", TextAlign.Center, theme);
            GUIButton okBtn = new GUIButton(new UDim2(0.5f, -75, 1f, -30), new UDim2(0, 150, 0, 30), "Okay", theme);

            okBtn.OnMouseClick += (btn, mbtn) => { Visible = false; };
            AddTopLevel(msgLabel, okBtn);
        }

        public void Show(string message)
        {
            msgLabel.Text = message;
            Size.X.Offset = msgLabel.Font.MeasureString(message).X;
            Center();
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        public override void Update(float deltaTime)
        {
            if (Visible && Input.GetKeyDown(Key.Enter))
                Hide();

            base.Update(deltaTime);
        }
    }
}
