using AceOfSpades.Graphics.Renderers;
using Dash.Engine;
using Dash.Engine.Graphics;

/* VoxelRenderComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Graphics
{
    public class VoxelRenderComponent : Component
    {
        public VoxelObject VoxelObject;
        public Matrix4 WorldMatrix = Matrix4.Identity;

        public bool RenderFront;
        public bool RenderAsWireframe;
        public bool ApplyNoLighting;
        public Color ColorOverlay = Color.White;
        public RenderPass? OnlyRenderFor;
        public float Lighting = 1f;

        VoxelRenderer renderer;

        public VoxelRenderComponent()
        {
            renderer = MasterRenderer.Instance.GetRenderer3D<VoxelRenderer>();
            IsDrawable = true;
        }

        protected override void Draw()
        {
            if (VoxelObject != null)
                renderer.Batch(this);
        }
    }
}
