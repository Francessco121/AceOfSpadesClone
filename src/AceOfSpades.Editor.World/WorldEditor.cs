using AceOfSpades.Editor.World.Tools;
using Dash.Engine;
using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;

namespace AceOfSpades.Editor.World
{
    public class WorldEditor
    {
        public TerrainEditor TerrainEditor { get; }

        EditorScreen screen;

        Dictionary<EditorToolType, EditorTool> tools;
        EditorTool selectedTool;

        EditorWorldRaycastResult raycastResult;

        public WorldEditor(EditorScreen screen)
        {
            this.screen = screen;
            TerrainEditor = new TerrainEditor(screen);
            tools = new Dictionary<EditorToolType, EditorTool>();

            AddTool(new SelectTool(screen, this));
            AddTool(new AddTool(screen, this));
            AddTool(new RemoveTool(screen, this));
            AddTool(new PaintTool(screen, this));
            AddTool(new TerrainMoveTool(screen, this));
            AddTool(new TerraformTool(screen, this));

            SetToolType(EditorToolType.Select);
        }

        public void SetToolType(EditorToolType type)
        {
            EditorTool tool;
            if (tools.TryGetValue(type, out tool))
                EquipTool(tool);
            else
                throw new ArgumentException("No such terrain tool of type '" + type.ToString() + "' is defined!");
        }

        void AddTool(EditorTool tool)
        {
            tools.Add(tool.Type, tool);
        }

        void EquipTool(EditorTool tool)
        {
            UnequipTool();

            selectedTool = tool;
            selectedTool.Equipped();
        }

        void UnequipTool()
        {
            if (selectedTool != null)
                selectedTool.Unequipped();

            selectedTool = null;
        }

        public void Update(float deltaTime)
        {
            // Try equip tool
            if (!GUISystem.HandledMouseInput)
            {
                if (Input.GetKeyDown(Key.Tilde))
                {
                    if (selectedTool != null)
                        selectedTool.Unequipped();

                    selectedTool = null;
                }
                else
                {
                    foreach (EditorTool tool in tools.Values)
                    {
                        if (Input.GetKeyDown(tool.KeyBind))
                        {
                            EquipTool(tool);
                            screen.UI.SetToolType(tool.Type);
                            break;
                        }
                    }
                }
            }

            raycastResult = screen.World.Raycast();

            if (selectedTool != null)
            {
                selectedTool.Update(raycastResult, deltaTime);

                TerrainEditorTool terrainTool = selectedTool as TerrainEditorTool;
                if (terrainTool != null)
                    TerrainEditor.Update(deltaTime, terrainTool, raycastResult.TerrainResult);
            }
        }

        public void Draw()
        {
            if (selectedTool != null)
                selectedTool.Draw(raycastResult);
        }
    }
}
