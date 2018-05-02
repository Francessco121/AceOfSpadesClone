using Dash.Engine.Graphics;
using System;
using System.Runtime.InteropServices;

namespace Dash.Engine
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3 : IEquatable<Vector3>
    {
        public Vector2 Xy
        {
            get { return new Vector2(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Yz
        {
            get { return new Vector2(Y, Z); }
            set
            {
                Y = value.X;
                Z = value.Y;
            }
        }

        public Vector2 Xz
        {
            get { return new Vector2(X, Z); }
            set
            {
                X = value.X;
                Z = value.Y;
            }
        }

        public float X, Y, Z;

        #region Static Constructors
        public static Vector3 Identity
        {
            get { return new Vector3(1.0f, 1.0f, 1.0f); }
        }

        public static Vector3 Zero
        {
            get { return new Vector3(0.0f, 0.0f, 0.0f); }
        }

        public static Vector3 Up
        {
            get { return new Vector3(0.0f, 1.0f, 0.0f); }
        }

        public static Vector3 Down
        {
            get { return new Vector3(0.0f, -1.0f, 0.0f); }
        }

        public static Vector3 Forward
        {
            get { return new Vector3(0.0f, 0.0f, -1.0f); }
        }

        public static Vector3 Backward
        {
            get { return new Vector3(0.0f, 0.0f, 1.0f); }
        }

        public static Vector3 Left
        {
            get { return new Vector3(-1.0f, 0.0f, 0.0f); }
        }

        public static Vector3 Right
        {
            get { return new Vector3(1.0f, 0.0f, 0.0f); }
        }

        public static Vector3 UnitX
        {
            get { return new Vector3(1.0f, 0.0f, 0.0f); }
        }

        public static Vector3 UnitY
        {
            get { return new Vector3(0.0f, 1.0f, 0.0f); }
        }

        public static Vector3 UnitZ
        {
            get { return new Vector3(0.0f, 0.0f, 1.0f); }
        }

        public static Vector3 UnitScale
        {
            get { return new Vector3(1.0f, 1.0f, 1.0f); }
        }
        #endregion

        #region Operators
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3 operator +(Vector3 v, float s)
        {
            return new Vector3(v.X + s, v.Y + s, v.Z + s);
        }

        public static Vector3 operator +(float s, Vector3 v)
        {
            return new Vector3(v.X + s, v.Y + s, v.Z + s);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3 operator -(Vector3 v, float s)
        {
            return new Vector3(v.X - s, v.Y - s, v.Z - s);
        }

        public static Vector3 operator -(float s, Vector3 v)
        {
            return new Vector3(s - v.X, s - v.Y, s - v.Z);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static Vector3 operator *(float s, Vector3 v)
        {
            return new Vector3(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vector3 operator *(Vector3 v, float s)
        {
            return new Vector3(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vector3 operator /(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        public static Vector3 operator /(float s, Vector3 v)
        {
            return new Vector3(s / v.X, s / v.Y, s / v.Z);
        }

        public static Vector3 operator /(Vector3 v, float s)
        {
            return new Vector3(v.X / s, v.Y / s, v.Z / s);
        }

        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return (v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z);
        }

        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return (v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z);
        }
        #endregion

        #region Constructors
        /// <summary>Create a Vector3 structure, normally used to store Vertex positions.</summary>
        /// <param name="xyz">xyz values</param>
        public Vector3(float xyz)
        {
            this.X = xyz; this.Y = xyz; this.Z = xyz;
        }

        /// <summary>Create a Vector3 structure, normally used to store Vertex positions.</summary>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        /// <param name="z">z value</param>
        public Vector3(float x, float y, float z)
        {
            this.X = x; this.Y = y; this.Z = z;
        }

        /// <summary>Creates a Vector3 structure, normally used to store Vertex positions.  Casted to floats for OpenGL.</summary>
        /// <param name="x">x value</param>
        /// <param name="y">y value</param>
        /// <param name="z">z value</param>
        public Vector3(double x, double y, double z)
        {
            this.X = (float)x; this.Y = (float)y; this.Z = (float)z;
        }

        /// <summary>Creates a Vector3 structure from a Vector4, normally used to store Vertex positions.</summary>
        /// <param name="vec4">vector4</param>
        public Vector3(Vector4 vec4)
        {
            X = vec4.X;
            Y = vec4.Y;
            Z = vec4.Z;
        }

        /// <summary>Creates a Vector3 tructure from a float array (assuming the float array is of length 3).</summary>
        /// <param name="vector">The float array to convert to a Vector3.</param>
        public Vector3(float[] vector)
        {
            if (vector.Length != 3) throw new Exception(string.Format("float[] vector was of length {0}.  Was expecting a length of 3.", vector.Length));
            this.X = vector[0]; this.Y = vector[1]; this.Z = vector[2];
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3)) return false;

            return this.Equals((Vector3)obj);
        }

        public bool Equals(Vector3 other)
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
        /// Parses a JSON stream and produces a Vector3 struct.
        /// </summary>
        public static Vector3 Parse(string text)
        {
            string[] split = text.Trim(new char[] { '{', '}' }).Split(',');
            if (split.Length != 3) return Vector3.Zero;

            return new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
        }

        public float this[int a]
        {
            get { return (a == 0) ? X : (a == 1) ? Y : Z; }
            set { if (a == 0) X = value; else if (a == 1) Y = value; else Z = value; }
        }
        #endregion

        #region Methods
        public void SetX(float x)
        {
            X = x;
        }

        public void SetY(float y)
        {
            Y = y;
        }

        public void SetZ(float z)
        {
            Z = z;
        }

        public bool IsNan()
        {
            return float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z);
        }

        public Color4 ToColor4()
        {
            return new Color4(X, Y, Z, 1);
        }

        public float InnerProduct(Vector3 other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        /// <summary>
        /// Converts a Vector3 to a float array.  Useful for vector commands in GL.
        /// </summary>
        /// <returns>Float array representation of a Vector3</returns>
        public float[] ToFloat()
        {
            return new float[] { X, Y, Z };
        }

        /// <summary>
        /// Get the length of the Vector3 structure.
        /// </summary>
        public float Length
        {
            get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        /// <summary>
        /// Performs the Vector3 scalar dot product.
        /// </summary>
        /// <param name="v1">The left Vector3.</param>
        /// <param name="v2">The right Vector3.</param>
        /// <returns>Scalar dot product value</returns>
        public static float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Performs the Vector3 scalar dot product.
        /// </summary>
        /// <param name="v">Second dot product term</param>
        /// <returns>Vector3.Dot(this, v)</returns>
        public float Dot(Vector3 v)
        {
            return Vector3.Dot(this, v);
        }

        /// <summary>
        /// Returns the squared length of the Vector3 structure.
        /// </summary>
        public float LengthSquared
        {
            get { return X * X + Y * Y + Z * Z; }
        }

        /// <summary>
        /// Vector3 cross product
        /// </summary>
        /// <param name="v1">Vector1</param>
        /// <param name="v2">Vector2</param>
        /// <returns>Vector3 cross product value</returns>
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
        }

        /// <summary>
        /// Vector3 cross product
        /// </summary>
        /// <param name="v">Second cross product term</param>
        /// <returns>this x v</returns>
        public Vector3 Cross(Vector3 v)
        {
            return Vector3.Cross(this, v);
        }

        /// <summary>
        /// Normalizes the Vector3 structure to have a peak value of one.
        /// </summary>
        /// <returns>if (Length = 0) return Zero; else return Vector3(x,y,z)/Length</returns>
        public Vector3 Normalize()
        {
            if (Length == 0) return Zero;
            else return new Vector3(X, Y, Z) / Length;
        }

        /// <summary>
        /// Checks to see if any value (x, y, z) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.
        /// </summary>
        /// <returns>A truncated Vector3</returns>
        public Vector3 Truncate()
        {
            float _x = (Math.Abs(X) - 0.0001 < 0) ? 0 : X;
            float _y = (Math.Abs(Y) - 0.0001 < 0) ? 0 : Y;
            float _z = (Math.Abs(Z) - 0.0001 < 0) ? 0 : Z;
            return new Vector3(_x, _y, _z);
        }

        /// <summary>
        /// Checks to see if any value (x, y, z) are within 'bias' of 0.
        /// If so this method truncates that value to zero.
        /// </summary>
        /// <returns>A truncated Vector3</returns>
        public Vector3 Truncate(double bias)
        {
            float _x = (Math.Abs(X) - bias < 0) ? 0 : X;
            float _y = (Math.Abs(Y) - bias < 0) ? 0 : Y;
            float _z = (Math.Abs(Z) - bias < 0) ? 0 : Z;
            return new Vector3(_x, _y, _z);
        }

        /// <summary>
        /// Store the minimum values of x, y, and z between the two vectors.
        /// </summary>
        /// <param name="v">Vector to check against</param>
        public void TakeMin(Vector3 v)
        {
            if (v.X < X) X = v.X;
            if (v.Y < Y) Y = v.Y;
            if (v.Z < Z) Z = v.Z;
        }

        /// <summary>
        /// Store the maximum values of x, y, and z between the two vectors.
        /// </summary>
        /// <param name="v">Vector to check against</param>
        public void TakeMax(Vector3 v)
        {
            if (v.X > X) X = v.X;
            if (v.Y > Y) Y = v.Y;
            if (v.Z > Z) Z = v.Z;
        }

        /// <summary>
        /// Returns the maximum component of the Vector3.
        /// </summary>
        /// <returns>The maximum component of the Vector3</returns>
        public float Max()
        {
            return (X >= Y && X >= Z) ? X : (Y >= Z) ? Y : Z;
        }

        /// <summary>
        /// Returns the minimum component of the Vector3.
        /// </summary>
        /// <returns>The minimum component of the Vector3</returns>
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
        /// <returns>A linear interpolated Vector3.</returns>
        public static Vector3 Lerp(Vector3 v1, Vector3 v2, float amount)
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
        public static float CalculateAngle(Vector3 first, Vector3 second)
        {
            return (float)Math.Acos((Vector3.Dot(first, second)) / (first.Length * second.Length));
        }

        /// <summary>
        /// Calculates the angle (in radians) between two vectors.
        /// </summary>
        /// <param name="v">The second vector.</param>
        /// <returns>Angle (in radians) between the vectors.</returns>
        /// <remarks>Note that the returned angle is never bigger than the constant Pi.</remarks>
        public float CalculateAngle(Vector3 v)
        {
            return Vector3.CalculateAngle(this, v);
        }

        /// <summary>
        /// Swaps two Vector3 structures by passing via reference.
        /// </summary>
        /// <param name="v1">The first Vector3 structure.</param>
        /// <param name="v2">The second Vector3 structure.</param>
        public static void Swap(ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 t = v1;
            v1 = v2;
            v2 = t;
        }

        /// <summary>
        /// Create a quaternion that represents the rotation vector between this
        /// Vector3 and a destination Vector3.
        /// </summary>
        /// <param name="destination">The vector we would like to rotate to.</param>
        /// <returns>A quaternion representing the axis of rotation between this vector 
        /// and the destination vector.</returns>
        public Quaternion GetRotationTo(Vector3 destination)
        {
            // Based on Stan Melax's algorithm in "Game Programming Gems"
            Vector3 t_source = this.Normalize();
            Vector3 t_dest = destination.Normalize();

            float d = t_source.Dot(t_dest);

            // if dot == 1 then the vectors are the same
            if (d >= 1.0f) return Quaternion.Identity;
            else if (d < (1e-6f - 1.0f))
            {
                Vector3 t_axis = Vector3.UnitX.Cross(this);
                if (t_axis.LengthSquared < (1e-12)) // pick another if colinear
                    t_axis = Vector3.UnitY.Cross(this);
                t_axis = t_axis.Normalize();
                return Quaternion.FromAngleAxis((float)Math.PI, t_axis);
            }
            else
            {
                float t_sqrt = (float)Math.Sqrt((1 + d) * 2.0f);
                float t_invs = 1.0f / t_sqrt;

                Vector3 t_cross = t_source.Cross(t_dest);
                return new Quaternion(t_cross.X * t_invs, t_cross.Y * t_invs, t_cross.Z * t_invs, t_sqrt * 0.5f).Normalize();
            }
        }

        /// <summary>Transform a direction vector by the given Matrix
		/// Assumes the matrix has a bottom row of (0,0,0,1), that is the translation part is ignored.
		/// </summary>
		/// <param name="vec">The vector to transform</param>
		/// <param name="mat">The desired transformation</param>
		/// <returns>The transformed vector</returns>
		public static Vector3 TransformVector(Vector3 vec, Matrix4 mat)
        {
            Vector3 v;
            v.X = Vector3.Dot(vec, new Vector3(mat.Column1));
            v.Y = Vector3.Dot(vec, new Vector3(mat.Column2));
            v.Z = Vector3.Dot(vec, new Vector3(mat.Column3));
            return v;
        }

        /// <summary>Transform a direction vector by the given Matrix
        /// Assumes the matrix has a bottom row of (0,0,0,1), that is the translation part is ignored.
        /// </summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void TransformVector(ref Vector3 vec, ref Matrix4 mat, out Vector3 result)
        {
            result.X = vec.X * mat.Row1.X +
                vec.Y * mat.Row2.X +
                vec.Z * mat.Row3.X;

            result.Y = vec.X * mat.Row1.Y +
                vec.Y * mat.Row2.Y +
                vec.Z * mat.Row3.Y;

            result.Z = vec.X * mat.Row1.Z +
                vec.Y * mat.Row2.Z +
                vec.Z * mat.Row3.Z;
        }

        /// <summary>Transform a Normal by the given Matrix</summary>
        /// <remarks>
        /// This calculates the inverse of the given matrix, use TransformNormalInverse if you
        /// already have the inverse to avoid this extra calculation
        /// </remarks>
        /// <param name="norm">The normal to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed normal</returns>
        public static Vector3 TransformNormal(Vector3 norm, Matrix4 mat)
        {
            return TransformNormalInverse(norm, mat.Inverse());
        }

        /// <summary>Transform a Normal by the given Matrix</summary>
        /// <remarks>
        /// This calculates the inverse of the given matrix, use TransformNormalInverse if you
        /// already have the inverse to avoid this extra calculation
        /// </remarks>
        /// <param name="norm">The normal to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed normal</param>
        public static void TransformNormal(ref Vector3 norm, ref Matrix4 mat, out Vector3 result)
        {
            Matrix4 Inverse = mat.Inverse();
            Vector3.TransformNormalInverse(ref norm, ref Inverse, out result);
        }

        /// <summary>Transform a Normal by the (transpose of the) given Matrix</summary>
        /// <remarks>
        /// This version doesn't calculate the inverse matrix.
        /// Use this version if you already have the inverse of the desired transform to hand
        /// </remarks>
        /// <param name="norm">The normal to transform</param>
        /// <param name="invMat">The inverse of the desired transformation</param>
        /// <returns>The transformed normal</returns>
        public static Vector3 TransformNormalInverse(Vector3 norm, Matrix4 invMat)
        {
            Vector3 n;
            n.X = Vector3.Dot(norm, new Vector3(invMat.Row1));
            n.Y = Vector3.Dot(norm, new Vector3(invMat.Row2));
            n.Z = Vector3.Dot(norm, new Vector3(invMat.Row3));
            return n;
        }

        /// <summary>Transform a Normal by the (transpose of the) given Matrix</summary>
        /// <remarks>
        /// This version doesn't calculate the inverse matrix.
        /// Use this version if you already have the inverse of the desired transform to hand
        /// </remarks>
        /// <param name="norm">The normal to transform</param>
        /// <param name="invMat">The inverse of the desired transformation</param>
        /// <param name="result">The transformed normal</param>
        public static void TransformNormalInverse(ref Vector3 norm, ref Matrix4 invMat, out Vector3 result)
        {
            result.X = norm.X * invMat.Row1.X +
                norm.Y * invMat.Row1.Y +
                norm.Z * invMat.Row1.Z;

            result.Y = norm.X * invMat.Row2.X +
                norm.Y * invMat.Row2.Y +
                norm.Z * invMat.Row2.Z;

            result.Z = norm.X * invMat.Row3.X +
                norm.Y * invMat.Row3.Y +
                norm.Z * invMat.Row3.Z;
        }

        /// <summary>Transform a Position by the given Matrix</summary>
        /// <param name="pos">The position to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed position</returns>
        public static Vector3 TransformPosition(Vector3 pos, Matrix4 mat)
        {
            Vector3 p;
            p.X = Vector3.Dot(pos, new Vector3(mat.Column1)) + mat.Row4.X;
            p.Y = Vector3.Dot(pos, new Vector3(mat.Column2)) + mat.Row4.Y;
            p.Z = Vector3.Dot(pos, new Vector3(mat.Column3)) + mat.Row4.Z;
            return p;
        }

        /// <summary>Transform a Position by the given Matrix</summary>
        /// <param name="pos">The position to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed position</param>
        public static void TransformPosition(ref Vector3 pos, ref Matrix4 mat, out Vector3 result)
        {
            result.X = pos.X * mat.Row1.X +
                pos.Y * mat.Row2.X +
                pos.Z * mat.Row3.X +
                mat.Row4.X;

            result.Y = pos.X * mat.Row1.Y +
                pos.Y * mat.Row2.Y +
                pos.Z * mat.Row3.Y +
                mat.Row4.Y;

            result.Z = pos.X * mat.Row1.Z +
                pos.Y * mat.Row2.Z +
                pos.Z * mat.Row3.Z +
                mat.Row4.Z;
        }

        /// <summary>Transform a Vector by the given Matrix</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed vector</returns>
        public static Vector3 Transform(Vector3 vec, Matrix4 mat)
        {
            Vector3 result;
            Transform(ref vec, ref mat, out result);
            return result;
        }

        /// <summary>Transform a Vector by the given Matrix</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void Transform(ref Vector3 vec, ref Matrix4 mat, out Vector3 result)
        {
            Vector4 v4 = new Vector4(vec.X, vec.Y, vec.Z, 1.0f);
            Vector4.Transform(ref v4, ref mat, out v4);
            result = v4.Xyz;
        }

        /// <summary>
        /// Transforms a vector by a quaternion rotation.
        /// </summary>
        /// <param name="vec">The vector to transform.</param>
        /// <param name="quat">The quaternion to rotate the vector by.</param>
        /// <returns>The result of the operation.</returns>
        public static Vector3 Transform(Vector3 vec, Quaternion quat)
        {
            Vector3 result;
            Transform(ref vec, ref quat, out result);
            return result;
        }

        /// <summary>
        /// Transforms a vector by a quaternion rotation.
        /// </summary>
        /// <param name="vec">The vector to transform.</param>
        /// <param name="quat">The quaternion to rotate the vector by.</param>
        /// <param name="result">The result of the operation.</param>
        public static void Transform(ref Vector3 vec, ref Quaternion quat, out Vector3 result)
        {
            // Since vec.W == 0, we can optimize quat * vec * quat^-1 as follows:
            // vec + 2.0 * cross(quat.xyz, cross(quat.xyz, vec) + quat.w * vec)
            Vector3 xyz = quat.Xyz, temp, temp2;
            temp = xyz.Cross(vec);
            temp2 = vec * quat.W;
            temp = temp + temp2;
            temp = xyz.Cross(temp);
            temp = temp * 2;
            result = vec + temp;
            //Vector3.Cross(ref xyz, ref vec, out temp);
            //Vector3.Multiply(ref vec, quat.W, out temp2);
            //Vector3.Add(ref temp, ref temp2, out temp);
            //Vector3.Cross(ref xyz, ref temp, out temp);
            //Vector3.Multiply(ref temp, 2, out temp);
            //Vector3.Add(ref vec, ref temp, out result);
        }

        /// <summary>Transform a Vector3 by the given Matrix, and project the resulting Vector4 back to a Vector3</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed vector</returns>
        public static Vector3 TransformPerspective(Vector3 vec, Matrix4 mat)
        {
            Vector3 result;
            TransformPerspective(ref vec, ref mat, out result);
            return result;
        }

        /// <summary>Transform a Vector3 by the given Matrix, and project the resulting Vector4 back to a Vector3</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void TransformPerspective(ref Vector3 vec, ref Matrix4 mat, out Vector3 result)
        {
            Vector4 v = new Vector4(vec, 1);
            Vector4.Transform(ref v, ref mat, out v);
            result.X = v.X / v.W;
            result.Y = v.Y / v.W;
            result.Z = v.Z / v.W;
        }
        #endregion
    }
}
