using Dash.Engine;

/* TerrainRaycastResult.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class TerrainRaycastResult : RaycastResult
    {
        public Chunk Chunk { get; }
        public IndexPosition? BlockIndex { get; }
        public Block? Block { get; }
        public CubeSide? IntersectionCubeSide { get; }

        /// <summary>
        /// Creates a terrain raycast result representing no intersection.
        /// </summary>
        public TerrainRaycastResult(Ray ray) 
            : base(ray)
        { }

        public TerrainRaycastResult(Ray ray, bool intersects, Vector3? intersectionPosition, float? intersectionDistance,
            Chunk chunk, IndexPosition? blockIndex, Block? block, CubeSide? intersectionCubeSide) 
            : base(ray, intersects, intersectionPosition, intersectionDistance)
        {
            Chunk = chunk;
            BlockIndex = blockIndex;
            Block = block;
            IntersectionCubeSide = intersectionCubeSide;
        }
    }
}
