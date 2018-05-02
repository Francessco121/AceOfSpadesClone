using System;
using System.Runtime.InteropServices;

namespace Dash.Engine
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4i : IEquatable<Vector4i>
    {
        public int X, Y, Z, W;

        #region Static Constructors
        public static Vector4i Identity
        {
            get { return new Vector4i(1, 1, 1, 1); }
        }

        public static Vector4i Zero
        {
            get { return new Vector4i(0, 0, 0, 0); }
        }

        public static Vector4i UnitX
        {
            get { return new Vector4i(1, 0, 0, 0); }
        }

        public static Vector4i UnitY
        {
            get { return new Vector4i(0, 1, 0, 0); }
        }

        public static Vector4i UnitZ
        {
            get { return new Vector4i(0, 0, 1, 0); }
        }

        public static Vector4i UnitW
        {
            get { return new Vector4i(0, 0, 0, 1); }
        }
        #endregion

        #region Operators
        public static Vector4i operator +(Vector4i v1, Vector4i v2)
        {
            return new Vector4i(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
        }

        public static Vector4i operator +(Vector4i v, int s)
        {
            return new Vector4i(v.X + s, v.Y + s, v.Z + s, v.W + s);
        }

        public static Vector4i operator +(int s, Vector4i v)
        {
            return new Vector4i(v.X + s, v.Y + s, v.Z + s, v.W + s);
        }

        public static Vector4i operator -(Vector4i v1, Vector4i v2)
        {
            return new Vector4i(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);
        }

        public static Vector4i operator -(Vector4i v, int s)
        {
            return new Vector4i(v.X - s, v.Y - s, v.Z - s, v.W - s);
        }

        public static Vector4i operator -(int s, Vector4i v)
        {
            return new Vector4i(s - v.X, s - v.Y, s - v.Z, s - v.W);
        }

        public static Vector4i operator -(Vector4i v)
        {
            return new Vector4i(-v.X, -v.Y, -v.Z, -v.W);
        }

        public static Vector4i operator *(Vector4i v1, Vector4i v2)
        {
            return new Vector4i(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W);
        }

        public static Vector4i operator *(int s, Vector4i v)
        {
            return new Vector4i(v.X * s, v.Y * s, v.Z * s, v.W * s);
        }

        public static Vector4i operator *(Vector4i v, int s)
        {
            return new Vector4i(v.X * s, v.Y * s, v.Z * s, v.W * s);
        }

        public static Vector4i operator /(Vector4i v1, Vector4i v2)
        {
            return new Vector4i(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z, v1.W / v2.W);
        }

        public static Vector4i operator /(int s, Vector4i v)
        {
            return new Vector4i(s / v.X, s / v.Y, s / v.Z, s / v.W);
        }

        public static Vector4i operator /(Vector4i v, int s)
        {
            return new Vector4i(v.X / s, v.Y / s, v.Z / s, v.W / s);
        }

        public static bool operator ==(Vector4i v1, Vector4i v2)
        {
            return (v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z && v1.W == v2.W);
        }

        public static bool operator !=(Vector4i v1, Vector4i v2)
        {
            return (v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z || v1.W != v2.W);
        }
        #endregion

        #region Constructors
        /// <summary>Create a Vector4i structure.</summary>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        /// <param name="z">z value</param>
        /// <param name="w">w value</param>
        public Vector4i(int x, int y, int z, int w)
        {
            this.X = x; this.Y = y; this.Z = z; this.W = w;
        }

        /// <summary>Creates a Vector4i structure.  Casted to ints for OpenGL.</summary>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        /// <param name="z">z value</param>
        /// <param name="w">w value</param>
        public Vector4i(double x, double y, double z, double w)
        {
            this.X = (int)x; this.Y = (int)y; this.Z = (int)z; this.W = (int)w;
        }

        /// <summary>Creates a Vector4i structure based on a Vector3 and w.</summary>
        /// <param name="v">Vector3 to make up x,y,z</param>
        /// <param name="w">Double to make up the w component</param>
        public Vector4i(Vector3 v, int w)
        {
            X = (int)v.X; Y = (int)v.Y; Z = (int)v.Z; this.W = w;
        }

        /// <summary>Creates a Vector4i structure based on a Vector3i and w.</summary>
        /// <param name="v">Vector3 to make up x,y,z</param>
        /// <param name="w">Double to make up the w component</param>
        public Vector4i(Vector3i v, int w)
        {
            X = v.X; Y = v.Y; Z = v.Z; this.W = w;
        }

        /// <summary>
        /// Accepts a 24 bit color value and assumes an alpha of 1.0f.
        /// </summary>
        /// <param name="RGBByte">24bit color value</param>
        public Vector4i(byte[] RGBByte)
        {
            if (RGBByte.Length < 3) throw new Exception("Color data was not 24bit as expected.");
            X = (int)(RGBByte[0] / 256.0); Y = (int)(RGBByte[1] / 256.0); Z = (int)(RGBByte[2] / 256.0); W = 1;
        }

        /// <summary>Creates a Vector4i tructure from a int array (assuming the int array is of length 4).</summary>
        /// <param name="vector">The int array to convert to a Vector4i.</param>
        public Vector4i(int[] vector)
        {
            if (vector.Length != 4) throw new Exception(string.Format("int[] vector was of length {0}.  Was expecting a length of 4.", vector.Length));
            this.X = vector[0]; this.Y = vector[1]; this.Z = vector[2]; this.W = vector[3];
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Vector4i)) return false;

            return this.Equals((Vector4i)obj);
        }

        public bool Equals(Vector4i other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "{" + X + ", " + Y + ", " + Z + ", " + W + "}";
        }

        /// <summary>
        /// Parses a JSON stream and produces a Vector4i struct.
        /// </summary>
        public static Vector4i Parse(string text)
        {
            string[] split = text.Trim(new char[] { '{', '}' }).Split(',');
            if (split.Length != 4) return Vector4i.Zero;

            return new Vector4i(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]));
        }

        public int this[int a]
        {
            get { return (a == 0) ? X : (a == 1) ? Y : (a == 2) ? Z : W; }
            set
            {
                if (a == 0) X = value;
                else if (a == 1) Y = value;
                else if (a == 2) Z = value;
                else if (a == 3) W = value;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Get the length of the Vector4i structure.
        /// </summary>
        public int Length
        {
            get { return (int)Math.Sqrt(X * X + Y * Y + Z * Z + W * W); }
        }

        /// <summary>
        /// Get the squared length of the Vector4i structure.
        /// </summary>
        public int SquaredLength
        {
            get { return X * X + Y * Y + Z * Z + W * W; }
        }

        /// <summary>
        /// Gets or sets an OpenGl.Types.Vector2 with the x and y components of this instance.
        /// </summary>
        public Vector2i Xy { get { return new Vector2i(X, Y); } set { X = value.X; Y = value.Y; } }

        /// <summary>
        /// Gets or sets an OpenGl.Types.Vector3 with the x, y and z components of this instance.
        /// </summary>
        public Vector3i Xyz { get { return new Vector3i(X, Y, Z); } set { X = value.X; Y = value.Y; Z = value.Z; } }
        #endregion

        #region Methods
        /// <summary>Transform a Vector by the given Matrix</summary>
		/// <param name="vec">The vector to transform</param>
		/// <param name="mat">The desired transformation</param>
		/// <returns>The transformed vector</returns>
		public static Vector4i Transform(Vector4i vec, Matrix4 mat)
        {
            Vector4i result;
            Transform(ref vec, ref mat, out result);
            return result;
        }

        /// <summary>Transform a Vector by the given Matrix</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void Transform(ref Vector4i vec, ref Matrix4 mat, out Vector4i result)
        {
            result = new Vector4i(
                vec.X * mat.Row1.X + vec.Y * mat.Row2.X + vec.Z * mat.Row3.X + vec.W * mat.Row4.X,
                vec.X * mat.Row1.Y + vec.Y * mat.Row2.Y + vec.Z * mat.Row3.Y + vec.W * mat.Row4.Y,
                vec.X * mat.Row1.Z + vec.Y * mat.Row2.Z + vec.Z * mat.Row3.Z + vec.W * mat.Row4.Z,
                vec.X * mat.Row1.W + vec.Y * mat.Row2.W + vec.Z * mat.Row3.W + vec.W * mat.Row4.W);
        }

        /// <summary>
        /// Transforms a vector by a quaternion rotation.
        /// </summary>
        /// <param name="vec">The vector to transform.</param>
        /// <param name="quat">The quaternion to rotate the vector by.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector4i Transform(Vector4i vec, Quaternion quat)
        {
            Vector4i result;
            Transform(ref vec, ref quat, out result);
            return result;
        }

        /// <summary>
        /// Transforms a vector by a quaternion rotation.
        /// </summary>
        /// <param name="vec">The vector to transform.</param>
        /// <param name="quat">The quaternion to rotate the vector by.</param>
        /// <param name="result">The result of the operation.</param>
        public static void Transform(ref Vector4i vec, ref Quaternion quat, out Vector4i result)
        {
            Quaternion v = new Quaternion(vec.X, vec.Y, vec.Z, vec.W), i, t;
            i = quat.Inverse();
            t = quat * v;
            v = t * i;
            //Quaternion.Invert(ref quat, out i);
            //Quaternion.Multiply(ref quat, ref v, out t);
            //Quaternion.Multiply(ref t, ref i, out v);

            result = new Vector4i(v.X, v.Y, v.Z, v.W);
        }

        /// <summary>
        /// Vector4i scalar dot product.
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>Scalar dot product value</returns>
        public static int Dot(Vector4i v1, Vector4i v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W;
        }

        /// <summary>
        /// Vector4i scalar dot product.
        /// </summary>
        /// <param name="v">Second vector</param>
        /// <returns>Vector4i.Dot(this, v)</returns>
        public int Dot(Vector4i v)
        {
            return Vector4i.Dot(this, v);
        }

        /// <summary>
        /// Converts a Vector4i to a int array.  Useful for vector commands in GL.
        /// </summary>
        /// <returns>Float array representation of a Vector4i</returns>
        public int[] ToInt()
        {
            return new int[] { X, Y, Z, W };
        }

        /// <summary>
        /// Normalizes the Vector4i structure to have a peak value of one.
        /// </summary>
        /// <returns>if (Length = 0) return Zero; else return Vector4i(x,y,z,w)/Length</returns>
        public Vector4i Normalize()
        {
            if (Length == 0) return Zero;
            else return new Vector4i(X, Y, Z, W) / Length;
        }

        /// <summary>
        /// Checks to see if any value (x, y, z, w) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.
        /// </summary>
        /// <returns>A truncated Vector4i</returns>
        public Vector4i Truncate()
        {
            int _x = (Math.Abs(X) - 0.0001 < 0) ? 0 : X;
            int _y = (Math.Abs(Y) - 0.0001 < 0) ? 0 : Y;
            int _z = (Math.Abs(Z) - 0.0001 < 0) ? 0 : Z;
            int _w = (Math.Abs(W) - 0.0001 < 0) ? 0 : W;
            return new Vector4i(_x, _y, _z, _w);
        }

        /// <summary>
        /// Store the minimum values of x, y, z, and w between the two vectors.
        /// </summary>
        /// <param name="v">Vector to check against</param>
        public void TakeMin(Vector4i v)
        {
            if (v.X < X) X = v.X;
            if (v.Y < Y) Y = v.Y;
            if (v.Z < Z) Z = v.Z;
            if (v.W < W) W = v.W;
        }

        /// <summary>
        /// Store the maximum values of x, y, z, and w  between the two vectors.
        /// </summary>
        /// <param name="v">Vector to check against</param>
        public void TakeMax(Vector4i v)
        {
            if (v.X > X) X = v.X;
            if (v.Y > Y) Y = v.Y;
            if (v.Z > Z) Z = v.Z;
            if (v.W > W) W = v.W;
        }

        /// <summary>
        /// Linear interpolates between two vectors to get a new vector.
        /// </summary>
        /// <param name="v1">Initial vector (amount = 0).</param>
        /// <param name="v2">Final vector (amount = 1).</param>
        /// <param name="amount">Amount of each vector to consider (0->1).</param>
        /// <returns>A linear interpolated Vector3.</returns>
        public static Vector4i Lerp(Vector4i v1, Vector4i v2, int amount)
        {
            return v1 + (v2 - v1) * amount;
        }

        /// <summary>
        /// Swaps two Vector4i structures by passing via reference.
        /// </summary>
        /// <param name="v1">The first Vector4i structure.</param>
        /// <param name="v2">The second Vector4i structure.</param>
        public static void Swap(ref Vector4i v1, ref Vector4i v2)
        {
            Vector4i t = v1;
            v1 = v2;
            v2 = t;
        }
        #endregion
    }
}
