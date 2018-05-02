using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Physics;

/* VoxelTranslationHandles.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor
{
    public class VoxelTranslationHandles
    {
        public bool HasHold { get; private set; }
        public Axis Holding { get; private set; }

        VoxelTranslationHandle xAxisVo, yAxisVo, zAxisVo;
        Vector3 xAxisPos, yAxisPos, zAxisPos;
        float cubeSize = 2f;

        Vector3 startPos;

        EntityRenderer entRenderer;
        DebugRenderer debugRenderer;

        Vector3 lastCamPos;

        public VoxelTranslationHandles(MasterRenderer renderer)
        {
            xAxisVo = new VoxelTranslationHandle(6, cubeSize, Color.Blue);
            xAxisVo.MeshRotation = new Vector3(0, 0, -90);
            yAxisVo = new VoxelTranslationHandle(6, cubeSize, Color.Red);
            zAxisVo = new VoxelTranslationHandle(6, cubeSize, Color.Green);
            zAxisVo.MeshRotation = new Vector3(90, 0, 0);

            entRenderer = renderer.GetRenderer3D<EntityRenderer>();
            debugRenderer = renderer.GetRenderer3D<DebugRenderer>();
        }

        public void BaseCubeSizeChanged(float size)
        {
            cubeSize = 2f * (size / 6f);

            xAxisVo.ChangeCubeSize(cubeSize);
            yAxisVo.ChangeCubeSize(cubeSize);
            zAxisVo.ChangeCubeSize(cubeSize);
        }

        public void TryGrab(Camera camera)
        {
            float? xDist, yDist, zDist;
            camera.MouseRay.Intersects(GetXAABB(), out xDist);
            camera.MouseRay.Intersects(GetYAABB(), out yDist);
            camera.MouseRay.Intersects(GetZAABB(), out zDist);

            if (!xDist.HasValue) xDist = float.MaxValue;
            if (!yDist.HasValue) yDist = float.MaxValue;
            if (!zDist.HasValue) zDist = float.MaxValue;

            if (xDist.HasValue && xDist.Value < yDist.Value && xDist.Value < zDist.Value)
            {
                HasHold = true;
                Holding = Axis.X;
            }
            else if (yDist.HasValue && yDist.Value < xDist.Value && yDist.Value < zDist.Value)
            {
                HasHold = true;
                Holding = Axis.Y;
            }
            else if (zDist.HasValue && zDist.Value < xDist.Value && zDist.Value < yDist.Value)
            {
                HasHold = true;
                Holding = Axis.Z;
            }

            if (HasHold)
            {
                startPos = camera.MouseRay.Direction * 2;
                lastCamPos = camera.Position;
            }
        }

        public void ResetStartPos(Camera camera)
        {
            startPos = camera.MouseRay.Direction * 2;
        }
        
        public void LetGo()
        {
            HasHold = false;
        }

        void DebugRenderHandleAABBs()
        {
            //if (HasHold)
            {
               // if (Holding == Axis.X)
                    debugRenderer.Batch(GetXAABB());
                //if (Holding == Axis.Y)
                    debugRenderer.Batch(GetYAABB());
               // if (Holding == Axis.Z)
                    debugRenderer.Batch(GetZAABB());
            }
        }

        public bool Update(VoxelEditorObject eo, Camera camera)
        {
            if (!HasHold) return false;

            Vector3 newPos = camera.GetMouse3DPosition();
            Vector3 camDelta = camera.Position - lastCamPos;
            Vector3 delta = (newPos - startPos) * 8 * eo.CubeSize;
            delta -= camDelta;
            IndexPosition iDelta = new IndexPosition(
                (int)(delta.X / eo.CubeSize),
                (int)(delta.Y / eo.CubeSize),
                (int)(delta.Z / eo.CubeSize));

            lastCamPos = camera.Position;

            if (iDelta != IndexPosition.Zero)
            {
                if (Holding == Axis.X) eo.Translate(new IndexPosition(iDelta.X, 0, 0));
                if (Holding == Axis.Y) eo.Translate(new IndexPosition(0, iDelta.Y, 0));
                if (Holding == Axis.Z) eo.Translate(new IndexPosition(0, 0, iDelta.Z));

                startPos = newPos;
                return true;
            }

            return false;
        }

        public IndexPosition Update(float cubeSize, Camera camera)
        {
            if (!HasHold) return IndexPosition.Zero;

            Vector3 newPos = camera.MouseRay.Direction * 2;
            Vector3 delta = (newPos - startPos) * 8 * cubeSize;
            IndexPosition iDelta = new IndexPosition(
                (int)(delta.X / cubeSize),
                (int)(delta.Y / cubeSize),
                (int)(delta.Z / cubeSize));

            if (iDelta != IndexPosition.Zero)
            {
                if (Holding == Axis.X) { iDelta.Y = iDelta.Z = 0; }
                if (Holding == Axis.Y) { iDelta.X = iDelta.Z = 0; }
                if (Holding == Axis.Z) { iDelta.Y = iDelta.X = 0; }

                startPos = newPos;
                return iDelta;
            }

            return iDelta;
        }

        public Vector3 Update(Camera camera, float sensitivity = 8)
        {
            if (!HasHold) return Vector3.Zero;

            Vector3 newPos = camera.MouseRay.Direction * 2;
            Vector3 delta = (newPos - startPos) * sensitivity;
            startPos = newPos;

            if (Holding == Axis.X) { delta.Y = delta.Z = 0; }
            if (Holding == Axis.Y) { delta.X = delta.Z = 0; }
            if (Holding == Axis.Z) { delta.Y = delta.X = 0; }

            return delta;
        }

        public AxisAlignedBoundingBox GetXAABB()
        {
            float scale = GetXScale() / 2f;
            float cubeSize = this.cubeSize * scale;
            Vector3 off = new Vector3(-cubeSize * 0.5f, -cubeSize * 5f, -cubeSize * 0.5f);

            Vector3 min = -scale + off;
            Vector3 max = new Vector3(this.cubeSize * 12, this.cubeSize * 6, this.cubeSize * 6) * scale + off;

            return new AxisAlignedBoundingBox(min + xAxisPos, max + xAxisPos);
        }

        public AxisAlignedBoundingBox GetYAABB()
        {
            float scale = GetYScale() / 2f;
            float cubeSize = this.cubeSize * scale;
            Vector3 off = new Vector3(-cubeSize * 0.5f, -cubeSize * 0.5f, -cubeSize * 0.5f);

            Vector3 min = -scale + off;
            Vector3 max = new Vector3(this.cubeSize * 6, this.cubeSize * 12, this.cubeSize * 6) * scale + off;

            return new AxisAlignedBoundingBox(min + yAxisPos, max + yAxisPos);
        }

        public AxisAlignedBoundingBox GetZAABB()
        {
            float scale = GetZScale() / 2f;
            float cubeSize = this.cubeSize * scale;
            Vector3 off = new Vector3(-cubeSize * 0.5f, -cubeSize * 5f, -cubeSize * 0.5f);

            Vector3 min = -scale + off;
            Vector3 max = new Vector3(this.cubeSize * 6, this.cubeSize * 6, this.cubeSize *12) * scale + off;

            return new AxisAlignedBoundingBox(min + zAxisPos, max + zAxisPos);
        }

        public float GetXScale()
        {
            return (Camera.Active.Position - xAxisPos).Length * 0.005f;
        }

        public float GetYScale()
        {
            return (Camera.Active.Position - yAxisPos).Length * 0.005f;
        }

        public float GetZScale()
        {
            return (Camera.Active.Position - zAxisPos).Length * 0.005f;
        }

        public void PositionToEditorObject(VoxelEditorObject eo)
        {
            Vector3 eo3DCubeSize = new Vector3(eo.CubeSize);
            xAxisPos = new IndexPosition(eo.Max.X + 1, eo.Min.Y, eo.Min.Z) * eo3DCubeSize + new Vector3(-cubeSize, 0, -cubeSize * 2);
            yAxisPos = new IndexPosition(eo.Min.X, eo.Max.Y + 1, eo.Min.Z) * eo3DCubeSize + new Vector3(-cubeSize * 2, -cubeSize, -cubeSize * 2);
            zAxisPos = new IndexPosition(eo.Min.X, eo.Min.Y, eo.Max.Z + 1) * eo3DCubeSize + new Vector3(-cubeSize * 2, 0, -cubeSize);
        }

        public void PositionToMinMax(IndexPosition min, IndexPosition max, float cubeSize, Vector3 offset)
        {
            Vector3 eo3DCubeSize = new Vector3(cubeSize);
            xAxisPos = new IndexPosition(max.X + 1, min.Y, min.Z) * eo3DCubeSize
                + new Vector3(-cubeSize, 0, -cubeSize * 2) + offset;
            yAxisPos = new IndexPosition(min.X, max.Y + 1, min.Z) * eo3DCubeSize
                + new Vector3(-cubeSize * 2, -cubeSize, -cubeSize * 2) + offset;
            zAxisPos = new IndexPosition(min.X, min.Y, max.Z + 1) * eo3DCubeSize
                + new Vector3(-cubeSize * 2, 0, -cubeSize) + offset;
        }

        public void PositionToMinMax(Vector3 min, Vector3 max, Vector3 offset)
        {
            xAxisPos = new Vector3(max.X, min.Y, min.Z) + offset;
            yAxisPos = new Vector3(min.X, max.Y, min.Z) + offset;
            zAxisPos = new Vector3(min.X, min.Y, max.Z) + offset;
        }

        public void Draw()
        {
            xAxisVo.MeshScale = new Vector3(GetXScale());
            yAxisVo.MeshScale = new Vector3(GetYScale());
            zAxisVo.MeshScale = new Vector3(GetZScale());

            entRenderer.BatchFront(xAxisVo, xAxisPos);
            entRenderer.BatchFront(yAxisVo, yAxisPos);
            entRenderer.BatchFront(zAxisVo, zAxisPos);
        }
    }
}
