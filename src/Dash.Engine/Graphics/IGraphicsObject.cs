using System;

/* IGraphicsObject.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public interface IGraphicsObject : IDisposable
    {
        void Bind();
        void Unbind();
    }
}
