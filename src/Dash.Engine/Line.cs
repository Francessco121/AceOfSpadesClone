/* Line.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public struct Line3D
    {
        public Vector3 Start;
        public Vector3 End;

        public Line3D(Vector3 start, Vector3 end)
        {
            this.Start = start;
            this.End = end;
        }

        public float Length()
        {
            return Maths.Distance(Start, End);
        }

        public Vector3 MidPoint()
        {
            return new Vector3(
                (Start.X + End.X) / 2f,
                (Start.Y + End.Y) / 2f,
                (Start.Z + End.Z) / 2f);
        }

        Vector2 ToVec2(Vector3 vec, bool flatten)
        {
            return flatten ? new Vector2(vec.X, vec.Z) : new Vector2(vec.X, vec.Y);
        }

        public bool IsPointBetween(Vector3 point, float slack, bool flatten = true)
        {
            Vector2 start = ToVec2(Start, flatten);
            Vector2 end = ToVec2(End, flatten);
            Vector2 pnt = ToVec2(point, flatten);

            if (slack != 0)
            {
                Vector2 startToEnd = Vector2.Normalize(start - end) * slack;
                start += startToEnd;
                end -= startToEnd;
            }

            return Maths.PointBetweenSegment(pnt, start, end);
        }

        public double DistanceToLine_expiremental(Vector3 point)
        {
            return Maths.DistanceToLine(point, Start, End);
        }

        public double DistanceToLine(Vector3 point, bool flatten = true)
        {
            Vector2 start = ToVec2(Start, flatten);
            Vector2 end = ToVec2(End, flatten);
            Vector2 pnt = ToVec2(point, flatten);

            return Maths.DistanceToLine(pnt, start, end);
        }
    }

    public struct Line2D
    {
        public Vector2 Start;
        public Vector2 End;

        public Line2D(Vector2 start, Vector2 end)
        {
            this.Start = start;
            this.End = end;
        }

        public bool IsPointBetween(Vector2 point)
        {
            return Maths.PointBetweenSegment(point, Start, End);
        }

        public double DistanceToLine(Vector2 point)
        {
            return Maths.DistanceToLine(point, Start, End);
        }
    }
}
