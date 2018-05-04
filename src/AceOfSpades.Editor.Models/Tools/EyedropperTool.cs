using Dash.Engine;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Editor.Models.Tools
{
    class EyedropperTool : EditorTool
    {
        public EyedropperTool(EditorScreen screen, ModelEditor editor)
            : base(screen, editor, EditorToolType.Eyedropper, Key.Number4)
        { }

        public override void Update(VoxelObjectRaycastResult intersection, float deltaTime)
        {
            if (GUISystem.HandledMouseInput)
                return;

            if (intersection.Intersects && Input.GetMouseButtonDown(MouseButton.Left))
            {
                IndexPosition blockIndexPosition = intersection.IntersectedBlockIndex.Value;

                Block block = Screen.Model.Blocks[blockIndexPosition.Z, blockIndexPosition.Y, blockIndexPosition.X];

                UI.ColorWindow.ColorPicker.SetColor(block.GetColor());

                Editor.SetToolType(EditorToolType.Paint);
                UI.SetToolType(EditorToolType.Paint);
            }
        }

        public override void Draw(VoxelObjectRaycastResult intersection) { }
    }
}
