using System;

/* Renderer.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public abstract class Renderer : IDisposable
    {
        protected MasterRenderer Master { get; private set; }

        public Renderer(MasterRenderer master)
        {
            Master = master;
        }

        public virtual void Prepare() { }
        public virtual void Update(float deltaTime) { }

        public abstract void Dispose();
    }
}
