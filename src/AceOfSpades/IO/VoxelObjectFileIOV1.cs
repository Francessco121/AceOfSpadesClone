using AceOfSpades.Graphics;
using Dash.Engine.Graphics;
using System.Collections.Generic;
using System.IO;

/* VoxelObjectLoaderV1.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.IO
{
    public class VoxelObjectFileIOV1 : IVoxelObjectFileIO
    {
        struct PatternInstruction
        {
            public readonly bool Skip;
            public readonly ushort Count;

            public PatternInstruction(bool skip, ushort count)
            {
                this.Skip = skip;
                this.Count = count;
            }
        }

        class Pattern
        {
            public List<PatternInstruction> Instructions { get; private set; }

            public Pattern(BinaryReader reader)
            {
                Instructions = new List<PatternInstruction>();

                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    PatternInstruction inst = new PatternInstruction(
                        reader.ReadBoolean(),
                        reader.ReadUInt16());

                    Instructions.Add(inst);
                }
            }

            public Pattern(VoxelObject vo)
            {
                Instructions = new List<PatternInstruction>();
                ushort currentCount = 0;
                bool skiping = false;

                for (int x = 0; x < vo.Width; x++)
                    for (int y = 0; y < vo.Height; y++)
                        for (int z = 0; z < vo.Depth; z++)
                        {
                            Block type = vo.Blocks[z, y, x];
                            if (type == Block.AIR && !skiping)
                            {
                                if (currentCount > 0)
                                {
                                    PatternInstruction inst = new PatternInstruction(false, currentCount);
                                    Instructions.Add(inst);
                                }

                                currentCount = 0;
                                skiping = true;
                            }
                            else if (type == Block.STONE && skiping)
                            {
                                if (currentCount > 0)
                                {
                                    PatternInstruction inst = new PatternInstruction(true, currentCount);
                                    Instructions.Add(inst);
                                }

                                currentCount = 0;
                                skiping = false;
                            }

                            currentCount++;
                        }

                if (currentCount > 0)
                {
                    PatternInstruction inst = new PatternInstruction(skiping, currentCount);
                    Instructions.Add(inst);
                }
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(Instructions.Count);
                foreach (PatternInstruction inst in Instructions)
                {
                    writer.Write(inst.Skip);
                    writer.Write(inst.Count);
                }
            }
        }

        class ColorData
        {
            public List<Color> Colors { get; private set; }

            public ColorData(BinaryReader reader)
            {
                Colors = new List<Color>();

                int colors = reader.ReadInt32();
                for (int i = 0; i < colors; i++)
                {
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();
                    byte a = reader.ReadByte();

                    Colors.Add(new Color(r, g, b, a));
                }
            }

            public ColorData(VoxelObject vo)
            {
                Colors = new List<Color>();

                for (int x = 0; x < vo.Width; x++)
                    for (int y = 0; y < vo.Height; y++)
                        for (int z = 0; z < vo.Depth; z++)
                        {
                            if (vo.Blocks[z, y, x] != Block.AIR)
                                Colors.Add(vo.Blocks[z, y, x].GetColor());
                        }
            }

            float ByteToFloat(byte c)
            {
                return c / 255f;
            }

            byte FloatToByte(float c)
            {
                return (byte)(c * 255f);
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(Colors.Count);
                foreach (Color color in Colors)
                {
                    writer.Write(color.R);
                    writer.Write(color.G);
                    writer.Write(color.B);
                    writer.Write(color.A);
                }
            }
        }

        public bool Save(VoxelObject vo, BinaryWriter writer)
        {
            writer.Write("Generic Voxel Object");
            writer.Write(1.0f);

            writer.Write((ushort)vo.Width);
            writer.Write((ushort)vo.Height);
            writer.Write((ushort)vo.Depth);
            writer.Write(vo.CubeSize);

            Pattern pattern = new Pattern(vo);
            pattern.Write(writer);

            ColorData colors = new ColorData(vo);
            colors.Write(writer);

            return true;
        }

        public bool Load(BinaryReader reader, out VoxelObject vo)
        {
            int width = (int)reader.ReadUInt16();
            int height = (int)reader.ReadUInt16();
            int depth = (int)reader.ReadUInt16();
            float cubeSize = reader.ReadSingle();

            Pattern pattern = new Pattern(reader);
            ColorData colors = new ColorData(reader);

            vo = new VoxelObject(cubeSize);
            vo.InitBlocks(width, height, depth);
            
            int patternI = 0, patternCountFollowed = 0, colorI = 0;
            PatternInstruction inst = pattern.Instructions[patternI++];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    for (int z = 0; z < depth; z++)
                    {
                        if (patternCountFollowed == inst.Count)
                        {
                            patternCountFollowed = 0;
                            inst = pattern.Instructions[patternI++];
                        }

                        if (!inst.Skip)
                        {
                            Color color = colors.Colors[colorI++];
                            vo.Blocks[z, y, x] = new Block(Block.STONE.Material, color.R, color.G, color.B);
                        }

                        patternCountFollowed++;
                    }

            return true;
        }
    }
}
