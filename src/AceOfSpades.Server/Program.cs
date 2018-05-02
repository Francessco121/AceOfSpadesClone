using Dash.Engine.Diagnostics;
using Dash.Engine.IO;
using System;
using System.IO;
using System.Reflection;

namespace AceOfSpades.Server
{
    class Program
    {
        public static ConfigFile Config { get; private set; }

        static void Main(string[] args)
        {
            DashCMD.Start();
            DashCMD.Title = "Ace Of Spades Server";
            DashCMD.WriteLine("Game Version: {0}", ConsoleColor.Magenta, GameVersion.Current);

            SimulatedGame game = null;

            ProgramExceptionHandler.RunMainWithHandler(
                () => // tryAction
                {
                    LoadServerConfig();

                    game = new ServerGame();
                    game.Start("AOSServer Game Thread");

                    DashCMD.Listen(false);
                },
                () => // finallAyction
                {
                    DashCMD.Stop();
                },
                () => // shutdownAction
                {
                    if (game.IsRunning)
                        game.Stop();

                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                });
        }

        static void LoadServerConfig()
        {
            CreateServerConfigIfMissing();
            Config = new ConfigFile("server.cfg");
        }

        static void CreateServerConfigIfMissing()
        {
            if (!File.Exists("server.cfg"))
            {
                try
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AceOfSpades.Server.cfg.server.default.cfg"))
                    using (FileStream fs = File.Open("./server.cfg", FileMode.Create, FileAccess.Write))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fs);
                    }

                    DashCMD.WriteImportant("Created default server.cfg");
                }
                catch (Exception e)
                {
                    DashCMD.WriteError("Failed to create server.cfg!");
                    DashCMD.WriteError(e);
                }
            }
        }
    }
}
