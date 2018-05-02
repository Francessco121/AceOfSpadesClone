using Dash.Engine;

/* RaycastResult.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class RaycastResult
    {
        public Ray Ray { get; }
        public Vector3? IntersectionPosition { get; }
        public float? IntersectionDistance { get; }
        public bool Intersects { get; }

        /// <summary>
        /// Creates a raycast result representing no intersection.
        /// </summary>
        public RaycastResult(Ray ray)
        {
            Ray = ray;
            Intersects = false;
        }

        /// <summary>
        /// Builds a raycast result from an existing result.
        /// </summary>
        /// <param name="result"></param>
        public RaycastResult(RaycastResult result)
        {
            Ray = result.Ray;
            IntersectionPosition = result.IntersectionPosition;
            IntersectionDistance = result.IntersectionDistance;
            Intersects = result.Intersects;
        }

        public RaycastResult(Ray ray, bool intersects, Vector3? intersectionPosition, float? intersectionDistance)
        {
            Ray = ray;
            Intersects = intersects;
            IntersectionDistance = intersectionDistance;
            IntersectionPosition = intersectionPosition;
        }
    }
}
