using System;
using System.Runtime.InteropServices;

namespace Dash.Engine
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2i : IEquatable<Vector2i>
    {
        public int X, Y;

        #region Static Constructors
        public static Vector2i Identity
        {
            get { return new Vector2i(1, 1); }
        }

        public static Vector2i Zero
        {
            get { return new Vector2i(0, 0); }
        }

        public static Vector2i UnitX
        {
            get { return new Vector2i(1, 0); }
        }

        public static Vector2i UnitY
        {
            get { return new Vector2i(0, 1); }
        }
        #endregion

        #region Operators
        public static Vector2i operator +(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2i operator +(Vector2i v, int s)
        {
            return new Vector2i(v.X + s, v.Y + s);
        }

        public static Vector2i operator +(int s, Vector2i v)
        {
            return new Vector2i(v.X + s, v.Y + s);
        }

        public static Vector2i operator -(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2i operator -(Vector2i v, int s)
        {
            return new Vector2i(v.X - s, v.Y - s);
        }

        public static Vector2i operator -(int s, Vector2i v)
        {
            return new Vector2i(s - v.X, s - v.Y);
        }

        public static Vector2i operator -(Vector2i v)
        {
            return new Vector2i(-v.X, -v.Y);
        }

        public static Vector2i operator *(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static Vector2i operator *(int s, Vector2i v)
        {
            return new Vector2i(v.X * s, v.Y * s);
        }

        public static Vector2i operator *(Vector2i v, int s)
        {
            return new Vector2i(v.X * s, v.Y * s);
        }

        public static Vector2i operator /(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(v1.X / v2.X, v1.Y / v2.Y);
        }

        public static Vector2i operator /(int s, Vector2i v)
        {
            return new Vector2i(s / v.X, s / v.Y);
        }

        public static Vector2i operator /(Vector2i v, int s)
        {
            return new Vector2i(v.X / s, v.Y / s);
        }

        public static bool operator ==(Vector2i v1, Vector2i v2)
        {
            return (v1.X == v2.X && v1.Y == v2.Y);
        }

        public static bool operator !=(Vector2i v1, Vector2i v2)
        {
            return (v1.X != v2.X || v1.Y != v2.Y);
        }
        #endregion

        #region Constructors
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        public Vector2i(int x, int y)
        {
            this.X = x; this.Y = y;
        }

        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        public Vector2i(double x, double y)
        {
            this.X = (int)x; this.Y = (int)y;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Vector2i)) return false;

            return this.Equals((Vector2i)obj);
        }

        public bool Equals(Vector2i other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "{" + X + ", " + Y + "}";
        }

        /// <summary>
        /// Parses a JSON stream and produces a Vector2i struct.
        /// </summary>
        public static Vector2i Parse(string text)
        {
            string[] split = text.Trim(new char[] { '{', '}' }).Split(',');
            if (split.Length != 2) return Vector2i.Zero;

            return new Vector2i(int.Parse(split[0]), int.Parse(split[1]));
        }

        public int this[int a]
        {
            get { return (a == 0) ? X : Y; }
            set { if (a == 0) X = value; else Y = value; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Get the length of the Vector2i structure.
        /// </summary>
        public int Length
        {
            get { return (int)Math.Sqrt(X * X + Y * Y); }
        }

        /// <summary>
        /// Get the squared length of the Vector2i structure.
        /// </summary>
        public int SquaredLength
        {
            get { return X * X + Y * Y; }
        }

        /// <summary>
        /// Gets the perpendicular vector on the right side of this vector.
        /// </summary>
        public Vector2i PerpendicularRight
        {
            get { return new Vector2i(Y, -X); }
        }

        /// <summary>
        /// Gets the perpendicular vector on the left side of this vector.
        /// </summary>
        public Vector2i PerpendicularLeft
        {
            get { return new Vector2i(-Y, X); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts a Vector2i to an int array.  Useful for vector commands in GL.
        /// </summary>
        /// <returns>int array representation of a Vector2i</returns>
        public int[] ToInt()
        {
            return new int[] { X, Y };
        }

        /// <summary>
        /// Performs the Vector2i scalar dot product.
        /// </summary>
        /// <param name="v1">The left Vector2i.</param>
        /// <param name="v2">The right Vector2i.</param>
        /// <returns>v1.x * v2.x + v1.y * v2.y</returns>
        public static int Dot(Vector2i v1, Vector2i v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        /// <summary>
        /// Performs the Vector2i scalar dot product.
        /// </summary>
        /// <param name="v">Second dot product term</param>
        /// <returns>Vector2i.Dot(this, v)</returns>
        public int Dot(Vector2i v)
        {
            return this.Dot(v);
        }

        /// <summary>
        /// Normalizes the Vector2i structure to have a peak value of one.
        /// </summary>
        /// <returns>if (Length = 0) return Zero; else return Vector2i(x,y)/Length</returns>
        public Vector2i Normalize()
        {
            if (Length == 0) return Zero;
            else return new Vector2i(X, Y) / Length;
        }

        /// <summary>
        /// Checks to see if any value (x, y, z) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.
        /// </summary>
        /// <returns>A truncated Vector2i</returns>
        public Vector2i Truncate()
        {
            int _x = (Math.Abs(X) - 0.0001 < 0) ? 0 : X;
            int _y = (Math.Abs(Y) - 0.0001 < 0) ? 0 : Y;
            return new Vector2i(_x, _y);
        }

        /// <summary>
        /// Store the minimum values of x, and y between the two vectors.
        /// </summary>
        /// <param name="v">Vector to check against</param>
        public void TakeMin(Vector2i v)
        {
            if (v.X < X) X = v.X;
            if (v.Y < Y) Y = v.Y;
        }

        /// <summary>
        /// Store the maximum values of x, and y between the two vectors.
        /// </summary>
        /// <param name="v">Vector to check against</param>
        public void TakeMax(Vector2i v)
        {
            if (v.X > X) X = v.X;
            if (v.Y > Y) Y = v.Y;
        }

        /// <summary>
        /// Linear interpolates between two vectors to get a new vector.
        /// </summary>
        /// <param name="v1">Initial vector (amount = 0).</param>
        /// <param name="v2">Final vector (amount = 1).</param>
        /// <param name="amount">Amount of each vector to consider (0->1).</param>
        /// <returns>A linear interpolated Vector3.</returns>
        public static Vector2i Lerp(Vector2i v1, Vector2i v2, int amount)
        {
            return v1 + (v2 - v1) * amount;
        }

        /// <summary>
        /// Swaps two Vector2i structures by passing via reference.
        /// </summary>
        /// <param name="v1">The first Vector2i structure.</param>
        /// <param name="v2">The second Vector2i structure.</param>
        public static void Swap(ref Vector2i v1, ref Vector2i v2)
        {
            Vector2i t = v1;
            v1 = v2;
            v2 = t;
        }
        #endregion
    }
}
