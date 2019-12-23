using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Graphics
{
    public class IconRenderer : Component
    {
        public Image Image;
        public Vector3 Offset;
        public Vector2 Size;

        MasterRenderer renderer;
        SpriteBatch sb;

        public IconRenderer()
        {
            renderer = MasterRenderer.Instance;
            sb = renderer.Sprites.SpriteBatch;

            IsDrawable = true;
        }

        Vector2? GetScreenPosition(Vector3 world)
        {
            Camera camera = Camera.Active;
            Vector2 screenPos = camera.Project(world);

            if (Vector3.Dot(world - camera.Position, camera.LookVector) >= 0)
                return screenPos;
            else
                return null;
        }

        void Show3DIcon(Vector3 worldPos)
        {
            Vector2? tryScreenPos = GetScreenPosition(worldPos);

            if (tryScreenPos.HasValue)
            {
                Vector2 screenPos = tryScreenPos.Value;

                Vector2 halfSize = Size / 2f;

                screenPos.X = MathHelper.Clamp(screenPos.X - halfSize.X, halfSize.X, renderer.ScreenWidth - Size.X);
                screenPos.Y = MathHelper.Clamp(screenPos.Y - halfSize.Y, halfSize.Y, renderer.ScreenHeight - Size.Y);

                Image.Draw(sb, new Rectangle(screenPos, Size));
            }
        }

        protected override void Draw()
        {
            if (!renderer.Gui.Hide)
            {
                if (Image != null)
                {
                    Vector3 worldPosition = Transform.Position + Offset;
                    Show3DIcon(worldPosition);
                }
            }

            base.Draw();
        }
    }
}
