/* PostProcessShader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class PostProcessShader : Shader
    {
        public PostProcessShader()
            : base("postProcess.vert", "postProcess.frag", null, null)
        { }

        protected override void ConnectTextureUnits()
        {
            LoadInt("colorSampler", 0);
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "position");
        }
    }
}
