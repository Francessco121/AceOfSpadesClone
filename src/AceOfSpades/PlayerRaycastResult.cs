using AceOfSpades.Characters;
using Dash.Engine;

/* PlayerRaycastResult.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class PlayerRaycastResult : RaycastResult
    {
        public Player Player { get; }

        /// <summary>
        /// Creates a player raycast result representing no intersection.
        /// </summary>
        public PlayerRaycastResult(Ray ray) 
            : base(ray)
        { }

        public PlayerRaycastResult(Ray ray, bool intersects, Vector3? intersectionPosition, float? intersectionDistance,
            Player player) 
            : base(ray, intersects, intersectionPosition, intersectionDistance)
        {
            Player = player;
        }
    }
}
