using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System;
using System.Collections.Generic;
using System.Net;

/* AOSServer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public delegate bool NetPacketHookCallback(NetInboundPacket packet, CustomPacketType type);

    public class AOSServer : NetServer
    {
        public static AOSServer Instance { get; private set; }

        Dictionary<Type, NetComponent> components;
        List<NetPacketHookCallback> packetHooks;

        public AOSServer(NetServerConfig config) 
            : base(config)
        {
            if (Instance != null)
                throw new Exception("An AOSServer already exists!");

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
            OnUserConnected += AOSServer_OnUserConnected;
            OnUserDisconnected += AOSServer_OnUserDisconnected;

            // Add some diag commands
            DashCMD.AddCommand("list", "Shows a list of all connected players.",
                (args) =>
                {
                    DashCMD.WriteImportant("Players ({0}):", Connections.Count);
                    foreach (NetConnection conn in Connections.Values)
                        DashCMD.WriteStandard("  {0}", conn);

                    DashCMD.WriteStandard("");
                });
        }

        /// <summary>
        /// Creates and attempts to start an AOSServer with
        /// the game specific config.
        /// </summary>
        public static bool Initialize(int maxConnections, IPEndPoint endPoint, IPEndPoint receiveEndPoint = null)
        {
            GlobalNetwork.SetupLogging();

            NetServerConfig config = new NetServerConfig();
            config.MaxConnections = maxConnections;
            config.DontApplyPingControl = true;

            AOSServer server = new AOSServer(config);

            bool success = server.Start(endPoint);

            if (success)
            {
                GlobalNetwork.IsServer = true;
                GlobalNetwork.IsConnected = true;
            }

            return success;
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

        private void AOSServer_OnUserConnected(NetConnection connection)
        {
            foreach (NetComponent c in components.Values)
                c.OnConnected(connection);
        }

        private void AOSServer_OnUserDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            foreach (NetComponent c in components.Values)
                c.OnDisconnected(connection, reason, lostConnection);
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

            // Read packets
            for (int i = 0; i < 1000 && AvailablePackets > 0; i++)
            {
                NetInboundPacket packet = ReadPacket();

                if (packet.Position >= packet.Length)
                {
                    DashCMD.WriteError("[AOSServer] Received invalid custom packet from {0}! (bad packet position)",
                        packet.Sender);
                }
                else
                {
                    CustomPacketType type = (CustomPacketType)packet.ReadByte();

                    // Try and handle the packet
                    if (!HandlePacket(packet, type))
                        DashCMD.WriteWarning("[AOSServer] Received unknown custom packet {0}, from {1}",
                            type, packet.Sender);
                }
            }

            // Update each component
            foreach (NetComponent c in components.Values)
                c.Update(deltaTime);
        }
    }
}
