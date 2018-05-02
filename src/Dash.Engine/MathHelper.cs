using System;

/* Input.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public static class MathHelper
    {
        #region Constants
        /// <summary>
        /// Defines the value of tau divided by two as a <see cref="System.Single"/>.
        /// </summary>
        public const float Pi = 3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117067982148086513282306647093844609550582231725359408128481117450284102701938521105559644622948954930382f;

        /// <summary>
        /// Defines the value of Pi divided by two as a <see cref="System.Single"/>.
        /// </summary>
        public const float PiOver2 = Pi / 2;

        /// <summary>
        /// Defines the value of Pi divided by three as a <see cref="System.Single"/>.
        /// </summary>
        public const float PiOver3 = Pi / 3;

        /// <summary>
        /// Definesthe value of  Pi divided by four as a <see cref="System.Single"/>.
        /// </summary>
        public const float PiOver4 = Pi / 4;

        /// <summary>
        /// Defines the value of Pi divided by six as a <see cref="System.Single"/>.
        /// </summary>
        public const float PiOver6 = Pi / 6;

        /// <summary>
        /// Defines the value of two times pi as a <see cref="System.Single"/>.
        /// </summary>
        public const float TwoPi = 2 * Pi;

        /// <summary>
        /// Defines the value of tau as a <see cref="System.Single"/>.
        /// </summary>
        public const float Tau = 2 * Pi;

        /// <summary>
        /// Defines the value of tau divided by two as a <see cref="System.Single"/>.
        /// </summary>
        public const float TauOver2 = Pi;

        /// <summary>
        /// Defines the value of tau divided by four as a <see cref="System.Single"/>.
        /// </summary>
        public const float TauOver4 = Tau / 4f;

        /// <summary>
        /// Defines the value of tau divided by six as a <see cref="System.Single"/>.
        /// </summary>
        public const float TauOver6 = Tau / 6f;

        /// <summary>
        /// Defines the value of tau divided by eight as a <see cref="System.Single"/>.
        /// </summary>
        public const float TauOver8 = Tau / 8f;

        /// <summary>
        /// Defines the value of tau divided by twelve as a <see cref="System.Single"/>.
        /// </summary>
        public const float TauOver12 = Tau / 12f;

        /// <summary>
        /// Defines the value of Pi multiplied by 3 and divided by two as a <see cref="System.Single"/>.
        /// </summary>
        public const float ThreePiOver2 = 3 * Pi / 2;

        /// <summary>
        /// Defines the value of E as a <see cref="System.Single"/>.
        /// </summary>
        public const float E = 2.71828182845904523536f;

        /// <summary>
        /// Defines the base-10 logarithm of E.
        /// </summary>
        public const float Log10E = 0.434294482f;

        /// <summary>
        /// Defines the base-2 logarithm of E.
        /// </summary>
        public const float Log2E = 1.442695041f;
        #endregion

        #region Clamp
        public static int ClampToByteRange(int value)
        {
            return Clamp(value, byte.MinValue, byte.MaxValue);
        }

        public static byte Clamp(byte value, byte min, byte max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static short Clamp(short value, short min, short max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
        public static ushort Clamp(ushort value, ushort min, ushort max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
        public static uint Clamp(uint value, uint min, uint max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static long Clamp(long value, long min, long max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
        public static ulong Clamp(ulong value, ulong min, ulong max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
        #endregion

        /// <summary>
        /// Returns the next power of two that is larger than the specified number.
        /// </summary>
        /// <param name="n">The specified number.</param>
        /// <returns>The next power of two.</returns>
        public static long NextPowerOfTwo(long n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException("n", "Must be positive.");
            }
            return (long)Math.Pow(2, Math.Ceiling(Math.Log((double)n, 2)));
        }

        /// <summary>
        /// Returns the next power of two that is larger than the specified number.
        /// </summary>
        /// <param name="n">The specified number.</param>
        /// <returns>The next power of two.</returns>
        public static int NextPowerOfTwo(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException("n", "Must be positive.");
            }
            return (int)Math.Pow(2, Math.Ceiling(Math.Log((double)n, 2)));
        }

        /// <summary>
        /// Returns the next power of two that is larger than the specified number.
        /// </summary>
        /// <param name="n">The specified number.</param>
        /// <returns>The next power of two.</returns>
        public static float NextPowerOfTwo(float n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException("n", "Must be positive.");
            }
            return (float)Math.Pow(2, Math.Ceiling(Math.Log((double)n, 2)));
        }

        /// <summary>
        /// Returns the next power of two that is larger than the specified number.
        /// </summary>
        /// <param name="n">The specified number.</param>
        /// <returns>The next power of two.</returns>
        public static double NextPowerOfTwo(double n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException("n", "Must be positive.");
            }
            return Math.Pow(2, Math.Ceiling(Math.Log((double)n, 2)));
        }

        /// <summary>Calculates the factorial of a given natural number.
        /// </summary>
        /// <param name="n">The number.</param>
        /// <returns>n!</returns>
        public static long Factorial(int n)
        {
            long result = 1;

            for (; n > 1; n--)
            {
                result *= n;
            }

            return result;
        }

        /// <summary>
        /// Calculates the binomial coefficient <paramref name="n"/> above <paramref name="k"/>.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <param name="k">The k.</param>
        /// <returns>n! / (k! * (n - k)!)</returns>
        public static long BinomialCoefficient(int n, int k)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees">An angle in degrees</param>
        /// <returns>The angle expressed in radians</returns>
        public static float ToRadians(float degrees)
        {
            const float degToRad = (float)Tau / 360.0f;
            return degrees * degToRad;
        }

        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        /// <param name="radians">An angle in radians</param>
        /// <returns>The angle expressed in degrees</returns>
        public static float ToDegrees(float radians)
        {
            const float radToDeg = 360.0f / (float)Tau;
            return radians * radToDeg;
        }

        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees">An angle in degrees</param>
        /// <returns>The angle expressed in radians</returns>
        public static double ToRadians(double degrees)
        {
            const double degToRad = Math.PI / 180.0;
            return degrees * degToRad;
        }

        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        /// <param name="radians">An angle in radians</param>
        /// <returns>The angle expressed in degrees</returns>
        public static double ToDegrees(double radians)
        {
            const double radToDeg = 180.0 / Math.PI;
            return radians * radToDeg;
        }
    }
}
