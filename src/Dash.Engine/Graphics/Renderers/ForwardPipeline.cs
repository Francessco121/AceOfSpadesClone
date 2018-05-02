using Dash.Engine.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

/* ForwardPipeline.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class ForwardPipeline : RenderPipeline
    {
        WorldShader forwardShader;
        ShadowShader shadowShader;
        ShadowMap shadowMap;
        ShadowCamera shadowCamera;

        PostProcessBuffer ppBuffer;
        PostProcessShader ppShader;
        TexRenderTarget skyRenderTarg;

        float shadow_texelMultiplier = 0.08f;

        ScreenshotRequest screenshotRequest;
        TexRenderTarget screenshotRenderTarg;

        public ForwardPipeline(MasterRenderer renderer) 
            : base(renderer)
        {
            GLError.Begin();

            forwardShader = new WorldShader();
            shadowShader = new ShadowShader();
            shadowMap = new ShadowMap(GFXSettings.ShadowResolution, GFXSettings.ShadowResolution);
            shadowCamera = new ShadowCamera();
            ppBuffer = new PostProcessBuffer(ScreenWidth, ScreenHeight);
            ppShader = new PostProcessShader();
            skyRenderTarg = new TexRenderTarget(ScreenWidth, ScreenHeight);
            screenshotRenderTarg = new TexRenderTarget(1, 1);

            screenshotRenderTarg.Bind();
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            screenshotRenderTarg.Unbind();

            ErrorCode err = GLError.End();
            if (err != ErrorCode.NoError)
                throw new Exception(string.Format("Failed to initialize forward pipeline. OpenGL Error: {0}", err));
        }

        public override void TakeScreenshot(ScreenshotRequest request)
        {
            screenshotRequest = request;
            screenshotRenderTarg.Resize(request.RenderWidth, request.RenderHeight);
        }

        public override void Resize(int width, int height)
        {
            ppBuffer.Resize(width, height);
            skyRenderTarg.Resize(width, height);

            foreach (Renderer2D r in Renderer.Renderer2Ds.Values)
                r.Resize(width, height);

            GL.Viewport(0, 0,
                Renderer.ScreenWidth % 2 == 0 ? Renderer.ScreenWidth : Renderer.ScreenWidth - 1,
                Renderer.ScreenHeight % 2 == 0 ? Renderer.ScreenHeight : Renderer.ScreenHeight + 1);
        }

        public override void PrepareMesh(Mesh mesh, RenderPass pass)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh", "Cannot prepare a null mesh.");

            // Bind VAO
            mesh.Bind();
            // Enable shader attribs
            if (pass == RenderPass.Shadow)
                shadowShader.EnableAttributes();
            else
                forwardShader.EnableAttributes();

            // Toggle wireframe
            StateManager.ToggleWireframe(mesh.RenderAsWireframe);
        }

        public override void EndMesh()
        {
            // Unbind VAO
            GL.BindVertexArray(0);
        }

        void TryBindScreenshotTarg()
        {
            if (screenshotRequest != null)
                screenshotRenderTarg.Bind();
        }

        public override void Render(float deltaTime)
        {
            TryBindScreenshotTarg();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (IsRenderingEnabled(RendererFlags.Objects))
            {
                if (GFXSettings.RenderShadows)
                {
                    RenderShadowMap();
                    TryBindScreenshotTarg();
                }
            
                if (IsRenderingEnabled(RendererFlags.Sky))
                {
                    // Render the skybox
                    if (Renderer.FogEnabled && GFXSettings.FogQuality == FogQuality.High)
                    {
                        skyRenderTarg.Bind();

                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                        Renderer.Sky.Render(deltaTime, false);

                        skyRenderTarg.Unbind();
                    }
                }

                if (GFXSettings.EnablePostProcessing)
                {
                    ppBuffer.Bind();
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                }

                if (IsRenderingEnabled(RendererFlags.Sky))
                    Renderer.Sky.Render(deltaTime, true);

                RenderObjects();

                if (GFXSettings.EnablePostProcessing)
                {
                    ppBuffer.Unbind();
                    TryBindScreenshotTarg();
                }

                // Tell the renderers to clear their batches
                foreach (Renderer3D renderer in Renderer.Renderer3Ds.Values)
                    renderer.ClearBatch();

                if (GFXSettings.EnablePostProcessing)
                {
                    Renderer.Gui.PrepareRender(ppShader);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    ppBuffer.ColorTexture.Bind();

                    ppShader.LoadVector2("resolution", new Vector2(ScreenWidth, ScreenHeight));
                    ppShader.LoadMatrix4("transformationMatrix", Matrix4.Identity);
                    ppShader.LoadBool("apply_fxaa", GFXSettings.ApplyFXAA);

                    // Draw Post Process
                    GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);

                    ppBuffer.ColorTexture.Unbind();
                    Renderer.Gui.EndRender(ppShader);
                }
            }

            if (IsRenderingEnabled(RendererFlags.Gui2D))
            {
                StateManager.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                if (Renderer.DebugRenderShadowMap)
                    Renderer.Gui.DebugRenderShadowMap(shadowMap);

                StateManager.Disable(EnableCap.DepthTest);

                // Draw 2D UI
                foreach (Renderer2D renderer in Renderer.Renderer2Ds.Values)
                {
                    renderer.Prepare();
                    renderer.Render();
                }
            }

            if (screenshotRequest != null)
            {
                SaveCurrentBufferAsScreenshot();
                screenshotRenderTarg.Unbind();
                screenshotRequest = null;
            }
        }

        void SaveCurrentBufferAsScreenshot()
        {
            int width = screenshotRequest.RenderWidth;
            int height = screenshotRequest.RenderHeight;

            int[] pixels = new int[4 * width * height];
            GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GCHandle pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width, height, width * 4,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb, pixelsHandle.AddrOfPinnedObject());

                bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
                bmp.Save(screenshotRequest.OutputPath + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
            finally
            {
                pixelsHandle.Free();
            }
        }

        void RenderShadowMap()
        {
            if (Renderer.Sun != null)
            {
                shadowCamera.Position = Renderer.Sun.Position * 1500; // Extrapolate the normalized sun position a bit
                shadowCamera.Update();
            }

            // Enable global wireframe if needed
            if (Renderer.GlobalWireframe)
                StateManager.EnableWireframe();

            // Ensure back-face culling is enabled
            StateManager.Disable(EnableCap.CullFace);
            StateManager.Enable(EnableCap.DepthTest);

            shadowShader.Start();
            shadowShader.LoadMatrix4("lightSpaceMatrix", shadowCamera.LightSpaceMatrix);

            if (GFXSettings.ShadowResolution != shadowMap.Width)
                shadowMap.Resize(GFXSettings.ShadowResolution, GFXSettings.ShadowResolution);

            GL.Viewport(0, 0, shadowMap.Width, shadowMap.Height);
            shadowMap.Bind();

            GL.Clear(ClearBufferMask.DepthBufferBit);

            foreach (Renderer3D renderer in Renderer.Renderer3Ds.Values)
                renderer.Render(shadowShader, RenderPass.Shadow, false);
            GL.DepthFunc(DepthFunction.Always);
            foreach (Renderer3D renderer in Renderer.Renderer3Ds.Values)
                renderer.Render(shadowShader, RenderPass.Shadow, true);
            GL.DepthFunc(DepthFunction.Less);

            shadowMap.Unbind();
            shadowShader.Stop();

            StateManager.DisableWireframe(true);

            GL.Viewport(0, 0, 
                Renderer.ScreenWidth % 2 == 0 ? Renderer.ScreenWidth : Renderer.ScreenWidth - 1, 
                Renderer.ScreenHeight % 2 == 0 ? Renderer.ScreenHeight : Renderer.ScreenHeight + 1);
        }

        void RenderObjects()
        {
            // Enable global wireframe if needed
            if (Renderer.GlobalWireframe)
                StateManager.EnableWireframe();

            // Ensure back-face culling is enabled
            StateManager.Enable(EnableCap.CullFace);
            StateManager.Enable(EnableCap.DepthTest);
            StateManager.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            forwardShader.Start();
            forwardShader.LoadColor4("colorOverlay", Color.White);

            // Load global shader variables
            Camera camera = Camera.Active;
            forwardShader.LoadBool("fogEnabled", Renderer.FogEnabled);
            if (Renderer.FogEnabled)
            {
                forwardShader.LoadInt("fogQuality", (int)GFXSettings.FogQuality);
                forwardShader.LoadFloat("fogDensity", Renderer.FogDensity);
                forwardShader.LoadFloat("fogGradient", Renderer.FogGradient);
                forwardShader.LoadFloat("fogMin", Renderer.FogMin);
                forwardShader.LoadFloat("fogMax", Renderer.FogMax);

                GL.ActiveTexture(TextureUnit.Texture1);
                if (GFXSettings.FogQuality == FogQuality.High)
                    skyRenderTarg.Texture.Bind();
                else if (GFXSettings.FogQuality == FogQuality.Medium)
                {
                    Renderer.Sky.skyMap.Bind();
                    forwardShader.LoadFloat("skyMapOffset", Renderer.Sky.skyMapOffset);
                }
                else
                    forwardShader.LoadColor3("fogColor", Renderer.FogColor);
            }
            forwardShader.LoadLights(Renderer.Lights);
            forwardShader.LoadMatrix4("projectionMatrix", camera.ProjectionMatrix);
            forwardShader.LoadMatrix4("viewMatrix", camera.ViewMatrix);
            forwardShader.LoadVector3("cameraPosition", camera.Position);
            forwardShader.LoadFloat("ambientIntensity", Renderer.AmbientIntensity);
            forwardShader.LoadBool("renderShadows", GFXSettings.RenderShadows);
            forwardShader.LoadFloat("lightFalloff", Renderer.LightFalloff);

            if (GFXSettings.RenderShadows)
            {
                forwardShader.LoadMatrix4("lightSpaceMatrix", shadowCamera.LightSpaceMatrix);
                forwardShader.LoadInt("pcfSamples", GFXSettings.ShadowPCFSamples);
                forwardShader.LoadFloat("shadowTexelMultiplier", shadow_texelMultiplier);
                forwardShader.LoadFloat("shadowBias", Renderer.ShadowBias);
                forwardShader.LoadFloat("shadowVisibility", Renderer.ShadowVisibility);

                GL.ActiveTexture(TextureUnit.Texture0);
                shadowMap.BindTex();
            }

            // Render normal geometry
            foreach (Renderer3D renderer in Renderer.Renderer3Ds.Values)
            {
                renderer.Prepare();
                renderer.Render(forwardShader, RenderPass.Normal, false);
            }

            shadowMap.UnbindTex();

            // Render front geometry
            if (GFXSettings.RenderShadows)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                shadowMap.BindTex();
            }

            GL.Clear(ClearBufferMask.DepthBufferBit);
            foreach (Renderer3D renderer in Renderer.Renderer3Ds.Values)
            {
                renderer.Prepare();
                renderer.Render(forwardShader, RenderPass.Normal, true);
            }

            forwardShader.Stop();
            shadowMap.UnbindTex();
            
            // Reset wireframe
            StateManager.DisableWireframe(true);
        }

        public override void Dispose()
        {
            forwardShader.Dispose();
            shadowShader.Dispose();
            shadowMap.Dispose();
            ppBuffer.Dispose();
            ppShader.Dispose();
            screenshotRenderTarg.Dispose();
        }
    }
}
