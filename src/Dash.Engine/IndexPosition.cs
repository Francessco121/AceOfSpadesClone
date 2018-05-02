using System;

/* IndexPosition.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public struct IndexPosition
    {
        public static readonly IndexPosition Zero = new IndexPosition(0, 0, 0);

        public int X;
        public int Y;
        public int Z;

        public IndexPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public IndexPosition(Vector3 vec3)
        {
            X = (int)vec3.X;
            Y = (int)vec3.Y;
            Z = (int)vec3.Z;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", X, Y, Z);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public override int GetHashCode()
        {
            return X * Y + Y * Z + X;
        }

        public override bool Equals(object obj)
        {
            if (obj is IndexPosition)
            {
                IndexPosition obji = (IndexPosition)obj;
                return obji == this;
            }
            else
                return false;
        }

        public static float Distance(IndexPosition a, IndexPosition b)
        {
            return (float)Math.Sqrt(
                Math.Pow(b.X - a.X, 2) +
                Math.Pow(b.Y - a.Y, 2) +
                Math.Pow(b.Z - a.Z, 2)
            );
        }

        public static bool operator ==(IndexPosition a, IndexPosition b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(IndexPosition a, IndexPosition b)
        {
            return !(a == b);
        }

        public static IndexPosition operator +(IndexPosition a, IndexPosition b)
        {
            return new IndexPosition(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator +(IndexPosition a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static IndexPosition operator -(IndexPosition a, IndexPosition b)
        {
            return new IndexPosition(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static IndexPosition operator *(IndexPosition a, IndexPosition b)
        {
            return new IndexPosition(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3 operator *(IndexPosition a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3 operator *(IndexPosition a, float b)
        {
            return new Vector3(a.X * b, a.Y * b, a.Z * b);
        }

        public static IndexPosition operator /(IndexPosition a, IndexPosition b)
        {
            return new IndexPosition(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3 operator /(IndexPosition a, float b)
        {
            return new Vector3(a.X / b, a.Y / b, a.Z / b);
        }
    }
}
