/* SkyboxShader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class SkyboxShader : Shader
    {
        public SkyboxShader()
            : base("sky.vert", "sky.frag")
        { }

        protected override void ConnectTextureUnits()
        {
            LoadInt("skyMap", 0);
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "position");
        }
    }
}
