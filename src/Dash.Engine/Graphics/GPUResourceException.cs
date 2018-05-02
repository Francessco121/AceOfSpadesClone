/* GPUResourceException.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    class GPUResourceException : GLoaderException
    {
        public GPUResourceException(string message) 
            : base(message)
        { }
    }
}
