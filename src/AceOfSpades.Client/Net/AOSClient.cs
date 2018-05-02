using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Engine.IO;
using Dash.Net;
using System;
using System.Collections.Generic;
using System.Net;

/* AOSClient.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public delegate bool NetPacketHookCallback(NetInboundPacket packet, CustomPacketType type);

    public class AOSClient : NetClient
    {
        public static AOSClient Instance { get; private set; }

        Dictionary<Type, NetComponent> components;
        List<NetPacketHookCallback> packetHooks;

        private AOSClient(NetClientConfig config)
            : base(config)
        {
            if (Instance != null)
                throw new Exception("An AOSClient already exists!");

            Instance = this;
            components  = new Dictionary<Type, NetComponent>();
            packetHooks = new List<NetPacketHookCallback>();

            // Create each game channel
            foreach (object o in Enum.GetValues(typeof(AOSChannelType)))
                CreateChannel((ushort)o);

            // Add network components
            AddComponent(new ObjectNetComponent(this));
            AddComponent(new SnapshotNetComponent(this));
            AddComponent(new NetPlayerComponent(this));

            foreach (NetComponent component in components.Values)
                component.Initialize();

            // Hook into base events
            OnConnected += AOSClient_OnConnected;
            OnDisconnected += AOSClient_OnDisconnected;
        }

        /// <summary>
        /// Creates and attempts to start an AOSClient with
        /// the game specific config.
        /// </summary>
        public static bool Initialize()
        {
            GlobalNetwork.SetupLogging();

            ConfigSection netSection = Program.ConfigFile.GetSection("Network");
            IPAddress bindIp = null;
            int? bindPort = null;
            if (netSection != null)
            {
                bool autoFindEndpoint = netSection.GetBoolean("auto-find-endpoint") ?? true;
                if (!autoFindEndpoint)
                {
                    IPAddress.TryParse(netSection.GetString("bind-to-ip"), out bindIp);
                    bindPort = netSection.GetInteger("bind-to-port");
                }
            }
            
            if (bindIp == null)
                bindIp = NetHelper.GetInternalIP();
            if (!bindPort.HasValue)
                bindPort = 0;
            

            NetClientConfig config = new NetClientConfig();
            config.DontApplyPingControl = true;

            AOSClient client = new AOSClient(config);
            return client.Start(new IPEndPoint(bindIp, bindPort.Value));
        }

        public void AddPacketHook(NetPacketHookCallback hook)
        {
            packetHooks.Add(hook);
        }

        public bool RemovePacketHook(NetPacketHookCallback hook)
        {
            return packetHooks.Remove(hook);
        }

        public RemoteChannel GetChannel(AOSChannelType type)
        {
            return GetChannel((ushort)type);
        }

        public void AddComponent(NetComponent component)
        {
            components.Add(component.GetType(), component);
        }

        public bool RemoveComponent(NetComponent component)
        {
            return components.Remove(component.GetType());
        }

        public T GetComponent<T>()
            where T : NetComponent
        {
            NetComponent c;
            if (components.TryGetValue(typeof(T), out c))
                return (T)c;
            else
                return null;
        }

        private void AOSClient_OnConnected(NetConnection connection)
        {
            foreach (NetComponent c in components.Values)
                c.OnConnected(connection);
        }

        private void AOSClient_OnDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            foreach(NetComponent c in components.Values)
                c.OnDisconnected(connection, reason, lostConnection);
        }

        public override bool Connect(IPEndPoint serverAddress, out NetDenialReason? denialReason)
        {
            return base.Connect(serverAddress, out denialReason);
        }

        public override bool Connect(IPEndPoint serverAddress, string password, out NetDenialReason? denialReason)
        {
            DashCMD.WriteImportant("Connecting to {0}...", serverAddress);
            return base.Connect(serverAddress, password, out denialReason);
        }

        bool HandlePacket(NetInboundPacket packet, CustomPacketType type)
        {
            // Give each component a chance to try and handle the packet type,
            // if none process it we've received an unknown packet.
            foreach (NetComponent c in components.Values)
            {
                if (c.HandlePacket(packet, type))
                    return true;
            }

            // Attempt to defer packet to the custom handlers
            for (int i = 0; i < packetHooks.Count; i++)
                if (packetHooks[i](packet, type))
                    return true;

            return false;
        }

        public void Update(float deltaTime)
        {
            // Update internal messenger
            base.Update();

            // Ensure IsConnected is up to date for shared lib
            GlobalNetwork.IsConnected = IsConnected;

            // Read packets
            for (int i = 0; i < 1000 && AvailablePackets > 0; i++)
            {
                NetInboundPacket packet = ReadPacket();
                if (packet.Position >= packet.Length)
                {
                    DashCMD.WriteError("[AOSClient] Received invalid custom packet from {0}! (bad packet position)",
                        packet.Sender);
                }
                else
                {
                    CustomPacketType type = (CustomPacketType)packet.ReadByte();

                    // Try and handle the packet
                    if (!HandlePacket(packet, type))
                        DashCMD.WriteWarning("[AOSClient] Received unknown custom packet {0}, from {1}",
                            type, packet.Sender);
                }
            }

            // Update each component
            if (IsConnected)
            {
                foreach (NetComponent c in components.Values)
                    c.Update(deltaTime);
            }
        }
    }
}