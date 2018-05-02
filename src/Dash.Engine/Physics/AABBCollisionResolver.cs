using System;

/* AABBCollisionResolver.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Physics
{
    class AABBCollisionResolver
    {
        public void FixVelocity(ref Vector3 velocity, Vector3 normal)
        {
            if (normal.X != 0) velocity.X = 0;
            if (normal.Y != 0) velocity.Y = 0;
            if (normal.Z != 0) velocity.Z = 0;
        }

        public float StepDistance(AxisAlignedBoundingBox aabb, AxisAlignedBoundingBox ontoAABB)
        {
            return ontoAABB.Max.Y - aabb.Min.Y;
        }

        public float Sweep(AxisAlignedBoundingBox a, AxisAlignedBoundingBox b, Vector3 v, out Vector3 normal)
        {
            float xInvEntry, yInvEntry, zInvEntry;
            float xInvExit, yInvExit, zInvExit;
            float xMaxDist, yMaxDist, zMaxDist;

            if (v.X > 0)
            {
                xMaxDist = b.Max.X - a.Min.X;
                xInvEntry = b.Min.X - (a.Max.X);
                xInvExit = b.Max.X - a.Min.X;
            }
            else
            {
                xMaxDist = b.Min.X - a.Max.X;
                xInvEntry = b.Max.X - a.Min.X;
                xInvExit = b.Min.X - (a.Max.X);
            }

            if (v.Y > 0)
            {
                yMaxDist = b.Max.Y - a.Min.Y;
                yInvEntry = b.Min.Y - (a.Max.Y);
                yInvExit = b.Max.Y - a.Min.Y;
            }
            else
            {
                yMaxDist = b.Min.Y - a.Max.Y;
                yInvEntry = b.Max.Y - a.Min.Y;
                yInvExit = b.Min.Y - (a.Max.Y);
            }

            if (v.Z > 0)
            {
                zMaxDist = b.Max.Z - a.Min.Z;
                zInvEntry = b.Min.Z - (a.Max.Z);
                zInvExit = b.Max.Z - a.Min.Z;
            }
            else
            {
                zMaxDist = b.Min.Z - a.Max.Z;
                zInvEntry = b.Max.Z - a.Min.Z;
                zInvExit = b.Min.Z - (a.Max.Z);
            }

            float xEntry, yEntry, zEntry;
            float xExit, yExit, zExit;

            if (v.X == 0)
            {
                xEntry = float.MinValue;
                xExit = float.MaxValue;
            }
            else
            {
                xEntry = xInvEntry / v.X;
                xExit = xInvExit / v.X;
            }

            if (v.Y == 0)
            {
                yEntry = float.MinValue;
                yExit = float.MaxValue;
            }
            else
            {
                yEntry = yInvEntry / v.Y;
                yExit = yInvExit / v.Y;
            }

            if (v.Z == 0)
            {
                zEntry = float.MinValue;
                zExit = float.MaxValue;
            }
            else
            {
                zEntry = zInvEntry / v.Z;
                zExit = zInvExit / v.Z;
            }

            float entryTime = Math.Max(Math.Max(xEntry, yEntry), zEntry);
            float exitTime = Math.Min(Math.Min(xExit, yExit), zExit);

            if (entryTime > exitTime || xEntry < 0 && yEntry < 0 && zEntry < 0 || xEntry > 1 || yEntry > 1 || zEntry > 1)
            {
                normal = Vector3.Zero;
                return 1;
            }
            else
            {
                if (xEntry >= yEntry && xEntry >= zEntry)
                    normal = (xMaxDist < 0) ? Vector3.UnitX : -Vector3.UnitX;
                else if (yEntry >= xEntry && yEntry >= zEntry)
                    normal = (yMaxDist < 0) ? Vector3.UnitY : -Vector3.UnitY;
                else
                    normal = (zMaxDist < 0) ? Vector3.UnitZ : -Vector3.UnitZ;

                return entryTime;
            }
        }
    }
}
