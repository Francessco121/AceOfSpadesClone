using System.Collections.Generic;

/* IPhysicsEngineExtension.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Physics
{
    public interface IPhysicsEngineExtension
    {
        bool IsActive { get; }

        bool CanCheck(IntersectionType intersectType, bool objectIsStatic);
        IEnumerable<PhysicsBodyComponent> GetBroadphaseIntersections(AxisAlignedBoundingBox broad);
        void RecyclePhysicsObjects();
    }
}
