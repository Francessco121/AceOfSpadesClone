using System;
using System.Collections.Generic;

/* PhysicsEngine.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Physics
{
    public class PhysicsEngine : SceneComponent
    {
        public static Vector3 GlobalGravity = new Vector3(0, -9.81f * 6, 0);

        public static List<AxisAlignedBoundingBox> LastProcessedBoundingBoxes { get; }
        public static bool RecordProcessedBoundingBoxes;

        public Vector3 Gravity;

        public int NumPhysicsObjects { get { return physicsBodies.Count; } }

        List<PhysicsBodyComponent> physicsBodies;
        HashSet<Intersection> broadIntersections;
        Intersection[] intersections;

        AABBCollisionResolver sweepResolver;
        float timestep;
        float timestepDeltaTime;
        float accumulator;

        HashSet<IPhysicsEngineExtension> extensions;

        static PhysicsEngine()
        {
            LastProcessedBoundingBoxes = new List<AxisAlignedBoundingBox>();
        }

        public PhysicsEngine(float timestep)
        {
            this.timestep = timestep;
            timestepDeltaTime = 1f / timestep;

            Gravity = GlobalGravity;

            physicsBodies = new List<PhysicsBodyComponent>();
            broadIntersections = new HashSet<Intersection>();
            extensions = new HashSet<IPhysicsEngineExtension>();
            sweepResolver = new AABBCollisionResolver();
        }

        public void AddExtension(IPhysicsEngineExtension extension)
        {
            extensions.Add(extension);
        }

        public void AddPhysicsBody(PhysicsBodyComponent body)
        {
            physicsBodies.Add(body);
        }

        public void RemovePhysicsBody(PhysicsBodyComponent body)
        {
            physicsBodies.Remove(body);
        }

        public void Clear()
        {
            physicsBodies.Clear();
        }

        protected internal override void Update(float deltaTime)
        {
            accumulator += deltaTime;

            // Avoid spiral of death
            if (accumulator > 1)
                accumulator = 1;

            // Update physics
            // here we must update at least once, 
            // but are allowed to update as many times
            // as needed.
            do
            {
                float dt = Math.Min(accumulator, timestepDeltaTime);
                UpdatePhysics(dt);
                accumulator -= dt;
            } while (accumulator > timestepDeltaTime);
        }

        public void SimulateSingle(PhysicsBodyComponent ob, float deltaTime)
        {
            StepSingle(ob, deltaTime);
            BroadphaseSingle(IntersectionType.Soft, ob, deltaTime);
            CollisionResponse(deltaTime);
            BroadphaseSingle(IntersectionType.Rigid, ob, deltaTime);
            CollisionResponse(deltaTime);
            ApplyDeltaSnapshot(ob, deltaTime);
        }

        void UpdatePhysics(float deltaTime)
        {
            LastProcessedBoundingBoxes.Clear();

            foreach (IPhysicsEngineExtension ext in extensions)
                if (ext.IsActive)
                    ext.RecyclePhysicsObjects();

            Step(deltaTime);
            BroadphaseMulti(IntersectionType.Soft, deltaTime);
            CollisionResponse(deltaTime);
            BroadphaseMulti(IntersectionType.Rigid, deltaTime);
            CollisionResponse(deltaTime);
            ApplyDeltaSnapshots(deltaTime);
        }

        void Step(float deltaTime)
        {
            // Step objects
            for (int i = 0; i < physicsBodies.Count; i++)
            {
                PhysicsBodyComponent ob = physicsBodies[i];
                StepSingle(ob, deltaTime);
            }
        }

        void StepSingle(PhysicsBodyComponent ob, float deltaTime)
        {
            if (!ob.IsStatic && ob.IsEnabled && ob.GameObject.IsEnabled)
            {
                ob.PreUpdate(deltaTime);
                if (ob.IsAffectedByGravity)
                    ob.Velocity += Gravity * deltaTime;
                ob.Step(deltaTime);
            }
        }

        void BroadphaseMulti(IntersectionType forType, float deltaTime)
        {
            broadIntersections.Clear();

            // For every object, check if it CAN intersect with any other object
            for (int i = 0; i < physicsBodies.Count; i++)
                Broadphase(forType, physicsBodies[i], deltaTime, i + 1);

            // Sort the intersections by entry time, where first intersections are the first in the array.
            intersections = new Intersection[broadIntersections.Count];
            broadIntersections.CopyTo(intersections);
            Array.Sort(intersections, CompareIntersection);
        }

        void BroadphaseSingle(IntersectionType forType, PhysicsBodyComponent p1, float deltaTime)
        {
            broadIntersections.Clear();

            Broadphase(forType, p1, deltaTime, 0);

            // Sort the intersections by entry time, where first intersections are the first in the array.
            intersections = new Intersection[broadIntersections.Count];
            broadIntersections.CopyTo(intersections);
            Array.Sort(intersections, CompareIntersection);
        }

        void Broadphase(IntersectionType forType, PhysicsBodyComponent p1, float deltaTime, int start)
        {
            if (!p1.IsEnabled || !p1.GameObject.IsEnabled)
                return;
            if (!p1.CanCollide)
                return;

            // For every object, check if it CAN intersect with any other object
            AxisAlignedBoundingBox p1Collider = p1.GetCollider();
            AxisAlignedBoundingBox p1Broad = p1.GetBroadphase();
            AxisAlignedBoundingBox? p1StepCollider = null;
            if (p1.CanStep)
                p1StepCollider = p1.GetColliderAt(p1.Delta.StepPosition);

            if (RecordProcessedBoundingBoxes)
            {
                LastProcessedBoundingBoxes.Add(p1Collider);
                LastProcessedBoundingBoxes.Add(p1Broad);
            }

            for (int k = start; k < physicsBodies.Count; k++)
            {
                PhysicsBodyComponent p2 = physicsBodies[k];
                if (!p2.IsEnabled || !p2.GameObject.IsEnabled)
                    continue;
                if (p2 == p1)
                    continue;
                if (!p2.CanCollide)
                    continue;

                // Get the type of intersection to determine how it should be handled
                IntersectionType type = GetIntersectionType(p1.IsStatic, p2.IsStatic);
                if (type != forType)
                    continue;
                if (forType == IntersectionType.Soft && (!p2.CanCollideWithSoft || !p1.CanCollideWithSoft))
                    continue;

                AxisAlignedBoundingBox p2Collider = p2.GetCollider();
                AxisAlignedBoundingBox p2Broad = p2.GetBroadphase();
                AxisAlignedBoundingBox? p2StepCollider = null;
                if (p2.CanStep)
                    p2StepCollider = p2.GetColliderAt(p2.Delta.StepPosition);

                if (RecordProcessedBoundingBoxes)
                {
                    LastProcessedBoundingBoxes.Add(p2Collider);
                    LastProcessedBoundingBoxes.Add(p2Broad);
                }

                // Check if the objects CAN intersect
                if (CanIntersect(p1.IsStatic, p1Collider, p1Broad, p2.IsStatic, p2Collider, p2Broad))
                {
                    // Calculate max step
                    if (type == IntersectionType.Rigid)
                        CalculateMaxStep(p1, p2, p1Collider, p1StepCollider, p2Collider, p2StepCollider);

                    // Add the possible intersection
                    Intersection intersect = new Intersection(p1, p2, sweepResolver, deltaTime, type);
                    broadIntersections.Add(intersect);
                }
            }

            // Let extensions run
            foreach (IPhysicsEngineExtension ext in extensions)
            {
                if (!ext.IsActive || !ext.CanCheck(forType, p1.IsStatic))
                    continue;

                // Get intersections from extensions
                IEnumerable<PhysicsBodyComponent> intersections = ext.GetBroadphaseIntersections(p1Broad);

                foreach (PhysicsBodyComponent physOb in intersections)
                {
                    AxisAlignedBoundingBox? physObStepCollider = null;
                    if (physOb.CanStep)
                        physObStepCollider = physOb.GetColliderAt(physOb.Delta.StepPosition);

                    IntersectionType type = GetIntersectionType(p1.IsStatic, physOb.IsStatic);

                    // Calculate max step
                    if (type == IntersectionType.Rigid)
                        CalculateMaxStep(p1, physOb, p1Collider, p1StepCollider, physOb.GetCollider(), physObStepCollider);

                    // Add the possible intersection
                    Intersection intersection = new Intersection(p1, physOb, sweepResolver, deltaTime, IntersectionType.Rigid);
                    broadIntersections.Add(intersection);
                }
            }
        }

        void CalculateMaxStep(PhysicsBodyComponent p1, PhysicsBodyComponent p2,
            AxisAlignedBoundingBox p1Col, AxisAlignedBoundingBox? p1StepCol, 
            AxisAlignedBoundingBox p2Col, AxisAlignedBoundingBox? p2StepCol)
        {
            if (p2.IsStatic && p1StepCol.HasValue && p1StepCol.Value.Intersects(p2Col))
                p1.Delta.MaxStep = 0;
            else if (p1.IsStatic && p2StepCol.HasValue && p2StepCol.Value.Intersects(p1Col))
                p2.Delta.MaxStep = 0;
        }

        IntersectionType GetIntersectionType(bool p1Static, bool p2Static)
        {
            if (!p1Static && !p2Static)
                return IntersectionType.Soft;
            else
                return IntersectionType.Rigid;
        }

        bool CanIntersect(bool p1Static, AxisAlignedBoundingBox c1, AxisAlignedBoundingBox b1, 
            bool p2Static, AxisAlignedBoundingBox c2, AxisAlignedBoundingBox b2)
        {
            if (p1Static && p2Static) return false;
            else if (p1Static && !p2Static)
                return b2.Intersects(c1);
            else if (!p1Static && p2Static)
                return b1.Intersects(c2);
            else
                return b1.Intersects(b2);
        }

        void CollisionResponse(float deltaTime)
        {
            for (int i = 0; i < intersections.Length; i++)
            {
                Intersection intersection = intersections[i];
                PhysicsBodyComponent p1 = intersection.Object1;
                PhysicsBodyComponent p2 = intersection.Object2;
                AxisAlignedBoundingBox p1Collider = p1.GetCollider();
                AxisAlignedBoundingBox p1DeltaCollider = p1.GetColliderAt(p1.Delta.FinalPosition);
                AxisAlignedBoundingBox p2Collider = p2.GetCollider();
                AxisAlignedBoundingBox p2DeltaCollider = p2.GetColliderAt(p2.Delta.FinalPosition);

                if (intersection.Type == IntersectionType.Rigid)
                {
                    // Update the intersection data, so that previous updates for
                    // these objects are applied
                    intersection.UpdateFromDelta();

                    // If both entry times are invalid, the objects aren't intersecting anymore,
                    // so ignore the collision.
                    if (intersection.Object1EntryTime == 1 && intersection.Object2EntryTime == 1)
                        continue;

                    // Handle the collision based on static states
                    if (intersection.Object2.IsStatic)
                    {
                        // If we are past the first sweep pass for this object,
                        // and this intersection is no longer valid, then ignore it.
                        if (p1.Delta.DeltaPass > 0)
                            if (!p1DeltaCollider.Intersects(p2Collider))
                                continue;

                        // Gather intersection data
                        Vector3 surfaceNormal = intersection.Object2Normal;
                        float collisionTime = intersection.Object1EntryTime;
                        float remainingTime = 1f - collisionTime;
                        float stepDist = intersection.Resolver.StepDistance(p1DeltaCollider, p2Collider);

                        //if (surfaceNormal.Y == 0)
                        //    Diagnostics.DashCMD.WriteLine("{0} | {1}", stepDist.ToString(), Math.Min(p1.Delta.MaxStep, p1.MaxStep));
                        //Diagnostics.DashCMD.WriteLine(surfaceNormal);
                        // Try to step on object
                        if (p1.CanStep && p2.CanBeSteppedOn && surfaceNormal.Y == 0
                            && stepDist >= 0 && stepDist <= Math.Min(p1.Delta.MaxStep, p1.MaxStep))
                        {
                            // Step onto object
                            p1.Delta.FinalPosition.Y += stepDist + 0.001f;
                            p1.Delta.FinalVelocity.Y = 0;
                            p1.Delta.IsGrounded = p1.Delta.IsGrounded || true;
                            p1.Delta.Stepped = true;
                        }
                        else // Normal collision resolve
                        {
                            // Calculate the compensation in position from the collision
                            Vector3 compensation = surfaceNormal * (Maths.Abs(p1.Delta.FinalVelocity * deltaTime)) * remainingTime;
                            p1.Delta.FinalPosition += compensation;

                            // If this normal moved the object anywhere upward,
                            // that object is now considered grounded
                            if (surfaceNormal.Y > 0)
                                p1.Delta.IsGrounded = p1.Delta.IsGrounded || true;

                            // Fix the velocity
                            if (p1.BounceOnWallCollision || p1.BounceOnVerticalCollision)
                            {
                                if ((surfaceNormal.X != 0 || surfaceNormal.Z != 0) && p1.BounceOnWallCollision)
                                {
                                    if (surfaceNormal.X != 0) p1.Delta.FinalVelocity.X *= -(1f - p1.HorizontalBounceFalloff);
                                    if (surfaceNormal.Z != 0) p1.Delta.FinalVelocity.Z *= -(1f - p1.HorizontalBounceFalloff);
                                }
                                else if (surfaceNormal.Y != 0 && p1.BounceOnVerticalCollision)
                                {
                                    p1.Delta.FinalVelocity.Y *= -(1f - p1.VerticalBounceFalloff);
                                    p1.Delta.FinalVelocity.X *= p1.InverseFriction;
                                    p1.Delta.FinalVelocity.Z *= p1.InverseFriction;
                                }
                                else
                                    intersection.Resolver.FixVelocity(ref p1.Delta.FinalVelocity, surfaceNormal);
                            }
                            else
                                intersection.Resolver.FixVelocity(ref p1.Delta.FinalVelocity, surfaceNormal);
                        }

                        // Another delta pass has occured
                        p1.Delta.DeltaPass++;

                        p1.OnCollide(p2);
                        p2.OnCollide(p1);
                    }
                    else if (intersection.Object1.IsStatic)
                    {
                        // If we are past the first sweep pass for this object,
                        // and this intersection is no longer valid, then ignore it.
                        if (p2.Delta.DeltaPass > 0)
                            if (!p2DeltaCollider.Intersects(p1Collider))
                                continue;

                        // Gather intersection data
                        Vector3 surfaceNormal = intersection.Object1Normal;
                        float collisionTime = intersection.Object2EntryTime;
                        float remainingTime = 1f - collisionTime;
                        float stepDist = intersection.Resolver.StepDistance(p2DeltaCollider, p1Collider);

                        // Try to step on object
                        if (p2.CanStep && p1.CanBeSteppedOn && surfaceNormal.Y == 0
                            && stepDist >= 0 && stepDist <= Math.Min(p2.Delta.MaxStep, p2.MaxStep))
                        {
                            // Step onto object
                            p2.Delta.FinalPosition.Y += stepDist + 0.001f;
                            p2.Delta.FinalVelocity.Y = 0;
                            p2.Delta.IsGrounded = p2.Delta.IsGrounded || true;
                            p2.Delta.Stepped = true;
                        }
                        else // Normal collision resolve
                        {
                            // Calculate the compensation in position from the collision
                            Vector3 compensation = surfaceNormal * (Maths.Abs(p2.Delta.FinalVelocity * deltaTime)) * remainingTime;
                            p2.Delta.FinalPosition += compensation;

                            // If this normal moved the object anywhere upward,
                            // that object is now considered grounded
                            if (surfaceNormal.Y > 0)
                                p2.Delta.IsGrounded = p2.Delta.IsGrounded || true;

                            // Fix the velocity
                            if (p2.BounceOnWallCollision || p2.BounceOnVerticalCollision)
                            {
                                if ((surfaceNormal.X != 0 || surfaceNormal.Z != 0) && p2.BounceOnWallCollision)
                                {
                                    if (surfaceNormal.X != 0) p2.Delta.FinalVelocity.X *= -(1f - p2.HorizontalBounceFalloff);
                                    if (surfaceNormal.Z != 0) p2.Delta.FinalVelocity.Z *= -(1f - p2.HorizontalBounceFalloff);
                                }
                                else if (surfaceNormal.Y != 0 && p2.BounceOnVerticalCollision)
                                {
                                    p2.Delta.FinalVelocity.Y *= -(1f - p2.VerticalBounceFalloff);
                                    p1.Delta.FinalVelocity.X *= p1.InverseFriction;
                                    p1.Delta.FinalVelocity.Z *= p1.InverseFriction;
                                }
                                else
                                    intersection.Resolver.FixVelocity(ref p2.Delta.FinalVelocity, surfaceNormal);
                            }
                            else
                                intersection.Resolver.FixVelocity(ref p2.Delta.FinalVelocity, surfaceNormal);
                        }

                        // Another delta pass has occured
                        p2.Delta.DeltaPass++;

                        p1.OnCollide(p2);
                        p2.OnCollide(p1);
                    }
                }
                else
                {
                    // If we are past the first sweep pass for this object,
                    // and this intersection is no longer valid, then ignore it.
                    if ((!p1DeltaCollider.Intersects(p2Collider))
                        && (!p2DeltaCollider.Intersects(p1Collider)))
                        continue;


                    float p1DeltaMiddleX = p1.Delta.FinalPosition.X + p1.Size.X / 2f;
                    float p2DeltaMiddleX = p2.Delta.FinalPosition.X + p2.Size.X / 2f;
                    float p1DeltaMiddleZ = p1.Delta.FinalPosition.Z + p1.Size.Z / 2f;
                    float p2DeltaMiddleZ = p2.Delta.FinalPosition.Z + p2.Size.Z / 2f;
                    Vector3 p1Move = new Vector3((p2DeltaMiddleX - p1DeltaMiddleX), 0, (p2DeltaMiddleZ - p1DeltaMiddleZ));
                    Vector3 p2Move = new Vector3((p1DeltaMiddleX - p2DeltaMiddleX), 0, (p1DeltaMiddleZ - p2DeltaMiddleZ));

                    if (p1Move.X == p2Move.X)
                    {
                        p1Move.X += Maths.Random.Next((int)-p2.Size.X, (int)p2.Size.X) / deltaTime;
                    }

                    Vector2 masses = new Vector2((p2.Mass / p1.Mass), (p1.Mass / p2.Mass)) * 0.5f;

                    if (p1.CanBePushedBySoft)
                    {
                        p1.Delta.FinalVelocity -= p1Move * masses.X;
                        p1.Delta.FinalPosition = p1.Transform.Position + p1.Delta.FinalVelocity * deltaTime;
                    }

                    if (p2.CanBePushedBySoft)
                    {
                        p2.Delta.FinalVelocity -= p2Move * masses.Y;
                        p2.Delta.FinalPosition = p2.Transform.Position + p2.Delta.FinalVelocity * deltaTime;
                    }

                    p1.OnCollide(p2);
                    p2.OnCollide(p1);
                }
            }
        }

        void ApplyDeltaSnapshots(float deltaTime)
        {
            foreach (PhysicsBodyComponent ob in physicsBodies)
                ApplyDeltaSnapshot(ob, deltaTime);
        }

        void ApplyDeltaSnapshot(PhysicsBodyComponent ob, float deltaTime)
        {
            if (!ob.IsStatic)
            {
                // Apply all the changes to the physics objects from,
                // the collision response and step
                ob.Transform.Position = ob.Delta.FinalPosition;
                ob.Velocity = ob.Delta.FinalVelocity;
                ob.IsGrounded = ob.Delta.IsGrounded;
                ob.Delta.Reset();
            }

            ob.PostUpdate(deltaTime);
        }

        int CompareIntersection(Intersection a, Intersection b)
        {
            float entry1 = Math.Min(a.Object1EntryTime, a.Object2EntryTime);
            float entry2 = Math.Min(b.Object1EntryTime, b.Object2EntryTime);

            bool y1 = a.Object1Normal.Y != 0 || a.Object2Normal.Y != 0;
            bool y2 = b.Object1Normal.Y != 0 || b.Object2Normal.Y != 0;

            bool typesDifferent = a.Type != b.Type;

            if (a.Type == IntersectionType.Soft && b.Type == IntersectionType.Rigid
                || (!typesDifferent && (entry1 < entry2 || y1 && !y2)))
                return -1;
            else if (b.Type == IntersectionType.Soft && a.Type == IntersectionType.Rigid
                || (!typesDifferent && (entry1 > entry2 || !y1 && y2)))
                return 1;
            else
                return 0;
        }
    }
}
