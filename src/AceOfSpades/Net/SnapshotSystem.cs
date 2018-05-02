using Dash.Engine.Diagnostics;
using Dash.Net;
using System.Collections.Generic;

/* SnapshotSystem.cs
 * Ethan Lafrenais
 * Responsible for managing snapshots between games.
*/

namespace AceOfSpades.Net
{
    public class SnapshotSystem
    {
        class SnapshotConnectionInterface
        {
            public NetConnection Connection { get; }
            public Dictionary<string, ushort> IdStash { get; }
            public Dictionary<string, Snapshot> WaitingSnapshots { get; }
            public Dictionary<ushort, Snapshot> InboundDeltaSnapshots { get; }
            public Dictionary<ushort, Snapshot> OutboundDeltaSnapshots { get; }

            public SnapshotConnectionInterface(NetConnection connection)
            {
                Connection             = connection;
                InboundDeltaSnapshots  = new Dictionary<ushort, Snapshot>();
                OutboundDeltaSnapshots = new Dictionary<ushort, Snapshot>();
                IdStash                = new Dictionary<string, ushort>();
                WaitingSnapshots       = new Dictionary<string, Snapshot>();
            }

            public void AddDeltaSnapshot(Snapshot snapshot)
            {
                if (snapshot.IsAppOwner)
                    OutboundDeltaSnapshots.Add(snapshot.Id, snapshot);
                else
                    InboundDeltaSnapshots.Add(snapshot.Id, snapshot);
            }
            
            public bool RemoveDeltaSnapshot(Snapshot snapshot)
            {
                if (snapshot.IsAppOwner)
                    return OutboundDeltaSnapshots.Remove(snapshot.Id);
                else
                    return InboundDeltaSnapshots.Remove(snapshot.Id);
            }

            public bool TryGetDeltaSnapshot(ushort id, bool isAppOwner, out Snapshot snapshot)
            {
                if (isAppOwner)
                    return OutboundDeltaSnapshots.TryGetValue(id, out snapshot);
                else
                    return InboundDeltaSnapshots.TryGetValue(id, out snapshot);
            }
        }

        Dictionary<NetConnection, SnapshotConnectionInterface> interfaces;
        HashSet<Snapshot> snapshotsAwaitingDeltaSupport;
        
        IdAllocatorUInt16 idAllocator;
        RemoteChannel channel;

        public SnapshotSystem(NetMessenger messenger)
        {
            idAllocator = new IdAllocatorUInt16();
            interfaces = new Dictionary<NetConnection, SnapshotConnectionInterface>();
            snapshotsAwaitingDeltaSupport = new HashSet<Snapshot>();

            channel = messenger.GetChannel((ushort)AOSChannelType.SnapshotSystem);
            channel.AddRemoteEvent("AllocateSnapshotId", R_AllocateSnapshotId);

            DashCMD.SetCVar("log_snapshots", false);
        }

        public void AwaitAllocation(Snapshot snapshot)
        {
            SnapshotConnectionInterface sci = GetOrCreateInterface(snapshot.OtherConnection);
            string snapshotId = snapshot.GetUniqueId();

            ushort stashedId;
            // First check stash to see if we are late
            if (sci.IdStash.TryGetValue(snapshotId, out stashedId))
            {
                // All set!
                sci.IdStash.Remove(snapshotId);
                snapshot.SetId(stashedId);
                WriteDebug("[SS:{0}] Setting snapshot '{1}'s id to {2}.",
                    snapshot.OtherConnection, snapshotId, stashedId);
            }
            else
            {
                // Wait for other connection to allocate the id
                if (sci.WaitingSnapshots.ContainsKey(snapshotId))
                    sci.WaitingSnapshots[snapshotId] = snapshot;
                else
                    sci.WaitingSnapshots.Add(snapshotId, snapshot);
            }
        }

        void WriteDebug(string msg, params object[] args)
        {
            if (DashCMD.GetCVar<bool>("log_snapshots"))
                DashCMD.WriteStandard(msg, args);
        }

        SnapshotConnectionInterface GetOrCreateInterface(NetConnection connection)
        {
            SnapshotConnectionInterface sci;
            if (interfaces.TryGetValue(connection, out sci))
                return sci;
            else
            {
                sci = new SnapshotConnectionInterface(connection);
                interfaces.Add(connection, sci);
                return sci;
            }
        }

        public void Clear()
        {
            interfaces.Clear();
        }

