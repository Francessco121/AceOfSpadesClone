/* GuiTexture.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class GuiTexture : Texture
    {
        public Vector2 Position;
        public Vector2 Scale;

        public GuiTexture(Vector2 position, Vector2 scale)
        {
            this.Position = position;
            this.Scale = scale;
        }
    }
}
