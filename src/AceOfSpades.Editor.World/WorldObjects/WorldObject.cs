using AceOfSpades.Graphics;
using Dash.Engine;

namespace AceOfSpades.Editor.World.WorldObjects
{
    public abstract class WorldObject : EditorObject
    {
        public string Tag;
        public IconRenderer Icon { get; }

        public WorldObject(Vector3 position) 
            : base(position)
        {
            Icon = new IconRenderer();
            AddComponent(Icon);
        }
    }
}
