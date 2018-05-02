using Dash.Engine;
using Dash.Engine.Graphics;

namespace AceOfSpades.Editor.Models
{
    abstract class EditorTool
    {
        public EditorToolType Type { get; }
        public Key KeyBind { get; }

        protected MasterRenderer Renderer { get; }
        protected ModelEditor Editor { get; }
        protected EditorUI UI { get; }
        protected EditorScreen Screen { get; }

        public EditorTool(EditorScreen screen, ModelEditor editor,
            EditorToolType type, Key keyBind)
        {
            Renderer = screen.Window.Renderer;
            Editor = editor;
            UI = screen.UI;
            Screen = screen;
            Type = type;
            KeyBind = keyBind;
        }

        public virtual void Equipped() { }
        public virtual void Unequipped() { }
        public abstract void Update(VoxelObjectRaycastResult intersection, float deltaTime);
        public abstract void Draw(VoxelObjectRaycastResult intersection);
    }
}
