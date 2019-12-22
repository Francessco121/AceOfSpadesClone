using AceOfSpades.Client.Gui;
using AceOfSpades.Client.Net;
using AceOfSpades.Graphics;
using AceOfSpades.Graphics.Renderers;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Audio;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.IO;
using Dash.Engine.Physics;
using Dash.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

/* MainWindow.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public class MainWindow : GameWindow
    {
        public StaticGui StaticGui { get; private set; }

        float lastDeltaTime;

        GameScreen activeScreen;
        Dictionary<string, GameScreen> screens;

        string switchToScreen;
        object[] screenArgs;

        bool debugRenderBoundingBoxes;

        public MainWindow(GraphicsOptions options)
            : base("Ace of Spades", 960, 540, true, options)
        {
            GlobalNetwork.IsClient = true;
            screens = new Dictionary<string, GameScreen>();

            DashCMD.Title = "Console: Ace of Spades Client";
            DashCMD.WriteLine("Game Version: {0}", ConsoleColor.Magenta, GameVersion.Current);
        }

        void AddScreen(GameScreen screen)
        {
            screens.Add(screen.Name, screen);
        }

        public void SwitchScreen(string name, params object[] args)
        {
            screenArgs = args;
            switchToScreen = name;
        }

        void SwitchScreen()
        {
            if (switchToScreen == null)
                return;

            GameScreen screen;
            if (screens.TryGetValue(switchToScreen, out screen))
            {
                switchToScreen = null;

                if (activeScreen != null)
                    activeScreen.Unload();

                activeScreen = screen;
                activeScreen.Load(screenArgs);
            }
            else
            {
                switchToScreen = null;
                throw new KeyNotFoundException(string.Format("Failed to switch screen. Screen '{0}' does not exist!",
                    switchToScreen));
            }
        }

        void LoadFromConfig()
        {
            ConfigSection graphics = Program.ConfigFile.GetSection("Graphics");
            if (graphics == null)
            {
                SetVSync(false);
                TargetFrameRate = 60;
            }
            else
            {
                SetVSync(graphics.GetBoolean("vsync") ?? false);
                TargetFrameRate = graphics.GetInteger("target-fps") ?? 60;

                int? fov = graphics.GetInteger("fov");

                if (fov != null)
                {
                    Camera.Active.DefaultFOV = fov.Value;
                    Camera.Active.FOV = fov.Value;
                }
            }

            ConfigSection input = Program.ConfigFile.GetSection("Input");
            if (input != null)
            {
                float? fpsSens = input.GetFloat("fps-mouse-sensitivity");
                float? arcBallSens = input.GetFloat("free-cam-mouse-sensitivity");

                if (fpsSens != null)
                {
                    Camera.Active.DefaultFPSMouseSensitivity = fpsSens.Value;
                    Camera.Active.FPSMouseSensitivity = fpsSens.Value;
                }

                if (arcBallSens != null)
                {
                    Camera.Active.DefaultArcBallMouseSensitivity = arcBallSens.Value;
                    Camera.Active.ArcBallMouseSensitivity = arcBallSens.Value;
                }
            }
        }

        protected override void Load()
        {
            base.Load();
            LogOpenGLDrivers();

            //if (AL.Efx == null)
            //    throw new Exception("Sound card does not support OpenAL Efx!");

            AL.DistanceModel(ALDistanceModel.LinearDistance);

            // 1 meter = 1 block
            Camera.Active.AudioListener.EfxMetersPerUnit = 1f / Block.CUBE_SIZE;

            Camera.Active.AudioListener.Gain = 0.5f;

            LoadFromConfig();

            DashCMD.SetCVar("r_vsync", GetVSync());
            DashCMD.SetCVar("r_targfps", TargetFrameRate);
            DashCMD.SetCVar("r_exp_shadows", false);

            GLError.Begin();
            Renderer.AddRenderer(new VoxelRenderer(Renderer));
            Renderer.AddRenderer(new EntityRenderer(Renderer));
            Renderer.AddRenderer(new ChunkRenderer(Renderer));
            Renderer.AddRenderer(new DebugRenderer(Renderer));

            Light sun = new Light(new Vector3(2, 1, 2), LightType.Directional, 1.75f, new Color(255, 255, 255, 255));
            Renderer.Lights.Add(sun);
            Renderer.Sun = sun;

            Camera.Active.SetMode(CameraMode.ArcBall);
            Camera.Active.SmoothCamera = true;

            StaticGui = new StaticGui(this, Renderer);

            SetupDefaultBinds();

            AddScreen(new MainMenuScreen(this));
            AddScreen(new SingleplayerScreen(this));
            AddScreen(new MultiplayerScreen(this));
            AddScreen(new NewText.NewTextScreen(this));

            DashCMD.AddScreen(new DashCMDScreen("dt", "", true,
                (screen) =>
                {
                    screen.WriteLine("DeltaTime: {0}s", lastDeltaTime);
                })
            {
                SleepTime = 30
            });

#if DEBUG
            DashCMD.AddCommand("connect",
                "Connects to a server",
                "connect <ip:port>",
                (args) =>
                {
                    if (args.Length < 1)
                    {
                        DashCMD.ShowSyntax("connect");
                        return;
                    }

                    string[] parts = args[0].Split(':');
                    if (parts.Length != 2)
                    {
                        DashCMD.WriteError("Invalid arguments. (connect ip:port)");
                        return;
                    }

                    IPAddress ip;
                    if (!NetHelper.TryParseIP(parts[0], out ip))
                    {
                        DashCMD.WriteError("Invalid ip address");
                        return;
                    }

                    int port;
                    if (!int.TryParse(parts[1], out port))
                    {
                        DashCMD.WriteError("Invalid port.");
                        return;
                    }

                    SwitchScreen("Multiplayer", new IPEndPoint(ip, port), "TestPlayer1");
                });
#endif
            SwitchScreen("MainMenu");
        }
        //AudioSource source;

        /*void TestAudio()
        {
            //AL.DistanceModel(ALDistanceModel.LinearDistance);

            // 1 meter = 1 block
            Camera.Active.AudioListener.EfxMetersPerUnit = 1f / Block.CUBE_SIZE;

            AudioBuffer buffer = new OggFile("Content/Sounds/Keep_Dancing_mono.ogg");

            source = new AudioSource(buffer);
            //source.MaxDistance = 400;
            source.Position = new Vector3(0, 300, 0);
            //source.Play();

            AudioSource source2 = new AudioSource(buffer);
            source2.MaxDistance = 400;
            source2.Position = new Vector3(200, 300, 200);
            //source2.Play();



            // Create Auxiliary effect slot
            int auxEffectSlot = AL.Efx.GenAuxiliaryEffectSlot();
            AL.Utils.CheckError();


            // Create effect
            int effect = AL.Efx.GenEffect();
            AL.Utils.CheckError();

            // Set effect to reverb and set decay time
            List<EfxEffectType> supportedTypes = new List<EfxEffectType>();

            foreach (object obj in Enum.GetValues(typeof(EfxEffectType)))
            {
                AL.GetError();
                AL.Efx.Effect(effect, EfxEffecti.EffectType, (int)obj);
                if (AL.GetError() == ALError.NoError)
                {
                    supportedTypes.Add((EfxEffectType)obj);
                    DashCMD.WriteImportant("Supports effect {0}", obj);
                }
            }

            AL.Efx.Effect(effect, EfxEffecti.EffectType, (int)EfxEffectType.Equalizer);
            AL.Utils.CheckError();
            //AL.Efx.Effect(effect, EfxEffectf.ReverbDecayTime, 8f);
            //AL.Efx.Effect(effect, EfxEffectf.reverb, 6f);
            //AL.Efx.Effect(effect, EfxEffectf.ReverbDiffusion, 0);
            //AL.Efx.Effect(effect, EfxEffectf.ReverbAirAbsorptionGainHF, 2);
            //AL.Efx.Effect(effect, EfxEffectf.EqualizerLowGain, 7.8f);
            AL.Efx.Effect(effect, EfxEffectf.EqualizerLowGain, 3f);
            AL.Efx.Effect(effect, EfxEffectf.EqualizerHighGain, 5f);
            AL.Utils.CheckError();

            // Create a filter
            int filter = AL.Efx.GenFilter();
            AL.Utils.CheckError();

            // Set Filter to low-pass
            AL.Efx.Filter(filter, EfxFilteri.FilterType, (int)EfxFilterType.Lowpass);
            AL.Utils.CheckError();
            AL.Efx.Filter(filter, EfxFilterf.LowpassGain, 0.5f);
            AL.Efx.Filter(filter, EfxFilterf.LowpassGainHF, 1);
            AL.Utils.CheckError();


            // Attach effect to aux slot
            AL.Efx.AuxiliaryEffectSlot(auxEffectSlot, EfxAuxiliaryi.EffectslotEffect, effect);
            AL.Utils.CheckError();


            // Set source send 0 to feed effect without filtering
            //AL.Source(source.SourceId, ALSourcei.EfxDirectFilter, filter);
            AL.Source(source.SourceId, ALSource3i.EfxAuxiliarySendFilter, auxEffectSlot, 0, 0);
            AL.Utils.CheckError();


            source.Play();
        }*/

        void SetupDefaultBinds()
        {
            // Movement
            Input.Bind("MoveForward", Key.W);
            Input.Bind("MoveLeft", Key.A);
            Input.Bind("MoveBackward", Key.S);
            Input.Bind("MoveRight", Key.D);
            Input.Bind("Jump", Key.Space);
            Input.Bind("Crouch", Key.LeftControl);
            Input.Bind("Sprint", Key.LeftShift);
            Input.Bind("Walk", Key.LeftAlt);

            // Combat
            Input.Bind("PrimaryFire", MouseButton.Left);
            Input.Bind("SecondaryFire", MouseButton.Right);
            Input.Bind("Reload", Key.R);
            Input.Bind("ToggleFlashlight", Key.F);
            Input.Bind("PickColor", MouseButton.Middle);
            Input.Bind("DropIntel", Key.T);
            //Input.Bind("ToggleGameIcons", Key.H);

            // Misc
            Input.Bind("ToggleMenu", Key.Escape);
            Input.Bind("ShowLeaderboard", Key.Tab);
            Input.Bind("Chat", Key.Enter);
            Input.Bind("1080p Screenshot", Key.F9);
            Input.Bind("1440p Screenshot", Key.F10);
            Input.Bind("Hide UI", Key.F11);
        }

        protected override void Unload()
        {
            if (activeScreen != null)
                activeScreen.Unload();

            base.Unload();
        }

        protected override void Resized(int width, int height)
        {
            DashCMD.WriteLine("Resized({0},{1})", width, height);

            if (activeScreen != null)
                activeScreen.OnScreenResized(width, height);
        }

        protected override void Update(float deltaTime)
        {
            SwitchScreen();

            lastDeltaTime = deltaTime;

            StaticGui.Update(deltaTime);

            if (AOSClient.Instance != null)
                AOSClient.Instance.Update(deltaTime);

            if (activeScreen != null)
                activeScreen.Update(deltaTime);

            if (Input.GetControlDown("Hide UI"))
                Renderer.Gui.Hide = !Renderer.Gui.Hide;

            bool r_vsync = DashCMD.GetCVar<bool>("r_vsync");
            if (r_vsync != GetVSync())
                SetVSync(r_vsync);

            TargetFrameRate = DashCMD.GetCVar<float>("r_targfps");
            ShadowCamera.UseExperimental = DashCMD.GetCVar<bool>("r_exp_shadows");

            //if (Input.GetKey(Key.K)) source.Pitch += 0.01f;
            //if (Input.GetKey(Key.J)) source.Pitch -= 0.01f;

            if (Input.GetControlDown("1080p Screenshot")) TakeScreenshot(1920, 1080);
            if (Input.GetControlDown("1440p Screenshot")) TakeScreenshot(2560, 1440);
            //if (Input.GetKeyDown(Key.F12)) TakeScreenshot(8192, 4608);

            if (Input.GetKeyDown(Key.F6))
                debugRenderBoundingBoxes = !debugRenderBoundingBoxes;
            PhysicsEngine.RecordProcessedBoundingBoxes = debugRenderBoundingBoxes;
        }

        void TakeScreenshot(int width, int height)
        {
            if (!Directory.Exists("Screenshots"))
                Directory.CreateDirectory("Screenshots");

            HashSet<string> existing = new HashSet<string>(Directory.GetFiles("Screenshots"));
            DateTime now = DateTime.Now;
            string fileName;
            int i = 0;
            do
            {
                fileName = string.Format("Screenshot_{0}-{1}-{2}_{3}", now.Month, now.Day, now.Year, i++);
            } while (existing.Contains("Screenshots\\" + fileName + ".png"));

            Renderer.TakeScreenshot(Path.Combine("Screenshots", fileName), width, height);
        }

        protected override void Draw(float deltaTime)
        {
            if (activeScreen != null)
                activeScreen.Draw();

            if (debugRenderBoundingBoxes)
                DrawPhysicsEngineBoundingBoxes();
        }

        void DrawPhysicsEngineBoundingBoxes()
        {
            DebugRenderer r = Renderer.GetRenderer3D<DebugRenderer>();

            foreach (AxisAlignedBoundingBox p in PhysicsEngine.LastProcessedBoundingBoxes)
                r.Batch(p);
        }
    }
}
