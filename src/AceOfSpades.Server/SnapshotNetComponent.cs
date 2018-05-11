using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System;
using System.Collections.Generic;

/* (Server)SnapshotNetComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class SnapshotNetComponent : NetComponent
    {
        public event EventHandler<WorldSnapshot> OnWorldSnapshotOutbound;
        public Dictionary<NetConnection, NetConnectionSnapshotState> ConnectionStates { get; private set; }

        public SnapshotSystem SnapshotSystem
        {
            get { return snapshotSystem; }
        }

        // Old comment for DEFAULT_TICKRATE: // Target: 30 p/s, Actual (@ 60fps): ~20 p/s

        const int DEFAULT_TICKRATE = 60;
        float tickrate
        {
            get { return 1f / DashCMD.GetCVar<int>("sv_tickrate"); }
        }

        SnapshotSystem snapshotSystem;
        CharacterSnapshotSystem charSnapshotSystem;
        ObjectNetComponent objectComponent;

        float timeSinceLastTickSend;

        public SnapshotNetComponent(AOSServer server) 
            : base(server)
        {
            snapshotSystem     = new SnapshotSystem(server);
            charSnapshotSystem = new CharacterSnapshotSystem(this, snapshotSystem);
            ConnectionStates   = new Dictionary<NetConnection, NetConnectionSnapshotState>();

            objectComponent = server.GetComponent<ObjectNetComponent>();
            objectComponent.OnCreatableInstantiated += ObjectComponent_OnCreatableInstantiated;
            objectComponent.OnCreatableDestroyed += ObjectComponent_OnCreatableDestroyed;

            DashCMD.AddScreen(new DashCMDScreen("snapshot", "Displays information about the snapshot system.", true,
                (screen) =>
                {
                    try
                    {
                        foreach (KeyValuePair<NetConnection, NetConnectionSnapshotState> pair in ConnectionStates)
                        {
                            SnapshotStats stats = pair.Value.Stats;

                            screen.WriteLine("[{0}]:", pair.Key);
                            screen.WriteLine("Snapshot Round-Trip Time: {0}", pair.Value.RoundTripTime);
                            screen.WriteLine("PacketHeader: {0} bytes", stats.PacketHeader);
                            screen.WriteLine("Acks: {0} bytes", stats.Acks);
                            screen.WriteLine("PlayerData: {0} bytes", stats.PlayerData);
                            screen.WriteLine("TerrainData: {0} bytes", stats.TerrainData);
                            screen.WriteLine("Total: {0} bytes", stats.Total);
                            screen.WriteLine("");
                        }
                    }
                    catch (Exception) { }
                })
            { SleepTime = 30 });

            DashCMD.SetCVar("sv_tickrate", DEFAULT_TICKRATE);
            DashCMD.SetCVar("sv_await_cl_snap", false);
            DashCMD.SetCVar<ushort>("ag_max_cl_tickrate", 100);
            DashCMD.SetCVar("ag_cl_force_await_snap", false);

            //DashCMD.SetCVar("sv_tickrate", 25);
            //DashCMD.SetCVar("sv_await_cl_snap", true);
            //DashCMD.SetCVar<ushort>("ag_max_cl_tickrate", 30);
            //DashCMD.SetCVar("ag_cl_force_await_snap", true);
        }

        private void ObjectComponent_OnCreatableInstantiated(object sender, NetCreatableInfo info)
        {
            if (info.Owner != null)
                charSnapshotSystem.OnCreatableInstantiated(info, ConnectionStates[info.Owner]);
        }

        private void ObjectComponent_OnCreatableDestroyed(object sender, NetCreatableInfo info)
        {
            charSnapshotSystem.OnCreatableDestroyed(info.Id);
        }

        public override void OnConnected(NetConnection connection)
        {
            ConnectionStates.Add(connection, new NetConnectionSnapshotState(snapshotSystem, connection));
            base.OnConnected(connection);
        }

        public override void OnDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            ConnectionStates.Remove(connection);
            base.OnDisconnected(connection, reason, lostConnection);
        }

        public override bool HandlePacket(NetInboundPacket packet, CustomPacketType type)
        {
            if (type == CustomPacketType.Snapshot)
            {
                ushort pid = packet.ReadUInt16();
                NetConnectionSnapshotState connState;
                if (ConnectionStates.TryGetValue(packet.Sender, out connState))
                {
                    //connState.GotPacket = true;
                    if (connState.MeasuringRTT)
                    {
                        connState.MeasuringRTT = false;
                        connState.RoundTripTime = Interpolation.Linear(connState.RoundTripTime,
                            connState.RTT_TimeSinceLastSend, 0.15f);
                    }

                    ushort ppid = connState.LastInboundSnapshotId;
                    connState.LastInboundSnapshotId = pid;
                    if (pid <= ppid && pid != 0)
                    {
                        DashCMD.WriteWarning("[SnapshotNC] Dropping late client snapshot...");
                        return true;
                    }

                    snapshotSystem.OnInbound(packet);
                    charSnapshotSystem.OnServerInbound(packet, connState);
                }

                return true;
            }
            else
                return false;
        }

        public override void Update(float deltaTime)
        {
            //foreach (NetConnectionSnapshotState state in ConnectionStates.Values)
            //{
            //    if (!state.Ready)
            //        continue;

                //bool gotClientSnapshot = state.GotPacket || !DashCMD.GetCVar<bool>("sv_await_cl_snap");

                //if (state.TimeSinceLastSend >= tickrate && gotClientSnapshot)
                if (timeSinceLastTickSend >= tickrate)
                {
                    foreach (NetConnectionSnapshotState state in ConnectionStates.Values)
                    {
                        if (!state.Ready)
                            continue;

                        SendSnapshotTo(state.Connection, state, deltaTime);
                        
                        state.RTT_TimeSinceLastSend = 0;
                        state.MeasuringRTT = true;
                    }

                    charSnapshotSystem.OnPostServerOutbound();
                    timeSinceLastTickSend -= deltaTime;
                }
                else
                {
                    foreach (NetConnectionSnapshotState state in ConnectionStates.Values)
                    {
                        if (!state.Ready)
                            continue;

                        //state.TimeSinceLastSend += deltaTime;

                        if (state.MeasuringRTT)
                            state.RTT_TimeSinceLastSend += deltaTime;
                    }

                    timeSinceLastTickSend += deltaTime;
                }
            //}

            base.Update(deltaTime);
        }

        void SendSnapshotTo(NetConnection conn, NetConnectionSnapshotState connState, float deltaTime)
        {
            WorldSnapshot worldSnapshot = connState.WorldSnapshot;

            ushort epid = connState.OutboundSnapshotId;
            connState.OutboundSnapshotId++;

            //connState.TimeSinceLastSend -= deltaTime;
            //connState.GotPacket = false;

            connState.WorldSnapshot.MaxClientTickrate = DashCMD.GetCVar<ushort>("ag_max_cl_tickrate");
            connState.WorldSnapshot.ForceSnapshotAwait = DashCMD.GetCVar<bool>("ag_cl_force_await_snap");

            NetOutboundPacket packet = new NetOutboundPacket(NetDeliveryMethod.Unreliable);
            packet.SendImmediately = true;
            int size = packet.Length;
            packet.Write((byte)CustomPacketType.Snapshot);
            packet.Write(epid);
            int _packetheader = packet.Length - size; size = packet.Length;

            // Write snapshot system data
            snapshotSystem.OnOutbound(packet, conn);
            int _acks = packet.Length - size; size = packet.Length;

            // Write players
            charSnapshotSystem.OnServerOutbound(packet, connState);

            // Invoke event
            if (OnWorldSnapshotOutbound != null)
                OnWorldSnapshotOutbound(this, worldSnapshot);

            // Serialize snapshot
            NetBuffer buffer = new NetBuffer();
            worldSnapshot.Serialize(buffer);

            packet.Write((ushort)buffer.Length);
            packet.WriteBytes(buffer.Data, 0, buffer.Length);


            int _playerdata = packet.Length - size; size = packet.Length;
            int _terraindata = connState.WorldSnapshot.TerrainSnapshot.LastByteSize;
            _playerdata -= _terraindata;

            // Send packet
            conn.SendPacket(packet);

            if (connState != null)
            {
                SnapshotStats stats = connState.Stats;
                stats.PacketHeader = _packetheader;
                stats.Acks = _acks;
                stats.PlayerData = _playerdata;
                stats.TerrainData = _terraindata;
            }
        }
    }
}
