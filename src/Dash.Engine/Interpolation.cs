using System;

namespace Dash.Engine
{
    public static class Interpolation
    {
        public static double Linear(double a, double b, double t)
        {
            return a + t * (b - a);
        }

        public static float Linear(float a, float b, float t)
        {
            if (a == b)
                return b;

            return a + t * (b - a);
        }

        public static float Sine(float a, float b, float t)
        {
            float t2 = (1 - (float)Math.Sin(t * MathHelper.Pi));
            return a * (1 - t2) + b * t2;
        }

        public static float CubicBezier(float p0, float p1, float p2, float p3, float t)
        {
            float inverseT = 1 - t;

            double p0f = Math.Pow(inverseT, 3) * p0;
            double p1f = 3 * t * Math.Pow(inverseT, 2) * p1;
            double p2f = 3 * Math.Pow(t, 2) * inverseT * p2;
            double p3f = Math.Pow(t, 3) * p3;

            return (float)(p0f + p1f + p2f + p3f);
        }

        public static double InverseLerp(double a, double b, double l)
        {
            // l = a + f * (b - a)
            // l - a = f * (b - a)
            // (l - a) / (b - a) = f
            return (l - a) / (b - a);
        }

        public static float InverseLerp(float a, float b, float l)
        {
            // l = a + f * (b - a)
            // l - a = f * (b - a)
            // (l - a) / (b - a) = f
            if (b - a == 0)
                return 0;
            else
                return (l - a) / (b - a);
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, float f)
        {
            float x = Linear(a.X, b.X, f);
            float y = Linear(a.Y, b.Y, f);
            float z = Linear(a.Z, b.Z, f);

            return new Vector3(x, y, z);
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 l)
        {
            float xl = InverseLerp(a.X, b.X, l.X);
            float yl = InverseLerp(a.Y, b.Y, l.Y);
            float zl = InverseLerp(a.Z, b.Z, l.Z);
            return (xl + yl + zl) / 3f;
        }

        public static Vector3 LerpDegrees(Vector3 start, Vector3 end, float i)
        {
            return new Vector3(
                LerpDegrees(start.X, end.X, i),
                LerpDegrees(start.Y, end.Y, i),
                LerpDegrees(start.Z, end.Z, i));
        }

        // http://stackoverflow.com/questions/2708476/rotation-interpolation
        public static float LerpDegrees(float start, float end, float amount)
        {
            if (start == end) return end;

            float difference = Math.Abs(end - start);
            if (difference > 180)
            {
                // We need to add on to one of the values.
                if (end > start)
                {
                    // We'll add it on to start...
                    start += 360;
                }
                else
                {
                    // Add it on to end.
                    end += 360;
                }
            }

            // Interpolate it.
            float value = (start + ((end - start) * amount));

            // Wrap it..
            float rangeZero = 360;

            if (value >= 0 && value <= 360)
                return value;

            return (value % rangeZero);
        }

        public static float LerpRadians(float start, float end, float amount)
        {
            float difference = Math.Abs(end - start);
            if (difference > MathHelper.Pi)
            {
                // We need to add on to one of the values.
                if (end > start)
                {
                    // We'll add it on to start...
                    start += MathHelper.TwoPi;
                }
                else
                {
                    // Add it on to end.
                    end += MathHelper.TwoPi;
                }
            }

            // Interpolate it.
            float value = (start + ((end - start) * amount));

            // Wrap it..
            float rangeZero = MathHelper.TwoPi;

            if (value >= 0 && value <= MathHelper.TwoPi)
                return value;

            return (value % rangeZero);
        }
    }
}
