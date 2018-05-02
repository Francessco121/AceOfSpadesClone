/* Renderer2D.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public abstract class Renderer2D : Renderer
    {
        public Renderer2D(MasterRenderer master) 
            : base(master)
        { }

        public abstract void Resize(int width, int height);
        public abstract void Render();
    }
}
