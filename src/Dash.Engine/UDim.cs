/* UDim.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    /// <summary>
    /// Defines a universal dimension.
    /// <see cref="http://wiki.roblox.com/index.php?title=UDim"/>
    /// </summary>
    public struct UDim
    {
        public static readonly UDim Zero = new UDim(0, 0);
        public static readonly UDim Fill = new UDim(1f, 0);

        public float Scale;
        public float Offset;

        public UDim(float scale, float offset)
        {
            Scale = scale;
            Offset = offset;
        }

        public float GetValue(float relativeTo)
        {
            return (Scale * relativeTo) + Offset;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
            {
                if (obj is UDim)
                    return (UDim)obj == this;
                else
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return (int)(Scale + Offset);
        }

        public static bool operator ==(UDim a, UDim b)
        {
            return a.Offset == b.Offset && a.Scale == b.Scale;
        }

        public static bool operator !=(UDim a, UDim b)
        {
            return a.Offset != b.Offset || a.Scale != b.Scale;
        }

        public static UDim operator +(UDim a, UDim b)
        {
            return new UDim(a.Scale + b.Scale, a.Offset + b.Offset);
        }

        public static UDim operator +(UDim a, float offset)
        {
            return new UDim(a.Scale, a.Offset + offset);
        }

        public static UDim operator -(UDim a, UDim b)
        {
            return new UDim(a.Scale - b.Scale, a.Offset - b.Offset);
        }

        public static UDim operator -(UDim a, float offset)
        {
            return new UDim(a.Scale, a.Offset - offset);
        }

        public static UDim operator *(UDim a, UDim b)
        {
            return new UDim(a.Scale * b.Scale, a.Offset * b.Offset);
        }

        public static UDim operator *(UDim a, float scalar)
        {
            return new UDim(a.Scale * scalar, a.Offset * scalar);
        }

        public static UDim operator /(UDim a, UDim b)
        {
            return new UDim(a.Scale / b.Scale, a.Offset / b.Offset);
        }
    }

    /// <summary>
    /// Defines a pair of universal dimensions.
    /// <see cref="http://wiki.roblox.com/index.php?title=UDim2"/>
    /// </summary>
    public struct UDim2
    {
        public static readonly UDim2 Zero = new UDim2(0, 0, 0, 0);
        public static readonly UDim2 Fill = new UDim2(1f, 0, 1f, 0);

        public UDim X;
        public UDim Y;

        public UDim2(UDim x, UDim y)
        {
            X = x;
            Y = y;
        }

        public UDim2(float xScale, float xOffset, float yScale, float yOffset)
        {
            X = new UDim(xScale, xOffset);
            Y = new UDim(yScale, yOffset);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
            {
                if (obj is UDim2)
                    return (UDim2)obj == this;
                else
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return (int)(X.Offset + X.Scale + Y.Offset + Y.Scale);
        }

        public Vector2 GetValue(float relativeToX, float relativeToY, bool leave1x1Range = false)
        {
            Vector2 av = new Vector2(X.GetValue(relativeToX), Y.GetValue(relativeToY));

            if (leave1x1Range)
            {
                av.X /= relativeToX;
                av.Y /= relativeToY;
            }

            return av;
        }

        public static bool operator ==(UDim2 a, UDim2 b)
        {
            return (a.X == b.X && a.Y == b.Y);
        }

        public static bool operator !=(UDim2 a, UDim2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static UDim2 operator +(UDim2 a, UDim2 b)
        {
            return new UDim2(a.X + b.X, a.Y + b.Y);
        }

        public static UDim2 operator +(UDim2 a, Vector2 offset)
        {
            return new UDim2(a.X + offset.X, a.Y + offset.Y);
        }

        public static UDim2 operator -(UDim2 a, UDim2 b)
        {
            return new UDim2(a.X - b.X, a.Y - b.Y);
        }

        public static UDim2 operator -(UDim2 a, Vector2 offset)
        {
            return new UDim2(a.X - offset.X, a.Y - offset.Y);
        }

        public static UDim2 operator *(UDim2 a, UDim2 b)
        {
            return new UDim2(a.X * b.X, a.Y * b.Y);
        }

        public static UDim2 operator *(UDim2 a, float scalar)
        {
            return new UDim2(a.X * scalar, a.Y * scalar);
        }

        public static UDim2 operator /(UDim2 a, UDim2 b)
        {
            return new UDim2(a.X / b.X, a.Y / b.Y);
        }
    }
}
