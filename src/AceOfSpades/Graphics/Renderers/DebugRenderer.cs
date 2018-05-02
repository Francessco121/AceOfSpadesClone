using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using Dash.Engine.Physics;
using System;
using System.Collections.Generic;

/* EntityRenderer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Graphics
{
    public class DebugRenderer : Renderer3D
    {
        class Impact
        {
            public Color Color { get; }
            public float TimeLeft { get; set; }
            public Vector3 Position { get; }

            public Impact(Vector3 position, Color color)
            {
                Position = position;
                Color = color;
                TimeLeft = 7f;
            }
        }

        HashSet<AxisAlignedBoundingBox> batchedAABBs;
        HashSet<RenderableRay> batchedRays;
        List<Impact> bulletImpacts;
        List<Impact> playerRollbacks;

        DebugCube aabbDebugBox;

        public DebugRenderer(MasterRenderer master)
            : base(master)
        {
            batchedAABBs = new HashSet<AxisAlignedBoundingBox>();
            batchedRays = new HashSet<RenderableRay>();
            bulletImpacts = new List<Impact>();
            playerRollbacks = new List<Impact>();

            aabbDebugBox = new DebugCube(Color4.White, 1);
        }

        public void Batch(AxisAlignedBoundingBox aabb)
        {
            batchedAABBs.Add(aabb);
        }

        public void Batch(RenderableRay ray)
        {
            batchedRays.Add(ray);
        }

        public void AddBulletImpact(Vector3 origin, Color color)
        {
            bulletImpacts.Add(new Impact(origin, color));
        }

        public void AddPlayerRollback(Vector3 origin, Color color)
        {
            playerRollbacks.Add(new Impact(origin, color));
        }

        public override void ClearBatch()
        {
            batchedAABBs.Clear();
            batchedRays.Clear();
        }

        public override void Render(Shader shader, RenderPass pass, bool frontPass)
        {
            if (frontPass)
                return;

            if (!pass.HasFlag(RenderPass.Alpha))
            {
                if (batchedAABBs.Count > 0)
                    RenderAABBs(shader, pass);
                if (batchedRays.Count > 0)
                    RenderRays(shader, pass);
                if (bulletImpacts.Count > 0)
                    RenderBulletImpacts(shader, pass);
                if (playerRollbacks.Count > 0)
                    RenderPlayerRollbacks(shader, pass);
            }
        }

        public override void Update(float deltaTime)
        {
            for (int i = bulletImpacts.Count - 1; i >= 0; i--)
            {
                Impact imp = bulletImpacts[i];
                imp.TimeLeft -= deltaTime;

                if (imp.TimeLeft <= 0)
                    bulletImpacts.RemoveAt(i);
            }

            for (int i = playerRollbacks.Count - 1; i >= 0; i--)
            {
                Impact imp = playerRollbacks[i];
                imp.TimeLeft -= deltaTime;

                if (imp.TimeLeft <= 0)
                    playerRollbacks.RemoveAt(i);
            }

            base.Update(deltaTime);
        }

        void RenderPlayerRollbacks(Shader shader, RenderPass pass)
        {
            Master.PrepareMesh(aabbDebugBox.VoxelObject.Mesh, pass);
            StateManager.EnableWireframe();
            foreach (Impact imp in playerRollbacks)
            {
                // Create translation matrix
                Matrix4 transMatrix = Maths.CreateTransformationMatrix(imp.Position, Vector3.Zero, new Vector3(5, 11, 5));
                shader.LoadMatrix4("transformationMatrix", transMatrix);

                if (!pass.HasFlag(RenderPass.Shadow))
                    shader.LoadColor4("colorOverlay", imp.Color);

                // Draw
                GL.DrawElements(BeginMode.Triangles, aabbDebugBox.VoxelObject.Mesh.VertexCount,
                    DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            StateManager.DisableWireframe();
            Master.EndMesh();
        }

        void RenderBulletImpacts(Shader shader, RenderPass pass)
        {
            Master.PrepareMesh(aabbDebugBox.VoxelObject.Mesh, pass);
            foreach (Impact impact in bulletImpacts)
            {
                // Create translation matrix
                Matrix4 transMatrix = Matrix4.CreateTranslation(impact.Position);
                shader.LoadMatrix4("transformationMatrix", transMatrix);

                if (!pass.HasFlag(RenderPass.Shadow))
                    shader.LoadColor4("colorOverlay", impact.Color);

                // Draw
                GL.DrawElements(BeginMode.Triangles, aabbDebugBox.VoxelObject.Mesh.VertexCount,
                    DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            Master.EndMesh();
        }

        void RenderAABBs(Shader shader, RenderPass pass)
        {
            Master.PrepareMesh(aabbDebugBox.VoxelObject.Mesh, pass);
            StateManager.EnableWireframe();
            foreach (AxisAlignedBoundingBox box in batchedAABBs)
            {
                // Create translation matrix
                Matrix4 transMatrix = Maths.CreateTransformationMatrix(box.Center, Vector3.Zero, box.Size);
                shader.LoadMatrix4("transformationMatrix", transMatrix);

                if (!pass.HasFlag(RenderPass.Shadow))
                    shader.LoadColor4("colorOverlay", Color.Red);

                // Draw
                GL.DrawElements(BeginMode.Triangles, aabbDebugBox.VoxelObject.Mesh.VertexCount, 
                    DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            StateManager.DisableWireframe();
            Master.EndMesh();
        }

        void RenderRays(Shader shader, RenderPass pass)
        {
            if (!pass.HasFlag(RenderPass.Shadow))
            {
                shader.LoadBool("skipLight", true);
                shader.LoadBool("renderShadows", false);
                shader.LoadColor4("colorOverlay", Color.White);
            }

            foreach (RenderableRay ray in batchedRays)
            {
                if (ray == null) continue;

                Master.PrepareMesh(ray.Mesh, pass);

                // Load shine vars
                if (!pass.HasFlag(RenderPass.Shadow))
                {
                    shader.LoadFloat("specularPower", 0);
                    shader.LoadFloat("specularIntensity", 0);
                }

                // Create translation matrix
                shader.LoadMatrix4("transformationMatrix", Matrix4.Identity);

                // Draw
                GL.DrawElements(BeginMode.Lines, ray.Mesh.VertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
                Master.EndMesh();
            }
        }

        public override void Dispose()
        {
            aabbDebugBox.VoxelObject.Dispose();
        }
    }
}
