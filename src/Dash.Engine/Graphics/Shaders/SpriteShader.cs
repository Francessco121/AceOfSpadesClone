/* SpriteShader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class SpriteShader : Shader
    {
        public SpriteShader()
            : base("sprite.vert", "sprite.frag")
        { }

        protected override void BindAttributes()
        {
            BindAttribute(0, "position");
            BindAttribute(1, "uv");
            BindAttribute(2, "color");
        }

        protected override void ConnectTextureUnits()
        {
            LoadInt("spriteTex", 0);
        }
    }
}
