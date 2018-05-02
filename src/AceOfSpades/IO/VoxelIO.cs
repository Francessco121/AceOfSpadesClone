using AceOfSpades.Graphics;
using Dash.Engine.Graphics;
using System;
using System.IO;

/* VoxelIO.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.IO
{
    public static class VoxelIO
    {
        public static bool Load(string path, out VoxelObject vo)
        {
            path = Path.Combine(GLoader.RootDirectory, path);

            // Check if file exists
            if (!File.Exists(path))
            {
                vo = null;
                return false;
            }
            
            // Load file
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    VoxelObjectFileHeader header = new VoxelObjectFileHeader(reader);
                    IVoxelObjectFileIO io = GetIOFromHeader(header);

                    if (io == null)
                        throw new VoxelIOException("Failed to get IO for VoxelObject File! Header: " + header.ToString());
                    else
                        return io.Load(reader, out vo);
                }
            }
            catch (FileNotFoundException)
            {
                vo = null;
                return false;
            }
        }

        public static bool Save(string path, VoxelObject vo)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create, FileAccess.Write)))
                {
                    IVoxelObjectFileIO io = new VoxelObjectFileIOV1();
                    return io.Save(vo, writer);
                }
            }
            catch (Exception)
            {
                vo = null;
                return false;
            }
        }

        static IVoxelObjectFileIO GetIOFromHeader(VoxelObjectFileHeader header)
        {
            if (header.Version == 1.0f)
                return new VoxelObjectFileIOV1();
            else
                return null;
        }
    }
}
