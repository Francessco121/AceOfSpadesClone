using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;

/* VoxelRenderer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Graphics.Renderers
{
    public class VoxelRenderer : Renderer3D
    {
        Batch<VoxelObject, VoxelRenderComponent> batch;

        public VoxelRenderer(MasterRenderer master) 
            : base(master)
        {
            batch = new Batch<VoxelObject, VoxelRenderComponent>();   
        }

        public void Batch(VoxelRenderComponent component)
        {
            batch.BatchItem(component.VoxelObject, component, component.RenderFront);
        }

        public override void ClearBatch()
        {
            batch.Clear();
        }

        Mesh PrepareVO(VoxelObject vo, RenderPass pass)
        {
            Mesh mesh = pass.HasFlag(RenderPass.Alpha) ? vo.AlphaMesh : vo.Mesh;
            if (mesh == null && pass.HasFlag(RenderPass.Normal))
                throw new InvalidOperationException("Attempted to render null opaque mesh!");

            if (mesh != null)
                Master.PrepareMesh(mesh, pass);

            return mesh;
        }

        void RenderList(Shader shader, RenderPass pass, List<VoxelRenderComponent> list, VoxelObject commonVO = null)
        {
            // Pre-load mesh
            Mesh mesh = null;
            if (commonVO != null)
            {
                mesh = PrepareVO(commonVO, pass);
                if (mesh == null)
                    return;
            }

            // Go through batch
            foreach (VoxelRenderComponent vrc in list)
            {
                if (vrc.OnlyRenderFor.HasValue && !vrc.OnlyRenderFor.Value.HasFlag(pass))
                    continue;

                // Load mesh if were not using a common vo
                if (commonVO == null)
                {
                    mesh = PrepareVO(vrc.VoxelObject, pass);
                    if (mesh == null)
                        continue;
                }

                // Load the world matrix
                shader.LoadMatrix4("transformationMatrix", vrc.WorldMatrix);

                // Prepare the entity
                StateManager.ToggleWireframe(vrc.RenderAsWireframe);

                if (!pass.HasFlag(RenderPass.Shadow))
                {
                    shader.LoadBool("skipLight", vrc.ApplyNoLighting);
                    shader.LoadColor4("colorOverlay", vrc.ColorOverlay);
                    shader.LoadFloat("entityLighting", vrc.Lighting);
                }

                // Render the entity
                GL.DrawElements(mesh.BeginMode, mesh.VertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

                // If we're not using a common vo, end it's mesh
                if (commonVO == null)
                    Master.EndMesh();
            }

            // If we are using a common vo, end it's mesh last
            if (commonVO != null)
                Master.EndMesh();
        }

        public override void Render(Shader shader, RenderPass pass, bool frontPass)
        {
            // Set global uniforms
            if (!pass.HasFlag(RenderPass.Shadow))
            {
                shader.LoadFloat("specularPower", 0);
                shader.LoadFloat("specularIntensity", 0);
            }

            // Render Entities
            if (!frontPass)
                foreach (BatchGroup<VoxelObject, VoxelRenderComponent> pair in batch)
                    RenderList(shader, pass, pair.List, pair.Key);
            else
                RenderList(shader, pass, batch.UnoptimizedBatch);
        }

        public override void Dispose() { }
    }
}
