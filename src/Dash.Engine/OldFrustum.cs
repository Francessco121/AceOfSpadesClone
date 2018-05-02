using Dash.Engine.Graphics;
using System;

/* Frustum.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public enum FrustumIntersectionType
    {
        Outside, Inside, Intersect
    }

    public class OldFrustum
    {
        public struct Plane
        {
            public Vector3 Normal;
            public float Distance;

            public Plane(Vector3 n, Vector3 p)
            {
                Normal = n.Normalize();
                Distance = -Normal.InnerProduct(p);
            }

            public float DistanceTo(Vector3 p)
            {
                return Distance + Normal.InnerProduct(p);
            }
        }

        public Plane[] Planes { get; }

        public Vector3 FTL { get; }
        public Vector3 FTR { get; }
        public Vector3 FBL { get; }
        public Vector3 FBR { get; }

        public Vector3 NTL { get; }
        public Vector3 NTR { get; }
        public Vector3 NBL { get; }
        public Vector3 NBR { get; }

        public Vector3 FarCenter { get; }
        public Vector3 NearCenter { get; }

        public Vector3 XVector { get; }
        public Vector3 YVector { get; }
        public Vector3 ZVector { get; }

        public OldFrustum(Camera camera, bool calculateCorners)
        {
            Vector3 p = camera.Position;
            Vector3 z = -camera.LookVector;
            Vector3 u = Vector3.UnitY;

            Vector3 x = Vector3.Cross(u, z);
            x = x.Normalize();
            Vector3 y = Vector3.Cross(z, x);
            y = y.Normalize();

            float nearDist = camera.NearPlane;
            float farDist = camera.FarPlane;

            float twoTanFOV = (float)Math.Tan(MathHelper.ToRadians(camera.FOV) / 2f);

            float hNear = twoTanFOV * nearDist;
            float wNear = hNear * camera.AspectRatio;

            float hFar = twoTanFOV * farDist;
            float wFar = hFar * camera.AspectRatio;

            Vector3 fc = p - z * farDist;
            Vector3 nc = p - z * nearDist;

            if (calculateCorners)
            {
                NTL = nc + y * hNear - x * wNear;
                NTR = nc + y * hNear + x * wNear;
                NBL = nc - y * hNear - x * wNear;
                NBR = nc - y * hNear + x * wNear;

                FTL = fc + y * hFar - x * wFar;
                FTR = fc + y * hFar + x * wFar;
                FBL = fc - y * hFar - x * wFar;
                FBR = fc - y * hFar + x * wFar;

                FarCenter = fc;
                NearCenter = nc;
                XVector = x;
                YVector = y;
                ZVector = z;
            }

            Vector3 aux;

            aux = (nc + y * hNear) - p;
            aux = aux.Normalize();
            Vector3 topNorm = Vector3.Cross(aux, x);

            aux = (nc - y * hNear) - p;
            aux = aux.Normalize();
            Vector3 botNorm = Vector3.Cross(x, aux);

            aux = (nc - x * wNear) - p;
            aux = aux.Normalize();
            Vector3 leftNorm = Vector3.Cross(aux, y);

            aux = (nc + x * wNear) - p;
            aux = aux.Normalize();
            Vector3 rightNorm = Vector3.Cross(y, aux);

            Planes = new Plane[]
           {
                new Plane(topNorm, nc + y * hNear),   // Top
                new Plane(botNorm, nc - y * hNear),   // Bottom
                new Plane(leftNorm, nc - x * wNear),   // Left
                new Plane(rightNorm, nc + x * wNear),   // Right
                new Plane(-z, nc),   // Near
                new Plane(z, fc)    // Far
           };
        }

        public FrustumIntersectionType ContainsPoint(Vector3 p)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Planes[i].DistanceTo(p) < 0)
                    return FrustumIntersectionType.Outside;
            }

            return FrustumIntersectionType.Inside;
        }

        public FrustumIntersectionType IntersectsAABB(AxisAlignedBoundingBox aabb)
        {
            FrustumIntersectionType result = FrustumIntersectionType.Inside;
            for (int i = 0; i < 6; i++)
            {
                int numIn = 0, numOut = 0;

                for (int k = 0; k < 8 && (numIn == 0 || numOut == 0); k++)
                {
                    if (Planes[i].DistanceTo(aabb[k]) < 0)
                        numOut++;
                    else
                        numIn++;
                }

                if (numIn == 0) return FrustumIntersectionType.Outside;
                else if (numOut > 0) result = FrustumIntersectionType.Intersect;
            }

            return result;
        }
    }
}
