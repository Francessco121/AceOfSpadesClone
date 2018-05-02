using Dash.Engine;

namespace AceOfSpades.Editor.Models.Tools
{
    class NoneTool : EditorTool
    {
        public NoneTool(EditorScreen screen, ModelEditor editor) 
            : base(screen, editor, EditorToolType.None, Key.Number1)
        { }

        public override void Draw(VoxelObjectRaycastResult intersection) { }

        public override void Update(VoxelObjectRaycastResult intersection, float deltaTime) { }
    }
}
