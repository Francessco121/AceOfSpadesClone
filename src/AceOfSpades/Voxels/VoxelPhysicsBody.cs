using Dash.Engine.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dash.Engine;

namespace AceOfSpades
{
    public class VoxelPhysicsBody : PhysicsBodyComponent
    {
        Vector3 cubeSizeOffset;

        public VoxelPhysicsBody(Vector3 size, float cubeSize)
            : this(size, 1f, cubeSize)
        { }

        public VoxelPhysicsBody(Vector3 size, float mass, float cubeSize) 
            : base(size, mass)
        {
            cubeSizeOffset = new Vector3(-cubeSize / 2f, -cubeSize / 2f, -cubeSize / 2f);
        }

        public override AxisAlignedBoundingBox GetBroadphase()
        {
            Vector3 max = Maths.Max(Transform.Position + Size + cubeSizeOffset, Delta.FinalPosition + Size + cubeSizeOffset);
            Vector3 min = Maths.Min(Transform.Position + cubeSizeOffset, Delta.FinalPosition + cubeSizeOffset);

            return new AxisAlignedBoundingBox(min, max);
        }

        public override AxisAlignedBoundingBox GetCollider()
        {
            return new AxisAlignedBoundingBox(Transform.Position + cubeSizeOffset, Transform.Position + Size + cubeSizeOffset);
        }

        public override AxisAlignedBoundingBox GetColliderAt(Vector3 position)
        {
            return new AxisAlignedBoundingBox(position + cubeSizeOffset, position + Size + cubeSizeOffset);
        }
    }
}
