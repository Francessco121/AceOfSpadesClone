using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

/* AddTool.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World.Tools
{
    public class AddTool : TerrainEditorTool
    {
        CubeSide lastNonSelectingNormal;

        public AddTool(EditorScreen screen, WorldEditor editor) 
            : base(screen, editor, EditorToolType.Add, Key.Number2)
        { }

        public override IndexPosition GetRayIntersectionIndex(TerrainRaycastResult rayIntersection)
        {
            IndexPosition pos = base.GetRayIntersectionIndex(rayIntersection);
            if (!TerrainEditor.IsSelecting)
                pos += new IndexPosition(Maths.CubeSideToSurfaceNormal(rayIntersection.IntersectionCubeSide.Value));
            else
                pos += new IndexPosition(Maths.CubeSideToSurfaceNormal(lastNonSelectingNormal));

            return pos;
        }

        public override void Update(EditorWorldRaycastResult intersection, float deltaTime)
        {
            if (intersection.HitTerrain)
            {
                TerrainRaycastResult terrainIntersection = intersection.TerrainResult;

                if (!TerrainEditor.IsSelecting)
                    lastNonSelectingNormal = terrainIntersection.IntersectionCubeSide.Value;

                if (TerrainEditor.IsSelecting && Input.GetMouseButtonUp(MouseButton.Left))
                {
                    ApplyActionToSelection(TerrainEditor.SelectionBox,
                        (chunk, blockPos) =>
                        {
                            Block block = Block.CUSTOM;
                            Color color = TerrainEditor.BlockColor;
                            block.R = color.R;
                            block.G = color.G;
                            block.B = color.B;

                            TerrainEditor.SetBlock(chunk, block, blockPos);
                        });
                }
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
                        * Block.CUBE_3D_SIZE
                        + Maths.CubeSideToSurfaceNormal(terrainIntersection.IntersectionCubeSide.Value) * Block.CUBE_3D_SIZE;

                Vector3 scale = TerrainEditor.IsSelecting
                    ? TerrainEditor.SelectionBox.Size().ToVector3() + Vector3.UnitScale
                    : Vector3.UnitScale;

                cursorCube.Position = blockCoords;
                cursorCube.VoxelObject.MeshScale = scale;
                cursorCube.RenderAsWireframe = false;
                Color color = TerrainEditor.BlockColor;
                color.A = 128;
                cursorCube.ColorOverlay = color;

                entRenderer.Batch(cursorCube);
            }
        }
    }
}
