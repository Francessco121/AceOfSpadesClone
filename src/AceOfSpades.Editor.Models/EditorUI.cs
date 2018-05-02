using AceOfSpades.Editor.Gui;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;
using System.IO;

/* EditorUI.cs
 * Ethan Lafrenais
 * Tristan Smith
*/

namespace AceOfSpades.Editor.Models
{
    class EditorUI
    {
        public GUIColorPickerWindow ColorWindow { get; }
        public GUITheme Theme { get; }
        public GUISystem GUISystem { get; }

        GUIArea area;
        MasterRenderer renderer;
        EditorScreen screen;
        GUILabel statusLeft, statusRight;
        GUIDropDownButton[] editModeButtons;
        GUILabel currentToolLabel;
        FileBrowserWindow openFileWindow;
        FileBrowserWindow saveFileWindow;
        MessageWindow popup;

        public EditorUI(MasterRenderer renderer, EditorScreen screen)
        {
            this.renderer = renderer;
            this.screen = screen;

            GUISystem = renderer.Sprites.GUISystem;

            area = new GUIArea(GUISystem);
            renderer.Sprites.Add(area);

            Theme = EditorTheme.Glass;

            GUIFrame topBar = new GUIFrame(UDim2.Zero, new UDim2(1, 0, 0, 40), Theme);

            const float menuItemWidth = 220;

            GUIDropDown fileMenu = new GUIDropDown(UDim2.Zero, new UDim2(0, menuItemWidth, 1, 0), Theme, false) { Parent = topBar, Text = "File" };
            fileMenu.AddItem("New", null, (d, b) => { screen.LoadNewModel(); });
            fileMenu.AddItem("Open", null, (d, b) => { openFileWindow.Visible = true; });
            fileMenu.AddItem("Save", null, (d, b) => { if (screen.CurrentFile != null) screen.SaveModel(); else saveFileWindow.Visible = true; });
            fileMenu.AddItem("Save As...", null, (d, b) => { saveFileWindow.Visible = true; });

            GUIDropDown editMenu = new GUIDropDown(new UDim2(0, menuItemWidth, 0, 0), new UDim2(0, menuItemWidth, 1, 0), Theme, false) { Parent = topBar, Text = "Edit" };

            GUIDropDown editModeMenu = new GUIDropDown(UDim2.Zero, new UDim2(0, menuItemWidth, 1, 0), Theme, false) { HideMainButton = true };
            editMenu.AddItemSub("Mode", editModeMenu);
            editModeButtons = new GUIDropDownButton[] {
                editModeMenu.AddItem("None", null, OnEditModeSelected),
                editModeMenu.AddItem("Add", null, OnEditModeSelected),
                editModeMenu.AddItem("Delete", null, OnEditModeSelected),
                editModeMenu.AddItem("Paint", null, OnEditModeSelected),
                editModeMenu.AddItem("Move", null, OnEditModeSelected),
            };

            editModeButtons[0].Toggled = true;

            GUIDropDown gfxMenu = new GUIDropDown(new UDim2(0, menuItemWidth * 2, 0, 0), new UDim2(0, menuItemWidth, 1, 0), Theme, false)
            { Parent = topBar, Text = "Graphics" };
            gfxMenu.AddItem("FXAA", null, (d, b) => { TogglePostProcess(b, true); });

            GUIDropDown viewMenu = new GUIDropDown(new UDim2(0, menuItemWidth * 3, 0, 0), new UDim2(0, menuItemWidth, 1, 0), Theme, false) { Parent = topBar, Text = "View" };
            viewMenu.AddItem("Color Picker", null, (d, b) => { ColorWindow.Visible = true; });

            currentToolLabel = new GUILabel(new UDim2(1f, -5, 0, 5), UDim2.Zero, "Current Tool: None", TextAlign.TopRight, Theme) { Parent = topBar };

            SetupDefaultGraphicsSettings(gfxMenu);
            area.AddTopLevel(topBar);

            GUIFrame bottomBar = new GUIFrame(new UDim2(0, 0, 1, -30), new UDim2(1, 0, 0, 30), Theme);

            statusLeft = new GUILabel(UDim2.Zero, new UDim2(0.5f, 0, 1, 0), "<left status>", TextAlign.Left, Theme) { Parent = bottomBar };
            statusRight = new GUILabel(new UDim2(0.5f, 0, 0, 0), new UDim2(0.5f, 0, 1, 0), "<right status>", TextAlign.Right, Theme) { Parent = bottomBar };

            area.AddTopLevel(bottomBar);

            openFileWindow = new FileBrowserWindow(GUISystem, Theme, new UDim2(0.75f, 0, 0.75f, 0), "Open Model",
                FileBrowserMode.OpenFile, new string[] { ".aosm" },
                (window) =>
                {
                    if (File.Exists(window.FileName))
                        screen.LoadModel(window.FileName);
                });

            saveFileWindow = new FileBrowserWindow(GUISystem, Theme, new UDim2(0.75f, 0, 0.75f, 0), "Save Model",
                FileBrowserMode.Save, new string[] { ".aosm" },
                (window) =>
                {
                    string fullPath = Path.Combine(window.CurrentDirectory, window.FileName);

                    if (!Path.HasExtension(fullPath))
                        fullPath += ".aosm";

                    screen.SaveModel(fullPath);
                });

            ColorWindow = new GUIColorPickerWindow(GUISystem, new UDim2(0.3f, 0, 0.3f, 0), Theme);
            ColorWindow.Visible = true;
            ColorWindow.Position = new UDim2(0.7f, -10, 0.7f, -10);
            ColorWindow.MinSize = new UDim2(0, 400, 0, 300);
            ColorWindow.MaxSize = new UDim2(0, 550, 0, 400);

            popup = new MessageWindow(GUISystem, Theme, new UDim2(0.6f, 0, 0.3f, 0), "Alert!");
            popup.MinSize = new UDim2(0, 215, 0, 200);
            popup.MaxSize = new UDim2(0, 600, 0, 275);

            GUISystem.Add(ColorWindow, openFileWindow, saveFileWindow, popup);
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

        public void Update(float deltaTime)
        {
            if (screen.Model != null)
                statusLeft.Text = string.Format("Dimensions: {0}x{1}x{2}",
                    screen.Model.Width, screen.Model.Height, screen.Model.Depth);
            else
                statusLeft.Text = "";

            statusRight.Text = string.Format("{0}fps", (int)Math.Ceiling(screen.Window.FPS));
        }

        void SetupDefaultGraphicsSettings(GUIDropDown menu)
        {
            menu.Items[0].Toggled = renderer.GFXSettings.ApplyFXAA;
        }

        void TogglePostProcess(GUIDropDownButton btn, bool fxaa)
        {
            if (fxaa) btn.Toggled = renderer.GFXSettings.ApplyFXAA = !renderer.GFXSettings.ApplyFXAA;
            fxaa = renderer.GFXSettings.ApplyFXAA;
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
            SetToolType(mode);
        }
    }
}
