using AceOfSpades.Characters;
using Dash.Net;
using System.Collections.Generic;
using System;
using Dash.Engine.Diagnostics;

/* WorldSnapshot.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// The 'global' snapshot of everything a client needs
    /// to know about the current gamestate.
    /// </summary>
    public class WorldSnapshot : Snapshot
    {
        public IEnumerable<SnapshotField> Players
        {
            get { return playerFields.Values; }
        }

        public float Time
        {
            get { return (float)time.Value; }
            set { time.Value = value; }
        }

        public ushort MaxClientTickrate
        {
            get { return (ushort)ag_max_cl_tickrate.Value; }
            set { ag_max_cl_tickrate.Value = value; }
        }

        public bool ForceSnapshotAwait
        {
            get { return (bool)ag_force_cl_await_snap.Value; }
            set { ag_force_cl_await_snap.Value = value; }
        }

        public TerrainDeltaSnapshot TerrainSnapshot { get; }
        public NetworkPlayerListSnapshot NetworkPlayerListSnapshot { get; }
        public NetEntityListSnapshot NetEntityListSnapshot { get; }

        Dictionary<ushort, DynamicSnapshotField> playerFields;

        SnapshotField time;
        SnapshotField ag_max_cl_tickrate;
        SnapshotField ag_force_cl_await_snap;

        public WorldSnapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection, 
            bool dontAllocateId = false)
            : base(snapshotSystem, otherConnection, dontAllocateId)
        {
            playerFields = new Dictionary<ushort, DynamicSnapshotField>();

            AddCustomField(TerrainSnapshot = new TerrainDeltaSnapshot());
            AddNestedField(NetworkPlayerListSnapshot = new NetworkPlayerListSnapshot(snapshotSystem,
                otherConnection, dontAllocateId));
            AddNestedField(NetEntityListSnapshot = new NetEntityListSnapshot(snapshotSystem,
                otherConnection, dontAllocateId));

            time = AddPrimitiveField<float>();
            ag_max_cl_tickrate = AddPrimitiveField<ushort>();
            ag_force_cl_await_snap = AddPrimitiveField<bool>();

            Setup();
            EnableDeltaCompression(16);
        }

        public override string GetUniqueId()
        {
            return "WorldData";
        }

        public bool PlayerFieldExists(ushort id)
        {
            return playerFields.ContainsKey(id);
        }

        public bool TryGetPlayer(ushort id, out PlayerSnapshot player)
        {
            DynamicSnapshotField field;
            if (playerFields.TryGetValue(id, out field))
            {
                player = (PlayerSnapshot)field.Value;
                return true;
            }
            else
            {
                player = null;
                return false;
            }
        }

        public void AddPlayer(ushort id, bool isAppOwner, bool isCreator)
        {
            DynamicSnapshotField field = AddNestedField(id, 
                new PlayerSnapshot(snapshotSystem, OtherConnection, isAppOwner, id, !isCreator));
            playerFields.Add(id, field);
        }

        public bool RemovePlayer(MPPlayer player)
        {
            return RemovePlayer(player.StateInfo.Id);
        }

        public bool IsPlayerAdded(ushort id)
        {
            return playerFields.ContainsKey(id);
        }

        public bool RemovePlayer(ushort netId)
        {
            DynamicSnapshotField field;
            if (playerFields.TryGetValue(netId, out field))
                RemoveDynamicField(field);

            return playerFields.Remove(netId);
        }
    }
}
