using AceOfSpades.Editor.Gui;
using AceOfSpades.Editor.World.Gui;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;
using System.IO;

/* EditorUI.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World
{
    public class EditorUI
    {
        public GUIColorPickerWindow ColorWindow { get; }
        public GUITheme Theme { get; }
        public GUISystem GUISystem { get; }

        GUIArea area;
        MasterRenderer renderer;
        EditorScreen screen;
        GUILabel statusLeft, statusRight, statusMid;
        GUIDropDownButton[] fogButtons;
        GUIDropDownButton[] editModeButtons;
        GUIDropDownButton[] pcfButtons;
        FileBrowserWindow openWorldWindow;
        FileBrowserWindow saveWorldWindow;
        GUILabel currentToolLabel;
        MessageWindow popup;
        NewWorldWindow newWindow;

        public EditorUI(MasterRenderer renderer, EditorScreen screen)
        {
            this.renderer = renderer;
            this.screen = screen;

            GUISystem = renderer.Sprites.GUISystem;

            area = new GUIArea(GUISystem);
            renderer.Sprites.Add(area);

            Theme = EditorTheme.BasicEdtior;

            TranslateTerrainWindow transTerrainWindow;

            newWindow = new NewWorldWindow(GUISystem, screen, Theme);

            transTerrainWindow = new TranslateTerrainWindow(GUISystem, Theme);
            transTerrainWindow.OnApply += (sender, d) => { screen.World.TranslateTerrain(d); };

            GUIFrame topBar = new GUIFrame(UDim2.Zero, new UDim2(1, 0, 0, 40), Theme);

            float menuItemWidth = 220;

            GUIDropDown fileMenu = new GUIDropDown(UDim2.Zero, new UDim2(0, menuItemWidth, 1, 0), Theme, false) { Parent = topBar, Text = "File" };
            fileMenu.AddItem("New", null, (d, b) => { newWindow.Visible = true; });
            fileMenu.AddItem("Open", null, (d, b) => { openWorldWindow.Visible = true; });
            fileMenu.AddItem("Save", null, (d, b) => { if (screen.CurrentFile != null) screen.SaveWorld(); else saveWorldWindow.Visible = true; });
            fileMenu.AddItem("Save As...", null, (d, b) => { saveWorldWindow.Visible = true; });

            GUIDropDown editMenu = new GUIDropDown(new UDim2(0, menuItemWidth, 0, 0), new UDim2(0, menuItemWidth, 1, 0), Theme, false) { Parent = topBar, Text = "Edit" };

            GUIDropDown editModeMenu = new GUIDropDown(UDim2.Zero, new UDim2(0, menuItemWidth, 1, 0), Theme, false) { HideMainButton = true };
            editMenu.AddItemSub("Mode", editModeMenu);
            editModeButtons = new GUIDropDownButton[] {
                editModeMenu.AddItem("Select", null, OnEditModeSelected),
                editModeMenu.AddItem("Add", null, OnEditModeSelected),
                editModeMenu.AddItem("Delete", null, OnEditModeSelected),
                editModeMenu.AddItem("Paint", null, OnEditModeSelected),
                editModeMenu.AddItem("Terrain Move", null, OnEditModeSelected),
                editModeMenu.AddItem("Terraform", null, OnEditModeSelected),
            };

            GUIDropDown insertSubMenu = new GUIDropDown(UDim2.Zero, new UDim2(0, menuItemWidth, 1, 0), Theme, false) { HideMainButton = true };
            editMenu.AddItemSub("Insert", insertSubMenu);
            GUIDropDownButton[]  insertButtons = new GUIDropDownButton[] {
                insertSubMenu.AddItem("Command Post", null, (d, b) => { screen.World.AddNewCommandPost(); }),
                insertSubMenu.AddItem("Intel", null, (d, b) => { screen.World.AddNewIntel(); }),
            };

            editMenu.AddItem("Bake Damage Colors", null, (d, b) => { screen.WorldEditor.TerrainEditor.BakeDamageColors(); });
            editMenu.AddItem("Translate Terrain", null, (d, b) => { transTerrainWindow.Visible = true; });

            editModeButtons[0].Toggled = true;

            GUIDropDown gfxMenu = new GUIDropDown(new UDim2(0, menuItemWidth * 2, 0, 0), new UDim2(0, menuItemWidth, 1, 0), Theme, false)
            { Parent = topBar, Text = "Graphics" };
            gfxMenu.AddItem("FXAA", null, (d, b) => { TogglePostProcess(b, true); });
            gfxMenu.AddItem("Shadows", null, (d, b) => { b.Toggled = renderer.GFXSettings.RenderShadows = !renderer.GFXSettings.RenderShadows; });

            GUIDropDown gfxFogMenu = new GUIDropDown(UDim2.Zero, new UDim2(0, menuItemWidth, 1, 0), Theme, false) { HideMainButton = true };
            gfxMenu.AddItemSub("Fog", gfxFogMenu);
            fogButtons = new GUIDropDownButton[4];
            fogButtons[0] = gfxFogMenu.AddItem("Off", null, OnFogSelected);
            fogButtons[1] = gfxFogMenu.AddItem("Low", null, OnFogSelected);
            fogButtons[2] = gfxFogMenu.AddItem("Medium", null, OnFogSelected);
            fogButtons[3] = gfxFogMenu.AddItem("High", null, OnFogSelected);

            GUIDropDown gfxPCFMenu = new GUIDropDown(UDim2.Zero, new UDim2(0, menuItemWidth, 1, 0), Theme, false) { HideMainButton = true };
            gfxMenu.AddItemSub("PCF Samples", gfxPCFMenu);
            pcfButtons = new GUIDropDownButton[5];
            pcfButtons[0] = gfxPCFMenu.AddItem("1", 1, OnPCFSelected);
            pcfButtons[1] = gfxPCFMenu.AddItem("2", 2, OnPCFSelected);
            pcfButtons[2] = gfxPCFMenu.AddItem("4", 4, OnPCFSelected);
            pcfButtons[3] = gfxPCFMenu.AddItem("6", 6, OnPCFSelected);
            pcfButtons[4] = gfxPCFMenu.AddItem("12", 12, OnPCFSelected);

            GUIDropDown viewMenu = new GUIDropDown(new UDim2(0, menuItemWidth * 3, 0, 0), new UDim2(0, menuItemWidth, 1, 0), Theme, false) { Parent = topBar, Text = "View" };
            viewMenu.AddItem("Color Picker", null, (d, b) => { ColorWindow.Visible = true; });
            viewMenu.AddItem("Chunk Borders", null, (d, b) => { b.Toggled = screen.World.ShowChunkBorders = !screen.World.ShowChunkBorders; });

            currentToolLabel = new GUILabel(new UDim2(1f, -5, 0, 5), UDim2.Zero, "Current Tool: Add", TextAlign.TopRight, Theme) { Parent = topBar };

            SetupDefaultGraphicsSettings(gfxMenu);
            area.AddTopLevel(topBar);

            GUIFrame bottomBar = new GUIFrame(new UDim2(0, 0, 1, -30), new UDim2(1, 0, 0, 30), Theme);

            statusLeft = new GUILabel(UDim2.Zero, new UDim2(0.5f, 0, 1, 0), "<left status>", TextAlign.Left, Theme) { Parent = bottomBar };
            statusRight = new GUILabel(new UDim2(0.5f, 0, 0, 0), new UDim2(0.5f, 0, 1, 0), "<right status>", TextAlign.Right, Theme) { Parent = bottomBar };
            statusMid = new GUILabel(new UDim2(0.25f, 0, 0, 0), new UDim2(0.5f, 0, 1f, 0), "", TextAlign.Center, Theme) { Parent = bottomBar };

            area.AddTopLevel(bottomBar);

            openWorldWindow = new FileBrowserWindow(GUISystem, Theme, new UDim2(0.75f, 0, 0.75f, 0), "Open World", 
                FileBrowserMode.OpenFile, new string[] { ".aosw" },
                (window) =>
                {
                    if (File.Exists(window.FileName))
                        screen.LoadWorld(window.FileName);
                });

            saveWorldWindow = new FileBrowserWindow(GUISystem, Theme, new UDim2(0.75f, 0, 0.75f, 0), "Save World", 
                FileBrowserMode.Save, new string[] { ".aosw" },
                (window) =>
                {
                    string fullPath = Path.Combine(window.CurrentDirectory, window.FileName);

                    if (!Path.HasExtension(fullPath))
                        fullPath += ".aosw";

                    screen.SaveWorld(fullPath);
                });

            ColorWindow = new GUIColorPickerWindow(GUISystem, new UDim2(0.3f, 0, 0.3f, 0), Theme);
            ColorWindow.Visible = true;
            ColorWindow.Position = new UDim2(0.7f, -10, 0.7f, -10);
            ColorWindow.MinSize = new UDim2(0, 400, 0, 300);
            ColorWindow.MaxSize = new UDim2(0, 550, 0, 400);
            popup = new MessageWindow(GUISystem, Theme, new UDim2(0.6f, 0, 0.3f, 0), "Alert!");
            popup.MinSize = new UDim2(0, 215, 0, 200);
            popup.MaxSize = new UDim2(0, 600, 0, 275);

            GUISystem.Add(ColorWindow, transTerrainWindow, openWorldWindow, saveWorldWindow, newWindow, popup);
        }

        public void SetMidStatus(string message)
        {
            statusMid.Text = message;
        }

        public void ShowPopup(string title, string message)
        {
            popup.Title = title;
            popup.Show(message);
        }

        public void HidePopup()
        {
            popup.Hide();
        }

        void SetupDefaultGraphicsSettings(GUIDropDown menu)
        {
            menu.Items[0].Toggled = renderer.GFXSettings.ApplyFXAA;
            menu.Items[1].Toggled = renderer.GFXSettings.RenderShadows;

            if (renderer.FogEnabled)
                menu.Items[2].Sub.Items[0].Toggled = true;
            else
            {
                if (!renderer.FogEnabled) menu.Items[2].Sub.Items[0].Toggled = true;
                else if(renderer.GFXSettings.FogQuality == FogQuality.Low) menu.Items[2].Sub.Items[1].Toggled = true;
                else if (renderer.GFXSettings.FogQuality == FogQuality.Medium) menu.Items[2].Sub.Items[2].Toggled = true;
                else if (renderer.GFXSettings.FogQuality == FogQuality.High) menu.Items[2].Sub.Items[3].Toggled = true;

            }

            switch (renderer.GFXSettings.ShadowPCFSamples)
            {
                case 1:
                    menu.Items[3].Sub.Items[0].Toggled = true; break;
                case 2:
                    menu.Items[3].Sub.Items[1].Toggled = true; break;
                case 4:
                    menu.Items[3].Sub.Items[2].Toggled = true; break;
                case 6:
                    menu.Items[3].Sub.Items[3].Toggled = true; break;
                case 12:
                    menu.Items[3].Sub.Items[4].Toggled = true; break;
            }
        }

        void TogglePostProcess(GUIDropDownButton btn, bool fxaa)
        {
            if (fxaa) btn.Toggled = renderer.GFXSettings.ApplyFXAA = !renderer.GFXSettings.ApplyFXAA;
            fxaa = renderer.GFXSettings.ApplyFXAA;
        }

        void OnPCFSelected(GUIDropDown dropDown, GUIDropDownButton btn)
        {
            for (int i = 0; i < pcfButtons.Length; i++)
                pcfButtons[i].Toggled = false;

            int v = (int)btn.Value;
            renderer.GFXSettings.ShadowPCFSamples = v;

            switch (v)
            {
                case 1:
                    dropDown.Items[0].Toggled = true; break;
                case 2:
                    dropDown.Items[1].Toggled = true; break;
                case 4:
                    dropDown.Items[2].Toggled = true; break;
                case 6:
                    dropDown.Items[3].Toggled = true; break;
                case 12:
                    dropDown.Items[4].Toggled = true; break;
            }
        }

        public void SetToolType(EditorToolType mode)
        {
            for (int i = 0; i < editModeButtons.Length; i++)
                editModeButtons[i].Toggled = false;

            editModeButtons[(int)mode].Toggled = true;
            currentToolLabel.Text = string.Format("Current Tool: {0}", mode);
        }

        void OnEditModeSelected(GUIDropDown dropDown, GUIDropDownButton btn)
        {
            EditorToolType mode = (EditorToolType)btn.Index;
            SetToolType(mode);
            screen.WorldEditor.SetToolType(mode);
        }

        void OnFogSelected(GUIDropDown dropDown, GUIDropDownButton btn)
        {
            for (int i = 0; i < fogButtons.Length; i++)
                fogButtons[i].Toggled = false;

            fogButtons[btn.Index].Toggled = true;

            if (btn.Index == 0)
                renderer.FogEnabled = false;
            else
            {
                renderer.FogEnabled = true;

                if (btn.Index == 1) renderer.GFXSettings.FogQuality = FogQuality.Low;
                else if (btn.Index == 2) renderer.GFXSettings.FogQuality = FogQuality.Medium;
                else if (btn.Index == 3) renderer.GFXSettings.FogQuality = FogQuality.High;
            }
        }

        public void Update(float deltaTime)
        {
            FixedTerrain terrain = screen.World.Terrain;
            if (terrain != null)
                statusLeft.Text = string.Format("Dimensions: {0}x{1}x{2}",
                    terrain.Width, terrain.Height, terrain.Depth);
            else
                statusLeft.Text = "";

            statusRight.Text = string.Format("{0}fps", (int)Math.Round(screen.Window.FPS));
        }
    }
}
