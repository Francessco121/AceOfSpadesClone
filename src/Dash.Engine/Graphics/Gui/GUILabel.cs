using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;

/* GUILabel.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    public class GUILabel : GUIElement
    {
        public string Text;
        public string TextExtension;
        public TextAlign TextAlign;
        public BMPFont Font;

        public Color TextColor;
        public Color? TextShadowColor;
        public Image BackgroundImage;
        public Vector4 TextPadding = new Vector4(2, 2, 2, 2);

        public GUILabel() { }

        public GUILabel(UDim2 position, UDim2 size, string text, GUITheme theme)
            : this(position, size, text, TextAlign.Center, theme)
        { }

        public GUILabel(UDim2 position, UDim2 size, string text, Color textColor, GUITheme theme)
            : this(position, size, text, TextAlign.Center,
                  theme.GetField<BMPFont>(null, "Label.Font", "Font"),
                  textColor,
                  theme.GetField<Color?>(null, "Label.TextShadowColor"))
        {
            Theme = theme;
        }

        public GUILabel(UDim2 position, UDim2 size, string text, TextAlign textAlign, GUITheme theme)
            : this(position, size, text, textAlign,
                  theme.GetField<BMPFont>(null, "Label.Font", "Font"),
                  theme.GetField<Color>(Color.Black, "Label.TextColor", "TextColor"),
                  theme.GetField<Color?>(null, "Label.TextShadowColor"))
        {
            Theme = theme;
        }

        public GUILabel(UDim2 position, UDim2 size, string text, TextAlign textAlign, Color textColor, GUITheme theme)
            : this(position, size, text, textAlign,
                  theme.GetField<BMPFont>(null, "Label.Font", "Font"),
                  textColor,
                  theme.GetField<Color?>(null, "Label.TextShadowColor"))
        {
            Theme = theme;
        }

        public GUILabel(UDim2 position, UDim2 size, string text, TextAlign textAlign,
            BMPFont font, Color textColor, Color? shadowColor = null)
        {
            if (text == null)
                throw new ArgumentNullException("text", "GUILabel expected string text, got null");
            if (font == null)
                throw new ArgumentNullException("font", "GUILabel requires a non-null font!");

            Position = position;
            Size = size;
            Text = text;
            TextAlign = textAlign;
            Font = font;
            TextColor = textColor;
            TextShadowColor = shadowColor;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (BackgroundImage != null)
                BackgroundImage.Draw(sb, CalculatedRectangle);

            string text = Text + TextExtension;
            if (!string.IsNullOrEmpty(text))
            {
                Vector2 textPos = (CalculatedRectangle.IsEmpty 
                    ? CalculatedRectangle.Location 
                    : GetTextPosition()) + GetTextOffset();
                Font.DrawString(text, textPos.X, textPos.Y, sb, TextAlign, TextColor, TextShadowColor);
            }
        }

        Vector2 GetTextPosition()
        {
            switch (TextAlign)
            {
                case TextAlign.TopCenter:
                    return new Vector2(CalculatedRectangle.X + CalculatedRectangle.Width / 2f, CalculatedRectangle.Y);
                case TextAlign.TopRight:
                    return CalculatedRectangle.TopRight;
                case TextAlign.Left:
                    return new Vector2(CalculatedRectangle.X, CalculatedRectangle.Y + CalculatedRectangle.Height / 2f);
                case TextAlign.Center:
                    return CalculatedRectangle.AbsoluteCenter;
                case TextAlign.Right:
                    return new Vector2(CalculatedRectangle.Right, CalculatedRectangle.Y + CalculatedRectangle.Height / 2f);
                case TextAlign.BottomLeft:
                    return CalculatedRectangle.BottomLeft;
                case TextAlign.BottomCenter:
                    return new Vector2(CalculatedRectangle.X + CalculatedRectangle.Width / 2f, CalculatedRectangle.Bottom);
                case TextAlign.BottomRight:
                    return CalculatedRectangle.BottomRight;
                default:
                    return CalculatedRectangle.Location;
            }
        }

        Vector2 GetTextOffset()
        {
            switch (TextAlign)
            {
                case TextAlign.TopCenter:
                    return new Vector2(0, TextPadding.Y);
                case TextAlign.TopRight:
                    return new Vector2(-TextPadding.Z, TextPadding.Y);
                case TextAlign.Left:
                    return new Vector2(TextPadding.X, 0);
                case TextAlign.Right:
                    return new Vector2(-TextPadding.Z, 0);
                case TextAlign.BottomLeft:
                    return new Vector2(TextPadding.X, -TextPadding.W);
                case TextAlign.BottomCenter:
                    return new Vector2(0, -TextPadding.W);
                case TextAlign.BottomRight:
                    return new Vector2(-TextPadding.Z, -TextPadding.W);
                default:
                    return Vector2.Zero;
            }
        }
    }
}
