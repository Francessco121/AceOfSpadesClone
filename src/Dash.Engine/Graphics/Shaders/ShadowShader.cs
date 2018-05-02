/* ShadowShader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class ShadowShader : Shader
    {
        public ShadowShader()
            : base("shadow.vert", "shadow.frag", null, null)
        { }

        protected override void BindAttributes()
        {
            BindAttribute(0, "position");
        }

        protected override void ConnectTextureUnits() { }
    }
}
