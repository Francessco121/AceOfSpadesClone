using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.IO;
using System.IO;
using System.Reflection;

/* Program.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    class Program
    {
        public static ConfigFile ConfigFile { get; private set; }

        static void Main(string[] args)
        {
            LoadConfig();

            if (GetConfigString("Debug/console") == "true")
            {
                DashCMD.Start(true);
                DashCMD.Listen(true);

                DashCMD.WriteStandard("Loaded config '{0}'", ConfigFile.Name);
            }

            ProgramExceptionHandler.RunMainWithHandler(tryAction: () =>
            {
                ConfigSection gfx = GetConfigSection("Graphics");

                if (gfx == null)
                    DashCMD.WriteError("[game.cfg] Graphics section was not found!");

                GraphicsOptions options = gfx != null ? GraphicsOptions.Init(gfx) : null;

                using (MainWindow window = new MainWindow(options))
                    window.Run(60);
            },
            finallyAction: DashCMD.Stop,
            shutdownAction: () => { });
        }

        public static string GetConfigString(string path)
        {
            if (ConfigFile == null)
                return null;

            object v = ConfigFile.Navigate(path);
            return v != null ? v as string : null;
        }

        public static ConfigSection GetConfigSection(string path)
        {
            if (ConfigFile == null)
                return null;

            object v = ConfigFile.Navigate(path);
            return v != null ? v as ConfigSection : null;
        }

        static void LoadConfig()
        {
            string pathUsed;
            string file = ReadConfigFile(out pathUsed);
            ConfigFile = new ConfigFile(file, pathUsed);

#if !DEBUG
            if (!File.Exists("./game.cfg"))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AceOfSpades.Client.cfg.game.default.cfg"))
                using (FileStream fs = File.Open("./game.cfg", FileMode.Create, FileAccess.Write))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fs);
                }
            }
#endif
        }

        static string ReadConfigFile(out string pathUsed)
        {
            
#if DEBUG
            string resourceName = "AceOfSpades.Client.cfg.game.debug.cfg";
            pathUsed = resourceName;

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

#else
            string resourceName = "AceOfSpades.Client.cfg.game.default.cfg";
            string physicalPath = "./game.cfg";

            Stream stream;
            if (File.Exists(physicalPath))
            {
                pathUsed = physicalPath;
                stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                pathUsed = resourceName;
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            }
#endif

            using (stream)
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
