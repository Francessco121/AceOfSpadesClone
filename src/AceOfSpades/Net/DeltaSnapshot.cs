using System.Collections.Generic;

/* DeltaSnapshot.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// Contains information about an already processed snapshot.
    /// This is used for delta-compression by comparing two of these
    /// to determine what data changed.
    /// </summary>
    public class DeltaSnapshot
    {
        public bool IsAcknowledged { get; private set; }
        public Dictionary<ushort, object> StaticFields { get; }
        public Dictionary<object, object> DynamicFields { get; }

        public DeltaSnapshot(Dictionary<ushort, StaticSnapshotField> staticFields, 
            Dictionary<object, DynamicSnapshotField> dynamicFields)
        {
            StaticFields = new Dictionary<ushort, object>();
            DynamicFields = new Dictionary<object, object>();

            // Simply copies each primitive field from the fields sent
            // from a Snapshot
            foreach (KeyValuePair<ushort, StaticSnapshotField> pair in staticFields)
                if (pair.Value.Type == SnapshotFieldType.Primitive)
                    if (!pair.Value.NeverCompress)
                        StaticFields.Add(pair.Key, pair.Value.Value);

            foreach (KeyValuePair<object, DynamicSnapshotField> pair in dynamicFields)
                if (pair.Value.Type == SnapshotFieldType.Primitive)
                    if (!pair.Value.NeverCompress)
                        DynamicFields.Add(pair.Key, pair.Value.Value);
        }

        public void Acknowledge()
        {
            IsAcknowledged = true;
        }

        public override string ToString()
        {
            return string.Format("IsAcknowledged: {0}", IsAcknowledged);
        }
    }
}
