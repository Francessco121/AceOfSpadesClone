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

            AddTool(new AddTool(screen, this));
            AddTool(new DeleteTool(screen, this));
            AddTool(new PaintTool(screen, this));
            AddTool(new EyedropperTool(screen, this));
        }

        public void SetToolType(EditorToolType? type)
        {
            if (type.HasValue)
            {
                EditorTool tool;
                if (tools.TryGetValue(type.Value, out tool))
                    EquipTool(tool);
                else
                    throw new ArgumentException($"No such tool of type '{type}' is defined!");
            }
            else
            {
                UnequipTool();
            }
        }

        public void Update(float deltaTime)
        {
            // Update mouse raycast
            raycastResult = screen.Model.Raycast(Camera.Active.MouseRay, screen.Model.CubeSize);

            // Ignore input if the UI is already handling input
            if (!GUISystem.HandledMouseInput)
            {
                // Try equip tool
                if (Input.GetKeyDown(Key.Tilde))
                {
                    UnequipTool();
                    screen.UI.SetToolType(null);
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

                // Center camera on middle mouse
                if (raycastResult.Intersects && Input.GetMouseButtonDown(MouseButton.Middle))
                {
                    Camera.Active.SetTarget(raycastResult.IntersectedBlockIndex.Value * screen.Model.CubeSize);
                }
            }

            // Update currently selected tool
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
