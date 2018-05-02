using System;
using System.IO;
using System.IO.Compression;

/* HandshakeTerrainData.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class HandshakeTerrainData
    {
        public byte[][] Sections { get; }
        public int TotalPacketSize { get; }
        public int UncompressedSize { get; }

        public HandshakeTerrainData(Terrain terrain, int maxSectionSize)
        {
            byte[] finalData;
            byte[] uncompressed;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((ushort)terrain.Chunks.Count);
                foreach (Chunk chunk in terrain.Chunks.Values)
                {
                    writer.Write((byte)0);
                    writer.Write((short)chunk.IndexPosition.X);
                    writer.Write((short)chunk.IndexPosition.Y);
                    writer.Write((short)chunk.IndexPosition.Z);

                    Block? lastBlock = null;
                    int numRead = 0;

                    for (int x = 0; x < chunk.Width; x++)
                        for (int y = 0; y < chunk.Height; y++)
                            for (int z = 0; z < chunk.Depth; z++)
                            {
                                Block block = chunk.Blocks[z, y, x];
                                bool blocksDiff = lastBlock.HasValue ? BlocksDifferent(block, lastBlock.Value) : true;

                                if (blocksDiff)
                                {
                                    Block b = lastBlock ?? block;
                                    WriteBlocks(writer, b, numRead);
                                    numRead = 0;
                                }

                                numRead++;
                                lastBlock = block;
                            }

                    if (numRead > 0)
                        WriteBlocks(writer, lastBlock.Value, numRead);
                }

                uncompressed = ms.ToArray();
            }

            using (MemoryStream finalStream = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(finalStream, CompressionMode.Compress))
                using (MemoryStream ms = new MemoryStream(uncompressed))
                    ms.CopyTo(zip);

                finalData = finalStream.ToArray();
            }

            int numSections = (int)Math.Ceiling(finalData.Length / (float)maxSectionSize);
            Sections = new byte[numSections][];

            UncompressedSize = uncompressed.Length;
            TotalPacketSize = finalData.Length;
            int bytesLeft = finalData.Length;
            int pos = 0;
            for (int i = 0; i < numSections; i++)
            {
                int sectionSize = Math.Min(bytesLeft, maxSectionSize);
                Sections[i] = new byte[sectionSize];
                Buffer.BlockCopy(finalData, pos, Sections[i], 0, sectionSize);
                bytesLeft -= sectionSize;
                pos += sectionSize;
            }
        }

        void WriteBlocks(BinaryWriter writer, Block block, int num)
        {
            writer.Write((byte)1);
            writer.Write((ushort)num);
            writer.Write(block.Data.Value);

            if (block.Material == Block.CUSTOM.Material)
            {
                writer.Write(block.R);
                writer.Write(block.G);
                writer.Write(block.B);
            }
        }

        string BlockToString(Block block)
        {
            return string.Format("RGBD: {0},{1},{2},{3}", block.R, block.G, block.B, block.Data.Value);
        }

        bool BlocksDifferent(Block a, Block b)
        {
            if (a.Material == Block.CUSTOM.Material || b.Material == Block.CUSTOM.Material)
                return a.R != b.R
                    || a.G != b.G
                    || a.B != b.B
                    || a.Data.Value != b.Data.Value;
            else
                return a.Data.Value != b.Data.Value;
        }
    }
}
