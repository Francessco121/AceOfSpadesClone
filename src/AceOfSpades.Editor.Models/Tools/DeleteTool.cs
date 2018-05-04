using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Editor.Models.Tools
{
    class DeleteTool : EditorTool
    {
        readonly EntityRenderer entRenderer;

        static DebugCube cursorCube;

        public DeleteTool(EditorScreen screen, ModelEditor editor)
            : base(screen, editor, EditorToolType.Delete, Key.Number2)
        {
            entRenderer = Renderer.GetRenderer3D<EntityRenderer>();

            if (cursorCube == null)
            {
                cursorCube = new DebugCube(Color4.Black, 1f);
                cursorCube.RenderAsWireframe = true;
                cursorCube.ApplyNoLighting = true;
                cursorCube.OnlyRenderFor = RenderPass.Normal;
            }
        }

        public override void Update(VoxelObjectRaycastResult intersection, float deltaTime)
        {
            if (GUISystem.HandledMouseInput)
                return;

            if (Screen.Model.BlockCount <= 1)
                return;

            if (intersection.Intersects && Input.GetMouseButtonDown(MouseButton.Left))
            {
                IndexPosition blockIndexPosition = intersection.IntersectedBlockIndex.Value;

                Screen.Model.ChangeBlock(blockIndexPosition, new Block(Block.AIR.Material));
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

                entRenderer.Batch(cursorCube);
            }
        }
    }
}
