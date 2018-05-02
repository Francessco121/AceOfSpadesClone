using System.IO;
using System.IO.Compression;

/* NetCompressor.cs
 * Author: Ethan Lafrenais
 * Last Update: 5/15/15
*/

namespace Dash.Net
{
    class NetCompressor
    {
        public static void Compress(NetOutboundPacket buffer)
        {
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                // Just incase this wasn't forced, make sure it knows it's compressed
                buffer.isCompressed = true;
                byte[] originalData = buffer.Data;
                int originalPosition = buffer.position;
                int originalIndex = buffer.unpaddedIndex;

                // Save original size
                int originalSize = buffer.data.Length;

                // Write data to the zip buffer and compress
                zip.Write(buffer.data, 0, buffer.data.Length);
                zip.Close();
                ms.Position = 0;

                // Overwrite the packets data with the compressed data
                buffer.data = new byte[ms.Length + 4];
                buffer.position = 0;
                buffer.Write(originalSize); // Prepend the original size of the data
                ms.Read(buffer.data, 4, buffer.data.Length - 4); // Write the compressed data

                // If the compressed version is bigger then,
                // just revert it back to it's original state
                if (buffer.data.Length > originalData.Length)
                {
                    buffer.data = originalData;
                    buffer.position = originalPosition;
                    buffer.unpaddedIndex = originalIndex;
                    buffer.isCompressed = false;
                }
                else
                    buffer.unpaddedIndex = buffer.data.Length;
            }
        }

        public static void Decompress(NetInboundPacketBase buffer)
        {
            // Pack buffer into a stream
            using (MemoryStream ms = new MemoryStream())
            {
                int headerSize = buffer.HasHeader ? 4 + NetOutboundPacket.PacketHeaderSize : 4;

                // Get the original size
                int msgLength = buffer.ReadInt32();
                ms.Write(buffer.data, headerSize, buffer.data.Length - headerSize);

                // Clear the packet
                buffer.data = new byte[msgLength];
                buffer.position = 0;
                ms.Position = 0;

                // Decompress and re-write to the packet
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                    zip.Read(buffer.data, 0, msgLength);

                buffer.isCompressed = false;
                buffer.HasHeader = false;
            }
        }
    }
}