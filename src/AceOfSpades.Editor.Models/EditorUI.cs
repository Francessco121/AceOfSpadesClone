using AceOfSpades.Editor.Gui;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;
using System.IO;

/* EditorUI.cs
 * Ethan Lafrenais
 * Tristan Smith
*/

namespace AceOfSpades.Editor.Models
{
    public class EditorUI
    {
        GUIArea area;
        MasterRenderer renderer;
        EditorScreen screen;
        GUITheme theme;
        GUILabel statusLeft, statusRight;
        FileBrowserWindow openFileWindow;
        FileBrowserWindow saveFileWindow;

        List<GUIWindow> windowElements = new List<GUIWindow>();

        GUIFrame TopBar;
        ToolBarHelper TopBarHelper;
        bool setup;

        public EditorUI(MasterRenderer renderer, EditorScreen screen)
        {
            this.renderer = renderer;
            this.screen = screen;

            GUISystem gsys = renderer.Sprites.GUISystem;

            area = new GUIArea(gsys);
            renderer.Sprites.Add(area);

            theme = EditorTheme.Glass;


            GenBar(renderer.ScreenWidth);

            openFileWindow = new FileBrowserWindow(gsys, theme, new UDim2(0.75f, 0, 0.75f, 0), "Open Model",
                FileBrowserMode.OpenFile, new string[] { ".aosm" },
                (window) =>
                {
                    if (File.Exists(window.FileName))
                        screen.LoadModel(window.FileName);
                });

            saveFileWindow = new FileBrowserWindow(gsys, theme, new UDim2(0.75f, 0, 0.75f, 0), "Save Model",
                FileBrowserMode.Save, new string[] { ".aosm" },
                (window) =>
                {
                    string fullPath = Path.Combine(window.CurrentDirectory, window.FileName);

                    if (!Path.HasExtension(fullPath))
                        fullPath += ".aosm";

                    screen.SaveModel(fullPath);
                });

            gsys.Add(openFileWindow, saveFileWindow);
        }


        /// <summary>
        /// Set a GFX option and reflect setting on button
        /// </summary>
        /// <param name="dropDown"></param>
        /// <param name="button"></param>
        void SetGfxOption(GUIDropDown dropDown, GUIDropDownButton button)
        {
            if (button.Value == null)
                return; //it should never be null but just in case...

            if (button.Value.GetType() == typeof(int))
            {
                var SubMenu = TopBarHelper.GetSubMenus();
                foreach (KeyValuePair<string, GUIDropDownButton> btn in SubMenu["PCF Samples"])
                {
                    if (btn.Value.Toggled)
                    {
                        btn.Value.Toggled = false; //only toggle off if on
                        break; //prevent more than whats needed
                    }
                }
                renderer.GFXSettings.ShadowPCFSamples = (button.Value as int? != null ? (int)button.Value : 1); //we try to safe cast as int nullable; if we fail we hard set to 1
                button.Toggled = true;
            }

            if (button.Value.GetType() == typeof(FogQuality))
            {
                FogQuality myType = (FogQuality)button.Value;


                var SubMenu = TopBarHelper.GetSubMenus();
                foreach (KeyValuePair<string, GUIDropDownButton> btn in SubMenu["Fog"])
                {
                    if (btn.Value.Toggled)
                    {
                        btn.Value.Toggled = false; //only toggle off if on
                        break; //prevent more than whats needed
                    }
                }

                switch (myType)
                {
                    case FogQuality.Off:
                        {
                            bool currentSetting = renderer.FogEnabled;
                            renderer.FogEnabled = !currentSetting;
                            button.Toggled = currentSetting; //dont need to invert the bool, all buttons are false by default due to the foreach loop
                            break;
                        }

                    case FogQuality.Low:
                        {
                            renderer.FogEnabled = true;
                            renderer.GFXSettings.FogQuality = FogQuality.Low;
                            button.Toggled = true;
                            break;
                        }

                    case FogQuality.Medium:
                        {
                            renderer.FogEnabled = true;
                            renderer.GFXSettings.FogQuality = FogQuality.Medium;
                            button.Toggled = true;
                            break;
                        }

                    case FogQuality.High:
                        {
                            renderer.FogEnabled = true;
                            renderer.GFXSettings.FogQuality = FogQuality.High;
                            button.Toggled = true;
                            break;
                        }
                }
            }

            if (button.Value.GetType() == typeof(gfxType))
            { //check for enum gfxType
                gfxType myType = (gfxType)button.Value; // /should/ be safe to case as gfxType
                switch (myType)
                {
                    case gfxType.fxaa:
                        {
                            bool currentSetting = renderer.GFXSettings.ApplyFXAA; //easier to read
                            renderer.GFXSettings.ApplyFXAA = !currentSetting;
                            button.Toggled = !currentSetting;
                            break;
                        }

                    case gfxType.shadows:
                        {
                            bool currentSetting = renderer.GFXSettings.RenderShadows; //easier to read
                            renderer.GFXSettings.RenderShadows = !currentSetting;
                            button.Toggled = !currentSetting;
                            break;
                        }

                    case gfxType.wireframe:
                        {
                            bool currentSetting = renderer.GlobalWireframe; //easier to read
                            renderer.GlobalWireframe = !currentSetting;
                            button.Toggled = !currentSetting;
                            break;
                        }
                }
            }
        }

