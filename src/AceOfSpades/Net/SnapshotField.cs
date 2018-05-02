/* SnapshotField.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    public abstract class SnapshotField
    {
        public Snapshot Parent { get; }
        public SnapshotFieldType Type { get; }
        public SnapshotPrimitiveType PrimitiveType { get; }
        public object Value;
        public bool NeverCompress;

        internal SnapshotField(Snapshot parent, SnapshotFieldType type, 
            object defaultValue, SnapshotPrimitiveType primtiveType)
        {
            Parent = parent;
            Type = type;
            Value = defaultValue;
            PrimitiveType = primtiveType;
        }
    }

    public class StaticSnapshotField : SnapshotField
    {
        public ushort Id { get; }

        internal StaticSnapshotField(Snapshot parent, ushort id, SnapshotFieldType type, object defaultValue,
            SnapshotPrimitiveType primitiveType = SnapshotPrimitiveType.None)
            : base(parent, type, defaultValue, primitiveType)
        {
            Id = id;
        }
    }

    public class DynamicSnapshotField : SnapshotField
    {
        public object Id { get; }
        public SnapshotPrimitiveType IdPrimitiveType { get; }

        internal DynamicSnapshotField(Snapshot parent, object id, SnapshotFieldType type, object defaultValue,
            SnapshotPrimitiveType idPrimitiveType,
            SnapshotPrimitiveType primitiveType = SnapshotPrimitiveType.None)
            : base(parent, type, defaultValue, primitiveType)
        {
            Id = id;
            IdPrimitiveType = idPrimitiveType;
        }
    }
}
