using AceOfSpades.Editor.Models.Tools;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;

namespace AceOfSpades.Editor.Models
{
    class ModelEditor
    {
        readonly EditorScreen screen;
        readonly Dictionary<EditorToolType, EditorTool> tools;

        EditorTool selectedTool;
        VoxelObjectRaycastResult raycastResult;

        public ModelEditor(EditorScreen screen)
        {
            this.screen = screen;

            tools = new Dictionary<EditorToolType, EditorTool>();

            AddTool(new NoneTool(screen, this));
            AddTool(new AddTool(screen, this));

            SetToolType(EditorToolType.None);
        }

        public void SetToolType(EditorToolType type)
        {
            EditorTool tool;
            if (tools.TryGetValue(type, out tool))
                EquipTool(tool);
            else
                throw new ArgumentException($"No such tool of type '{type}' is defined!");
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

            raycastResult = screen.Model.Raycast(Camera.Active.MouseRay, screen.Model.CubeSize);

            if (selectedTool != null)
            {
                selectedTool.Update(raycastResult, deltaTime);
            }
        }

        public void Draw()
        {
            if (selectedTool != null)
                selectedTool.Draw(raycastResult);
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
    }
}
