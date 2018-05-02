using Dash.Engine;
using Dash.Engine.Physics;

/* Character.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Characters
{
    public class CharacterController : PhysicsBodyComponent
    {
        public bool IsMoving { get; private set; }
        public bool IsCrouching { get; set; }

        public Vector3 MoveVector { get; set; }
        public Vector3 MoveVectorOffset { get; set; }
        public float MoveVectorOffsetFactor { get; set; }
        public float MovementSmoothingInterp { get; set; } = 1f;
        public Vector3 DeltaPosition { get; private set; }
        public Vector3 LastNonZeroMoveVector { get; private set; }
        public float NormalHeight { get; private set; }
        public float CrouchHeight { get; private set; }
        
        Vector3 lastMoveVector;
        Vector3 lastPosition;

        public CharacterController(Vector3 size, float crouchHeight)
            : base(size)
        {
            NormalHeight = size.Y;
            CrouchHeight = crouchHeight;

            AlwaysApplyFriction = true;
            CanStep = true;
        }

        public AxisAlignedBoundingBox GetStandingCollider()
        {
            Vector3 halfSize = new Vector3(Size.X, NormalHeight, Size.Z) / 2f;
            return new AxisAlignedBoundingBox(Transform.Position - halfSize, Transform.Position + halfSize);
        }

        public void TryUncrouch(World world)
        {
            bool canUncrouch = true;

            float highestY;
            float bias = 0.01f;

            AxisAlignedBoundingBox standingCollider = GetStandingCollider();
            bool intersectsTerrain = world.TerrainPhysics.AABBIntersectsTerrain(standingCollider, out highestY);

            if (intersectsTerrain)
            {
                if (highestY >= standingCollider.Max.Y)
                    canUncrouch = false;
                else if (!IsGrounded)
                {
                    float pen = highestY - standingCollider.Min.Y;

                    if (pen <= (NormalHeight / 2f) - (CrouchHeight / 2f))
                        Transform.Position.Y += pen + bias;
                }
                else if (IsGrounded)
                    Transform.Position.Y += (NormalHeight / 2f) - (CrouchHeight / 2f);
            }

            if (canUncrouch)
                IsCrouching = false;
        }

        protected override void PreUpdate(float deltaTime)
        {
            Size.Y = IsCrouching ? CrouchHeight : NormalHeight;

            if (!IsStatic)
            {
                IsMoving = MoveVector.X != 0 || MoveVector.Z != 0;

                lastMoveVector = Interpolation.Lerp(lastMoveVector, MoveVector, MovementSmoothingInterp);
                lastMoveVector = Interpolation.Lerp(lastMoveVector, MoveVectorOffset, MoveVectorOffsetFactor);
                if (lastMoveVector.X != 0) LastNonZeroMoveVector.SetX(Velocity.X = lastMoveVector.X);
                if (MoveVector.Y != 0) LastNonZeroMoveVector.SetY(Velocity.Y = MoveVector.Y);
                if (lastMoveVector.Z != 0) LastNonZeroMoveVector.SetZ(Velocity.Z = lastMoveVector.Z);
            }
            else
            {
                MoveVector = Velocity = Vector3.Zero;
                IsMoving = false;
            }
            
            base.PreUpdate(deltaTime);
        }

        protected override void PostUpdate(float deltaTime)
        {
            DeltaPosition = Transform.Position - lastPosition;
            lastPosition = Transform.Position;
            base.PostUpdate(deltaTime);
        }
    }
}
