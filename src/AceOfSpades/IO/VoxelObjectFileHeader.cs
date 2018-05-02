using System.IO;

/* VoxelObjectFileHeader.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.IO
{
    public class VoxelObjectFileHeader
    {
        public string Title { get; private set; }
        public float Version { get; private set; }

        public VoxelObjectFileHeader(BinaryReader reader)
        {
            Title = reader.ReadString();
            Version = reader.ReadSingle();
        }

        public override string ToString()
        {
            return string.Format("Title: {0}, Version: {1}", Title, Version);
        }
    }
}
