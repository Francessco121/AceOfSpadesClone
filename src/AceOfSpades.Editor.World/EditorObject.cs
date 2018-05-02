using AceOfSpades.Graphics;
using AceOfSpades.IO;
using Dash.Engine;
using Dash.Engine.Graphics;

namespace AceOfSpades.Editor.World
{
    public abstract class EditorObject : GameObject
    {
        public Color Color
        {
            get { return renderComponent.ColorOverlay; }
            set { renderComponent.ColorOverlay = value; }
        }

        public string EditorName;

        VoxelRenderComponent renderComponent;
        Vector3 size;
        Vector3 halfCubeSize;

        public EditorObject(Vector3 position)
            : base(position)
        {
            renderComponent = new VoxelRenderComponent();
            AddComponent(renderComponent);

            EditorName = GetType().Name;
        }

        public virtual WorldObjectDescription CreateIODescription()
        {
            WorldObjectDescription desc = new WorldObjectDescription();
            desc.Tag = "GameObject";
            desc.AddField("Position", Transform.Position);

            return desc;
        }

        protected void SetVoxelObject(VoxelObject vo)
        {
            renderComponent.VoxelObject = vo;
            size = vo.UnitSize;
            halfCubeSize = new Vector3(vo.CubeSize / 2f);
        }

        public AxisAlignedBoundingBox GetCollider()
        {
            return new AxisAlignedBoundingBox(Transform.Position - halfCubeSize, Transform.Position + size - halfCubeSize);
        }

        protected override void Draw()
        {
            renderComponent.WorldMatrix = Transform.Matrix;
            base.Draw();
        }
    }
}
