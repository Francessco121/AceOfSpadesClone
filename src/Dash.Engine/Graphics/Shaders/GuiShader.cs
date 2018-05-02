/* GuiShader.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class GuiShader : Shader
    {
        public GuiShader()
            : base("gui.vert", "gui.frag")
        { }

        public void LoadTransformationMatrix(Matrix4 mat4)
        {
            LoadMatrix4("transformationMatrix", mat4);
        }

        protected override void ConnectTextureUnits()
        {
            LoadInt("guiTexture", 0);
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "position");
        }
    }
}
