using System;

/* Rectangle.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    /// <summary>
    /// Represents a rectangle that uses floats for values.
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// Returns a rectangle of dimensions (0, 0, 0, 0).
        /// </summary>
        public static Rectangle Zero { get { return new Rectangle(0, 0, 0, 0); } }

        /// <summary>
        /// The X location of this rectangle.
        /// </summary>
        public float X;
        /// <summary>
        /// The Y location of this rectangle.
        /// </summary>
        public float Y;
        /// <summary>
        /// The width of this rectangle.
        /// </summary>
        public float Width;
        /// <summary>
        /// The height of this rectangle.
        /// </summary>
        public float Height;

        /// <summary>
        /// Gets the right value of this rectangle (aka X + Width).
        /// </summary>
        public float Right { get { return X + Width; } }
        /// <summary>
        /// Gets the bottom value of this rectangle (aka Y + Height).
        /// </summary>
        public float Bottom { get { return Y + Height; } }

        /// <summary>
        /// Gets the center of this rectangle in world space.
        /// <para>X + (Width / 2), Y + (Height / 2)</para>
        /// </summary>
        public Vector2 AbsoluteCenter { get { return new Vector2(X + (Width / 2), Y + (Height / 2)); } }
        /// <summary>
        /// Gets the relative center of this rectangle, which only accounts for width and height.
        /// <para>X and Y are ignored in this position.</para>
        /// <para>Width / 2, Height / 2</para>
        /// </summary>
        public Vector2 RelativeCenter { get { return new Vector2(Width / 2, Height / 2); } }
        /// <summary>
        /// Gets or Sets the position of this rectangle.
        /// </summary>
        public Vector2 Location
        {
            get { return new Vector2(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        /// <summary>
        /// Gets the top right position.
        /// </summary>
        public Vector2 TopRight { get { return new Vector2(Right, Y); } }
        /// <summary>
        /// Gets the bottom left position.
        /// </summary>
        public Vector2 BottomLeft { get { return new Vector2(X, Bottom); } }
        /// <summary>
        /// Gets the bottom right position.
        /// </summary>
        public Vector2 BottomRight { get { return new Vector2(Right, Bottom); } }

        /// <summary>
        /// Gets or Sets the size of this rectangle.
        /// </summary>
        public Vector2 Size
        {
            get { return new Vector2(Width, Height); }
            set { Width = value.X; Height = value.Y; }
        }

        /// <summary>
        /// Returns whether this rectangle is empty.
        /// <para>It is empty when the width or height is 
        /// less than or equal to zero.</para>
        /// </summary>
        public bool IsEmpty { get { return Width <= 0 || Height <= 0; } }

        #region Constructors
        public Rectangle(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Rectangle(Vector2 position, float width, float height)
        {
            this.X = position.X;
            this.Y = position.Y;
            this.Width = width;
            this.Height = height;
        }

        public Rectangle(Vector2 position, Vector2 size)
        {
            this.X = position.X;
            this.Y = position.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }

        public Rectangle(float x, float y, Vector2 size)
        {
            this.X = x;
            this.Y = y;
            this.Width = size.X;
            this.Height = size.Y;
        }
        #endregion

        /// <summary>
        /// Returns a scaled version of this rectangle
        /// </summary>
        public Rectangle GetScaled(float x, float y)
        {
            Rectangle rect = this.Clone();
            rect.Width *= x;
            rect.Height *= y;

            return rect;
        }

        /// <summary>
        /// Makes an exact copy of this rectangle
        /// </summary>
        public Rectangle Clone()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        /// <summary>
        /// Returns whether the given position is inside of this rectangle.
        /// </summary>
        public bool Contains(Vector2 position)
        {
            return (position.X >= X && position.Y >= Y
                && position.X <= X + Width && position.Y <= Y + Height);
        }

        /// <summary>
        /// Returns whether the given position is inside of this rectangle.
        /// </summary>
        public bool Contains(Vector2i position)
        {
            return (position.X >= X && position.Y >= Y
                && position.X <= X + Width && position.Y <= Y + Height);
        }

        public bool ExclusiveContains(Vector2 position)
        {
            return (position.X > X && position.Y > Y
                && position.X < X + Width && position.Y < Y + Height);
        }

        public bool ExclusiveContains(Vector2i position)
        {
            return (position.X > X && position.Y > Y
                && position.X < X + Width && position.Y < Y + Height);
        }

        /// <summary>
        /// Returns whether the given rectangle is 
        /// intersecting with this rectangle.
        /// </summary>
        public bool Intersects(Rectangle rect)
        {
            return rect.Right >= X
                && rect.Bottom >= Y
                && rect.X <= Right
                && rect.Y <= Bottom;
        }

        /// <summary>
        /// Returns the union of this rectangle and another.
        /// <para>Union is a rectangle that tightly fits both rectangles.</para>
        /// </summary>
        public Rectangle Union(Rectangle other)
        {
            return Rectangle.Union(this, other);
        }

        /// <summary>
        /// Returns the intersection of this rectangle and another.
        /// <para>Intersection is a rectangle that is only the space 
        /// that both rectangles occupy.</para>
        /// </summary>
        public Rectangle Intersection(Rectangle other)
        {
            return Rectangle.Intersection(this, other);
        }

        #region Operators
        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !a.Equals(b);
        }

        public static Rectangle operator *(Rectangle a, Rectangle b)
        {
            return Rectangle.Multiply(a, b);
        }

        public static Rectangle operator /(Rectangle a, Rectangle b)
        {
            return Rectangle.Divide(a, b);
        }

        public static Rectangle operator +(Rectangle a, Rectangle b)
        {
            return Rectangle.Add(a, b);
        }

        public static Rectangle operator +(Rectangle a, Vector2 b)
        {
            return Rectangle.Add(a, b);
        }

        public static Rectangle operator -(Rectangle a, Rectangle b)
        {
            return Rectangle.Subtract(a, b);
        }

        public static Rectangle operator -(Rectangle a, Vector2 b)
        {
            return Rectangle.Subtract(a, b);
        }
        #endregion

        #region Math
        public static Rectangle Multiply(Rectangle a, Rectangle b)
        {
            return new Rectangle(a.X * b.X, a.Y * b.Y, a.Width * b.Width, a.Height * b.Height);
        }

        public static Rectangle Add(Rectangle a, Rectangle b)
        {
            return new Rectangle(a.X + b.X, a.Y + b.Y, a.Width + b.Width, a.Height + b.Height);
        }

        public static Rectangle Add(Rectangle a, Vector2 b)
        {
            return new Rectangle(a.X + b.X, a.Y + b.Y, a.Width, a.Height);
        }

        public static Rectangle Subtract(Rectangle a, Rectangle b)
        {
            return new Rectangle(a.X - b.X, a.Y - b.Y, a.Width - b.Width, a.Height - b.Height);
        }

        public static Rectangle Subtract(Rectangle a, Vector2 b)
        {
            return new Rectangle(a.X - b.X, a.Y - b.Y, a.Width, a.Height);
        }

        public static Rectangle Divide(Rectangle a, Rectangle b)
        {
            return new Rectangle(a.X / b.X, a.Y / b.Y, a.Width / b.Width, a.Height / b.Height);
        }

        /// <summary>
        /// Returns the union of two rectangles.
        /// <para>Union is a rectangle that tightly fits both rectangles.</para>
        /// </summary>
        public static Rectangle Union(Rectangle a, Rectangle b)
        {
            float x1 = Math.Min(a.X, b.Y);
            float x2 = Math.Max(a.Right, b.Right);
            float y1 = Math.Min(a.Y, b.Y);
            float y2 = Math.Max(a.Bottom, b.Bottom);

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        /// <summary>
        /// Returns the intersection of two rectangles.
        /// <para>Intersection is a rectangle that is only the space 
        /// that both rectangles occupy.</para>
        /// </summary>
        public static Rectangle Intersection(Rectangle a, Rectangle b)
        {
            // Modifed From http://kickjava.com/src/java/awt/Rectangle.java.htm
            float tx1 = a.X;
            float ty1 = a.Y;
            float rx1 = b.X;
            float ry1 = b.Y;

            float tx2 = a.Right;
            float ty2 = a.Bottom;
            float rx2 = b.Right;
            float ry2 = b.Bottom;

            if (tx1 < rx1) tx1 = rx1;
            if (ty1 < ry1) ty1 = ry1;
            if (tx2 > rx2) tx2 = rx2;
            if (ty2 > ry2) ty2 = ry2;

            tx2 -= tx1;
            ty2 -= ty1;

            return new Rectangle(tx1, ty1, tx2, ty2);
        }
        #endregion

        #region .Net Overrides
        /// <summary>
        /// Returns whether this rectangle equals the other object in value.
        /// <para>Only true if the object is a RectangleF or Rectangle and 
        /// their values are the same.</para>
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                if (obj is Rectangle)
                {
                    Rectangle other = (Rectangle)obj;
                    return X == other.X
                        && Y == other.Y
                        && Width == other.Width
                        && Height == other.Height;
                }
                else if (obj is Rectangle)
                {
                    Rectangle other = (Rectangle)obj;
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
        /// Converts this rectangle to an integer rectanglei.
        /// </summary>
        public Rectanglei ToRectaglei()
        {
            return new Rectanglei((int)X, (int)Y, (int)Width, (int)Height);
        }
    }
}
