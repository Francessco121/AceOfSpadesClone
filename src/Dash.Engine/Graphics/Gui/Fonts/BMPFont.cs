using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/* BMPFont.cs
 * Author: Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    /// <summary>
    /// An angelcode BMP font character.
    /// </summary>
    public class BMPFontCharacter
    {
        public readonly char Character;
        public readonly uint X;
        public readonly uint Y;
        public readonly uint Width;
        public readonly uint Height;
        public readonly int XOffset;
        public readonly int YOffset;
        public readonly int XAdvance;
        public readonly int PageIndex;
        public readonly CharacterTextureChannel Channel;

        internal BMPFontCharacter(uint id, uint x, uint y, uint width, uint height,
            int xOffset, int yOffset, int xAdvance, uint page, byte channel)
        {
            Character = (char)id;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            XOffset = xOffset;
            YOffset = yOffset;
            XAdvance = xAdvance;
            PageIndex = (int)page;
            Channel = (CharacterTextureChannel)channel;
        }
    }

    /// <summary>
    /// An angelcode BMP font.
    /// http://www.angelcode.com/products/bmfont
    /// </summary>
    public class BMPFont
    {
        public string Name { get; private set; }
        public int Size { get; private set; }

        public bool IsSmooth { get; private set; }
        public bool IsUnicode { get; private set; }
        public bool IsItalic { get; private set; }
        public bool IsBold { get; private set; }
        public bool IsFixedHeight { get; private set; }
        /// <summary>
        /// Set to true if the monochrome characters have been packed into each of the texture channels. 
        /// In this case AlphaChannel describes what is stored in each channel.
        /// </summary>
        public bool IsPacked { get; private set; }

        /// <summary>
        /// The supersampling level used. 1 means no supersampling was used.
        /// </summary>
        public int SuperSampling { get; private set; }
        /// <summary>
        /// The padding for each character (up, right, down, left).
        /// </summary>
        public Vector4i Padding { get; private set; }
        /// <summary>
        /// The spacing for each character (horizontal, vertical).
        /// </summary>
        public Vector2i Spacing { get; private set; }
        /// <summary>
        /// The outline thickness for the characters.
        /// </summary>
        public int Outline { get; private set; }

        /// <summary>
        /// The distance in pixels between each line of text.
        /// </summary>
        public int LineHeight { get; private set; }
        /// <summary>
        /// The number of pixels from the absolute top of the line 
        /// to the base of the characters.
        /// </summary>
        public int Base { get; private set; }
        /// <summary>
        /// The width of the texture, normally used to scale the 
        /// x pos of the character image.
        /// </summary>
        public int PageWidth { get; private set; }
        /// <summary>
        /// The height of the texture, normally used to scale the 
        /// y pos of the character image.
        /// </summary>
        public int PageHeight { get; private set; }

        public FontChannelInformation AlphaChannel { get; private set; }
        public FontChannelInformation RedChannel { get; private set; }
        public FontChannelInformation GreenChannel { get; private set; }
        public FontChannelInformation BlueChannel { get; private set; }

        public string[] PageNames { get; private set; }
        public Texture[] Pages { get; private set; }

        Dictionary<char, BMPFontCharacter> characters;
        Dictionary<Tuple<char, char>, int> kerningPairs;

        public BMPFont(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(
                    string.Format("Failed to find angel bmp font at '{0}'!", filePath), filePath);

            string dir = Path.GetDirectoryName(filePath);

            using (FileStream stream = File.OpenRead(filePath))
            {
                bool isBinaryFile = false;

                // Detect format
                using (StreamReader r = new StreamReader(stream, Encoding.Default, false, 1, true))
                {
                    string line1 = r.ReadLine();
                    if (!line1.StartsWith("info"))
                        isBinaryFile = true;

                    stream.Seek(0, SeekOrigin.Begin);
                }

                // Read file
                BMPFontLoader loader;
                if (isBinaryFile)
                    loader = new BMPFontBinaryLoader(stream);
                else
                    loader = new BMPFontTextLoader(stream);

                using (loader)
                {
                    // Load Info Block
                    BMPFontLoader.InfoBlock info = loader.GetInfoBlock();
                    Name = info.FontName;
                    Size = info.FontSize;
                    IsBold = info.IsBold;
                    IsItalic = info.IsItalic;
                    IsUnicode = info.IsUnicode;
                    IsSmooth = info.IsSmooth;
                    IsFixedHeight = info.IsFixedHeight;
                    SuperSampling = (int)info.AA;
                    Padding = new Vector4i(info.PaddingUp, info.PaddingRight, info.PaddingDown, info.PaddingLeft);
                    Spacing = new Vector2i(info.SpacingHoriz, info.SpacingVert);
                    Outline = info.Outline;

                    // Load Common Block
                    BMPFontLoader.CommonBlock common = loader.GetCommonBlock();
                    LineHeight = (int)common.LineHeight;
                    Base = (int)common.Base;
                    PageWidth = (int)common.ScaleW;
                    PageHeight = (int)common.ScaleH;
                    IsPacked = common.IsPacked;
                    AlphaChannel = (FontChannelInformation)common.AlphaChannel;
                    RedChannel = (FontChannelInformation)common.RedChannel;
                    GreenChannel = (FontChannelInformation)common.GreenChannel;
                    BlueChannel = (FontChannelInformation)common.BlueChannel;

                    // Load Pages
                    PageNames = loader.GetPages();
                    Pages = new Texture[PageNames.Length];
                    for (int i = 0; i < PageNames.Length; i++)
                    {
                        string fileName = PageNames[i];
                        string pageFilePath = Path.Combine(dir, fileName);

                        Pages[i] = GLoader.LoadTexture(pageFilePath, 
                            TextureMinFilter.Nearest, TextureMagFilter.Nearest, true);
                    }

                    // Load Characters
                    characters = loader.GetCharacters();

                    // Load Kerning Pairs
                    kerningPairs = loader.GetKerningPairs();
                }
            }
        }

        /// <summary>
        /// Gets the character information for this font.
        /// Defaults to space if not a supported character,
        /// and defaults to null if space is not supported.
        /// </summary>
        public BMPFontCharacter GetCharacter(char c)
        {
            BMPFontCharacter g;
            if (characters.TryGetValue(c, out g))
                return g;
            else if (c != ' ')
                return GetCharacter(' ');
            else
                return null;
        }

        /// <summary>
        /// Gets the x kerning offset that is applied
        /// to the second char, when immediatly after
        /// the first.
        /// </summary>
        public int GetKerning(char first, char second)
        {
            int amount;
            if (kerningPairs != null && kerningPairs.TryGetValue(new Tuple<char, char>(first, second), out amount))
                return amount;
            else
                return 0;
        }

        public Vector2 MeasureString(string str)
        {
            float xSize = 0;
            float ySize = 0;

            string[] lines = str.Split('\n');

            for (int l = 0; l < lines.Length; l++)
            {
                ySize += LineHeight;
                string line = lines[l];

                int lineXSize = 0;
                char? lastC = null;
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (c == '\r')
                        continue;

                    BMPFontCharacter g = GetCharacter(c);
                    if (g != null)
                    {
                        int kerningOffset = lastC.HasValue ? GetKerning(lastC.Value, c) : 0;
                        lineXSize += g.XAdvance + kerningOffset;
                    }

                    lastC = c;
                }

                xSize = Math.Max(lineXSize, xSize);
            }

            return new Vector2(xSize, ySize);
        }

        public void DrawString(string str, float x, float y, SpriteBatch sb, TextAlign align, Color color, 
            Color? shadowColor = null)
        {
            float startX = x;
            float startY = y;
            string[] lines = str.Split('\n');

            bool topAlign = align == TextAlign.TopLeft 
                || align == TextAlign.TopCenter
                || align == TextAlign.TopRight;

            bool bottomAlign = align == TextAlign.BottomLeft 
                || align == TextAlign.BottomCenter
                || align == TextAlign.BottomRight;

            bool leftAlign = align == TextAlign.TopLeft
                || align == TextAlign.Left
                || align == TextAlign.BottomLeft;

            bool rightAlign = align == TextAlign.TopRight
                || align == TextAlign.Right
                || align == TextAlign.BottomRight;

            for (int l = 0; l < lines.Length; l++)
            {
                string line = lines[l];
                Vector2 lineSize = MeasureString(line);

                if (leftAlign)
                    x = startX;
                else if (rightAlign)
                    x = startX - lineSize.X;
                else
                    x = startX - lineSize.X / 2f;

                if (topAlign)
                    y = startY + LineHeight * l;
                else if (bottomAlign)
                    y = startY - LineHeight * (lines.Length - l);
                else
                    y = startY - (LineHeight * (lines.Length / 2f)) + LineHeight * l;

                char? lastC = null;
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (c == '\r')
                        continue;

                    BMPFontCharacter g = GetCharacter(c);
                    if (g != null)
                    {
                        int kerningOffset = lastC.HasValue ? GetKerning(lastC.Value, c) : 0;
                        Rectangle rect = new Rectangle(x + g.XOffset + kerningOffset, y + g.YOffset, g.Width, g.Height);
                        Rectangle clip = new Rectangle(g.X, g.Y, g.Width, g.Height);

                        if (shadowColor.HasValue)
                            sb.Draw(Pages[g.PageIndex], rect + new Vector2(1, 1), clip, shadowColor.Value);

                        sb.Draw(Pages[g.PageIndex], rect, clip, color);

                        x += g.XAdvance + kerningOffset;
                    }

                    lastC = c;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
