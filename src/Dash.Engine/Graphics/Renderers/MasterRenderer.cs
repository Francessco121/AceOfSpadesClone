using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/* MasterRenderer.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public enum FogQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Off = 3 //see line #81 of AceOfSpades.Editor.Models.EditorUI.SetGfxOption()
    }

    [Flags]
    public enum RendererFlags
    {
        None = 0,
        Sky = 1,
        Objects = 2,
        Gui2D = 4,
        Gui3D = 8,

        Gui = Gui2D | Gui3D,
        All = ~0,
    }

    [Flags]
    public enum RenderPass
    {
        Normal = 1,
        Alpha = 2,
        Shadow = 4
    }

    public class ScreenshotRequest
    {
        public int RenderWidth { get; }
        public int RenderHeight { get; }
        public string OutputPath { get; }

        public ScreenshotRequest(int renderWidth, int renderHeight, string outPath)
        {
            RenderWidth = renderWidth;
            RenderHeight = renderHeight;
            OutputPath = outPath;
        }
    }

    public class MasterRenderer : IDisposable
    {
        public static MasterRenderer Instance { get; private set; }
        public static float GLVersion { get; private set; }

        public const int MAX_LIGHTS = 64; // Ensure this is changed in the shaders as well

        public GraphicsOptions GFXSettings;

        public Color4 FogColor = Color4.Lavender;
        public float FogDensity = 0.001f;
        public float FogGradient = 6f;
        public float FogMin = 0;
        public float FogMax = 1f;
        public bool FogEnabled = true;
        public float AmbientIntensity = 0.2f;
        public float ShadowVisibility = 0.3f;
        public float ShadowBias = 0.0025f;
        public float LightFalloff;

        public bool GlobalWireframe;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Dictionary<Type, Renderer3D> Renderer3Ds { get; private set; }
        public Dictionary<Type, Renderer2D> Renderer2Ds { get; private set; }

        public SkyboxRenderer Sky { get; private set; }
        public GuiRenderer Gui { get; private set; }
        public SpriteRenderer Sprites { get; private set; }

        public RendererFlags EnabledRendering = RendererFlags.All;
        public bool DebugRenderShadowMap;

        public Light Sun { get; set; }
        public LightList Lights { get; private set; }

        RenderPipeline activePipeline;
        ScreenshotRequest screenshotRequest;

        public MasterRenderer(int screenWidth, int screenHeight, GraphicsOptions options = null)
        {
            Instance = this;
            GFXSettings = options ?? new GraphicsOptions();

            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            if (GLVersion == 0)
            {
                int major = GL.GetInteger(GetPName.MajorVersion);
                int minor = GL.GetInteger(GetPName.MinorVersion);
                GLVersion = major + minor * 0.1f;

                //if (major < 4)
                //    throw new Exception(string.Format("OpenGL 4.0 or later is required to run this game. This machine is running: {0}", GLVersion));
                if (major < 4)
                    DashCMD.WriteWarning("[OpenGL] OpenGL 4.0 or later is required to run this game properly!. This machine is running: {0}", GLVersion);
                else
                    DashCMD.WriteLine("[OpenGL] Version: {0}", ConsoleColor.DarkGray, GLVersion);
            }

            GLError.Begin();

            Camera camera = new Camera(this);
            camera.MakeActive();

            Lights = new LightList();

            Texture.Blank = GLoader.LoadBlankTexture(Color.White);

            Renderer3Ds = new Dictionary<Type, Renderer3D>();
            Renderer2Ds = new Dictionary<Type, Renderer2D>();

            Gui = new GuiRenderer(this);
            Sprites = new SpriteRenderer(this);
            Sky = new SkyboxRenderer(this);

            AddRenderer(Gui);
            AddRenderer(Sprites);

            activePipeline = new ForwardPipeline(this);

            StateManager.Enable(EnableCap.CullFace);
            StateManager.Enable(EnableCap.DepthTest);

            GL.CullFace(CullFaceMode.Back);

            ErrorCode mInitError = GLError.End();
            if (mInitError != ErrorCode.NoError)
                throw new Exception(string.Format("Uncaught master renderer init opengl error: {0}", mInitError));
        }

        #region Renderer Add/Remove/Get
        public void AddRenderer(Renderer2D renderer)
        {
            Renderer2Ds.Add(renderer.GetType(), renderer);
        }

        public void AddRenderer(Renderer3D renderer)
        {
            Renderer3Ds.Add(renderer.GetType(), renderer);
        }

        public void RemoveRenderer(Renderer2D renderer)
        {
            Renderer2Ds.Remove(renderer.GetType());
        }

        public void RemoveRenderer(Renderer3D renderer)
        {
            Renderer3Ds.Remove(renderer.GetType());
        }

        public T GetRenderer2D<T>() where T : Renderer2D
        {
            return (T)Renderer2Ds[typeof(T)];
        }

        public T GetRenderer3D<T>() where T : Renderer3D
        {
            return (T)Renderer3Ds[typeof(T)];
        }
        #endregion

        public void TakeScreenshot(string outFilePath, int? renderWidth = null, int? renderHeight = null)
        {
            int width = renderWidth ?? ScreenWidth;
            int height = renderHeight ?? ScreenHeight;

            screenshotRequest = new ScreenshotRequest(width, height, outFilePath);
        }

        public void Prepare() { }

        public void OnResize(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;

            activePipeline.Resize(width, height);

            // Resize camera
            Camera.Active.OnResize(width, height);
        }

        public void PrepareMesh(Mesh mesh, RenderPass pass)
        {
            activePipeline.PrepareMesh(mesh, pass);
        }

        public void EndMesh()
        {
            activePipeline.EndMesh();
        }

        public void Update(float deltaTime)
        {
            if (Camera.Active.NeedsResize)
                Camera.Active.OnResize(ScreenWidth, ScreenHeight);

            foreach (Renderer2D r in Renderer2Ds.Values)
                r.Update(deltaTime);
            foreach (Renderer3D r in Renderer3Ds.Values)
                r.Update(deltaTime);
        }

        public void Render(float deltaTime)
        {
            int lastScreenWidth = ScreenWidth;
            int lastScreenHeight = ScreenHeight;

            if (screenshotRequest != null)
            {
                OnResize(screenshotRequest.RenderWidth, screenshotRequest.RenderHeight);
                Update(deltaTime);
                Camera.Active.Update(deltaTime);
                activePipeline.TakeScreenshot(screenshotRequest);
            }

            activePipeline.Render(deltaTime);

            if (screenshotRequest != null)
            {
                OnResize(lastScreenWidth, lastScreenHeight);
                screenshotRequest = null;
            }

            Camera.Active.Update(deltaTime);
        }

        public void Dispose()
        {
            activePipeline.Dispose();

            foreach (Renderer3D renderer in Renderer3Ds.Values) renderer.Dispose();
            foreach (Renderer2D renderer in Renderer2Ds.Values) renderer.Dispose();
            Sky.Dispose();

            GManager.CleanUpFinal();
        }
    }
}
