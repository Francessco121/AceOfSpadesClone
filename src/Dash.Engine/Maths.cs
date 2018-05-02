using System;

/* Maths.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public static partial class Maths
    {
        public static Random Random;

        static Maths()
        {
            Random = new Random();
        }

        public static bool Maybe()
        {
            return Random.NextDouble() >= 0.5;
        }

        /// <summary>
        /// Just maybe...
        /// </summary>
        public static bool Maybe(this Random rnd)
        {
            return rnd.NextDouble() >= 0.5;
        }

        public static int NegativeRound(float value)
        {
            if (value >= 0) return (int)value;
            else return (int)(value - 1);
        }

        //public static FrustrumIntersectType AABBFrustumIntersects(BoundingBox box, Frustum fustrum)
        //{
        //    float m, n;
        //    FrustrumIntersectType result = FrustrumIntersectType.Inside;

        //    for (int i = 0; i < fustrum.Planes.Length; i++)
        //    {
        //        Plane plane = fustrum.Planes[i];
        //        m = (box.Center.X * plane.A) + (box.Center.Y * plane.B) + (box.Center.Z * plane.C) + plane.D;
        //        n = (box.Extent.X * Math.Abs(plane.A)) + (box.Extent.Y * Math.Abs(plane.B)) + (box.Extent.Z * Math.Abs(plane.C));

        //        if (m + n < 0) return FrustrumIntersectType.Outside;
        //        if (m - n < 0) result = FrustrumIntersectType.Intersect;
        //    }

        //    return result;
        //}

        public static float RandomRange(float min, float max)
        {
            float dif = max - min;
            double rand = Random.NextDouble();
            return (float)(min + (dif * rand));
        }

        public static int RandomSign(float negChance)
        {
            return Random.NextDouble() <= negChance ? -1 : 1;
        }

        public static double ReScale(double x, double currentMin, double currentMax, double newMin, double newMax)
        {
            /*  Where:
                x = x
                a = currentMin
                b = currentMax
                c = newMin
                d = newMax

                scaled = ( ((d - c) * (x - a)) / (b - a) ) + c;
            */

            return (((newMax - newMin) * (x - currentMin)) / (currentMax - currentMin)) + newMin;
        }

        public static float ReScale(float x, float currentMin, float currentMax, float newMin, float newMax)
        {
            return (((newMax - newMin) * (x - currentMin)) / (currentMax - currentMin)) + newMin;
        }

        public static double Mix(double a, double b, double p)
        {
            return (p * a) + ((1 - p) * b);
        }

        public static float Mix(float a, float b, float p)
        {
            return (p * a) + ((1 - p) * b);
        }

        public static CubeSide ReverseCubeSide(CubeSide side)
        {
            int i = (int)side;
            if (i % 2 == 0)
                return (CubeSide)(i + 1);
            else
                return (CubeSide)(i - 1);
        }
    }
}
