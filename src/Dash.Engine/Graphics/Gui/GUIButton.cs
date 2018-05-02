using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

/* GUIButton.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    public delegate void GUIButtonClick(GUIButton btn, MouseButton button);

    public class GUIButton : GUIElement
    {
        public event GUIButtonClick OnMouseClick;

        public Image NormalImage;
        public Image HoverImage;
        public Image ActiveImage;
        public Image ToggledImage;

        public GUILabel Label;

        public bool Toggled;

        protected bool MousePressedDown { get; private set; }

        public GUIButton(UDim2 position, UDim2 size, GUITheme theme)
            : this(position, size, "", theme)
        { }

        public GUIButton(UDim2 position, UDim2 size, string text, GUITheme theme)
            : this(position, size, text, TextAlign.Center, theme)
        { }

        public GUIButton(UDim2 position, UDim2 size, string text, TextAlign textAlign, GUITheme theme)
            : this(position, size, text, textAlign, theme,
                  theme.GetField<Image>(Image.Blank, "Button.NormalImage"),
                  theme.GetField<Image>(Image.Blank, "Button.HoverImage"),
                  theme.GetField<Image>(Image.Blank, "Button.ActiveImage"),
                  theme.GetField<Image>(Image.Blank, "Button.ToggledImage"))
        { }

        public GUIButton(UDim2 position, UDim2 size, string text, TextAlign textAlign, GUITheme theme,
            Image normalImg, Image hoverImg, Image activeImg, Image toggledImage)
            : base(theme)
        {
            Position = position;
            Size = size;

            CapturesMouseClicks = true;
            Label = new GUILabel(UDim2.Zero, new UDim2(1f, 0, 1f, 0), text, textAlign,
                theme.GetField<Color>(Color.White, "Button.TextColor", "Label.TextColor", "TextColor"),
                theme)
            { Parent = this };

            NormalImage = normalImg;
            HoverImage = hoverImg;
            ActiveImage = activeImg;
            ToggledImage = toggledImage;
        }

        public GUIButton(UDim2 position, UDim2 size, string text, TextAlign textAlign,
            BMPFont font, Color textColor,
            Image normalImg, Image hoverImg, Image activeImg, Image toggledImage)
        {
            Position = position;
            Size = size;

            CapturesMouseClicks = true;
            Label = new GUILabel(UDim2.Zero, new UDim2(1f, 0, 1f, 0), text, textAlign, font, textColor)
            { Parent = this };

            NormalImage = normalImg;
            HoverImage = hoverImg;
            ActiveImage = activeImg;
            ToggledImage = toggledImage;
        }

        public override void MouseButtonDown(MouseButton mbtn)
        {
            MousePressedDown = true;
            base.MouseButtonDown(mbtn);
        }

        public override void MouseButtonUp(MouseButton mbtn)
        {
            MousePressedDown = false;

            if (IsMouseOver)
                MouseClick(mbtn);

            base.MouseButtonUp(mbtn);
        }

        protected virtual void MouseClick(MouseButton mbtn)
        {
            if (OnMouseClick != null)
                OnMouseClick(this, mbtn);
        }

        public override void Draw(SpriteBatch sb)
        {
            Image image = NormalImage;
            if (Toggled)
                image = ToggledImage;
            else if (MousePressedDown && IsMouseOver)
                image = ActiveImage;
            else if (IsMouseOver)
                image = HoverImage;

            if (image != null)
                image.Draw(sb, CalculatedRectangle);
        }
    }
}
