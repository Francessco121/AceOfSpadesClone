using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;

namespace Dash.Engine.Graphics.Gui
{
    public class GUICheckbox : GUIElement
    {
        public event EventHandler<bool> OnCheckChanged;

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                Label.Visible = value;
            }
        }
        bool isChecked;

        public Image NormalImage;
        public Image HoverImage;
        public Image ActiveImage;

        public GUILabel Label;

        bool pressedDown;

        public GUICheckbox(UDim2 position, float size, GUITheme theme)
            : this(position, new UDim2(0, size, 0, size), "X", theme)
        { }

        public GUICheckbox(UDim2 position, UDim2 size, GUITheme theme)
            : this(position, size, "X", theme)
        { }

        public GUICheckbox(UDim2 position, UDim2 size, string checkedText, GUITheme theme)
            : this(position, size, checkedText, theme,
                  theme.GetField<Image>(Image.Blank, "Checkbox.NormalImage"),
                  theme.GetField<Image>(Image.Blank, "Checkbox.HoverImage"),
                  theme.GetField<Image>(Image.Blank, "Checkbox.ActiveImage"))
        { }

        public GUICheckbox(UDim2 position, UDim2 size, GUITheme theme,
            Image normalImg, Image hoverImg, Image activeImg)
            : this(position, size, "X", theme, normalImg, hoverImg, activeImg)
        { }

        public GUICheckbox(UDim2 position, UDim2 size, string checkedText, GUITheme theme,
            Image normalImg, Image hoverImg, Image activeImg)
            : base(theme)
        {
            Position = position;
            Size = size;

            CapturesMouseClicks = true;
            Label = new GUILabel(UDim2.Zero, new UDim2(1f, 0, 1f, 0), checkedText,
                theme.GetField<Color>(Color.White, "Checkbox.TextColor", "Label.TextColor", "TextColor"),
                theme)
            { Parent = this, Visible = false };

            NormalImage = normalImg;
            HoverImage = hoverImg;
            ActiveImage = activeImg;
        }

        public GUICheckbox(UDim2 position, UDim2 size, string checkedText,
            BMPFont font, Color textColor,
            Image normalImg, Image hoverImg, Image activeImg)
        {
            Position = position;
            Size = size;

            CapturesMouseClicks = true;
            Label = new GUILabel(UDim2.Zero, new UDim2(1f, 0, 1f, 0), checkedText, TextAlign.Center, font, textColor)
            { Parent = this, Visible = false };

            NormalImage = normalImg;
            HoverImage = hoverImg;
            ActiveImage = activeImg;
        }

        public override void MouseButtonDown(MouseButton mbtn)
        {
            pressedDown = true;
            base.MouseButtonDown(mbtn);
        }

        public override void MouseButtonUp(MouseButton mbtn)
        {
            pressedDown = false;

            if (IsMouseOver)
                MouseClick(mbtn);

            base.MouseButtonUp(mbtn);
        }

        protected virtual void MouseClick(MouseButton mbtn)
        {
            IsChecked = !IsChecked;

            if (OnCheckChanged != null)
                OnCheckChanged(this, IsChecked);
        }

        public override void Draw(SpriteBatch sb)
        {
            Image image = NormalImage;
            if (pressedDown && IsMouseOver)
                image = ActiveImage;
            else if (IsMouseOver)
                image = HoverImage;

            image.Draw(sb, CalculatedRectangle);
        }
    }
}
