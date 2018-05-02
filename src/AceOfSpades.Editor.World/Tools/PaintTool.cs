using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

/* PaintTool.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World.Tools
{
    public class PaintTool : TerrainEditorTool
    {
        int grainFactor { get { return window.Grain; } }

        PaintWindow window;

        public PaintTool(EditorScreen screen, WorldEditor editor)
            : base(screen, editor, EditorToolType.Paint, Key.Number4)
        {
            window = new PaintWindow(UI.GUISystem, UI.Theme);
            window.Visible = false;
            UI.GUISystem.Add(window);
        }

        public override void Equipped()
        {
            window.Visible = true;
            base.Equipped();
        }

        public override void Unequipped()
        {
            window.Visible = false;
            base.Unequipped();
        }

        public override void Update(EditorWorldRaycastResult intersection, float deltaTime)
        {
            if (intersection.HitTerrain && TerrainEditor.IsSelecting && Input.GetMouseButtonUp(MouseButton.Left))
            {
                ApplyActionToSelection(TerrainEditor.SelectionBox,
                    (chunk, blockPos) =>
                    {
                        if (chunk.GetBlockSafe(blockPos.X, blockPos.Y, blockPos.Z) != Block.AIR)
                        {
                            Block block = Block.CUSTOM;
                            Color color = TerrainEditor.BlockColor;
                            int grainRadius = grainFactor;
                            block.R = (byte)MathHelper.ClampToByteRange(color.R + Maths.Random.Next(-grainRadius, grainRadius));
                            block.G = (byte)MathHelper.ClampToByteRange(color.G + Maths.Random.Next(-grainRadius, grainRadius));
                            block.B = (byte)MathHelper.ClampToByteRange(color.B + Maths.Random.Next(-grainRadius, grainRadius));

                            TerrainEditor.SetBlock(chunk, block, blockPos);
                        }
                    });
            }
        }

        public override void Draw(EditorWorldRaycastResult intersection)
        {
            if (intersection.HitTerrain && !GUISystem.HandledMouseOver)
            {
                TerrainRaycastResult terrainIntersection = intersection.TerrainResult;

                Vector3 blockCoords = TerrainEditor.IsSelecting
                   ? TerrainEditor.SelectionBox.Center()
                   : GetGlobalBlockCoords(terrainIntersection.Chunk.IndexPosition, terrainIntersection.BlockIndex.Value) 
                        * Block.CUBE_3D_SIZE;

                Vector3 scale = TerrainEditor.IsSelecting
                    ? TerrainEditor.SelectionBox.Size().ToVector3() + Vector3.UnitScale
                    : Vector3.UnitScale;

                cursorCube.Position = blockCoords;
                cursorCube.VoxelObject.MeshScale = scale + new Vector3(0.01f, 0.01f, 0.01f);
                cursorCube.RenderAsWireframe = true;
                cursorCube.ColorOverlay = Color.Black;

                entRenderer.Batch(cursorCube);
            }
        }
    }
}
