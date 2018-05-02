namespace Dash.Engine.Graphics.Gui
{
    public class Sprite
    {
        public Texture Texture { get; set; }
        public Rectangle AtlasRegion;
        public bool UseBackAssMatrix;

        public Rectangle Rect;
        public Color4 OverlayColor;
        public float Rotation;
        public Vector2 Origin;
        public UDim2 TextureDrawSize = new UDim2(1, 0, 1, 0);
        public bool MeshBatch;

        public Sprite(Texture tex, Rectangle rect, Color4 color)
        {
            Texture = tex;
            Rect = rect;
            OverlayColor = color;
            Origin = rect.RelativeCenter;
        }
    }
}
