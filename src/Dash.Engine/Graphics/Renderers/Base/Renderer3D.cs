/* Renderer3D.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public abstract class Renderer3D : Renderer
    {
        public Renderer3D(MasterRenderer master)
            : base(master)
        { }

        public abstract void ClearBatch();
        public abstract void Render(Shader shader, RenderPass pass, bool frontPass);
    }
}
