using System;

/* Rectanglei.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    /// <summary>
    /// Represents a Rectangle that uses ints for values.
    /// </summary>
    public struct Rectanglei
    {
        /// <summary>
        /// Returns a Rectanglei of dimensions (0, 0, 0, 0).
        /// </summary>
        public static Rectanglei Zero { get { return new Rectanglei(0, 0, 0, 0); } }

        /// <summary>
        /// The X location of this Rectanglei.
        /// </summary>
        public int X;
        /// <summary>
        /// The Y location of this Rectanglei.
        /// </summary>
        public int Y;
        /// <summary>
        /// The width of this Rectanglei.
        /// </summary>
        public int Width;
        /// <summary>
        /// The height of this Rectanglei.
        /// </summary>
        public int Height;

        /// <summary>
        /// Gets the right value of this Rectanglei (aka X + Width).
        /// </summary>
        public int Right { get { return X + Width; } }
        /// <summary>
        /// Gets the bottom value of this Rectanglei (aka Y + Height).
        /// </summary>
        public int Bottom { get { return Y + Height; } }

        /// <summary>
        /// Gets the center of this Rectanglei in world space.
        /// <para>X + (Width / 2), Y + (Height / 2)</para>
        /// </summary>
        public Vector2i AbsoluteCenter { get { return new Vector2i(X + (Width / 2), Y + (Height / 2)); } }
        /// <summary>
        /// Gets the relative center of this Rectanglei, which only accounts for width and height.
        /// <para>X and Y are ignored in this position.</para>
        /// <para>Width / 2, Height / 2</para>
        /// </summary>
        public Vector2i RelativeCenter { get { return new Vector2i(Width / 2, Height / 2); } }
        /// <summary>
        /// Gets or Sets the position of this Rectanglei.
        /// </summary>
        public Vector2i Location
        {
            get { return new Vector2i(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        /// <summary>
        /// Gets the top right position.
        /// </summary>
        public Vector2i TopRight { get { return new Vector2i(Right, Y); } }
        /// <summary>
        /// Gets the bottom left position.
        /// </summary>
        public Vector2i BottomLeft { get { return new Vector2i(X, Bottom); } }
        /// <summary>
        /// Gets the bottom right position.
        /// </summary>
        public Vector2i BottomRight { get { return new Vector2i(Right, Bottom); } }

        /// <summary>
        /// Gets or Sets the size of this Rectanglei.
        /// </summary>
        public Vector2i Size
        {
            get { return new Vector2i(Width, Height); }
            set { Width = value.X; Height = value.Y; }
        }

        /// <summary>
        /// Returns whether this Rectanglei is empty.
        /// <para>It is empty when the width or height is 
        /// less than or equal to zero.</para>
        /// </summary>
        public bool IsEmpty { get { return Width <= 0 || Height <= 0; } }

        #region Constructors
        public Rectanglei(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Rectanglei(Vector2i position, int width, int height)
        {
            this.X = position.X;
            this.Y = position.Y;
            this.Width = width;
            this.Height = height;
        }

        public Rectanglei(Vector2i position, Vector2i size)
        {
            this.X = position.X;
            this.Y = position.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }

        public Rectanglei(int x, int y, Vector2i size)
        {
            this.X = x;
            this.Y = y;
            this.Width = size.X;
            this.Height = size.Y;
        }
        #endregion

        /// <summary>
        /// Returns a scaled version of this Rectanglei
        /// </summary>
        public Rectanglei GetScaled(int x, int y)
        {
            Rectanglei rect = this.Clone();
            rect.Width *= x;
            rect.Height *= y;

            return rect;
        }

        /// <summary>
        /// Makes an exact copy of this Rectanglei
        /// </summary>
        public Rectanglei Clone()
        {
            return new Rectanglei(X, Y, Width, Height);
        }

        /// <summary>
        /// Returns whether the given position is inside of this Rectanglei.
        /// </summary>
        public bool Contains(Vector2i position)
        {
            return (position.X >= X && position.Y >= Y
                && position.X <= X + Width && position.Y <= Y + Height);
        }

        public bool ExclusiveContains(Vector2i position)
        {
            return (position.X > X && position.Y > Y
                && position.X < X + Width && position.Y < Y + Height);
        }

        /// <summary>
        /// Returns whether the given Rectanglei is 
        /// intersecting with this Rectanglei.
        /// </summary>
        public bool Intersects(Rectanglei rect)
        {
            return rect.Right >= X
                && rect.Bottom >= Y
                && rect.X <= Right
                && rect.Y <= Bottom;
        }

        /// <summary>
        /// Returns the union of this Rectanglei and another.
        /// <para>Union is a Rectanglei that tightly fits both Rectangleis.</para>
        /// </summary>
        public Rectanglei Union(Rectanglei other)
        {
            return Rectanglei.Union(this, other);
        }

        /// <summary>
        /// Returns the intersection of this Rectanglei and another.
        /// <para>Intersection is a Rectanglei that is only the space 
        /// that both Rectangleis occupy.</para>
        /// </summary>
        public Rectanglei Intersection(Rectanglei other)
        {
            return Rectanglei.Intersection(this, other);
        }

        #region Operators
        public static bool operator ==(Rectanglei a, Rectanglei b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Rectanglei a, Rectanglei b)
        {
            return !a.Equals(b);
        }

        public static Rectanglei operator *(Rectanglei a, Rectanglei b)
        {
            return Rectanglei.Multiply(a, b);
        }

        public static Rectanglei operator /(Rectanglei a, Rectanglei b)
        {
            return Rectanglei.Divide(a, b);
        }

        public static Rectanglei operator +(Rectanglei a, Rectanglei b)
        {
            return Rectanglei.Add(a, b);
        }

        public static Rectanglei operator +(Rectanglei a, Vector2i b)
        {
            return Rectanglei.Add(a, b);
        }

        public static Rectanglei operator -(Rectanglei a, Rectanglei b)
        {
            return Rectanglei.Subtract(a, b);
        }

        public static Rectanglei operator -(Rectanglei a, Vector2i b)
        {
            return Rectanglei.Subtract(a, b);
        }
        #endregion

        #region Math
        public static Rectanglei Multiply(Rectanglei a, Rectanglei b)
        {
            return new Rectanglei(a.X * b.X, a.Y * b.Y, a.Width * b.Width, a.Height * b.Height);
        }

        public static Rectanglei Add(Rectanglei a, Rectanglei b)
        {
            return new Rectanglei(a.X + b.X, a.Y + b.Y, a.Width + b.Width, a.Height + b.Height);
        }

        public static Rectanglei Add(Rectanglei a, Vector2i b)
        {
            return new Rectanglei(a.X + b.X, a.Y + b.Y, a.Width, a.Height);
        }

        public static Rectanglei Subtract(Rectanglei a, Rectanglei b)
        {
            return new Rectanglei(a.X - b.X, a.Y - b.Y, a.Width - b.Width, a.Height - b.Height);
        }

        public static Rectanglei Subtract(Rectanglei a, Vector2i b)
        {
            return new Rectanglei(a.X - b.X, a.Y - b.Y, a.Width, a.Height);
        }

        public static Rectanglei Divide(Rectanglei a, Rectanglei b)
        {
            return new Rectanglei(a.X / b.X, a.Y / b.Y, a.Width / b.Width, a.Height / b.Height);
        }

        /// <summary>
        /// Returns the union of two Rectangleis.
        /// <para>Union is a Rectanglei that tightly fits both Rectangleis.</para>
        /// </summary>
        public static Rectanglei Union(Rectanglei a, Rectanglei b)
        {
            int x1 = Math.Min(a.X, b.Y);
            int x2 = Math.Max(a.Right, b.Right);
            int y1 = Math.Min(a.Y, b.Y);
            int y2 = Math.Max(a.Bottom, b.Bottom);

            return new Rectanglei(x1, y1, x2 - x1, y2 - y1);
        }

        /// <summary>
        /// Returns the intersection of two Rectangleis.
        /// <para>Intersection is a Rectanglei that is only the space 
        /// that both Rectangleis occupy.</para>
        /// </summary>
        public static Rectanglei Intersection(Rectanglei a, Rectanglei b)
        {
            // Modifed From http://kickjava.com/src/java/awt/Rectanglei.java.htm
            int tx1 = a.X;
            int ty1 = a.Y;
            int rx1 = b.X;
            int ry1 = b.Y;

            int tx2 = a.Right;
            int ty2 = a.Bottom;
            int rx2 = b.Right;
            int ry2 = b.Bottom;

            if (tx1 < rx1) tx1 = rx1;
            if (ty1 < ry1) ty1 = ry1;
            if (tx2 > rx2) tx2 = rx2;
            if (ty2 > ry2) ty2 = ry2;

            tx2 -= tx1;
            ty2 -= ty1;

            return new Rectanglei(tx1, ty1, tx2, ty2);
        }
        #endregion

        #region .Net Overrides
        /// <summary>
        /// Returns whether this Rectanglei equals the other object in value.
        /// <para>Only true if the object is a RectangleiF or Rectanglei and 
        /// their values are the same.</para>
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                if (obj is Rectanglei)
                {
                    Rectanglei other = (Rectanglei)obj;
                    return X == other.X
                        && Y == other.Y
                        && Width == other.Width
                        && Height == other.Height;
                }
                else if (obj is Rectanglei)
                {
                    Rectanglei other = (Rectanglei)obj;
                    return X == other.X
                        && Y == other.Y
                        && Width == other.Width
                        && Height == other.Height;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)(X + Y * Width + Height);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", X, Y, Width, Height);
        }
        #endregion

        /// <summary>
        /// Converts this Rectanglei to a float-based Rectangle.
        /// </summary>
        public Rectangle ToRectagle()
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }
}
