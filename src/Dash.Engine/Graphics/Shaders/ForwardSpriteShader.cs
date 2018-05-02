/* ForwardSpriteShader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class ForwardSpriteShader : Shader
    {
        public ForwardSpriteShader()
            : base("forwardSprite.vert", "forwardSprite.frag")
        { }

        protected override void ConnectTextureUnits()
        {
            LoadInt("spriteTex", 0);
        }

        protected override void BindAttributes()
        {
            // Connect attributes
            BindAttribute(0, "position");
            BindAttribute(1, "uv");
            BindAttribute(2, "color");
        }
    }
}
