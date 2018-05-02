using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace AceOfSpades.Graphics
{
    public class ChunkRenderer : Renderer3D
    {
        HashSet<Chunk> batchedChunks;

        public ChunkRenderer(MasterRenderer master)
            : base(master)
        {
            batchedChunks = new HashSet<Chunk>();
        }

        public void Batch(Chunk item)
        {
            batchedChunks.Add(item);
        }

        public override void ClearBatch()
        {
            batchedChunks.Clear();
        }

        public override void Render(Shader shader, RenderPass pass, bool frontPass)
        {
            if (frontPass)
                return;

            bool alphaPass = pass == RenderPass.Alpha;

            // Set global uniforms
            if (pass != RenderPass.Shadow)
            {
                shader.LoadFloat("specularPower", alphaPass ? 40 : 0);
                shader.LoadFloat("specularIntensity", alphaPass ? 0.5f : 0);
                shader.LoadColor4("colorOverlay", Color.White);
            }

            foreach (Chunk chunk in batchedChunks)
            {
                if (chunk.Culled && pass != RenderPass.Shadow)
                    continue;

                if (chunk.Mesh == null || chunk.IsEmpty || (alphaPass && chunk.AlphaMesh == null))
                    continue;

                Mesh mesh = alphaPass ? chunk.AlphaMesh : chunk.Mesh;
                Master.PrepareMesh(mesh, pass);

                // Create translation matrix
                Matrix4 transMatrix = Matrix4.CreateTranslation(chunk.Position + chunk.RenderOffset);
                shader.LoadMatrix4("transformationMatrix", transMatrix);

                // Draw
                GL.DrawElements(BeginMode.Triangles, mesh.VertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

                Master.EndMesh();
            }
        }

        public override void Dispose() { }
    }
}
