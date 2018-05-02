/* BorderedImage.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    /// <summary>
    /// An image that is drawn in 9 parts, the area
    /// of the specified texture to be drawn has the corners
    /// drawn seperatly at a fixed size (this prevents
    /// bordered images from having stretched borders).
    /// </summary>
    public class BorderedImage : Image
    {
        public Vector2 Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                thirdX = baseThirdX * value.X;
                thirdY = baseThirdY * value.Y;
            }
        }

        Rectangle tl, tr, bl, br, m, t, l, b, r;
        float thirdX, thirdY;
        float baseThirdX, baseThirdY;
        Vector2 scale;

        public BorderedImage(Texture texture)
            : base(texture)
        {
            GenerateClips(new Rectangle(0, 0, texture.Width, texture.Height));
        }

        public BorderedImage(Texture texture, Color color)
            : base(texture, color)
        {
            GenerateClips(new Rectangle(0, 0, texture.Width, texture.Height));
        }

        public BorderedImage(Texture spriteSheet, Rectangle clippingRect)
            : base(spriteSheet)
        {
            GenerateClips(clippingRect);
        }

        void GenerateClips(Rectangle borderSquare)
        {
            baseThirdX = borderSquare.Width / 3f;
            baseThirdY = borderSquare.Height / 3f;
            Scale = new Vector2(1, 1);

            tl = new Rectangle(borderSquare.X, borderSquare.Y, thirdX, thirdY);
            t = new Rectangle(borderSquare.X + thirdX, borderSquare.Y, thirdX, thirdY);
            tr = new Rectangle(borderSquare.X + thirdX * 2, borderSquare.Y, thirdX, thirdY);
            l = new Rectangle(borderSquare.X, borderSquare.Y + thirdY, thirdX, thirdY);
            m = new Rectangle(borderSquare.X + thirdX, borderSquare.Y + thirdY, thirdX, thirdY);
            r = new Rectangle(borderSquare.X + thirdX * 2, borderSquare.Y + thirdY, thirdX, thirdY);
            bl = new Rectangle(borderSquare.X, borderSquare.Y + thirdY * 2, thirdX, thirdY);
            b = new Rectangle(borderSquare.X + thirdX, borderSquare.Y + thirdY * 2, thirdX, thirdY);
            br = new Rectangle(borderSquare.X + thirdX * 2, borderSquare.Y + thirdY * 2, thirdX, thirdY);
        }

        public override void Draw(SpriteBatch sb, Rectangle rect)
        {
            sb.Draw(Texture, new Rectangle(rect.X + thirdX, rect.Y + thirdY, rect.Width - thirdX, rect.Height - thirdY), m, Color);

            sb.Draw(Texture, new Rectangle(rect.X, rect.Y, thirdX, thirdY), tl, Color);
            sb.Draw(Texture, new Rectangle(rect.Right - thirdX, rect.Y, thirdX, thirdY), tr, Color);
            sb.Draw(Texture, new Rectangle(rect.X, rect.Bottom - thirdY, thirdX, thirdY), bl, Color);
            sb.Draw(Texture, new Rectangle(rect.Right - thirdX, rect.Bottom - thirdY, thirdX, thirdY), br, Color);

            sb.Draw(Texture, new Rectangle(rect.X + thirdX, rect.Y, rect.Width - (thirdX * 2), thirdY), t, Color);
            sb.Draw(Texture, new Rectangle(rect.X + thirdX, rect.Bottom - thirdY, rect.Width - (thirdX * 2), thirdY), b, Color);
            sb.Draw(Texture, new Rectangle(rect.X, rect.Y + thirdY, thirdX, rect.Height - (thirdY * 2)), l, Color);
            sb.Draw(Texture, new Rectangle(rect.Right - thirdX, rect.Y + thirdY, thirdX, rect.Height - (thirdY * 2)), r, Color);
        }
    }
}
