using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;

/* EntityRenderer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Graphics
{
    public class EntityRenderer : Renderer3D
    {
        class BatchedEntity
        {
            public VoxelObject VoxelObject;
            public Vector3 Position;
            public Vector3 MeshRotation;

            public bool RenderFront;
            public bool RenderAsWireframe;
            public bool ApplyNoLighting;
            public Color ColorOverlay;
            public RenderPass? OnlyRenderFor;

            public Matrix4? CustomMatrix;
        }

        Batch<VoxelObject, BatchedEntity> entityBatch;
        Batch<VoxelObject, Matrix4> rawBatch;
        Batch<VoxelObject, Matrix4> rawBatchFront;

        public EntityRenderer(MasterRenderer master)
            : base(master)
        {
            entityBatch = new Batch<VoxelObject, BatchedEntity>();
            rawBatch = new Batch<VoxelObject, Matrix4>();
            rawBatchFront = new Batch<VoxelObject, Matrix4>();
        }

        public void Batch(Entity entity)
        {
            Batch(entity, null);
        }

        public void Batch(Entity entity, Matrix4? worldMatrix)
        {
            entityBatch.BatchItem(entity.VoxelObject, new BatchedEntity()
            {
                VoxelObject = entity.VoxelObject,
                Position = entity.Position,
                MeshRotation = entity.MeshRotation,
                RenderFront = entity.RenderFront,
                RenderAsWireframe = entity.RenderAsWireframe,
                ApplyNoLighting = entity.ApplyNoLighting,
                ColorOverlay = entity.ColorOverlay,
                CustomMatrix = worldMatrix,
                OnlyRenderFor = entity.OnlyRenderFor
            }, entity.RenderFront);
        }

        public void BatchFront(VoxelObject vo, Vector3 position)
        {
            Matrix4 mat4 = Maths.CreateTransformationMatrix(position, vo.MeshRotation, vo.MeshScale);
            rawBatchFront.BatchItem(vo, mat4);
        }
        
        public void BatchFront(VoxelObject vo, Matrix4 worldMatrix)
        {
            rawBatchFront.BatchItem(vo, worldMatrix);
        }

        public void Batch(VoxelObject vo, Vector3 position)
        {
            Matrix4 mat4 = Maths.CreateTransformationMatrix(position, vo.MeshRotation, vo.MeshScale);
            Batch(vo, mat4);
        }

        public void Batch(VoxelObject vo, Matrix4 mat4)
        {
            rawBatch.BatchItem(vo, mat4);
        }

        public override void ClearBatch()
        {
            entityBatch.Clear();
            rawBatch.Clear();
            rawBatchFront.Clear();
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

        void RenderEntityList(Shader shader, RenderPass pass, List<BatchedEntity> list, VoxelObject commonVO = null)
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
            foreach (BatchedEntity ent in list)
            {
                if (ent.OnlyRenderFor.HasValue && !ent.OnlyRenderFor.Value.HasFlag(pass))
                    continue;

                // Load mesh if were not using a common vo
                if (commonVO == null)
                {
                    mesh = PrepareVO(ent.VoxelObject, pass);
                    if (mesh == null)
                        continue;
                }

                // Determine the world matrix
                Matrix4 worldMatrix = ent.CustomMatrix
                    ?? Maths.CreateTransformationMatrix(ent.Position, ent.MeshRotation, ent.VoxelObject.MeshScale);
                shader.LoadMatrix4("transformationMatrix", worldMatrix);

                // Prepare the entity
                //if (ent.RenderFront && pass != RenderPass.Shadow) StateManager.DepthFunc(DepthFunction.Always);
                //else StateManager.DepthFunc(DepthFunction.Less);

                StateManager.ToggleWireframe(ent.RenderAsWireframe);

                if (!pass.HasFlag(RenderPass.Shadow))
                {
                    shader.LoadBool("skipLight", ent.ApplyNoLighting);
                    shader.LoadColor4("colorOverlay", ent.ColorOverlay);
                    shader.LoadFloat("entityLighting", 1f);
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
            bool alphaPass = pass == RenderPass.Alpha;

            // Set global uniforms
            if (!pass.HasFlag(RenderPass.Shadow))
            {
                shader.LoadFloat("specularPower", 0);
                shader.LoadFloat("specularIntensity", 0);
            }

            // Render Entities
            if (!frontPass)
                foreach (BatchGroup<VoxelObject, BatchedEntity> pair in entityBatch)
                    RenderEntityList(shader, pass, pair.List, pair.Key);
            else
                RenderEntityList(shader, pass, entityBatch.UnoptimizedBatch);

            if (!pass.HasFlag(RenderPass.Shadow))
            {
                shader.LoadBool("skipLight", false);
                shader.LoadColor4("colorOverlay", Color.White);
                shader.LoadFloat("entityLighting", 1f);
            }

            StateManager.DepthFunc(DepthFunction.Less);
            StateManager.DisableWireframe();

            // Render raw voxel objects
            if (!frontPass)
            {
                foreach (BatchGroup<VoxelObject, Matrix4> pair in rawBatch)
                {
                    if (alphaPass && pair.Key.AlphaMesh == null)
                        continue;

                    Mesh mesh = alphaPass ? pair.Key.AlphaMesh : pair.Key.Mesh;
                    Master.PrepareMesh(mesh, pass);

                    foreach (Matrix4 transMatrix in pair.List)
                    {
                        // Load the transformation matrix
                        shader.LoadMatrix4("transformationMatrix", transMatrix);

                        // Draw
                        GL.DrawElements(mesh.BeginMode, mesh.VertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
                    }

                    Master.EndMesh();
                }
            }
            else
            {
                //StateManager.DepthFunc(DepthFunction.Always);

                foreach (BatchGroup<VoxelObject, Matrix4> pair in rawBatchFront)
                {
                    if (alphaPass && pair.Key.AlphaMesh == null)
                        continue;

                    Mesh mesh = alphaPass ? pair.Key.AlphaMesh : pair.Key.Mesh;
                    Master.PrepareMesh(mesh, pass);

                    foreach (Matrix4 transMatrix in pair.List)
                    {
                        // Load the transformation matrix
                        shader.LoadMatrix4("transformationMatrix", transMatrix);

                        // Draw
                        GL.DrawElements(mesh.BeginMode, mesh.VertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
                    }

                    Master.EndMesh();
                }

               // StateManager.DepthFunc(DepthFunction.Less);
            }
        }

        public override void Dispose() { }
    }
}
