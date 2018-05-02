using System;
using Dash.Engine.Graphics.OpenGL;

/* GuiRenderer.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class GuiRenderer : Renderer2D
    {
        public readonly SimpleMesh Quad;

        public bool Hide { get; set; }

        GuiShader shader;
        DepthDebugShader depthDebugShader;

        public GuiRenderer(MasterRenderer master)
            : base(master)
        {
            float[] positions = new float[]
            {
                -1, 1, // top left
                -1, -1, // bottom left
                1, 1, // top right
                1, -1 // bottom right
            };

            Quad = new SimpleMesh(BufferUsageHint.StaticDraw, 2, positions);
            shader = new GuiShader();
            depthDebugShader = new DepthDebugShader();
        }

        #region Debug Rendering
        public void DebugRenderTexture(Texture tex)
        {
            PrepareRender(shader);
            shader.LoadColor4("overlayColor", Color4.White);
            shader.LoadBool("flipY", true);

            GL.ActiveTexture(TextureUnit.Texture0);
            // Bind the Opaque texture
            tex.Bind();
            // Set transformation matrix
            shader.LoadTransformationMatrix(Matrix4.Identity);
            // Draw
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);

            // Ubind the FBO texture
            GL.BindTexture(TextureTarget.Texture2D, 0);
            EndRender(shader);
        }

        public void DebugRenderTexture(Texture tex, int width, int height)
        {
            PrepareRender(shader);
            shader.LoadColor4("overlayColor", Color4.White);
            shader.LoadBool("flipY", true);

            GL.ActiveTexture(TextureUnit.Texture0);
            // Bind the Opaque texture
            tex.Bind();
            // Set transformation matrix
            shader.LoadTransformationMatrix(Matrix4.CreateScale((float)width / Master.ScreenWidth, 
                (float)height / Master.ScreenHeight, 1));
            // Draw
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);

            // Ubind the FBO texture
            GL.BindTexture(TextureTarget.Texture2D, 0);
            EndRender(shader);
        }

        public void DebugRenderShadowMap(ShadowMap shadowMap)
        {
            PrepareRender(depthDebugShader);

            depthDebugShader.LoadBool("linearize", false);
            //depthDebugShader.LoadFloat("nearPlane", 10);
            //depthDebugShader.LoadFloat("farPlane", 4000);

            // Bind the FBO texture
            GL.ActiveTexture(TextureUnit.Texture0);
            shadowMap.BindTex();
            // Set transformation matrix
            depthDebugShader.LoadMatrix4("transformationMatrix", 
                Maths.CreateTransformationMatrix(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)));
            // Draw
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);

            // Ubind the FBO texture
            GL.BindTexture(TextureTarget.Texture2D, 0);
            EndRender(depthDebugShader);
        }

        public void DebugRenderTexRenderTargs(TexRenderTarget targetA, TexRenderTarget targetB)
        {
            PrepareRender(shader);
            shader.LoadColor4("overlayColor", Color4.White);
            shader.LoadBool("flipY", false);

            GL.ActiveTexture(TextureUnit.Texture0);
            // Bind the Opaque texture
            targetA.Texture.Bind();
            // Set transformation matrix
            Matrix4 transMatrix = Maths.CreateTransformationMatrix(new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            shader.LoadTransformationMatrix(transMatrix);
            // Draw
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);

            // Bind the Alpha texture
            targetB.Texture.Bind();
            // Set transformation matrix
            transMatrix = Maths.CreateTransformationMatrix(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            shader.LoadTransformationMatrix(transMatrix);
            // Draw
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);

            // Ubind the FBO texture
            GL.BindTexture(TextureTarget.Texture2D, 0);
            EndRender(shader);
        }
        #endregion

        public void PrepareRender(Shader shader, bool keepDepthTest = false)
        {
            // Start shader
            shader.Start();
            // Bind the quad
            Quad.Bind();
            // Enable the shader
            shader.EnableAttributes();
            // Setup blending and depth properties
            StateManager.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            if (!keepDepthTest)
                StateManager.Disable(EnableCap.DepthTest);
        }

        public void EndRender(Shader shader)
        {
            // Reset blend and depth settigns
            StateManager.Enable(EnableCap.DepthTest);
            StateManager.Disable(EnableCap.Blend);
            // Disable shader 
            shader.DisableAttributes();
            // Unbind the quad
            GL.BindVertexArray(0);
            // Stop the shader
            shader.Stop();
        }

        public override void Resize(int width, int height) { }
        public override void Render() { }

        public override void Dispose()
        {
            shader.Dispose();
            depthDebugShader.Dispose();
        }
    }
}