        enum gfxType
        {
            fxaa,
            shadows,
            wireframe
        }

        /// <summary>
        /// Regenerate topBar for stuff and stuff
        /// </summary>
        /// <param name="rendWidth"></param>
        /// <param name="force">Force Regen?</param>
        public void GenBar(float rendWidth, bool force = false)
        {
            if (setup || force) //prevent recreating toolbars unless forced
                return;

            #region File Menu Buttons
            GUIDropDownButtonConfig[] FileMenuButtons = new GUIDropDownButtonConfig[4];
            FileMenuButtons[0] = new GUIDropDownButtonConfig() { text = "New", value = null, callback = (d, b) => { screen.LoadNewModel(); } };
            FileMenuButtons[1] = new GUIDropDownButtonConfig() { text = "Open", value = null, callback = (d, b) => { openFileWindow.Visible = true; } };
            FileMenuButtons[2] = new GUIDropDownButtonConfig() { text = "Save", value = null, callback = (d, b) => { if (screen.CurrentFile != null) screen.SaveModel(); else saveFileWindow.Visible = true; } };
            FileMenuButtons[3] = new GUIDropDownButtonConfig() { text = "Save As...", value = null, callback = (d, b) => { saveFileWindow.Visible = true; } };
            #endregion

            #region Gfx Menu Buttons
            GUIDropDownButtonConfig[] GfxMenuButtons = new GUIDropDownButtonConfig[3];
            GfxMenuButtons[0] = new GUIDropDownButtonConfig() { text = "FXAA", value = gfxType.fxaa, callback = SetGfxOption };
            GfxMenuButtons[1] = new GUIDropDownButtonConfig() { text = "Shadows", value = gfxType.shadows, callback = SetGfxOption };
            GfxMenuButtons[2] = new GUIDropDownButtonConfig() { text = "Wireframe", value = gfxType.wireframe, callback = SetGfxOption };

            GUIDropDownButtonConfig[] fogButtons = new GUIDropDownButtonConfig[4];
            fogButtons[0] = new GUIDropDownButtonConfig() { text = "Off", value = FogQuality.Off, callback = SetGfxOption };
            fogButtons[1] = new GUIDropDownButtonConfig() { text = "Low", value = FogQuality.Low, callback = SetGfxOption };
            fogButtons[2] = new GUIDropDownButtonConfig() { text = "Medium", value = FogQuality.Medium, callback = SetGfxOption };
            fogButtons[3] = new GUIDropDownButtonConfig() { text = "High", value = FogQuality.High, callback = SetGfxOption };

            GUIDropDownButtonConfig[] pcfButtons = new GUIDropDownButtonConfig[5];
            pcfButtons[0] = new GUIDropDownButtonConfig() { text = "1", value = 1, callback = SetGfxOption };
            pcfButtons[1] = new GUIDropDownButtonConfig() { text = "2", value = 2, callback = SetGfxOption };
            pcfButtons[2] = new GUIDropDownButtonConfig() { text = "4", value = 4, callback = SetGfxOption };
            pcfButtons[3] = new GUIDropDownButtonConfig() { text = "6", value = 6, callback = SetGfxOption };
            pcfButtons[4] = new GUIDropDownButtonConfig() { text = "12", value = 12, callback = SetGfxOption };
            #endregion

            #region Editor Menu Buttons
            GUIDropDownButtonConfig[] EditorButtons = new GUIDropDownButtonConfig[5];
            EditorButtons[0] = new GUIDropDownButtonConfig() { text = "Color Picker", value = typeof(GUIColorPickerWindow), callback = showWindowElement };
            EditorButtons[1] = new GUIDropDownButtonConfig() { text = "Eyedropper", value = null, callback = null };
            EditorButtons[2] = new GUIDropDownButtonConfig() { text = "Paint", value = null, callback = null };
            EditorButtons[3] = new GUIDropDownButtonConfig() { text = "Create Block", value = null, callback = null };
            EditorButtons[4] = new GUIDropDownButtonConfig() { text = "Delete Block", value = null, callback = null };
            #endregion

            #region Debug Menu Buttons
            GUIDropDownButtonConfig[] DebugMenuButtons = new GUIDropDownButtonConfig[2];
            DebugMenuButtons[0] = new GUIDropDownButtonConfig() { text = "Regen ToolBar", value = null, callback = (d, b) => { GenBar(renderer.ScreenWidth, true); } };
            DebugMenuButtons[1] = new GUIDropDownButtonConfig() { text = "show colour menu", value = typeof(GUIColorPickerWindow), callback = showWindowElement };
            #endregion

            GUIFrame bottomBar = new GUIFrame(new UDim2(0, 0, 1, -30), new UDim2(1, 0, 0, 30), theme);

            statusLeft = new GUILabel(UDim2.Zero, new UDim2(0.5f, 0, 1, 0), "<left status>", TextAlign.Left, theme) { Parent = bottomBar };
            statusRight = new GUILabel(new UDim2(0.5f, 0, 0, 0), new UDim2(0.5f, 0, 1, 0), "<right status>", TextAlign.Right, theme) { Parent = bottomBar };

            GUIFrame rightHandBar = new GUIFrame(new UDim2(.5f, -45, 0, 0), new UDim2(1, 0, 1, 0), theme);
            rightHandBar.MinSize = new UDim2(0, 90, .75f, 90);
            rightHandBar.MaxSize = rightHandBar.MinSize;

            GUIButton toAddRBar = new GUIButton(UDim2.Zero, new UDim2(1, 0, 1, 0), "Test", theme) { Parent = rightHandBar };

            ToolBarCreator genTop = new ToolBarCreator(theme);

            genTop.SetButtonWidth(4);

            genTop.Add("File", FileMenuButtons);
            genTop.Add("GFX", GfxMenuButtons,
                new SubDropdownConfig() { Title = "Fog", subButtons = fogButtons },
                new SubDropdownConfig() { Title = "PCF Samples", subButtons = pcfButtons }
            ); //<!-- gfx settings -->
            genTop.Add("Tools", EditorButtons);
            genTop.Add("Debug", DebugMenuButtons);

            this.TopBar = genTop.GetToolBar();

            TopBarHelper = new ToolBarHelper(this.TopBar);

            GUIColorPickerWindow ColorWindow = new GUIColorPickerWindow(renderer.Sprites.GUISystem, new UDim2(0.3f, 0, 0.3f, 0), theme);
            ColorWindow.MinSize = new UDim2(0, 400, 0, 300);
            ColorWindow.MaxSize = new UDim2(0, 550, 0, 400);


            SetupDefaultGraphicsSettings();

            windowElements.Add(ColorWindow);

            renderer.Sprites.GUISystem.Add(ColorWindow);
            area.AddTopLevel(TopBar, bottomBar, rightHandBar);

        }

