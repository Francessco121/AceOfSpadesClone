using Dash.Engine.Graphics;
using System;
using System.Collections.Generic;

/* Maths.Vector.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public enum VectorFlattenMethod { XY, XZ, YZ }

    public static partial class Maths
    {
        public static Vector3 Abs(Vector3 vec)
        {
            return new Vector3(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));
        }

        public static Vector2 Abs(Vector2 vec)
        {
            return new Vector2(Math.Abs(vec.X), Math.Abs(vec.Y));
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            return (a - b).Length;
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            return (a - b).Length;
        }

        public static float DistanceSquared(Vector3 a, Vector3 b)
        {
            return (a - b).LengthSquared;
        }

        public static Vector3 MidPoint(Vector3 a, Vector3 b)
        {
            return new Vector3(
                (a.X + b.X) / 2f,
                (a.Y + b.Y) / 2f,
                (a.Z + b.Z) / 2f);
        }

        public static Vector2 MidPoint(Vector2 a, Vector2 b)
        {
            return new Vector2(
                (a.X + b.X) / 2f,
                (a.Y + b.Y) / 2f);
        }

        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            return new Vector3(
                Math.Max(a.X, b.X),
                Math.Max(a.Y, b.Y),
                Math.Max(a.Z, b.Z));
        }

        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(
                Math.Max(a.X, b.X),
                Math.Max(a.Y, b.Y));
        }

        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            return new Vector3(
                Math.Min(a.X, b.X),
                Math.Min(a.Y, b.Y),
                Math.Min(a.Z, b.Z));
        }

        public static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(
                Math.Min(a.X, b.X),
                Math.Min(a.Y, b.Y));
        }

        public static Vector2 AngleToVector2(float radians)
        {
            return new Vector2(
                (float)Math.Sin(radians),
                (float)Math.Cos(radians));
        }

        public static float VectorToAngle(float x, float y)
        {
            return (float)Math.Atan2(x, y);
        }

        public static float ToAngle(this Vector2 vec)
        {
            return VectorToAngle(vec.X, vec.Y);
        }

        // http://math.stackexchange.com/questions/13261/how-to-get-a-reflection-vector
        public static Vector2 Reflect(this Vector2 vec, Vector2 normal)
        {
            float dot = Vector2.Dot(vec, normal) * 2;
            return vec - (dot * normal);
        }

        public static Vector3 CubeSideToSurfaceNormal(CubeSide side)
        {
            if (side == CubeSide.Left) return -Vector3.UnitX;
            if (side == CubeSide.Right) return Vector3.UnitX;
            if (side == CubeSide.Top) return Vector3.UnitY;
            if (side == CubeSide.Bottom) return -Vector3.UnitY;
            if (side == CubeSide.Back) return Vector3.UnitZ;
            if (side == CubeSide.Front) return -Vector3.UnitZ;

            return Vector3.Zero;
        }

        public static Vector2 Flatten(Vector3 vec3, VectorFlattenMethod method)
        {
            if (method == VectorFlattenMethod.XY)
                return new Vector2(vec3.X, vec3.Y);
            else if (method == VectorFlattenMethod.XZ)
                return new Vector2(vec3.X, vec3.Z);
            else if (method == VectorFlattenMethod.YZ)
                return new Vector2(vec3.Y, vec3.Z);
            else
                return new Vector2(vec3.X, vec3.Y);
        }

        public static bool PointInPolygon(List<Vector2> polygon, Vector2 point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        #region Distance To Line
        // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_two_points
        public static double DistanceToLine(Vector2 point, Vector2 start, Vector2 end)
        {
            /* Where:
             * P1 = (x1, y1) = start
             * P2 = (x2, y2) = end
             * (x0, y0) = point
            */

            double numerator = Math.Abs(
                  (double)(end.Y - start.Y) * point.X
                - (double)(end.X - start.X) * point.Y
                + (double)(end.X * start.Y)
                - (double)(end.Y * start.X));

            double denominator = Math.Sqrt(
                  Math.Pow(end.Y - start.Y, 2)
                + Math.Pow(end.X - start.X, 2));

            return numerator / denominator;
        }

        public static double DistanceToLine(Vector3 point, Vector3 start, Vector3 end)
        {
            /* Where:
             * - = (x and z plane dist)
             * | = (y dist)
             * \ = pythag of - and |
             * . = line
             * + = point
             * 
             * +
             * |\
             * | \
             * |  \
             * ----.
            */

            double xz_dist = DistanceToLine(new Vector2(point.X, point.Z), new Vector2(start.X, start.Z), new Vector2(end.X, end.Y));
            double y_dist = Math.Abs(point.Y - ((start.Y + end.Y) / 2f));

            return Math.Sqrt(Math.Pow(xz_dist, 2) + Math.Pow(y_dist, 2));
        }
        #endregion

        // http://stackoverflow.com/questions/11907947/how-to-check-if-a-point-lies-on-a-line-between-2-other-points
        //public static bool PointBetweenSegment(Vector2 point, Vector2 start, Vector2 end)
        //{
        //    float dxl = end.X - start.X;
        //    float dyl = end.Y - start.Y;

        //    if (Math.Abs(dxl) >= Math.Abs(dyl))
        //    {
        //        return dxl > 0 ? start.X <= point.X && point.X <= end.X
        //            : end.X <= point.X && point.X <= start.X;
        //    }
        //    else
        //    {
        //        return dyl > 0 ? start.Y <= point.Y && point.Y <= end.Y
        //            : end.Y <= point.Y && point.Y <= start.Y;
        //    }
        //}

        public static bool PointBetweenSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 se = start - end;
            Vector2 pe = point - end;
            Vector2 es = end - start;
            Vector2 ps = point - start;

            float seDotpe = Vector2.Dot(se, pe);
            float esDotps = Vector2.Dot(es, ps);

            return seDotpe >= 0 && esDotps >= 0;
        }

        public static float BarryCentric(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 pos)
        {
            float det = (p2.Z - p3.Z) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Z - p3.Z);
            float l1 = ((p2.Z - p3.Z) * (pos.X - p3.X) + (p3.X - p2.X) * (pos.Y - p3.Z)) / det;
            float l2 = ((p3.Z - p1.Z) * (pos.X - p3.X) + (p1.X - p3.X) * (pos.Y - p3.Z)) / det;
            float l3 = 1.0f - l1 - l2;
            return l1 * p1.Y + l2 * p2.Y + l3 * p3.Y;
        }

        static bool GetIntersection(float fDst1, float fDst2, Line3D line, out Vector3 hit)
        {
            hit = Vector3.Zero;
            if ((fDst1 * fDst2) >= 0.0f) return false;
            if (fDst1 == fDst2) return false;
            hit = line.Start + (line.End - line.Start) * (-fDst1 / (fDst2 - fDst1));
            return true;
        }
    }
}
