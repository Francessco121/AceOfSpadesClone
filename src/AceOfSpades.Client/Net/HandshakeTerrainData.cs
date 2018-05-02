using Dash.Net;
using System.IO;
using System.IO.Compression;

/* HandshakeTerrainData.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class HandshakeTerrainData
    {
        public NetBuffer SourceData { get; }

        public HandshakeTerrainData(byte[][] data, int originalSize)
        {
            SourceData = new NetBuffer(originalSize);
            using (MemoryStream ms = new MemoryStream())
            {
                for (int i = 0; i < data.Length; i++)
                    ms.Write(data[i], 0, data[i].Length);

                ms.Position = 0;

                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                    zip.Read(SourceData.Data, 0, originalSize);
            }
        }
    }
}
