using Dash.Engine.Graphics;
using System;
using System.Runtime.InteropServices;

namespace Dash.Engine
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3i : IEquatable<Vector3i>
    {
        public int X, Y, Z;

        #region Static Constructors
        public static Vector3i Identity
        {
            get { return new Vector3i(1.0f, 1.0f, 1.0f); }
        }

        public static Vector3i Zero
        {
            get { return new Vector3i(0.0f, 0.0f, 0.0f); }
        }

        public static Vector3i Up
        {
            get { return new Vector3i(0.0f, 1.0f, 0.0f); }
        }

        public static Vector3i Down
        {
            get { return new Vector3i(0.0f, -1.0f, 0.0f); }
        }

        public static Vector3i Forward
        {
            get { return new Vector3i(0.0f, 0.0f, -1.0f); }
        }

        public static Vector3i Backward
        {
            get { return new Vector3i(0.0f, 0.0f, 1.0f); }
        }

        public static Vector3i Left
        {
            get { return new Vector3i(-1.0f, 0.0f, 0.0f); }
        }

        public static Vector3i Right
        {
            get { return new Vector3i(1.0f, 0.0f, 0.0f); }
        }

        public static Vector3i UnitX
        {
            get { return new Vector3i(1.0f, 0.0f, 0.0f); }
        }

        public static Vector3i UnitY
        {
            get { return new Vector3i(0.0f, 1.0f, 0.0f); }
        }

        public static Vector3i UnitZ
        {
            get { return new Vector3i(0.0f, 0.0f, 1.0f); }
        }

        public static Vector3i UnitScale
        {
            get { return new Vector3i(1.0f, 1.0f, 1.0f); }
        }
        #endregion

        #region Operators
        public static Vector3i operator +(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3i operator +(Vector3i v, int s)
        {
            return new Vector3i(v.X + s, v.Y + s, v.Z + s);
        }

        public static Vector3i operator +(Vector3i v, float s)
        {
            return new Vector3i(v.X + s, v.Y + s, v.Z + s);
        }

        public static Vector3i operator +(int s, Vector3i v)
        {
            return new Vector3i(v.X + s, v.Y + s, v.Z + s);
        }

        public static Vector3i operator +(float s, Vector3i v)
        {
            return new Vector3i(v.X + s, v.Y + s, v.Z + s);
        }

        public static Vector3i operator -(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3i operator -(Vector3i v, int s)
        {
            return new Vector3i(v.X - s, v.Y - s, v.Z - s);
        }

        public static Vector3i operator -(Vector3i v, float s)
        {
            return new Vector3i(v.X - s, v.Y - s, v.Z - s);
        }

        public static Vector3i operator -(int s, Vector3i v)
        {
            return new Vector3i(s - v.X, s - v.Y, s - v.Z);
        }

        public static Vector3i operator -(float s, Vector3i v)
        {
            return new Vector3i(s - v.X, s - v.Y, s - v.Z);
        }

        public static Vector3i operator -(Vector3i v)
        {
            return new Vector3i(-v.X, -v.Y, -v.Z);
        }

        public static Vector3i operator *(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static Vector3i operator *(int s, Vector3i v)
        {
            return new Vector3i(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vector3i operator *(float s, Vector3i v)
        {
            return new Vector3i(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vector3i operator *(Vector3i v, int s)
        {
            return new Vector3i(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vector3i operator *(Vector3i v, float s)
        {
            return new Vector3i(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vector3i operator /(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        public static Vector3i operator /(int s, Vector3i v)
        {
            return new Vector3i(s / v.X, s / v.Y, s / v.Z);
        }

        public static Vector3i operator /(float s, Vector3i v)
        {
            return new Vector3i(s / v.X, s / v.Y, s / v.Z);
        }

        public static Vector3i operator /(Vector3i v, int s)
        {
            return new Vector3i(v.X / s, v.Y / s, v.Z / s);
        }

        public static Vector3i operator /(Vector3i v, float s)
        {
            return new Vector3i(v.X / s, v.Y / s, v.Z / s);
        }

        public static bool operator ==(Vector3i v1, Vector3i v2)
        {
            return (v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z);
        }

        public static bool operator !=(Vector3i v1, Vector3i v2)
        {
            return (v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z);
        }
        #endregion

        #region Constructors
        /// <summary>Create a Vector3i structure, normally used to store Vertex positions.</summary>
        /// <param name="xyz">xyz values</param>
        public Vector3i(int xyz)
        {
            this.X = xyz; this.Y = xyz; this.Z = xyz;
        }

        /// <summary>Create a Vector3i structure, normally used to store Vertex positions.</summary>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        /// <param name="z">z value</param>
        public Vector3i(int x, int y, int z)
        {
            this.X = x; this.Y = y; this.Z = z;
        }

        /// <summary>Creates a Vector3i structure, normally used to store Vertex positions.  Casted to floats for OpenGL.</summary>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        /// <param name="z">z value</param>
        public Vector3i(double x, double y, double z)
        {
            this.X = (int)x; this.Y = (int)y; this.Z = (int)z;
        }

        public Vector3i(Vector4 vec4)
        {
            X = (int)vec4.X;
            Y = (int)vec4.Y;
            Z = (int)vec4.Z;
        }

        /// <summary>Creates a Vector3i tructure from a float array (assuming the float array is of length 3).</summary>
        /// <param name="vector">The float array to convert to a Vector3i.</param>
        public Vector3i(int[] vector)
        {
            if (vector.Length != 3) throw new Exception(string.Format("int[] vector was of length {0}.  Was expecting a length of 3.", vector.Length));
            this.X = vector[0]; this.Y = vector[1]; this.Z = vector[2];
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3i)) return false;

            return this.Equals((Vector3i)obj);
        }

        public bool Equals(Vector3i other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "{" + X + ", " + Y + ", " + Z + "}";
        }

        /// <summary>
        /// Parses a JSON stream and produces a Vector3i struct.
        /// </summary>
        public static Vector3i Parse(string text)
        {
            string[] split = text.Trim(new char[] { '{', '}' }).Split(',');
            if (split.Length != 3) return Vector3i.Zero;

            return new Vector3i(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
        }

        public int this[int a]
        {
            get { return (a == 0) ? X : (a == 1) ? Y : Z; }
        }
        #endregion

        #region Methods
        public bool IsNan()
        {
            return float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z);
        }

        public Color4 ToColor4()
        {
            return new Color4(X, Y, Z, 1);
        }

        public float InnerProduct(Vector3i other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        /// <summary>
        /// Converts a Vector3i to a float array.  Useful for vector commands in GL.
        /// </summary>
        /// <returns>Float array representation of a Vector3i</returns>
        public int[] ToInt()
        {
            return new int[] { X, Y, Z };
        }

        /// <summary>
        /// Get the length of the Vector3i structure.
        /// </summary>
        public float Length
        {
            get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        /// <summary>
        /// Performs the Vector3i scalar dot product.
        /// </summary>
        /// <param name="v1">The left Vector3i.</param>
        /// <param name="v2">The right Vector3i.</param>
        /// <returns>Scalar dot product value</returns>
        public static int Dot(Vector3i v1, Vector3i v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Performs the Vector3i scalar dot product.
        /// </summary>
        /// <param name="v">Second dot product term</param>
        /// <returns>Vector3i.Dot(this, v)</returns>
        public int Dot(Vector3i v)
        {
            return Vector3i.Dot(this, v);
        }

        /// <summary>
        /// Returns the squared length of the Vector3i structure.
        /// </summary>
        public float LengthSquared
        {
            get { return X * X + Y * Y + Z * Z; }
        }

        /// <summary>
        /// Vector3i cross product
        /// </summary>
        /// <param name="v1">Vector1</param>
        /// <param name="v2">Vector2</param>
        /// <returns>Vector3i cross product value</returns>
        public static Vector3i Cross(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
        }

        /// <summary>
        /// Vector3i cross product
        /// </summary>
        /// <param name="v">Second cross product term</param>
        /// <returns>this x v</returns>
        public Vector3i Cross(Vector3i v)
        {
            return Vector3i.Cross(this, v);
        }

        /// <summary>
        /// Normalizes the Vector3i structure to have a peak value of one.
        /// </summary>
        /// <returns>if (Length = 0) return Zero; else return Vector3i(x,y,z)/Length</returns>
        public Vector3i Normalize()
        {
            if (Length == 0) return Zero;
            else return new Vector3i(X, Y, Z) / Length;
        }

        /// <summary>
        /// Checks to see if any value (x, y, z) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.
        /// </summary>
        /// <returns>A truncated Vector3i</returns>
        public Vector3i Truncate()
        {
            float _x = (Math.Abs(X) - 0.0001 < 0) ? 0 : X;
            float _y = (Math.Abs(Y) - 0.0001 < 0) ? 0 : Y;
            float _z = (Math.Abs(Z) - 0.0001 < 0) ? 0 : Z;
            return new Vector3i(_x, _y, _z);
        }

        /// <summary>
        /// Checks to see if any value (x, y, z) are within 'bias' of 0.
        /// If so this method truncates that value to zero.
        /// </summary>
        /// <returns>A truncated Vector3i</returns>
        public Vector3i Truncate(double bias)
        {
            float _x = (Math.Abs(X) - bias < 0) ? 0 : X;
            float _y = (Math.Abs(Y) - bias < 0) ? 0 : Y;
            float _z = (Math.Abs(Z) - bias < 0) ? 0 : Z;
            return new Vector3i(_x, _y, _z);
        }

        /// <summary>
        /// Store the minimum values of x, y, and z between the two vectors.
        /// </summary>
        /// <param name="v">Vector to check against</param>
        public void TakeMin(Vector3i v)
        {
            if (v.X < X) X = v.X;
            if (v.Y < Y) Y = v.Y;
            if (v.Z < Z) Z = v.Z;
        }

        /// <summary>
        /// Store the maximum values of x, y, and z between the two vectors.
        /// </summary>
        /// <param name="v">Vector to check against</param>
        public void TakeMax(Vector3i v)
        {
            if (v.X > X) X = v.X;
            if (v.Y > Y) Y = v.Y;
            if (v.Z > Z) Z = v.Z;
        }

        /// <summary>
        /// Returns the maximum component of the Vector3i.
        /// </summary>
        /// <returns>The maximum component of the Vector3i</returns>
        public float Max()
        {
            return (X >= Y && X >= Z) ? X : (Y >= Z) ? Y : Z;
        }

        /// <summary>
        /// Returns the minimum component of the Vector3i.
        /// </summary>
        /// <returns>The minimum component of the Vector3i</returns>
        public float Min()
        {
            return (X <= Y && X <= Z) ? X : (Y <= Z) ? Y : Z;
        }

        /// <summary>
        /// Linear interpolates between two vectors to get a new vector.
        /// </summary>
        /// <param name="v1">Initial vector (amount = 0).</param>
        /// <param name="v2">Final vector (amount = 1).</param>
        /// <param name="amount">Amount of each vector to consider (0->1).</param>
        /// <returns>A linear interpolated Vector3i.</returns>
        public static Vector3i Lerp(Vector3i v1, Vector3i v2, float amount)
        {
            return v1 + (v2 - v1) * amount;
        }

        /// <summary>
        /// Calculates the angle (in radians) between two vectors.
        /// </summary>
        /// <param name="first">The first vector.</param>
        /// <param name="second">The second vector.</param>
        /// <returns>Angle (in radians) between the vectors.</returns>
        /// <remarks>Note that the returned angle is never bigger than the constant Pi.</remarks>
        public static float CalculateAngle(Vector3i first, Vector3i second)
        {
            return (float)Math.Acos((Vector3i.Dot(first, second)) / (first.Length * second.Length));
        }

        /// <summary>
        /// Calculates the angle (in radians) between two vectors.
        /// </summary>
        /// <param name="v">The second vector.</param>
        /// <returns>Angle (in radians) between the vectors.</returns>
        /// <remarks>Note that the returned angle is never bigger than the constant Pi.</remarks>
        public float CalculateAngle(Vector3i v)
        {
            return Vector3i.CalculateAngle(this, v);
        }

        /// <summary>
        /// Swaps two Vector3i structures by passing via reference.
        /// </summary>
        /// <param name="v1">The first Vector3i structure.</param>
        /// <param name="v2">The second Vector3i structure.</param>
        public static void Swap(ref Vector3i v1, ref Vector3i v2)
        {
            Vector3i t = v1;
            v1 = v2;
            v2 = t;
        }
        #endregion
    }
}
