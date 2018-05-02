using AceOfSpades.Graphics;
using System.IO;

/* IVoxelObjectLoader.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.IO
{
    public interface IVoxelObjectFileIO
    {
        bool Save(VoxelObject vo, BinaryWriter writer);
        bool Load(BinaryReader reader, out VoxelObject vo);
    }
}
