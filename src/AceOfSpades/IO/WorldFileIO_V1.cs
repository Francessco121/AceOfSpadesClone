using Dash.Engine;
using Dash.Engine.Graphics;
using System.IO;

namespace AceOfSpades.IO
{
    public class WorldFileIO_V1 : IWorldFileIO
    {
        public WorldDescription Load(Stream stream)
        {
            FixedTerrain terrain = new FixedTerrain(MasterRenderer.Instance);

            using (BinaryReader reader = new BinaryReader(stream))
            {
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
                        currentChunk.State = ChunkState.Unlit;
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

            return new WorldDescription(terrain);
        }

        public void Save(Stream stream, WorldDescription desc)
        {
            FixedTerrain terrain = desc.Terrain;

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(1.0f);
                writer.Write(terrain.Chunks.Count);

                foreach (Chunk chunk in terrain.Chunks.Values)
                {
                    writer.Write((ushort)chunk.IndexPosition.X);
                    writer.Write((ushort)chunk.IndexPosition.Y);
                    writer.Write((ushort)chunk.IndexPosition.Z);
                    writer.Write(false);
                    //if (!chunk.IsEmpty)
                    {
                        ushort skip = 0;

                        for (int x = 0; x < Chunk.HSIZE; x++)
                            for (int y = 0; y < Chunk.VSIZE; y++)
                                for (int z = 0; z < Chunk.HSIZE; z++)
                                {
                                    Block block = chunk.Blocks[z, y, x];
                                    //if (block != Block.AIR)
                                    {
                                        if (skip > 0)
                                        {
                                            writer.Write(true);
                                            writer.Write(skip);
                                            skip = 0;
                                        }

                                        writer.Write(block == Block.AIR);
                                        writer.Write(block.R);
                                        writer.Write(block.G);
                                        writer.Write(block.B);
                                        writer.Write(block.Data.Value);
                                    }
                                    //else
                                    //    skip++;
                                }

                        if (skip > 0)
                        {
                            writer.Write(true);
                            writer.Write(skip);
                            skip = 0;
                        }
                    }
                }
            }
        }
    }
}
