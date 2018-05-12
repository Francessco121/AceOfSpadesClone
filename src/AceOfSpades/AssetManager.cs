using AceOfSpades.Graphics;
using AceOfSpades.IO;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Audio;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using Dash.Engine.Graphics.OpenGL;
using System.Collections.Generic;
using System.IO;

namespace AceOfSpades
{
    public static class AssetManager
    {
        static Dictionary<string, VoxelObject> models = new Dictionary<string, VoxelObject>();
        static Dictionary<string, BMPFont> fonts = new Dictionary<string, BMPFont>();
        static Dictionary<string, AudioBuffer> sound = new Dictionary<string, AudioBuffer>();

        static AssetManager()
        {
            DashCMD.AddCommand("unusedfonts", "Locates any unused fonts",
                (args) =>
                {
                    string[] files = Directory.GetFiles(GLoader.GetContentRelativePath("Fonts"));
                    foreach (string file in files)
                    {
                        if (file.EndsWith(".fnt"))
                        {
                            if (!fonts.ContainsKey(file))
                                DashCMD.WriteStandard("[unusedfonts] {0}", Path.GetFileNameWithoutExtension(file));
                        }
                    }
                });
        }

        public static GUITheme CreateDefaultGameTheme()
        {
            GUITheme theme = GUITheme.Basic;
            string guiPalette = "Textures/Gui/gui-palette.png";
            theme.SetField("Button.NormalImage", new BorderedImage(GLoader.LoadTexture(guiPalette), new Rectangle(0, 0, 66, 66)));
            theme.SetField("Button.HoverImage", new BorderedImage(GLoader.LoadTexture(guiPalette), new Rectangle(66, 0, 66, 66)));
            theme.SetField("Button.ActiveImage", new BorderedImage(GLoader.LoadTexture(guiPalette), new Rectangle(132, 0, 66, 66)));
            theme.SetField("Button.ToggledImage", Image.CreateBlank(new Color(194, 37, 37)));
            theme.SetField("Button.TextColor", Color.White);
            theme.SetField("TextField.NormalImage", new BorderedImage(GLoader.LoadTexture(guiPalette), new Rectangle(0, 66, 66, 66)));
            theme.SetField("TextField.HoverImage", new BorderedImage(GLoader.LoadTexture(guiPalette), new Rectangle(66, 66, 66, 66)));
            theme.SetField("TextField.ActiveImage", new BorderedImage(GLoader.LoadTexture(guiPalette), new Rectangle(132, 66, 66, 66)));
            theme.SetField("Label.TextColor", Color.White);
            theme.SetField("Label.TextShadowColor", new Color(0, 0, 0, 0.6f));
            theme.SetField("Frame.Image", Image.CreateBlank(new Color(30, 30, 30, 240)));
            theme.SetField("Window.BackgroundImage", Image.CreateBlank(new Color(30, 30, 30, 200)));
            theme.SetField("Window.TitleBar.BackgroundImage", Image.CreateBlank(new Color(31, 47, 70)));
            theme.SetField("Window.TitleBar.CloseButton.NormalImage", Image.CreateBlank(new Color(152, 34, 34)));
            theme.SetField("Window.TitleBar.CloseButton.HoverImage", Image.CreateBlank(new Color(135, 15, 15)));
            theme.SetField("Window.TitleBar.CloseButton.ActiveImage", Image.CreateBlank(new Color(188, 84, 84)));
            theme.SetField("SmallFont", LoadFont("arial-bold-12"));
            theme.SetField("Font", LoadFont("arial-14"));
            theme.SetField("BigFont", LoadFont("arial-18"));

            return theme;
        }

        public static BMPFont LoadFont(string fileName)
        {
            if (GlobalNetwork.IsServer)
                return null;

            string filePath = GLoader.GetContentRelativePath(Path.Combine("Fonts", fileName + ".fnt"));

            BMPFont font;
            if (fonts.TryGetValue(filePath, out font))
                return font;
            else
            {
                font = new BMPFont(filePath);
                fonts.Add(filePath, font);
                return font;
            }
        }

        public static VoxelObject LoadVoxelObject(string filePath, BufferUsageHint usageHint)
        {
            if (GlobalNetwork.IsServer)
                return null;

            VoxelObject vo;
            if (models.TryGetValue(filePath, out vo))
                return vo;
            else
            {
                if (VoxelIO.Load(filePath, out vo))
                {
                    vo.BuildMesh(usageHint);
                    models.Add(filePath, vo);
                    return vo;
                }
                else
                    throw new FileNotFoundException(string.Format("Failed to load model {0}!", filePath));
            }
        }

        public static AudioBuffer LoadSound(string filePath)
        {
            if (GlobalNetwork.IsServer)
                return null;

            filePath = GLoader.GetContentRelativePath(Path.Combine("Sounds", filePath));

            AudioBuffer buffer;
            if (sound.TryGetValue(filePath, out buffer))
                return buffer;
            else
            {
                if (!File.Exists(filePath))
                    return null;

                string ext = Path.GetExtension(filePath);

                switch (ext)
                {
                    case ".wav":
                        buffer = new WavFile(filePath);
                        break;
                    case ".ogg":
                        buffer = new OggFile(filePath);
                        break;
                    default:
                        throw new IOException($"Audio files with extension {ext} are not supported!");
                }

                sound.Add(filePath, buffer);
                return buffer;
            }
        }
    }
}
