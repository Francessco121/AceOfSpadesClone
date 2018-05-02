using Dash.Engine;

namespace AceOfSpades.Editor
{
    public class VoxelObjectRaycastResult : RaycastResult
    {
        public IndexPosition? IntersectedBlockIndex { get; }

        public CubeSide? IntersectionSide { get; }

        /// <summary>
        /// Creates a voxel object raycast result representing no intersection.
        /// </summary>
        public VoxelObjectRaycastResult(Ray ray)
            : base(ray)
        { }

        public VoxelObjectRaycastResult(Ray ray, bool intersects, Vector3? intersectionPosition, float? intersectionDistance,
            IndexPosition? intersectedBlockIndex, CubeSide? intersectionSide)
            : base(ray, intersects, intersectionPosition, intersectionDistance)
        {
            IntersectedBlockIndex = intersectedBlockIndex;
            IntersectionSide = intersectionSide;
        }
    }
}
