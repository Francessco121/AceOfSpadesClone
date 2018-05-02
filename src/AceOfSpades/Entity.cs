using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;

/* Entity.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public abstract class Entity
    {
        public virtual Vector3 Position { get; set; }
        public virtual Vector3 MeshScale { get; set; } = new Vector3(1, 1, 1);
        public virtual Vector3 MeshRotation { get; set; }

        public virtual VoxelObject VoxelObject { get; protected set; }

        public bool ApplyNoLighting;
        public bool RenderAsWireframe;
        public bool RenderFront;
        public Color ColorOverlay = Color.White;
        public RenderPass? OnlyRenderFor;

        public Entity() { }

        public Entity(Vector3 position)
        {
            Position = position;
        }

        public Entity(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Position = position;
            MeshRotation = rotation;
            MeshScale = scale;
        }

        public virtual void Update(float deltaTime) { }
    }
}
