using Dash.Net;
using System;
using System.Collections.Generic;

/* Snapshot.Delta.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    public abstract partial class Snapshot : IDisposable
    {
        public bool IsDeltaCompressing { get; private set; }
        internal HashSet<byte> AcknowledgedDeltaIds { get; private set; }

        int maxDeltaSnapshots;
        List<DeltaSnapshot> previousStates;

        byte currentDeltaId;

        public void EnableDeltaCompression(int maxDeltaSnapshots)
        {
            this.maxDeltaSnapshots = maxDeltaSnapshots;

            AcknowledgedDeltaIds = new HashSet<byte>();
            previousStates = new List<DeltaSnapshot>();
            
            IsDeltaCompressing = true;

            snapshotSystem.SupportDeltaCompression(this);
        }

        public void Acknowledge(int index)
        {
            int i = (previousStates.Count - 1) - (currentDeltaId - index);
            if (i >= 0 && i < previousStates.Count)
                previousStates[i].Acknowledge();
        }

        void AddAsDeltaSnapshot()
        {
            DeltaSnapshot ds = new DeltaSnapshot(staticFields, dynamicFields);
            previousStates.Add(ds);

            if (previousStates.Count > maxDeltaSnapshots)
                previousStates.RemoveAt(0);

            currentDeltaId++;
        }

        DeltaSnapshot GetLastValidSnapshot()
        {
            if (!IsDeltaCompressing)
                return null;

            DeltaSnapshot snapshot = null;

            for (int i = previousStates.Count - 1; i >= 0; i--)
            {
                DeltaSnapshot ss = previousStates[i];
                if (ss.IsAcknowledged)
                {
                    snapshot = ss;
                    break;
                }
            }

            return snapshot;
        }

        public void Dispose()
        {
            snapshotSystem.Deallocate(this);

            foreach (SnapshotField field in staticFields.Values)
            {
                if (field.Type == SnapshotFieldType.Snapshot)
                {
                    Snapshot snapshot = (Snapshot)field.Value;
                    snapshot.Dispose();
                }
            }
        }
    }
}
