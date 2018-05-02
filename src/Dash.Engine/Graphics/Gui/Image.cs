namespace Dash.Engine.Graphics.Gui
{
    public class Image
    {
        public static Image Blank
        {
            get { return new Image(Texture.Blank); }
        }

        public Texture Texture { get; set; }
        public Rectangle ClippingRectangle { get; set; }
        public Color Color { get; set; }

        public Image(Texture texture)
        {
            Texture = texture;
            ClippingRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Color = Color.White;
        }

        public Image(Texture texture, Color color)
        {
            Texture = texture;
            ClippingRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Color = color;
        }

        public Image(Texture texture, Rectangle clippingRect)
        {
            Texture = texture;
            ClippingRectangle = clippingRect;
            Color = Color.White;
        }

        public Image(Texture texture, Rectangle clippingRect, Color color)
        {
            Texture = texture;
            ClippingRectangle = clippingRect;
            Color = color;
        }

        public virtual void Draw(SpriteBatch sb, Rectangle rect)
        {
            sb.Draw(Texture, rect, ClippingRectangle, Color);
        }

        public static Image CreateBlank(Color color)
        {
            Image img = Blank;
            img.Color = color;
            return img;
        }
    }
}
