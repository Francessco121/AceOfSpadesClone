using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.IO;
using Dash.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

/* ServerGame.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class ServerGame : SimulatedGame
    {
        public static IPAddress HostIP = IPAddress.Loopback;
        public static int HostPort = 12123;
        
        AOSServer server;

        float lastDeltaTime;

        GameScreen activeScreen;
        Dictionary<string, GameScreen> screens;

        public ServerGame()
            : base(120)
        {
            screens = new Dictionary<string, GameScreen>();

            ConfigSection serverSection = Program.Config.GetSection("Server");
            
            int maxPlayers = 32;

            if (serverSection == null)
                DashCMD.WriteError("[server.cfg - ServerGame] Section 'Server' is missing!");
            else
            {
                maxPlayers = serverSection.GetInteger("max-players") ?? 32;

                ConfigSection socketSection = serverSection.GetSection("Socket");

                if (socketSection == null)
                    DashCMD.WriteError("[server.cfg - ServerGame] Section 'Socket' is missing!");
                else
                {
                    string ip = socketSection.GetString("host-ip") ?? "127.0.0.1";
                    int port = socketSection.GetInteger("host-port") ?? 12123;

                    if (!NetHelper.TryParseIP(ip, out HostIP))
                        DashCMD.WriteError("[server.cfg - ServerGame] Socket.host-ip is invalid!");
                }
            }

            if (!AOSServer.Initialize(maxPlayers, new IPEndPoint(HostIP, HostPort)))
            {
                DashCMD.WriteError("Failed to initialize server!");
                DashCMD.StopListening();
                return;
            }

            server = AOSServer.Instance;
            InitializeDebugging();

            AddScreen(new MatchScreen(this));

            SwitchScreen("Match");
        }

        void InitializeDebugging()
        {
            DashCMD.AddCommand("endpoint", "Displays the server's ip endpoint.",
                (args) =>
                {
                    DashCMD.WriteLine("Bound IPEndPoint: {0}", AOSServer.Instance.BoundEndPoint);
                    DashCMD.WriteLine("Receive IPEndPoint: {0}", AOSServer.Instance.ReceiveEndPoint);
                    DashCMD.WriteLine("");
                });

            DashCMD.AddCommand("exit", "Stops the server.",
               (args) =>
               {
                   DashCMD.WriteImportant("Shutting down server...");
                   AOSServer.Instance.Shutdown("Server shutting down...");
                   Stop();
                   DashCMD.Stop();
               });

            DashCMD.AddScreen(new DashCMDScreen("dt", "", true,
                (screen) =>
                {
                    screen.WriteLine("DeltaTime: {0}s", lastDeltaTime);
                })
            {
                SleepTime = 30
            });

            DashCMD.AddScreen(new DashCMDScreen("network", "", true,
                (screen) =>
                {
                    screen.WriteLine("General Stats:", ConsoleColor.Green);
                    screen.WriteLine("Heartbeat Compution Time: {0}ms", AOSServer.Instance.HeartbeatComputionTimeMS);

                    int totalPhysicalPS = 0;
                    int totalVirtualPS = 0;

                    foreach (NetConnection client in server.Connections.Values)
                    {
                        totalPhysicalPS += client.Stats.PhysicalPacketsReceivedPerSecond;
                        totalVirtualPS += client.Stats.PacketsReceivedPerSecond;
                    }

                    screen.WriteLine("Total PPackets r/s: {0}", totalPhysicalPS);
                    screen.WriteLine("Total VPackets r/s: {0}", totalVirtualPS);

                    screen.WriteLine("");
                    foreach (NetConnection client in server.Connections.Values)
                    {
                        screen.WriteLine("[{0}]:", ConsoleColor.Green, client);
                        screen.WriteLine("Send Rate: {0}", client.PacketSendRate);
                        screen.WriteLine("MTU: {0}", client.Stats.MTU);
                        screen.WriteLine("Ping: {0}", client.Stats.Ping);
                        screen.WriteLine("VPackets s/s: {0}", client.Stats.PacketsSentPerSecond);
                        screen.WriteLine("VPackets r/s: {0}", client.Stats.PacketsReceivedPerSecond);
                        screen.WriteLine("Packets Lost: {0}", client.Stats.PacketsLost);
                        screen.WriteLine("PPackets s/s: {0}", client.Stats.PhysicalPacketsSentPerSecond);
                        screen.WriteLine("PPackets r/s: {0}", client.Stats.PhysicalPacketsReceivedPerSecond);
                    }
                }));
        }

        void AddScreen(GameScreen screen)
        {
            screens.Add(screen.Name, screen);
        }

        public void SwitchScreen(string name, params object[] args)
        {
            GameScreen screen;
            if (screens.TryGetValue(name, out screen))
            {
                if (activeScreen != null)
                    activeScreen.Unload();

                activeScreen = screen;
                activeScreen.Load(args);
            }
            else
                throw new KeyNotFoundException(string.Format("Failed to switch screen. Screen '{0}' does not exist!", name));
        }

        protected override void OnStopped()
        {
            if (activeScreen != null)
                activeScreen.Unload();
        }

        protected override void Update(float deltaTime)
        {
            lastDeltaTime = deltaTime;

            AOSServer.Instance.Update(deltaTime);

            if (activeScreen != null)
                activeScreen.Update(deltaTime);
        }
    }
}
