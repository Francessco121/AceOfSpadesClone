using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

/* StaticGui.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Gui
{
    public class StaticGui
    {
        public static bool IsVisible { get; set; } = true;
        public static bool ShowBackground { get; set; }

        MainWindow window;
        MasterRenderer renderer;

        ControlsWindow controls;

        GUILabel fpsLabel;
        GUILabel timeLabel;
        GUILabel versionLabel;

        static List<Texture> screenshots;
        GUIFrame background;

        GUIArea area;
        GUISystem gsys;

        public StaticGui(MainWindow window, MasterRenderer renderer)
        {
            this.window = window;
            this.renderer = renderer;

            gsys = renderer.Sprites.GUISystem;

            area = new GUIArea(gsys);
            renderer.Sprites.Add(area);

            BMPFont smallFont = AssetManager.LoadFont("arial-bold-12");
            BMPFont normalFont = AssetManager.LoadFont("arial-bold-14");
            BMPFont bigFont = AssetManager.LoadFont("arial-bold-20");
            BMPFont tinyFont = AssetManager.LoadFont("arial-bold-10");

            GUITheme theme = AssetManager.CreateDefaultGameTheme();
            theme.SetField("SmallFont", smallFont);
            theme.SetField("Font", normalFont);
            theme.SetField("TinyFont", tinyFont);

            controls = new ControlsWindow(gsys, theme);
            controls.ZIndex = 200;

            // Overlay
            fpsLabel = new GUILabel(UDim2.Zero, UDim2.Zero, "FPS: --", TextAlign.TopLeft, theme);
            timeLabel = new GUILabel(new UDim2(1f, 0, 0, 0), UDim2.Zero, "Time: --", TextAlign.TopRight, theme);
            versionLabel = new GUILabel(new UDim2(0, 0, 1f, 0), UDim2.Zero, GameVersion.Current.ToString(), 
                TextAlign.BottomLeft, theme);
            fpsLabel.Font = smallFont;
            timeLabel.Font = smallFont;
            versionLabel.Font = bigFont;

            if (screenshots == null)
            {
                string[] mainMenuFiles = Directory.GetFiles("Content/Textures/MainMenu");
                screenshots = new List<Texture>();

                foreach (string file in mainMenuFiles)
                {
                    // Skip thumbs.db
                    if (file.EndsWith(".db"))
                        continue;

                    try { screenshots.Add(GLoader.LoadTexture(file, TextureMinFilter.Linear, TextureMagFilter.Linear, true)); }
                    catch (Exception e) { DashCMD.WriteError("Failed to load main menu background '{1}'. \n{0}", e, file); }
                }
            }

            background = new GUIFrame(UDim2.Zero, new UDim2(1f, 0, 1f, 0), Image.Blank);
            background.ZIndex = -100;

            area.AddTopLevel(background, fpsLabel, timeLabel, versionLabel);
            gsys.Add(controls);
        }

        public void ShowRandomBackgroundImage()
        {
            if (screenshots == null || screenshots.Count == 0)
                return;

            Texture current = background.Image.Texture;
            Texture newTex;
            do
            {
                newTex = screenshots[Maths.Random.Next(0, screenshots.Count)];

                if (newTex != current)
                    break;
            }
            while (screenshots.Count > 1);

            background.Image.Texture = newTex;
            background.Image.ClippingRectangle = new Rectangle(0, 0, newTex.Width, newTex.Height);
        }

        public void ToggleControlsWindow(bool visible)
        {
            if (visible)
                controls.Show();
            else
                controls.Visible = false;
        }

        public void Update(float deltaTime)
        {
            background.Visible = ShowBackground;

            fpsLabel.Visible = timeLabel.Visible = IsVisible;
            versionLabel.Visible = !IsVisible;

            fpsLabel.Text = string.Format("FPS: {0}", (int)Math.Round(window.FPS));

            DateTime time = new DateTime().AddHours(renderer.Sky.currentHour);
            timeLabel.Text = string.Format("Time: {0}", time.ToString("h:mm tt"));
        }
    }
}