        public ushort Allocate(Snapshot snapshot)
        {
            // Allocate an id
            ushort id = idAllocator.Allocate();

            string snapshotId = snapshot.GetUniqueId();
            WriteDebug("[SS:{0}] Allocated snapshot '{1}' with id {2}.", 
                snapshot.OtherConnection, snapshotId, id);

            // Send the id to the other connection using the snapshot
            channel.FireEvent("AllocateSnapshotId", snapshot.OtherConnection, id, snapshotId);

            return id;
        }

        public bool Deallocate(Snapshot snapshot)
        {
            // Don't deallocate unready snapshots because
            // it would deallocate a snapshot that is ready
            // with the id of 0.
            if (!snapshot.IsReady)
                return false;
            {
                WriteDebug("[SS:{0}] Deallocating snapshot '{0}' with id {1}.", 
                    snapshot.OtherConnection, snapshot.GetUniqueId(), snapshot.Id);

                SnapshotConnectionInterface sci = GetOrCreateInterface(snapshot.OtherConnection);

                if (snapshot.IsDeltaCompressing)
                    sci.RemoveDeltaSnapshot(snapshot);

                return idAllocator.Deallocate(snapshot.Id);
            }
        }

        public void SupportDeltaCompression(Snapshot snapshot)
        {
            if (!snapshot.IsReady)
                // Enabling delta compression requires an allocated id,
                // so we'll add it to the waiting list.
                snapshotsAwaitingDeltaSupport.Add(snapshot);
            else
                AddDeltaSnapshot(snapshot);
        }

        void AddDeltaSnapshot(Snapshot snapshot)
        {
            WriteDebug("[SS:{0}] Supporting delta compression with snapshot '{1}'.", 
                snapshot.OtherConnection, snapshot.GetUniqueId());

            SnapshotConnectionInterface sci = GetOrCreateInterface(snapshot.OtherConnection);
            sci.AddDeltaSnapshot(snapshot);
        }

        void R_AllocateSnapshotId(NetConnection connection, NetBuffer data, ushort numArgs)
        {
            ushort id = data.ReadUInt16();
            string snapshotId = data.ReadString();

            SnapshotConnectionInterface sci = GetOrCreateInterface(connection);

            Snapshot unallocatedSnapshot;
            if (sci.WaitingSnapshots.TryGetValue(snapshotId, out unallocatedSnapshot))
            {
                WriteDebug("[SS:{0}] Setting snapshot '{1}'s id to {2}.", 
                    connection, snapshotId, id);

                // Set id of unallocated snapshot
                sci.IdStash.Remove(snapshotId);
                sci.WaitingSnapshots.Remove(snapshotId);
                unallocatedSnapshot.SetId(id);

                // Be sure to enable delta compression if the snapshot
                // requested it before it got it's id.
                if (snapshotsAwaitingDeltaSupport.Contains(unallocatedSnapshot))
                {
                    snapshotsAwaitingDeltaSupport.Remove(unallocatedSnapshot);
                    AddDeltaSnapshot(unallocatedSnapshot);
                }
            }
            else
            {
                WriteDebug("[SS:{0}] Stashing snapshot '{1}'s id {2}.", 
                    connection, snapshotId, id);

                // Stash id for later
                sci.IdStash.Add(snapshotId, id);
            }
        }

        public void OnOutbound(NetOutboundPacket packet, NetConnection connection)
        {
            // For each snapshot with delta support,
            // we will write every snapshot version
            // that was acknowledged.

            SnapshotConnectionInterface sci = GetOrCreateInterface(connection);

            packet.Write((ushort)sci.InboundDeltaSnapshots.Count);
            foreach (KeyValuePair<ushort, Snapshot> pair in sci.InboundDeltaSnapshots)
            {
                packet.Write(pair.Key);
                packet.Write((ushort)pair.Value.AcknowledgedDeltaIds.Count);

                foreach (byte deltaId in pair.Value.AcknowledgedDeltaIds)
                    packet.Write(deltaId);

                pair.Value.AcknowledgedDeltaIds.Clear();
            }
        }

        public void OnInbound(NetInboundPacket packet)
        {
            SnapshotConnectionInterface sci = GetOrCreateInterface(packet.Sender);

            ushort numDeltaSnapshots = packet.ReadUInt16();
            for (int i = 0; i < numDeltaSnapshots; i++)
            {
                ushort snapshotId = packet.ReadUInt16();
                ushort numAcks = packet.ReadUInt16();

                Snapshot snapshot;
                if (sci.TryGetDeltaSnapshot(snapshotId, true, out snapshot))
                {
                    for (int k = 0; k < numAcks; k++)
                        snapshot.Acknowledge(packet.ReadByte());
                }
                else
                    packet.Position += numAcks * 1;
            }
        }
    }
}
