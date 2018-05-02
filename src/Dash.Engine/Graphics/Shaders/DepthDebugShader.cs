/* DepthDebugShader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class DepthDebugShader : Shader
    {
        public DepthDebugShader()
            : base("fullscreenQuad.vert", "depthDebug.frag")
        { }

        protected override void BindAttributes()
        {
            BindAttribute(0, "position");
        }

        protected override void ConnectTextureUnits()
        {
            LoadInt("depthMap", 0);
        }
    }
}
