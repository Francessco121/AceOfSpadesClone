using Dash.Engine.Diagnostics;
using System;

/* PhysicsObject.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Physics
{
    public class PhysicsBodyComponent : Component
    {
        public Vector3 Velocity;
        public Vector3 Size;

        public float Mass;
        public float Friction
        {
            get { return 1f - InverseFriction; }
            set { InverseFriction = 1f - value; }
        }
        public float InverseFriction = 0.8f;
        public float MaxStep = 7;

        public bool IsGrounded;
        public bool IsStatic;
        public bool IsAffectedByGravity = true;

        public bool CanStep;
        public bool CanBeSteppedOn = true;
        public bool CanCollide = true;
        public bool CanCollideWithSoft = true;
        public bool CanBePushedBySoft = true;
        public bool CanCollideWithTerrain = true;
        public bool AlwaysApplyFriction = false;
        public bool BounceOnWallCollision = false;
        public bool BounceOnVerticalCollision = false;
        public float VerticalBounceFalloff;
        public float HorizontalBounceFalloff;

        public event EventHandler<PhysicsBodyComponent> OnCollision;

        protected internal DeltaSnapshot Delta { get; private set; }

        public PhysicsBodyComponent(Vector3 size)
            : this(size, 1f)
        { }

        public PhysicsBodyComponent(Vector3 size, float mass)
        {
            Size = size;
            Mass = mass;
            Delta = new DeltaSnapshot();
        }

        protected internal override void OnAddedToScene()
        {
            PhysicsEngine physics = Scene.GetComponent<PhysicsEngine>();
            if (physics != null)
                physics.AddPhysicsBody(this);
            else
                DashCMD.WriteWarning("[PhysicsBodyComponent] GameObject with physics body added to scene without physics engine!");

            base.OnAddedToScene();
        }

        protected internal override void OnRemovedFromScene()
        {
            PhysicsEngine physics = Scene.GetComponent<PhysicsEngine>();
            if (physics != null)
                physics.RemovePhysicsBody(this);

            base.OnRemovedFromScene();
        }

        public override void Dispose()
        {
            if (Scene != null)
            {
                PhysicsEngine physics = Scene.GetComponent<PhysicsEngine>();
                if (physics != null)
                    physics.RemovePhysicsBody(this);
            }

            base.Dispose();
        }

        protected override void OnAttached()
        {
            Delta.FinalPosition = Transform.Position;
            base.OnAttached();
        }

        public virtual AxisAlignedBoundingBox GetBroadphase()
        {
            Vector3 halfSize = Size / 2f;
            float stepConsideration = CanStep ? MaxStep : 0;
            Vector3 max = Maths.Max(Transform.Position + halfSize + new Vector3(0, stepConsideration, 0), 
                Delta.FinalPosition + halfSize);
            Vector3 min = Maths.Min(Transform.Position - halfSize, Delta.FinalPosition - halfSize);

            return new AxisAlignedBoundingBox(min, max);
        }

        public virtual void OnCollide(PhysicsBodyComponent with)
        {
            if (OnCollision != null)
                OnCollision(this, with);
        }

        public virtual AxisAlignedBoundingBox GetCollider()
        {
            return GetColliderAt(Transform.Position);
        }

        public virtual AxisAlignedBoundingBox GetColliderAt(Vector3 position)
        {
            Vector3 halfSize = Size / 2f;
            return new AxisAlignedBoundingBox(position - halfSize, position + halfSize);
        }

        internal protected virtual void PreUpdate(float deltaTime) { }
        internal protected virtual void PostUpdate(float deltaTime) { }

        internal void Step(float deltaTime)
        {
            if (GameObject.IsEnabled)
            {
                if (AlwaysApplyFriction || IsGrounded)
                {
                    Velocity.X *= InverseFriction;
                    Velocity.Z *= InverseFriction;
                }

                // Set delta snapshot
                Delta.FinalPosition = Transform.Position + Velocity * deltaTime;
                Delta.StepPosition = new Vector3(Delta.FinalPosition.X, Transform.Position.Y + MaxStep, Delta.FinalPosition.Z);
                Delta.FinalVelocity = Velocity;
                Delta.Stepped = false;
            }
        }
    }
}
