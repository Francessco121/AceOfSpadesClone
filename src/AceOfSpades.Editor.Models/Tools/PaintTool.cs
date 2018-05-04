using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Editor.Models.Tools
{
    class PaintTool : EditorTool
    {
        readonly EntityRenderer entRenderer;

        static DebugCube cursorCube;

        public PaintTool(EditorScreen screen, ModelEditor editor) 
            : base(screen, editor, EditorToolType.Paint, Key.Number3)
        {
            entRenderer = Renderer.GetRenderer3D<EntityRenderer>();

            if (cursorCube == null)
            {
                cursorCube = new DebugCube(Color4.White, 1f);
                cursorCube.RenderAsWireframe = true;
                cursorCube.ApplyNoLighting = true;
                cursorCube.OnlyRenderFor = RenderPass.Normal;
            }
        }

        public override void Update(VoxelObjectRaycastResult intersection, float deltaTime)
        {
            if (GUISystem.HandledMouseInput)
                return;

            if (intersection.Intersects && Input.GetMouseButton(MouseButton.Left))
            {
                IndexPosition blockIndexPosition = intersection.IntersectedBlockIndex.Value;

                Color color = UI.ColorWindow.ColorPicker.Color;

                Block existingBlock = Screen.Model.Blocks[blockIndexPosition.Z, blockIndexPosition.Y, blockIndexPosition.X];

                Block newBlock = new Block(Block.STONE.Material, color.R, color.G, color.B);

                // Don't constantly recreate the mesh if we're not actually changing this block
                if (existingBlock.Material != newBlock.Material || existingBlock.R != newBlock.R 
                    || existingBlock.G != newBlock.G || existingBlock.B != newBlock.B)
                {
                    Screen.Model.ChangeBlock(blockIndexPosition, new Block(Block.STONE.Material,
                        color.R, color.G, color.B));
                }
            }
        }

        public override void Draw(VoxelObjectRaycastResult intersection)
        {
            if (GUISystem.HandledMouseOver)
                return;

            if (intersection.Intersects)
            {
                cursorCube.Position = intersection.IntersectedBlockIndex.Value * Screen.Model.CubeSize;

                cursorCube.VoxelObject.MeshScale = new Vector3(Screen.Model.CubeSize);

                Color color = Screen.UI.ColorWindow.ColorPicker.Color;

                // TODO: We can't make the cube transparent, because we have no guarantee that it will render after the
                // editor model. If the cube renders first, it will only blend with the skybox.
                //color.A = 128;

                cursorCube.ColorOverlay = color;

                entRenderer.Batch(cursorCube);
            }
        }
    }
}
