using Dash.Engine.Audio;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics.Context;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Threading;

/* GameWindow.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public delegate void WindowScrollCallback(GameWindow window, int xScroll, int yScroll);
    public delegate void WindowCursorMoveCallback(GameWindow window, int x, int y);
    public delegate void WindowResizedCallback(GameWindow window, int width, int height);
    public delegate void WindowFocusCallback(GameWindow window, bool focused);
    public delegate void WindowKeyCallback(GameWindow window, Key key, int scancode, KeyAction action, KeyModifiers modifiers);

    public abstract class GameWindow : IDisposable
    {
        public event WindowScrollCallback OnScroll;
        public event WindowCursorMoveCallback OnCursorMoved;
        public event WindowResizedCallback OnResized;
        public event WindowFocusCallback OnFocusChanged;
        public event WindowKeyCallback OnKeyAction;

        public CursorMode CursorMode
        {
            get { return cursorMode; }
            set
            {
                cursorMode = value;
                Glfw.SetInputMode(handle, InputMode.CursorMode, value);
            }
        }
        CursorMode cursorMode = CursorMode.CursorNormal;

        public string Title
        {
            get { return title; }
            set
            {
                Glfw.SetWindowTitle(handle, value);
                title = value;
            }
        }

        string title;

        public bool HasFocus { get; private set; }

        public MasterRenderer Renderer { get; private set; }

        public static GameWindow Instance { get; private set; }

        public int Width
        {
            get { return width; }
            set { SetSize(value, height); }
        }

        public int Height
        {
            get { return height; }
            set { SetSize(width, value); }
        }

        int width, height;

        public int ScreenResolutionWidth
        {
            get { return primaryMonitorVideoMode.Width; }
        }
        public int ScreenResolutionHeight
        {
            get { return primaryMonitorVideoMode.Height; }
        }

        public AudioContext AudioContext { get; private set; }
        
        public float FPS { get; private set; }

        public float TargetFrameRate
        {
            get { return targetFPS; }
            set
            {
                targetFPS = value;
                targetDeltaTime = 1f / value;
            }
        }

        float[] fps_times = new float[10];
        int fps_timeI;

        GlfwWindowPtr handle;
        GlfwMonitorPtr primaryMonitor;
        GlfwVidMode primaryMonitorVideoMode;

        int startWidth;
        int startHeight;
        bool startResizable;
        GraphicsOptions startOptions;

        float targetFPS;
        double targetDeltaTime;

        bool vsyncEnabled;

        int lastDrawnWidth, lastDrawnHeight;

        public GameWindow(string title, int width, int height, 
            bool resizable = true, GraphicsOptions options = null)
        {
            this.title = title;
            startWidth = width;
            startHeight = height;
            startResizable = resizable;
            startOptions = options;
            lastDrawnWidth = width;
            lastDrawnHeight = height;

            Instance = this;
        }

        public void SetClipboard(string text)
        {
            Glfw.SetClipboardString(handle, text);
        }

        public string GetClipboard()
        {
            return Glfw.GetClipboardString(handle) ?? "";
        }

        protected void LogOpenGLDrivers()
        {
            string gl_version = GL.GetString(StringName.Version);
            string gl_vendor = GL.GetString(StringName.Vendor);
            string gl_gpu = GL.GetString(StringName.Renderer);
            string gl_shaderversion = GL.GetString(StringName.ShadingLanguageVersion);

            DashCMD.WriteImportant("OpenGL Drivers:");
            DashCMD.WriteStandard("Vendor: {0}", args: gl_vendor);
            DashCMD.WriteStandard("GPU: {0}", gl_gpu);
            DashCMD.WriteStandard("Version: {0}", gl_version);
            DashCMD.WriteStandard("Shader Version: {0}", gl_shaderversion);
        }

        void InitOpenAL()
        {
            // Load base OpenAL
            try
            { AudioContext = new AudioContext(); }
            catch (Exception e)
            { throw new Exception("Failed to initialize OpenAL!", e); }

            // Load OpenAL Efx
            try
            {
                new EffectsExtension();

                if (AL.Efx == null)
                    DashCMD.WriteWarning("OpenAL Efx is not supported!");
            }
            catch (Exception e)
            {
                DashCMD.WriteError("Failed to initialize OpenAL Efx!");
                DashCMD.WriteError(e);
            }

            // Load OpenAL XRam
            try
            {
                new XRamExtension();

                // It's ok if this isn't supported
                //if (AL.XRam == null)
                //    DashCMD.WriteWarning("OpenAL XRam is not supported!");
            }
            catch (Exception e)
            {
                DashCMD.WriteError("Failed to initialize OpenAL XRam!");
                DashCMD.WriteError(e);
            }
        }

        public void Run(float targetFPS)
        {
            TargetFrameRate = targetFPS;

            if (!Glfw.Init())
                throw new Exception("Failed to initialize glfw!");

            try
            {
                // Setup the error callback
                Glfw.SetErrorCallback(OnError);

                // Configure the window settings
                Glfw.WindowHint(WindowHint.Resizeable, startResizable ? 1 : 0);
                Glfw.WindowHint(WindowHint.Samples, 0);

                // Create the window
                handle = Glfw.CreateWindow(startWidth, startHeight, title,
                    GlfwMonitorPtr.Null, GlfwWindowPtr.Null);
                HasFocus = true;

                // TODO: check if window was initialized correctly

                // Set the gl context
                Glfw.MakeContextCurrent(handle);

                // Get the primary monitor
                primaryMonitor = Glfw.GetMonitors()[0];
                primaryMonitorVideoMode = Glfw.GetVideoMode(primaryMonitor);

                Glfw.GetWindowSize(handle, out width, out height);
                Center();

                // Setup window events
                Glfw.SetScrollCallback(handle, OnInputScroll);
                Glfw.SetCursorPosCallback(handle, OnMouseMove);
                Glfw.SetWindowSizeCallback(handle, OnSizeChanged);
                Glfw.SetWindowFocusCallback(handle, OnWindowFocusChanged);
                Glfw.SetKeyCallback(handle, OnInputKeyChanged);
                Input.Initialize(this);

                // Set defaults and load
                SetVSync(false);

                Renderer = new MasterRenderer(width, height, startOptions);
                InitOpenAL();

                GLError.Begin();
                Load();
                ErrorCode initError = GLError.End();
                if (initError != ErrorCode.NoError)
                    throw new Exception(string.Format("Uncaught opengl initialization error! {0}", initError));

                // Begin game loop
                double lastTime = Glfw.GetTime();

                while (!Glfw.WindowShouldClose(handle))
                {
                    double now = Glfw.GetTime();
                    float dt = (float)(now - lastTime);
                    lastTime = now;

                    // Process current deltatime
                    HandleFPS(dt);

                    // Check for input events before we call Input.Begin
                    Glfw.PollEvents();

                    // Check for window size change.
                    // We only call the OnResized event here so that 
                    // when a user is custom resizing it doesn't get invoked
                    // a thousand times.
                    if (lastDrawnWidth != width || lastDrawnHeight != height)
                    {
                        lastDrawnWidth = width;
                        lastDrawnHeight = height;
                        OnSafeResized();
                    }

                    // Update
                    Input.Begin();
                    Renderer.Update(dt);
                    Update(dt);
                    Input.End();

                    // Draw
                    Renderer.Prepare();
                    Draw(dt);
                    Renderer.Render(dt);

                    // Check for any uncaught opengl exceptions
                    ErrorCode glError = GL.GetError();
                    if (glError != ErrorCode.NoError)
                        throw new Exception(string.Format("Uncaught OpenGL Error: {0}", glError));

                    // Draw the buffers
                    Glfw.SwapBuffers(handle);

                    if (!vsyncEnabled)
                    {
                        // Sleep to avoid cpu cycle burning
                        double startSleepNow = Glfw.GetTime();
                        double timeToWait = targetDeltaTime - (startSleepNow - now);

                        while (timeToWait > 0)
                        {
                            Thread.Sleep(0);
                            double sleepNow = Glfw.GetTime();
                            timeToWait -= (sleepNow - startSleepNow);
                            startSleepNow = sleepNow;
                        }
                    }
                }

                Unload();
            }
            finally
            {
                Glfw.DestroyWindow(handle);
            }
        }

        void OnError(GlfwError code, string desc)
        {
            if (code != GlfwError.NoError)
                throw new Exception(string.Format("Glfw Error ({0}): {1}", code, desc));
        }

        void OnInputScroll(GlfwWindowPtr _, double x, double y)
        {
            if (OnScroll != null)
                OnScroll(this, (int)x, (int)y);
        }

        void OnMouseMove(GlfwWindowPtr _, double x, double y)
        {
            if (OnCursorMoved != null)
                OnCursorMoved(this, (int)x, (int)y);
        }

        void OnSizeChanged(GlfwWindowPtr _, int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        void OnSafeResized()
        {
            Resized(width, height);
            Renderer.OnResize(width, height);

            if (OnResized != null)
                OnResized(this, width, height);
        }

        void OnWindowFocusChanged(GlfwWindowPtr _, bool focused)
        {
            HasFocus = focused;

            FocusChanged(focused);

            if (OnFocusChanged != null)
                OnFocusChanged(this, focused);
        }

        void OnInputKeyChanged(GlfwWindowPtr _, Key key, int scancode, KeyAction action, KeyModifiers modifiers)
        {
            if (OnKeyAction != null)
                OnKeyAction(this, key, scancode, action, modifiers);
        }

        public void Center()
        {
            Glfw.SetWindowPos(handle, 
                ScreenResolutionWidth / 2 - width / 2, 
                ScreenResolutionHeight / 2 - height / 2);
        }

        public void SetSize(int width, int height)
        {
            Glfw.SetWindowSize(handle, width, height);
        }

        public bool GetVSync()
        {
            return vsyncEnabled;
        }

        public void SetVSync(bool on)
        {
            vsyncEnabled = on;
            Glfw.SwapInterval(on ? 1 : 0);
        }

        public void Exit()
        {
            Glfw.SetWindowShouldClose(handle, true);
        }

        public bool GetKey(Key key)
        {
            return Glfw.GetKey(handle, key);
        }

        public GlfwWindowPtr GetPointer()
        {
            return handle;
        }

        #region FPS Management
        float GetMeanTime()
        {
            float total = 0;
            for (int i = 0; i < fps_times.Length; i++)
                total += fps_times[i];

            return total / fps_times.Length;
        }

        void AddTime(float time)
        {
            fps_times[fps_timeI++] = time;
        }

        void HandleFPS(float ifps)
        {
            if (fps_timeI < fps_times.Length)
                AddTime(ifps);
            else
            {
                fps_timeI = 0;
                FPS = 1.0f / GetMeanTime();
            }
        }
        #endregion

        protected virtual void Load() { }
        protected virtual void Unload() { }

        protected virtual void Resized(int width, int height) { }
        protected virtual void FocusChanged(bool focused) { }

        protected abstract void Update(float deltaTime);
        protected abstract void Draw(float deltaTime);

        public void Dispose()
        {
            Glfw.Terminate();
        }
    }
}
