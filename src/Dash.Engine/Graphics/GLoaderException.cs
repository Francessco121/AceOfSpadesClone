using System;

/* GLoaderException.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class GLoaderException : Exception
    {
        public GLoaderException(string message) 
            : base(message)
        { }
    }
}
