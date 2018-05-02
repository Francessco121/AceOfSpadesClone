using Dash.Engine;
using Dash.Engine.Graphics;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace AceOfSpades.IO
{
    public class WorldFileIO_V3 : IWorldFileIO
    {
        public WorldDescription Load(Stream stream)
        {
            FixedTerrain terrain = new FixedTerrain(MasterRenderer.Instance);
            IEnumerable<WorldObjectDescription> objects;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                objects = ReadWorldObjects(reader);

                ushort numChunks = reader.ReadUInt16();
                Chunk currentChunk = null;
                int blockI = 0;
                int ci = 0;

                while (ci <= numChunks && reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte type = reader.ReadByte();

                    if (type == 0) // New Chunk
                    {
                        int ix = reader.ReadInt16();
                        int iy = reader.ReadInt16();
                        int iz = reader.ReadInt16();

                        IndexPosition ipos = new IndexPosition(ix, iy, iz);
                        currentChunk = new Chunk(terrain, ipos, AceOfSpades.Terrain.ChunkToWorldCoords(ipos));
                        currentChunk.InitBlocks(Chunk.HSIZE, Chunk.VSIZE, Chunk.HSIZE);
                        currentChunk.State = ChunkState.Unbuilt;
                        currentChunk.IsDirty = true;
                        terrain.Chunks.TryAdd(ipos, currentChunk);

                        blockI = 0;
                        ci++;
                    }
                    else if (type == 1) // Block section
                    {
                        ushort numBlocks = reader.ReadUInt16();
                        byte d = reader.ReadByte();
                        Nybble2 n = new Nybble2(d);
                        byte mat = n.Lower;
                        byte r = 255, g = 255, b = 255;
                        if (mat != Block.AIR.Material)
                        {
                            r = reader.ReadByte();
                            g = reader.ReadByte();
                            b = reader.ReadByte();
                        }

                        Block block = new Block(n, r, g, b);

                        for (int i = 0; i < numBlocks; i++)
                        {
                            int z = blockI % Chunk.HSIZE;
                            int y = (blockI / Chunk.HSIZE) % Chunk.VSIZE;
                            int x = blockI / (Chunk.VSIZE * Chunk.HSIZE);

                            currentChunk.Blocks[z, y, x] = block;
                            blockI++;
                        }
                    }
                }

                if (currentChunk != null)
                    currentChunk.BakeColors();
            }

            return new WorldDescription(terrain, objects);
        }

        public void Save(Stream stream, WorldDescription desc)
        {
            FixedTerrain terrain = desc.Terrain;

            using (GZipStream gz = new GZipStream(stream, CompressionMode.Compress))
            using (BinaryWriter writer = new BinaryWriter(gz))
            {
                writer.Write(3.0f);

                int totalObjects = 0;
                foreach (var objectGroup in desc.Objects)
                    foreach (WorldObjectDescription ob in objectGroup)
                        totalObjects++;

                writer.Write(totalObjects);
                foreach (var objectGroup in desc.Objects)
                    foreach (WorldObjectDescription ob in objectGroup)
                        ob.Serialize(writer);

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
                                bool blocksDiff = lastBlock.HasValue ? BlocksDifferent(block, lastBlock.Value) : false;

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
            }
        }

        static IEnumerable<WorldObjectDescription> ReadWorldObjects(BinaryReader reader)
        {
            int numObjects = reader.ReadInt32();
            List<WorldObjectDescription> objects = new List<WorldObjectDescription>();
            for (int i = 0; i < numObjects; i++)
            {
                WorldObjectDescription ob = new WorldObjectDescription();
                ob.Deserialize(reader);

                objects.Add(ob);
            }

            return objects;
        }

        static void WriteBlocks(BinaryWriter writer, Block block, int num)
        {
            writer.Write((byte)1);
            writer.Write((ushort)num);
            writer.Write(block.Data.Value);
            if (block != Block.AIR)
            {
                writer.Write(block.R);
                writer.Write(block.G);
                writer.Write(block.B);
            }
        }

        static bool BlocksDifferent(Block a, Block b)
        {
            return a.R != b.R
                || a.G != b.G
                || a.B != b.B
                || a.Data.Value != b.Data.Value;
        }
    }
}
