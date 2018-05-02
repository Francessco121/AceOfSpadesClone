using AceOfSpades.Editor.World.Tools;
using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;

/* EditorMode.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World
{
    public class TerrainEditor
    {
        public Color BlockColor
        {
            get { return colorPicker.Color; }
            set { colorPicker.SetColor(value); }
        }

        public EditorSelectionBox SelectionBox { get; private set; }
        public bool IsSelecting { get; private set; }

        WorldEditor editor { get; }
        FixedTerrain terrain { get { return screen.World.Terrain; } }
        TerrainPhysicsExtension terrainPhys;
        EditorScreen screen;
        MasterRenderer renderer;
        EntityRenderer entRenderer;

        DebugCube blockCursorCube;
        TerrainRaycastResult rayIntersection;

        GUIColorPicker colorPicker;

        Stack<TerrainOperationBatch> undoStack;
        Stack<TerrainOperationBatch> redoStack;
        TerrainOperationBatch operationBatch;
        TerrainOperationBatch undoBatch;
        TerrainOperationBatch redoBatch;

        public TerrainEditor(EditorScreen screen)
        {
            this.screen = screen;
            editor = screen.WorldEditor;
            renderer = screen.Window.Renderer;
            terrainPhys = new TerrainPhysicsExtension();
            entRenderer = renderer.GetRenderer3D<EntityRenderer>();
            colorPicker = screen.UI.ColorWindow.ColorPicker;

            blockCursorCube = new DebugCube(Color4.White, Block.CUBE_SIZE);
            SelectionBox = new EditorSelectionBox();

            undoStack = new Stack<TerrainOperationBatch>();
            redoStack = new Stack<TerrainOperationBatch>();
            operationBatch = new TerrainOperationBatch();

            rayIntersection = new TerrainRaycastResult(new Ray(Vector3.Zero, Vector3.UnitZ));
        }

        public void ClearUndoRedo()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public void SetBlock(Chunk chunk, Block block, IndexPosition blockPos)
        {
            if (chunk == null || !chunk.IsBlockCoordInRange(blockPos))
                return;

            operationBatch.Add(new TerrainOperation(chunk, block, blockPos));
        }

        public void BakeDamageColors()
        {
            if (terrain == null)
                return;

            foreach (Chunk chunk in terrain.Chunks.Values)
            {
                for (int x = 0; x < Chunk.HSIZE; x++)
                    for (int y = 0; y < Chunk.VSIZE; y++)
                        for (int z = 0; z < Chunk.HSIZE; z++)
                        {
                            Block block = chunk.Blocks[z, y, x];
                            Color color = block.GetColor4().ToColor();

                            chunk.Blocks[z, y, x] = new Block(block.Material, color.R, color.G, color.B);
                        }
            }
        }

        public void Update(float deltaTime, TerrainEditorTool selectedTool, TerrainRaycastResult rayIntersection)
        {
            terrainPhys.Terrain = terrain;

            if (terrain != null)
            {
                if (operationBatch.Count > 0)
                {
                    ApplyOperationBatch();
                    screen.UI.SetMidStatus("");
                }
                else if (undoBatch != null)
                {
                    redoStack.Push(undoBatch.GenerateUndo());
                    undoBatch.Apply();
                    undoBatch = null;
                    screen.UI.SetMidStatus("");
                }
                else if (redoBatch != null)
                {
                    undoStack.Push(redoBatch.GenerateUndo());
                    redoBatch.Apply();
                    redoBatch = null;
                    screen.UI.SetMidStatus("");
                }

                // Update selected tool
                if (selectedTool != null)
                {
                    // Process global intersection handling
                    if (rayIntersection.Intersects && !GUISystem.HandledMouseInput)
                    {
                        if (Input.GetMouseButton(MouseButton.Middle))
                            // Pick color
                            BlockColor = rayIntersection.Block.Value.GetColor();
                        else if (Input.GetMouseButtonDown(MouseButton.Left))
                        {
                            // Begin select
                            if (selectedTool.AllowUserSelecting())
                            {
                                SelectionBox.SetPrimary(selectedTool.GetRayIntersectionIndex(rayIntersection));
                                IsSelecting = true;
                            }
                        }
                        else if (Input.GetMouseButtonUp(MouseButton.Left))
                            IsSelecting = false;
                        else if (IsSelecting)
                        {
                            // Update selection box
                            bool ctrl = Input.IsControlHeld;
                            bool alt = Input.IsAltHeld;

                            if (ctrl && alt)
                            {
                                SelectionBox.SetSecondary(selectedTool.GetRayIntersectionIndex(rayIntersection));
                            }
                            else if (ctrl)
                            {
                                IndexPosition sp = selectedTool.GetRayIntersectionIndex(rayIntersection);
                                IndexPosition delta = new IndexPosition(
                                    Math.Abs(sp.X - SelectionBox.Primary.X),
                                    Math.Abs(sp.Y - SelectionBox.Primary.Y),
                                    Math.Abs(sp.Z - SelectionBox.Primary.Z));

                                IndexPosition fSp;

                                if (delta.X >= delta.Y && delta.X >= delta.Z)
                                {
                                    // X normal
                                    fSp = new IndexPosition(sp.X, SelectionBox.Primary.Y, SelectionBox.Primary.Z);
                                }
                                else if (delta.Y >= delta.X && delta.Y >= delta.Z)
                                {
                                    // Y normal
                                    fSp = new IndexPosition(SelectionBox.Primary.X, sp.Y, SelectionBox.Primary.Z);
                                }
                                else
                                {
                                    // Z normal
                                    fSp = new IndexPosition(SelectionBox.Primary.X, SelectionBox.Primary.Y, sp.Z);
                                }

                                SelectionBox.SetSecondary(fSp);
                            }
                            else
                                SelectionBox.SetSecondary(SelectionBox.Primary);
                        }
                    }
                }

                // Preform undo/redo
                if (Input.IsControlHeld)
                {
                    if (Input.GetKeyDown(Key.Z))
                    {
                        if (undoStack.Count > 0)
                            undoBatch = undoStack.Pop();
                    }
                    else if (Input.GetKeyDown(Key.Y))
                    {
                        if (redoStack.Count > 0)
                            redoBatch = redoStack.Pop();
                    }
                }

                // Show 'apply dialog' before application freezes from operation
                if (operationBatch.Count > 0 || undoBatch != null || redoBatch != null)
                    screen.UI.SetMidStatus("Applying Operation...");
            }
        }

        void ApplyOperationBatch()
        {
            redoStack.Clear();
            undoStack.Push(operationBatch.GenerateUndo());
            operationBatch.Apply();
            operationBatch.Clear();
        }
    }
}
