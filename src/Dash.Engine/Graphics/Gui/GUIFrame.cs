using Dash.Engine.Graphics.Gui;
using Dash.Engine.Graphics;

namespace Dash.Engine.Graphics.Gui
{
    public class GUIFrame : GUIElement
    {
        public Image Image { get; set; }
        public bool NeverCaptureMouse;

        public GUIFrame(UDim2 position, UDim2 size)
        {
            Image = Image.Blank;
            Image.Color = Color.Transparent;
            Position = position;
            Size = size;
        }

        public GUIFrame(UDim2 position, UDim2 size, Image image)
        {
            Image = image;
            Position = position;
            Size = size;
        }

        public GUIFrame(UDim2 position, UDim2 size, GUITheme theme) 
            : base(theme)
        {
            Image = theme.GetField<Image>(Image.Blank, "Frame.Image");
            Position = position;
            Size = size;
        }

        public override void Update(float deltaTime)
        {
            CapturesMouseClicks = !NeverCaptureMouse && Image != null && Image.Color.A > 0;
            base.Update(deltaTime);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Image != null && Image.Color.A > 0)
                Image.Draw(sb, CalculatedRectangle);
        }
    }
}
