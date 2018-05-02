using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace AceOfSpades.IO
{
    public static class WorldIO
    {
        static Dictionary<float, IWorldFileIO> ioHandlers;

        static WorldIO()
        {
            ioHandlers = new Dictionary<float, IWorldFileIO>();

            SetIOHandler(1.0f, new WorldFileIO_V1());
            SetIOHandler(2.0f, new WorldFileIO_V2());
            SetIOHandler(3.0f, new WorldFileIO_V3());
        }

        public static void SetIOHandler(float version, IWorldFileIO handler)
        {
            if (ioHandlers.ContainsKey(version))
                ioHandlers[version] = handler;
            else
                ioHandlers.Add(version, handler);
        }

        public static void RemoveIOHandler(float version)
        {
            ioHandlers.Remove(version);
        }

        public static void Save(string fileName, WorldDescription description,
             bool useRelativePath = true, float version = 3.0f)
        {
            IWorldFileIO handler;
            if (ioHandlers.TryGetValue(version, out handler))
            {
                using (FileStream fs = File.Create(useRelativePath ? "Content/Worlds/" + fileName + ".aosw" : fileName))
                {
                    handler.Save(fs, description);
                }
            }
            else
                throw new IOException("No world IO handler defined for version " + version + "!");
        }

        public static WorldDescription Load(string fileName, bool useRelativePath = true)
        {
            string filePath = useRelativePath ? "Content/Worlds/" + fileName + ".aosw" : fileName;

            // Open filestream
            Stream fileStream;
            if (!TryDecompress(filePath, out fileStream))
                fileStream = File.OpenRead(filePath);
            
            // Find the file version
            float version;
            using (BinaryReader reader = new BinaryReader(fileStream, Encoding.Default, true))
                version = reader.ReadSingle();

            // Load file
            WorldDescription desc;
            IWorldFileIO handler;
            if (ioHandlers.TryGetValue(version, out handler))
                desc = handler.Load(fileStream);
            else
                throw new IOException("No world IO handler defined for version " + version + "!");

            desc.Terrain.CreatedFromFile();
            return desc;
        }

        static bool TryDecompress(string filePath, out Stream stream)
        {
            try
            {
                stream = new MemoryStream();

                using (FileStream fs = File.OpenRead(filePath))
                using (GZipStream zip = new GZipStream(fs, CompressionMode.Decompress))
                {
                    zip.CopyTo(stream);
                }

                stream.Seek(0, SeekOrigin.Begin);
                return true;
            }
            catch (Exception)
            {
                stream = null;
                return false;
            }
        }
    }
}
