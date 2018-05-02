using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

/* RemoveTool.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World.Tools
{
    public class RemoveTool : TerrainEditorTool
    {
        public RemoveTool(EditorScreen screen, WorldEditor editor) 
            : base(screen, editor, EditorToolType.Delete, Key.Number3)
        { }

        public override void Update(EditorWorldRaycastResult intersection, float deltaTime)
        {
            if (intersection.HitTerrain && TerrainEditor.IsSelecting && Input.GetMouseButtonUp(MouseButton.Left))
            {
                ApplyActionToSelection(TerrainEditor.SelectionBox,
                    (chunk, blockPos) =>
                    {
                        TerrainEditor.SetBlock(chunk, Block.AIR, blockPos);
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
