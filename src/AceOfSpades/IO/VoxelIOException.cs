using System;

/* VoxelIOException.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.IO
{
    public class VoxelIOException : Exception
    {
        public VoxelIOException(string message)
            : base(message) { }
    }
}
