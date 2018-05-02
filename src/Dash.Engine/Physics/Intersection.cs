/* Intersection.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Physics
{
    public enum IntersectionType
    {
        Rigid, Soft
    }

    public class Intersection
    {
        public PhysicsBodyComponent Object1 { get; private set; }
        public PhysicsBodyComponent Object2 { get; private set; }
        public Vector3 Object1Normal;
        public Vector3 Object2Normal;
        public float Object1EntryTime { get; set; }
        public float Object2EntryTime { get; set; }
        public IntersectionType Type { get; private set; }

        internal AABBCollisionResolver Resolver { get; private set; }

        float deltaTime;

        internal Intersection(PhysicsBodyComponent object1, PhysicsBodyComponent object2, AABBCollisionResolver resolver, 
            float deltaTime, IntersectionType type)
        {
            Object1 = object1;
            Object2 = object2;
            Resolver = resolver;
            this.deltaTime = deltaTime;
            Type = type;

            Object1EntryTime = Resolver.Sweep(Object1.GetCollider(), Object2.GetCollider(), Object1.Velocity * deltaTime, out Object2Normal);
            Object2EntryTime = Resolver.Sweep(Object2.GetCollider(), Object1.GetCollider(), Object2.Velocity * deltaTime, out Object1Normal);
        }

        public void UpdateFromDelta()
        {
            if (!Object1.IsStatic)
            {
                // Vector2 fakeVelocity1a = Object1.Delta.FinalPosition - Object1.Position;
                Vector3 fakeVelocity1 = Object1.Delta.FinalVelocity * deltaTime;
                Object1EntryTime = Resolver.Sweep(Object1.GetCollider(), Object2.GetCollider(), fakeVelocity1, out Object2Normal);
            }
            else
                Object1EntryTime = 1f;

            if (!Object2.IsStatic)
            {
                //Vector2 fakeVelocity2a = Object2.Delta.FinalPosition - Object2.Position;
                Vector3 fakeVelocity2 = Object2.Delta.FinalVelocity * deltaTime;
                Object2EntryTime = Resolver.Sweep(Object2.GetCollider(), Object1.GetCollider(), fakeVelocity2, out Object1Normal);
            }
            else
                Object2EntryTime = 1f;
        }

        //public static Intersection SphereVsPlane(BoundingSphere sphere, Plane plane)
        //{
        //    float distFromSphereCenter = Math.Abs(Vector3.Dot(plane.Normal, sphere.Center) + plane.Distance);
        //    float distFromSphere = distFromSphereCenter - sphere.Radius;

        //    return new Intersection(distFromSphere < 0, distFromSphere);
        //}
    }
}