        /// <summary>
        /// Show/Hide a window element
        /// </summary>
        /// <param name="dropDown"></param>
        /// <param name="button"></param>
        void showWindowElement(GUIDropDown dropDown, GUIDropDownButton button)
        {
            Type lookingFor = button.Value as Type;
            for (int i = 0; i < windowElements.Count; i++)
            {
                var item = windowElements[i];
                if (item != null && item.GetType() == lookingFor)
                {
                    item.Visible = !item.Visible;
                }
            }
        }

        /// <summary>
        /// Setup graphics settings buttons on dropdown to reflect the default
        /// </summary>
        void SetupDefaultGraphicsSettings()
        {
            setup = true;

            renderer.GFXSettings.ApplyFXAA = true;
            renderer.GFXSettings.RenderShadows = true;
            renderer.GFXSettings.FogQuality = FogQuality.Off;
            renderer.GFXSettings.ShadowPCFSamples = 1;

            var LevelOne = TopBarHelper.GetLevelOne();
            var SubMenu = TopBarHelper.GetSubMenus();

            LevelOne["GFX"]["FXAA"].Toggled = true;
            LevelOne["GFX"]["Shadows"].Toggled = true;
            SubMenu["Fog"]["Low"].Toggled = true;
            SubMenu["PCF Samples"]["1"].Toggled = true;
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
    }
}
