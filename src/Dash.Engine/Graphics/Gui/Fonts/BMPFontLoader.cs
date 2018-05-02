using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/* BMPFontLoader.cs
 * Author: Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    /// <summary>
    /// Loads Angel BMP font data.
    /// File Format: http://www.angelcode.com/products/bmfont/doc/file_format.html
    /// </summary>
    abstract class BMPFontLoader : IDisposable
    {
        #region Block Classes
        public class InfoBlock
        {
            public int FontSize;
            public bool IsSmooth;
            public bool IsUnicode;
            public bool IsItalic;
            public bool IsBold;
            public bool IsFixedHeight;
            public uint CharSet;
            public uint StretchH;
            public uint AA;
            public uint PaddingUp;
            public uint PaddingRight;
            public uint PaddingDown;
            public uint PaddingLeft;
            public int SpacingHoriz;
            public int SpacingVert;
            public int Outline;
            public string FontName;
        }
        public class CommonBlock
        {
            public uint LineHeight;
            public uint Base;
            public uint ScaleW;
            public uint ScaleH;
            public uint Pages;
            public bool IsPacked;
            public uint AlphaChannel;
            public uint RedChannel;
            public uint GreenChannel;
            public uint BlueChannel;
        }
        #endregion

        public abstract InfoBlock GetInfoBlock();
        public abstract CommonBlock GetCommonBlock();
        public abstract string[] GetPages();
        public abstract Dictionary<char, BMPFontCharacter> GetCharacters();
        public abstract Dictionary<Tuple<char, char>, int> GetKerningPairs();

        public abstract void Dispose();
    }
    
    class BMPFontBinaryLoader : BMPFontLoader
    {
        class BMPFontReader : IDisposable
        {
            BinaryReader reader;

            public BMPFontReader(BinaryReader reader)
            {
                this.reader = reader;
            }

            public bool GetBit(byte b, int index) { return ((b & (1 << index)) != 0); }
            public int ReadInt16() { return reader.ReadInt16(); }
            public uint ReadUInt32() { return reader.ReadUInt32(); }
            public uint ReadUInt16() { return reader.ReadUInt16(); }
            public byte ReadByte() { return reader.ReadByte(); }

            public string ReadString()
            {
                StringBuilder sb = new StringBuilder();
                while (true)
                {
                    char c = reader.ReadChar();
                    if (c == '\0')
                        break;
                    else
                        sb.Append(c);
                }

                return sb.ToString();
            }

            public void Dispose()
            {
                reader.Dispose();
            }
        }

        Stream stream;
        BMPFontReader r;
        CommonBlock common;

        public BMPFontBinaryLoader(Stream stream)
        {
            this.stream = stream;

            BinaryReader binaryReader = new BinaryReader(stream, Encoding.ASCII);
            r = new BMPFontReader(binaryReader);

            char fid1 = (char)r.ReadByte();
            char fid2 = (char)r.ReadByte();
            char fid3 = (char)r.ReadByte();

            if (fid1 != 'B' || fid2 != 'M' || fid3 != 'F')
                throw new InvalidDataException("Failed to load binary angel font, specified file is not a binary angel bmp file!");

            byte version = r.ReadByte();

            if (version != 3)
                throw new NotSupportedException("Failed to load binary angel font, only version 3 is currently supported!");
        }

        public override InfoBlock GetInfoBlock()
        {
            byte blockId = r.ReadByte();
            int blockSize = r.ReadInt16();

            InfoBlock block = new InfoBlock();
            block.FontSize = r.ReadInt16();
            byte bitField = r.ReadByte();
            block.IsSmooth = r.GetBit(bitField, 0);
            block.IsUnicode = r.GetBit(bitField, 1);
            block.IsItalic = r.GetBit(bitField, 2);
            block.IsBold = r.GetBit(bitField, 3);
            block.IsFixedHeight = r.GetBit(bitField, 4);
            block.CharSet = r.ReadByte();
            block.StretchH = r.ReadUInt16();
            block.AA = r.ReadByte();
            block.PaddingUp = r.ReadByte();
            block.PaddingRight = r.ReadByte();
            block.PaddingDown = r.ReadByte();
            block.PaddingLeft = r.ReadByte();
            block.SpacingHoriz = r.ReadByte();
            block.SpacingVert = r.ReadByte();
            block.Outline = r.ReadByte();
            block.FontName = r.ReadString();

            return block;
        }

        public override CommonBlock GetCommonBlock()
        {
            byte blockId = r.ReadByte();
            int blockSize = r.ReadInt16();

            CommonBlock block = new CommonBlock();
            block.LineHeight = r.ReadUInt16();
            block.Base = r.ReadUInt16();
            block.ScaleW = r.ReadUInt16();
            block.ScaleH = r.ReadUInt16();
            block.Pages = r.ReadUInt16();
            byte bitField = r.ReadByte();
            block.IsPacked = r.GetBit(bitField, 7);
            block.AlphaChannel = r.ReadByte();
            block.RedChannel = r.ReadByte();
            block.GreenChannel = r.ReadByte();
            block.BlueChannel = r.ReadByte();

            // We need this later
            common = block;
            return block;
        }

        public override string[] GetPages()
        {
            byte blockId = r.ReadByte();
            int blockSize = r.ReadInt16();

            string[] pageNames = new string[common.Pages];
            for (int i = 0; i < common.Pages; i++)
                pageNames[i] = r.ReadString();

            return pageNames;
        }

        public override Dictionary<char, BMPFontCharacter> GetCharacters()
        {
            byte blockId = r.ReadByte();
            int blockSize = r.ReadInt16();

            int numCharacters = blockSize / 20;
            Dictionary<char, BMPFontCharacter> chars = new Dictionary<char, BMPFontCharacter>();

            for (int i = 0; i < numCharacters; i++)
            {
                uint charId = r.ReadUInt32();
                uint x = r.ReadUInt16();
                uint y = r.ReadUInt16();
                uint width = r.ReadUInt16();
                uint height = r.ReadUInt16();
                int xOffset = r.ReadInt16();
                int yOffset = r.ReadInt16();
                int xAdvance = r.ReadInt16();
                uint page = r.ReadByte();
                byte channel = r.ReadByte();

                BMPFontCharacter c = new BMPFontCharacter(charId, x, y, width, height,
                    xOffset, yOffset, xAdvance, page, channel);
                chars.Add(c.Character, c);
            }

            return chars;
        }

        public override Dictionary<Tuple<char, char>, int> GetKerningPairs()
        {
            Dictionary<Tuple<char, char>, int> pairs = new Dictionary<Tuple<char, char>, int>();

            if (stream.Position < stream.Length)
            {
                byte blockId = r.ReadByte();
                int blockSize = r.ReadInt16();

                int numPairs = blockSize / 10;

                for (int i = 0; i < numPairs; i++)
                {
                    uint first = r.ReadUInt32();
                    uint second = r.ReadUInt32();
                    int amount = r.ReadInt16();

                    pairs.Add(new Tuple<char, char>((char)first, (char)second), amount);
                }
            }

            return pairs;
        }

        public override void Dispose()
        {
            r.Dispose();
        }
    }

    class BMPFontTextLoader : BMPFontLoader
    {
        #region BMPFontTextReader Classes
        class BMPFontReader : IDisposable
        {
            StreamReader reader;

            public BMPFontReader(StreamReader reader)
            {
                this.reader = reader;
            }

            public TextLine ReadLine()
            {
                return new TextLine(reader);
            }

            public void Dispose()
            {
                reader.Dispose();
            }
        }

        class TextLine
        {
            public readonly string Id;
            readonly Dictionary<string, TextLineProperty> properties;

            public TextLine(StreamReader reader)
            {
                properties = new Dictionary<string, TextLineProperty>();
                string line = reader.ReadLine();
                StringBuilder sb = new StringBuilder();
                bool inQuotes = false;

                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    if (c == '"') inQuotes = !inQuotes;
                    if (c != ' ') sb.Append(c);

                    if (sb.Length > 0 && ((!inQuotes && c == ' ') || i == line.Length - 1))
                    {
                        if (Id == null)
                            Id = sb.ToString();
                        else
                        {
                            TextLineProperty prop = new TextLineProperty(sb.ToString());
                            properties.Add(prop.Name, prop);
                        }
                        sb.Clear();
                    }
                }
            }

            public TextLineProperty GetProperty(string name)
            {
                TextLineProperty prop;
                if (properties.TryGetValue(name, out prop))
                    return prop;
                else
                    return null;
            }

            public byte? GetByte(string propName)
            {
                TextLineProperty prop = GetProperty(propName);
                if (prop != null) return prop.ToByte();
                else return null;
            }

            public bool? GetBool(string propName)
            {
                TextLineProperty prop = GetProperty(propName);
                if (prop != null) return prop.ToBool();
                else return null;
            }

            public int? GetInt(string propName)
            {
                TextLineProperty prop = GetProperty(propName);
                if (prop != null) return prop.ToInt();
                else return null;
            }

            public uint? GetUInt(string propName)
            {
                TextLineProperty prop = GetProperty(propName);
                if (prop != null) return prop.ToUInt();
                else return null;
            }

            public Vector2i? GetVector2i(string propName)
            {
                TextLineProperty prop = GetProperty(propName);
                if (prop != null) return prop.ToVector2i();
                else return null;
            }

            public Vector4i? GetVector4i(string propName)
            {
                TextLineProperty prop = GetProperty(propName);
                if (prop != null) return prop.ToVector4i();
                else return null;
            }

            public string GetString(string propName)
            {
                TextLineProperty prop = GetProperty(propName);
                if (prop != null) return prop.Value;
                else return null;
            }
        }

        class TextLineProperty
        {
            public readonly string Name;
            public readonly string Value;

            public TextLineProperty(string text)
            {
                string[] parts = text.Split('=');

                if (parts.Length != 2)
                    throw new Exception(string.Format("Failed to load text bmp file, invalid line property: '{0}'", text));

                Name = parts[0];
                Value = parts[1].Trim('"');
            }

            public byte ToByte() { return Value == "" ? (byte)0 : byte.Parse(Value); }
            public bool ToBool() { return Value == "1" ? true : false; }
            public int ToInt() { return Value == "" ? 0 : int.Parse(Value); }
            public uint ToUInt() { return Value == "" ? (uint)0 : ushort.Parse(Value); }

            public Vector4i ToVector4i()
            {
                string[] parts = Value.Split(',');
                Vector4i v = new Vector4i();
                for (int i = 0; i < parts.Length; i++)
                    v[i] = int.Parse(parts[i]);
                return v;
            }

            public Vector2i ToVector2i()
            {
                string[] parts = Value.Split(',');
                Vector2i v = new Vector2i();
                for (int i = 0; i < parts.Length; i++)
                    v[i] = int.Parse(parts[i]);
                return v;
            }
        }
        #endregion

        Stream stream;
        BMPFontReader reader;
        CommonBlock common;

        public BMPFontTextLoader(Stream stream)
        {
            this.stream = stream;
            StreamReader reader = new StreamReader(stream);
            this.reader = new BMPFontReader(reader);
        }

        public override InfoBlock GetInfoBlock()
        {
            TextLine line = reader.ReadLine();
            if (line.Id != "info") throw new Exception("Failed to load angel bmp file, unexpected block '" + line.Id + "'");

            InfoBlock block = new InfoBlock();
            block.FontName = line.GetString("face") ?? "";
            block.FontSize = line.GetInt("size") ?? 0;
            block.IsBold = line.GetBool("bold") ?? false;
            block.IsItalic = line.GetBool("italic") ?? false;
            block.CharSet = line.GetUInt("charset") ?? 0;
            block.IsUnicode = line.GetBool("unicode") ?? false;
            block.StretchH = line.GetUInt("stretchH") ?? 0;
            block.IsSmooth = line.GetBool("smooth") ?? false;
            block.AA = line.GetUInt("aa") ?? 0;
            Vector4i padding = line.GetVector4i("padding") ?? Vector4i.Zero;
            block.PaddingUp = (uint)padding.X;
            block.PaddingRight = (uint)padding.Y;
            block.PaddingDown = (uint)padding.Z;
            block.PaddingLeft = (uint)padding.W;
            Vector2i spacing = line.GetVector2i("spacing") ?? Vector2i.Zero;
            block.SpacingHoriz = spacing.X;
            block.SpacingVert = spacing.Y;
            block.Outline = line.GetInt("outline") ?? 0;

            return block;
        }

        public override CommonBlock GetCommonBlock()
        {
            TextLine line = reader.ReadLine();
            if (line.Id != "common") throw new Exception("Failed to load angel bmp file, unexpected block '" + line.Id + "'");

            CommonBlock block = new CommonBlock();
            block.LineHeight = line.GetUInt("lineHeight") ?? 0;
            block.Base = line.GetUInt("base") ?? 0;
            block.ScaleW = line.GetUInt("scaleW") ?? 0;
            block.ScaleH = line.GetUInt("scaleH") ?? 0;
            block.Pages = line.GetUInt("pages") ?? 0;
            block.IsPacked = line.GetBool("packed") ?? false;
            block.AlphaChannel = line.GetUInt("alphaChnl") ?? 4;
            block.RedChannel = line.GetUInt("redChnl") ?? 4;
            block.GreenChannel = line.GetUInt("greenChnl") ?? 4;
            block.BlueChannel = line.GetUInt("blueChnl") ?? 4;

            common = block;
            return block;
        }

        public override string[] GetPages()
        {
            string[] pageNames = new string[common.Pages];
            for (int i = 0; i < common.Pages; i++)
            {
                TextLine line = reader.ReadLine();
                if (line.Id != "page") throw new Exception("Failed to load angel bmp file, unexpected block '" + line.Id + "'");

                uint id = line.GetUInt("id") ?? 0;
                pageNames[id] = line.GetString("file") ?? "MISSING";
            }

            return pageNames;
        }

        public override Dictionary<char, BMPFontCharacter> GetCharacters()
        {
            TextLine line = reader.ReadLine();
            if (line.Id != "chars") throw new Exception("Failed to load angel bmp file, unexpected block '" + line.Id + "'");

            uint numChars = line.GetUInt("count") ?? 0;
            Dictionary<char, BMPFontCharacter> chars = new Dictionary<char, BMPFontCharacter>();
            for (int i = 0; i < numChars; i++)
            {
                TextLine cline = reader.ReadLine();
                if (cline.Id != "char") throw new Exception("Failed to load angel bmp file, unexpected block '" + cline.Id + "'");

                uint charId = cline.GetUInt("id") ?? '\0';
                uint x = cline.GetUInt("x") ?? 0;
                uint y = cline.GetUInt("y") ?? 0;
                uint width = cline.GetUInt("width") ?? 0;
                uint height = cline.GetUInt("height") ?? 0;
                int xOffset = cline.GetInt("xoffset") ?? 0;
                int yOffset = cline.GetInt("yoffset") ?? 0;
                int xAdvance = cline.GetInt("xadvance") ?? 0;
                uint page = cline.GetUInt("page") ?? 0;
                byte channel = cline.GetByte("chnl") ?? 0;

                BMPFontCharacter c = new BMPFontCharacter(charId, x, y, width, height,
                    xOffset, yOffset, xAdvance, page, channel);

                chars.Add(c.Character, c);
            }

            return chars;
        }

        public override Dictionary<Tuple<char, char>, int> GetKerningPairs()
        {
            Dictionary<Tuple<char, char>, int> pairs = new Dictionary<Tuple<char, char>, int>();

            if (stream.Position < stream.Length)
            {
                TextLine line = reader.ReadLine();
                if (line.Id != "kernings")
                    throw new Exception("Failed to load angel bmp file, unexpected block '" + line.Id + "'");

                uint numPairs = line.GetUInt("count") ?? 0;

                for (int i = 0; i < numPairs; i++)
                {
                    TextLine kline = reader.ReadLine();
                    if (kline.Id != "kerning")
                        throw new Exception("Failed to load angel bmp file, unexpected block '" + kline.Id + "'");

                    uint first = kline.GetUInt("first") ?? 0;
                    uint second = kline.GetUInt("second") ?? 0;
                    int amount = kline.GetInt("amount") ?? 0;

                    pairs.Add(new Tuple<char, char>((char)first, (char)second), amount);
                }
            }

            return pairs;
        }

        public override void Dispose()
        {
            reader.Dispose();
        }
    }
}
