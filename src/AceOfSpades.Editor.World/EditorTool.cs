using Dash.Engine;
using Dash.Engine.Graphics;

namespace AceOfSpades.Editor.World
{
    public abstract class EditorTool
    {
        public EditorToolType Type { get; }
        public Key KeyBind { get; }

        protected MasterRenderer Renderer { get; }
        protected WorldEditor Editor { get; }
        protected EditorUI UI { get; }
        protected EditorScreen Screen { get; }

        public EditorTool(EditorScreen screen, WorldEditor editor,
            EditorToolType type, Key keyBind)
        {
            Renderer = screen.Window.Renderer;
            Editor = editor;
            UI = screen.UI;
            Screen = screen;
            Type = type;
            KeyBind = keyBind;
        }

        public virtual bool AllowUserSelecting() { return true; }

        public virtual void Equipped() { }
        public virtual void Unequipped() { }
        public abstract void Update(EditorWorldRaycastResult intersection, float deltaTime);
        public abstract void Draw(EditorWorldRaycastResult intersection);
    }
}
