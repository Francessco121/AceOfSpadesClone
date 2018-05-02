/* SnapshotId.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    public struct SnapshotId
    {
        public readonly ushort ParentId;
        public ushort? ChildId;

        public SnapshotId(ushort id)
        {
            ParentId = id;
            ChildId = null;
        }

        public SnapshotId(ushort parentId, ushort? childId)
        {
            ParentId = parentId;
            ChildId = childId;
        }

        public override bool Equals(object obj)
        {
            if (GetType() == obj.GetType())
            {
                SnapshotId other = (SnapshotId)obj;
                return other.ParentId == ParentId
                    && other.ChildId == ChildId;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return ParentId * (ChildId.HasValue ? ChildId.Value : 1);
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", ParentId, ChildId);
        }
    }
}
