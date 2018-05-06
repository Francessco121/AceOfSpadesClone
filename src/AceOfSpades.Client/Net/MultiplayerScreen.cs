using AceOfSpades.Client.Gui;
using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using Dash.Net;
using System;
using System.Collections.Generic;
using System.Net;

/* MultiplayerScreen.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class MultiplayerScreen : NetworkedScreen
    {
        public MPWorld World { get; private set; }
        DebugRenderer debugRenderer;

        Handshake handshake;

        Dictionary<GamemodeType, NetworkedGamemode> gamemodes;
        NetworkedGamemode currentGamemode;

        HUD hud;
        BMPFont font;
        MultiplayerLoadingBar loadingBar;
        MultiplayerMenu menu;
        Leaderboard leaderboard;
        ChatBox chat;
        GUILabel announcementLabel;
        GUITheme theme;

        string playerName;

        string message;
        float messageTime;
        float announcementTime;

        public MultiplayerScreen(MainWindow window)
            : base(window, "Multiplayer")
        {
            debugRenderer = Renderer.GetRenderer3D<DebugRenderer>();

            gamemodes = new Dictionary<GamemodeType, NetworkedGamemode>()
            {
                { GamemodeType.TDM, new TDMGamemode(this) },
                { GamemodeType.CTF, new CTFGamemode(this) }
            };

            // Build the UI elements
            theme = AssetManager.CreateDefaultGameTheme();
            font = theme.GetField<BMPFont>(null, "SmallFont");

            hud = new HUD(Renderer);
            loadingBar = new MultiplayerLoadingBar(GUISystem, theme);
            chat = new ChatBox(new UDim2(0, 40, 1f, -240), new UDim2(0, 350, 0, 165), theme, this);
            menu = new MultiplayerMenu(GUISystem, theme, Window);
            menu.OnClosed += Menu_OnClosed;
            announcementLabel = new GUILabel(new UDim2(0.5f, 0, 0.5f, 0), UDim2.Zero, "", TextAlign.Center, theme);
            announcementLabel.Font = AssetManager.LoadFont("karmasuture-32");
            announcementLabel.Visible = false;

            // Add each UI element
            GUIArea.AddTopLevel(chat, announcementLabel);
            GUISystem.Add(loadingBar, menu);
            Windows.Add(loadingBar);
            Windows.Add(menu);

            // Setup default multiplayer cvars
            DashCMD.SetCVar("cl_impacts", false);
            DashCMD.SetCVar("cl_interp", 0.5f); // Client interpolation with server position
            DashCMD.SetCVar("cl_interp_movement_smooth", 1f); // Client player movement smoothing (1f = no smoothing)
            DashCMD.SetCVar("cl_interp_rep", 20f); // Replicated entities interpolation
            DashCMD.SetCVar("cl_max_error_dist", 12f); // Max distance the client's position can be off from the server's
        }

        protected override void OnLoad(object[] args)
        {
            // By default we want the menu to not be visible
            menu.Visible = false;

            // Hook into window focus event so we can
            // toggle the menu when needed
            Window.OnFocusChanged += Window_OnFocusChanged;

            // Attempt to grab the server address and playername
            // from the args.
            IPEndPoint endPoint;
            if (args.Length != 2 
                || (endPoint = args[0] as IPEndPoint) == null 
                || (playerName = args[1] as string) == null)
                throw new ArgumentException("MultiplayerScreen requires a IPEndPoint and a string argument.");

            // Attempt to connect to the server
            NetDenialReason? denialReason;
            if (TryConnect(endPoint, out denialReason))
            {
                // Hook into disconnected event so we can handle
                // unexpected disconnections
                client.OnDisconnected += Client_OnDisconnected;

                // Hook into packet receiving
                client.AddPacketHook(OnCustomPacket);

                // Hook into component events
                objectComponent.OnCreatableInstantiated += ObjectComponent_OnCreatableInstantiated;
                objectComponent.OnCreatableDestroyed += ObjectComponent_OnCreatableDestroyed;

                // Show the loading bar
                loadingBar.ClearAndShow();

                // Create the world and hook into it's events
                World = new MPWorld(Renderer);
                hud.SetWorld(World);

                // Send our information to the server
                netPlayerComponent.SendClientInfo(playerName);

                // Clear the chat
                chat.Clear();
            }
            else
                // If we fail to connect, just kickback to the MainMenu with a message.
                Window.SwitchScreen("MainMenu", "Failed to connect to server: " + denialReason.Value.ToString());

            base.OnLoad(args);
        }

        private void ObjectComponent_OnCreatableInstantiated(object sender, NetCreatableInfo e)
        {
            ClientPlayer player = e.Creatable as ClientPlayer;
            if (player != null && player.StateInfo.IsAppOwner)
            {
                // Setup input based on menu state
                ToggleFPSUserInput(!menu.Visible, (ClientMPPlayer)player);

                // Setup camera with object
                Camera.Active.SetMode(CameraMode.FPS);
                Camera.Active.LockedToTransform = player.Transform;

                hud.Player = player;
                hud.ShowCharacterInformation = true;
            }
        }

        public HUD GetHUD()
        {
            return hud;
        }

        private void ObjectComponent_OnCreatableDestroyed(object sender, NetCreatableInfo e)
        {
            ClientPlayer player = e.Creatable as ClientPlayer;
            if (player != null && player.StateInfo.IsAppOwner)
            {
                // Unlock cursor on player destroyed
                Input.IsCursorLocked = false;
                Input.IsCursorVisible = true;
                Camera.Active.HoldM2ToLook = true;

                Camera.Active.FOV = Camera.Active.DefaultFOV;
                Camera.Active.ArcBallMouseSensitivity = Camera.Active.DefaultArcBallMouseSensitivity;
                Camera.Active.FPSMouseSensitivity = Camera.Active.DefaultFPSMouseSensitivity;
            }
        }

        public override void Unload()
        {
            // Disconnect the ondisconnected event so on a normal leave,
            // we don't display a message saying we lost connection.
            client.OnDisconnected -= Client_OnDisconnected;

            // Unhook from packet receiving
            client.RemovePacketHook(OnCustomPacket);

            // Unload the networked screen causing the client to disconnect.
            base.Unload();
        }

        protected override void OnUnload()
        {
            // Unhook from the window focus event so we don't
            // randomly change the camera and input settings outside
            // of this screen.
            Window.OnFocusChanged -= Window_OnFocusChanged;

            // Unhook from component events
            objectComponent.OnCreatableInstantiated -= ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed    -= ObjectComponent_OnCreatableDestroyed;

            // Stop the gamemode
            if (currentGamemode != null)
            {
                currentGamemode.Stop();
                currentGamemode = null;
                hud.SetGamemode(null);
            }

            // Unload the world
            UnloadWorld();

            // Disable the hud
            hud.Disable();

            // Hide the winner label
            announcementTime = 0;

            base.OnUnload();
        }

        private void Menu_OnClosed(GUIWindowBase e)
        {
            // Re-enable user input when the menu closes
            ToggleFPSUserInput(true);
        }

        private void Window_OnFocusChanged(GameWindow window, bool focused)
        {
            if (!focused)
            {
                // Open the menu when the window loses focus
                menu.Visible = true;
                ToggleFPSUserInput(false);
            }
        }

        void ToggleFPSUserInput(bool enabled)
        {
            ToggleFPSUserInput(enabled, World != null ? World.OurPlayer : null);
        }

        void ToggleFPSUserInput(bool enabled, ClientMPPlayer ourPlayer)
        {
            if (ourPlayer != null)
            {
                ourPlayer.AllowUserInput = enabled;
                Input.IsCursorLocked = enabled;
                Input.IsCursorVisible = !enabled;
                Camera.Active.HoldM2ToLook = !enabled;
                Camera.Active.SmoothCamera = !enabled;
            }
            else
            {
                // We only want to lock the cursor if we actually
                // have a player.
                Input.IsCursorLocked = false;
                Input.IsCursorVisible = true;
                Camera.Active.HoldM2ToLook = true;
                Camera.Active.SmoothCamera = true;
            }

            Camera.Active.AllowUserControl = enabled;
        }

        private void Client_OnDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            Window.SwitchScreen("MainMenu", lostConnection ? "Lost Connection to Server" : string.Format("Lost Connection: {0}", reason));
        }

        public override void OnScreenResized(int width, int height)
        {
            if (World != null)
                World.OnScreenResized(width, height);

            base.OnScreenResized(width, height);
        }

        void InitializeCMD()
        {
            NetLogger.LogObjectStateChanges = true;
            NetLogger.LogVerboses = true;

            DashCMD.AddScreen(new DashCMDScreen("network", "", true,
                (screen) =>
                {
                    screen.WriteLine("Heartbeat Compution Time: {0}ms", client.HeartbeatComputionTimeMS);
                    screen.WriteLine("Send Rate: {0}", client.ServerConnection.PacketSendRate);
                    screen.WriteLine("MTU: {0}", client.ServerConnection.Stats.MTU);
                    screen.WriteLine("Ping: {0}", client.ServerConnection.Stats.Ping);
                    screen.WriteLine("VPackets s/s: {0}", client.ServerConnection.Stats.PacketsSentPerSecond);
                    screen.WriteLine("VPackets r/s: {0}", client.ServerConnection.Stats.PacketsReceivedPerSecond);
                    screen.WriteLine("Packets Lost: {0}", client.ServerConnection.Stats.PacketsLost);
                    screen.WriteLine("PPackets s/s: {0}", client.ServerConnection.Stats.PhysicalPacketsSentPerSecond);
                    screen.WriteLine("PPackets r/s: {0}", client.ServerConnection.Stats.PhysicalPacketsReceivedPerSecond);
                }));
        }

        protected override void OnClientInitialized()
        {
            // Initialize remotes
            channel.AddRemoteEvent("Client_UnloadWorld", R_UnloadWorld);
            channel.AddRemoteEvent("Client_Announcement", R_Announcement);
            channel.AddRemoteEvent("Client_AddFeedItem", R_AddFeedItem);
            channel.AddRemoteEvent("Client_ChatItem", R_ChatItem);
            channel.AddRemoteEvent("Client_SwitchGamemode", R_SwitchGamemode);
            channel.AddRemoteEvent("Client_TeamWon", R_TeamWon);

            // Create the leaderboard
            leaderboard = new Leaderboard(GUISystem, theme, netPlayerComponent);
            GUISystem.Add(leaderboard);
            Windows.Add(leaderboard);

            // Enable some debugging
            InitializeCMD();

            base.OnClientInitialized();
        }

        bool TryConnect(IPEndPoint to, out NetDenialReason? denialReason)
        {
            if (client.Connect(to, out denialReason))
            {
                DashCMD.WriteImportant("Successfully connected to server!");
                return true;
            }
            else
            {
                DashCMD.WriteError("Failed to connect to server: {0}", denialReason.Value);
                return false;
            }
        }

        public void ChatOut(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                string fullMessage = string.Format("<{0}> {1}", playerName, message);
                channel.FireEvent("Server_ChatItem", client.ServerConnection, fullMessage);
            }

            ToggleFPSUserInput(!menu.Visible);
            chat.Unfocus();
        }

        public void ShowAnnouncement(string text, float time, bool centered)
        {
            announcementTime = time;
            announcementLabel.Text = text;
            if (centered)
                announcementLabel.Position = new UDim2(0.5f, 0, 0.5f, 0);
            else
                announcementLabel.Position = new UDim2(0.5f, 0, 0.25f, 0);
        }

        void R_ChatItem(NetConnection server, NetBuffer data, ushort numArgs)
        {
            string message = data.ReadString();
            chat.AddLine(message);
        }

        void R_TeamWon(NetConnection server, NetBuffer data, ushort numArgs)
        {
            Team winner = (Team)data.ReadByte();
            string text = winner == Team.A ? "Red Team Wins!" : winner == Team.B ? "Blue Team Wins!" : "It's a Tie!";
            ShowAnnouncement(text, 1000, true);
            hud.ShowCharacterInformation = false;

            ToggleFPSUserInput(true, null);
            Camera.Active.LockedToTransform = null;
            Camera.Active.SetMode(CameraMode.FPS);
            Camera.Active.HoldM2ToLook = true;
            Input.IsCursorLocked = false;
            Input.IsCursorVisible = true;
        }

        public void OnHandshakeDoneDownloading(HandshakeTerrainData terrainData)
        {
            // Start loading the world from the data
            // downloaded by the server.
            World.LoadServerTerrain(terrainData.SourceData);

            // Hide the background so the user can
            // watch the world load.
            StaticGui.ShowBackground = false;

            // Enable user input, so the user can fly around
            // while waiting for the world to finish loading
            ToggleFPSUserInput(true);
            Camera.Active.LockedToTransform = null;
            Camera.Active.SetMode(CameraMode.FPS);
            Camera.Active.HoldM2ToLook = true;
            Camera.Active.Position = new Vector3(32, 400, 32);
            Camera.Active.Yaw = 135;
            Camera.Active.Pitch = 20;

            Input.IsCursorLocked = false;
            Input.IsCursorVisible = true;

            // Tell the loading bar we've moved to the next stage.
            loadingBar.SwitchToTerrainLoading(World.Terrain);
        }

        public void OnHandshakeComplete()
        {
            // we gud
            handshake = null;
            hud.Enable();
        }

        bool OnCustomPacket(NetInboundPacket packet, CustomPacketType type)
        {
            if (type == CustomPacketType.HandshakeInitiate)
            {
                if (handshake != null)
                {
                    DashCMD.WriteError("Got handshake initiate packet, but we are already in a handshake!");
                    return false;
                }

                // Begin the handshake on our end
                handshake = new Handshake(client, this, packet);
                loadingBar.SetHandshake(handshake);

                // We don't want to have the user staring at a blank skybox,
                // so lets show them pictures :D
                StaticGui.ShowBackground = true;
                return true;
            }
            else if (type == CustomPacketType.WorldSection)
            {
                if (handshake != null)
                    // Notify the handshake we have received another
                    // piece of the world data.
                    handshake.OnLevelChunkInbound(packet);
                else
                {
                    DashCMD.WriteError("Got handshake world section packet, but we are not in a handshake!");

                    // We did acknowledge the packet,
                    // but since we are not in the right state
                    // act as if its unknown (little extra protection
                    // from a rogue server).
                    return false;
                }

                return true;
            }

            return false;
        }

        void UnloadWorld()
        {
            // Reset some network values so this screen
            // can be reused
            handshake = null;

            // Dispose of the world if it exists
            if (World != null)
            {
                DashCMD.WriteImportant("[MultiplayerScreen] Unloading world...");
                World.Dispose();
            }

            hud.Disable();
            announcementTime = 0;
            objectComponent.HoldInstantiationPackets = true;
        }

        void R_SwitchGamemode(NetConnection server, NetBuffer data, ushort numArgs)
        {
            DashCMD.WriteStandard("Switching gamemode...");

            GamemodeType type = (GamemodeType)data.ReadByte();
            if (currentGamemode != null)
                currentGamemode.Stop();

            currentGamemode = null;
            hud.SetGamemode(null);

            NetworkedGamemode gamemode;
            if (gamemodes.TryGetValue(type, out gamemode))
            {
                currentGamemode = gamemode;
                currentGamemode.Start();
                hud.SetGamemode(gamemode);
                leaderboard.SetGamemode(gamemode);
                objectComponent.HoldInstantiationPackets = false;
            }
            else
            {
                string message = string.Format("Failed to switch to gamemode '{0}'!", type);
                DashCMD.WriteError("[MultiplayerScreen] {0}", message);
                client.Disconnect("Critical client-side error");
                Window.SwitchScreen("MainMenu", message);
            }
        }

        void R_Announcement(NetConnection server, NetBuffer data, ushort numArgs)
        {
            ShowMessage(data.ReadString(), data.ReadFloat());
        }

        void R_AddFeedItem(NetConnection server, NetBuffer data, ushort numArgs)
        {
            string left = data.ReadString();
            string leftAssist = data.ReadString();
            Color leftColor = new Color(data.ReadByte(), data.ReadByte(), data.ReadByte());
            string middle = data.ReadString();
            string right = data.ReadString();
            Color rightColor = new Color(data.ReadByte(), data.ReadByte(), data.ReadByte());

            hud.AddFeedItem(left, leftAssist, leftColor, "[ " + middle + " ]", right, rightColor);
        }

        void R_UnloadWorld(NetConnection server, NetBuffer data, ushort numArgs)
        {
            // Unload the world
            UnloadWorld();

            // Kickback the UI to a loading state
            loadingBar.ClearAndShow();
            if (!StaticGui.ShowBackground)
                Window.StaticGui.ShowRandomBackgroundImage();
            StaticGui.ShowBackground = true;

            // Generate the new world object in preparation
            // for the new data from the server.
            World = new MPWorld(Renderer);
            hud.SetWorld(World);
        }

        public void ShowMessage(string message, float time)
        {
            this.message = message;
            messageTime = time;
        }

        public override void Update(float deltaTime)
        {
            // Complete handshake when terrain is done
            if (handshake != null && World.Terrain != null && World.Terrain.UnfinishedChunks == 0)
                handshake.Complete();

            // Toggle the menu via user input
            if (Input.GetControlDown("ToggleMenu"))
            {
                chat.Unfocus();
                menu.Visible = !menu.Visible;
                ToggleFPSUserInput(!menu.Visible);
            }

            // Toggle chat focus
            if (Input.GetControlDown("Chat"))
            {
                ToggleFPSUserInput(false);
                chat.Focus();
            }

            // Show the leaderboard via user input
            leaderboard.Visible = Input.GetControl("ShowLeaderboard");

            // Read terrain changes
            if (client != null && snapshotComponent.WorldSnapshot != null)
            {
                TerrainDeltaSnapshot terrainDelta = snapshotComponent.WorldSnapshot.TerrainSnapshot;

                // Simply apply each change sent by the server to the specified chunks.
                foreach (TerrainDeltaChange change in terrainDelta.ReceivedChanges)
                {
                    Chunk chunk;
                    if (World.Terrain.Chunks.TryGetValue(change.ChunkIndex, out chunk))
                        chunk.SetBlock(change.Block, change.BlockIndex);
                    else
                        DashCMD.WriteError("[AOSNet - TerrainDelta] Received update for non-existant chunk! IPos: {0}",
                            change.ChunkIndex);
                }

                // Clear the changes so we don't apply them twice
                terrainDelta.ReceivedChanges.Clear();
            }

            // Update the world
            if (World != null)
                World.Update(deltaTime);

            // Update the gamemode
            if (currentGamemode != null && currentGamemode.IsActive)
                currentGamemode.Update(deltaTime);

            // Update the hud
            hud.Update(deltaTime);

            // Run down the message timer if shown
            if (messageTime > 0)
                messageTime -= deltaTime;
            if (announcementTime > 0)
                announcementTime -= deltaTime;
        }

        public override void Draw()
        {
            if (World != null)
            {
                // Draw the world
                World.Draw();

                if (World.Terrain != null)
                {
                    // Show the name of the player the mouse is over
                    DrawMousedOverPlayerName();
                }

                // Show the current message
                if (messageTime > 0)
                {
                    font.DrawString(message, Renderer.ScreenWidth / 2f, 0, 
                        Renderer.Sprites.SpriteBatch, TextAlign.TopRight, Color.White, new Color(0, 0, 0, 0.6f));
                }

                announcementLabel.Visible = announcementTime > 0;
                hud.Draw(Renderer.Sprites.SpriteBatch);
            }
        }

        void DrawMousedOverPlayerName()
        {
            WorldRaycastResult result = World.Raycast(new Ray(Camera.Active.Position, Camera.Active.LookVector), true, 2000f, World.OurPlayer);
            ClientPlayer mousedOverPlayer = result.HitPlayer ? (ClientPlayer)result.PlayerResult.Player : null;

            if (mousedOverPlayer != null && mousedOverPlayer.StateInfo != null)
            {
                NetworkPlayer netPlayer = null;
                // Find the networkplayer from their character
                foreach (NetworkPlayer np in netPlayerComponent.NetPlayers)
                    if (np.CharacterId.HasValue && np.CharacterId.Value == mousedOverPlayer.StateInfo.Id)
                    {
                        netPlayer = np;
                        break;
                    }

                if (netPlayer != null)
                {
                    Vector2 nameSize = font.MeasureString(netPlayer.Name);
                    font.DrawString(netPlayer.Name, Renderer.ScreenWidth / 2f, Renderer.ScreenHeight / 2f - 125,
                        Renderer.Sprites.SpriteBatch, TextAlign.Center, Color.White, new Color(0, 0, 0, 0.6f));
                }
            }
        }
    }
}
