using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System;

/* (Client)SnapshotNetComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class SnapshotNetComponent : NetComponent
    {
        public WorldSnapshot WorldSnapshot { get; private set; }
        public event EventHandler<WorldSnapshot> OnWorldSnapshotInbound;

        public SnapshotSystem SnapshotSystem
        {
            get { return snapshotSystem; }
        }

        public float SnapshotRoundTripTime
        {
            get { return rtt; }
        }

        const int DEFAULT_TICKRATE = 90; // Target: 50 p/s, Actual (@ 60fps): ~30 p/s
        float tickrate
        {
            get
            {
                if (WorldSnapshot != null)
                    return 1f / Math.Max(Math.Min(DashCMD.GetCVar<int>("cl_tickrate"),
                        WorldSnapshot.MaxClientTickrate), 1f);
                else
                    return 1f;
            }
        }
        float syncTime;

        SnapshotStats lastOutboundPacketStats = new SnapshotStats();
        ushort pid = 0;
        ushort lastServerPId;

        bool gotPacket;

        float timeSinceSend;
        float rtt;
        bool measuringRTT;

        CharacterSnapshotSystem charSnapshotSystem;
        SnapshotSystem snapshotSystem;
        ObjectNetComponent objectComponent;

        public SnapshotNetComponent(AOSClient client) 
            : base(client)
        {
            snapshotSystem     = new SnapshotSystem(client);
            charSnapshotSystem = new CharacterSnapshotSystem(this, snapshotSystem);

            objectComponent = client.GetComponent<ObjectNetComponent>();
            objectComponent.OnCreatableInstantiated += ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed    += ObjectComponent_OnCreatableDestroyed;

            DashCMD.AddScreen(new DashCMDScreen("snapshot", "Displays information about the snapshot system.", true,
                (screen) =>
                {
                    screen.WriteLine("Snapshot Round-Trip Time: {0}s", rtt);
                    screen.WriteLine("Last Outbound Snapshot:", ConsoleColor.Cyan);
                    screen.WriteLine("PacketHeader: {0} bytes", lastOutboundPacketStats.PacketHeader);
                    screen.WriteLine("Acks: {0} bytes", lastOutboundPacketStats.Acks);
                    screen.WriteLine("PlayerData: {0} bytes", lastOutboundPacketStats.PlayerData);
                    screen.WriteLine("Total: {0} bytes", lastOutboundPacketStats.Total);
                })
            { SleepTime = 30 });

            DashCMD.SetCVar("cl_tickrate", DEFAULT_TICKRATE);
            DashCMD.SetCVar("cl_await_sv_snap", false);

            //DashCMD.SetCVar("cl_tickrate", 25);
            //DashCMD.SetCVar("cl_await_sv_snap", true);

            syncTime = tickrate;
        }

        private void ObjectComponent_OnCreatableInstantiated(object sender, NetCreatableInfo info)
        {
            if (info.Owner != null)
                charSnapshotSystem.OnCreatableInstantiated(info, WorldSnapshot);
        }

        private void ObjectComponent_OnCreatableDestroyed(object sender, NetCreatableInfo info)
        {
            charSnapshotSystem.OnCreatableDestroyed(info, WorldSnapshot);
        }

        public override void OnConnected(NetConnection connection)
        {
            WorldSnapshot = new WorldSnapshot(snapshotSystem, connection, true);

            base.OnConnected(connection);
        }

        public override void OnDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            charSnapshotSystem.Clear();
            snapshotSystem.Clear();

            pid                     = 0;
            lastServerPId           = 0;
            gotPacket               = false;
            timeSinceSend           = 0;
            rtt                     = 0;
            measuringRTT            = false;
            lastOutboundPacketStats = new SnapshotStats();
            WorldSnapshot           = null;

            base.OnDisconnected(connection, reason, lostConnection);
        }

        public override bool HandlePacket(NetInboundPacket packet, CustomPacketType type)
        {
            if (type == CustomPacketType.Snapshot)
            {
                if (measuringRTT)
                {
                    rtt = Interpolation.Linear(rtt, timeSinceSend, 0.15f);
                    measuringRTT = false;
                }

                gotPacket = true;

                ushort pid = packet.ReadUInt16();

                if (pid <= lastServerPId && pid != 0)
                {
                    DashCMD.WriteWarning("[SnapshotNC] Dropping late server snapshot...");
                    return true;
                }

                snapshotSystem.OnInbound(packet);

                ushort snapshotLength = packet.ReadUInt16();

                if (WorldSnapshot != null)
                {
                    // Read the snapshot
                    WorldSnapshot.Deserialize(packet);

                    // Update players
                    charSnapshotSystem.OnClientInbound(WorldSnapshot);

                    // Invoke event
                    if (OnWorldSnapshotInbound != null)
                        OnWorldSnapshotInbound(this, WorldSnapshot);
                }
                else
                {
                    packet.Position += snapshotLength;
                    DashCMD.WriteWarning("[SnapshotNC] Received snapshot before worldsnapshot was intialized!");
                }

                lastServerPId = pid;

                return true;
            }
            else
                return false;
        }

        public override void Update(float deltaTime)
        {
            if (measuringRTT)
                timeSinceSend += deltaTime;

            bool svForceAwait = WorldSnapshot != null
                ? WorldSnapshot.ForceSnapshotAwait : true;
            bool gotServerSnapshot = gotPacket
                || (!DashCMD.GetCVar<bool>("cl_await_sv_snap") && !svForceAwait);

            if (syncTime <= 0 && gotServerSnapshot || syncTime <= -1)
            {
                syncTime = tickrate;
                gotPacket = false;

                // Create and send client state snapshot
                NetOutboundPacket packet = new NetOutboundPacket(NetDeliveryMethod.Unreliable);
                packet.SendImmediately = true;
                int size = packet.Length;
                packet.Write((byte)CustomPacketType.Snapshot);
                packet.Write(pid++);

                lastOutboundPacketStats.PacketHeader = packet.Length - size; size = packet.Length;

                // Write snapshot delta data
                snapshotSystem.OnOutbound(packet, client.ServerConnection);
                lastOutboundPacketStats.Acks = packet.Length - size; size = packet.Length;

                // Write player data
                charSnapshotSystem.OnClientOutbound(packet);
                lastOutboundPacketStats.PlayerData = packet.Length - size; size = packet.Length;

                // Send packet
                client.SendPacket(packet);

                timeSinceSend = 0;
                measuringRTT = true;
            }
            else
                syncTime -= deltaTime;

            base.Update(deltaTime);
        }
    }
}
