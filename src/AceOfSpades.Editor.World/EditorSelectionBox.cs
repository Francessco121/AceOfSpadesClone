using Dash.Engine;
using System;

/* EditorSelectionBox.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World
{
    public class EditorSelectionBox
    {
        public IndexPosition Min { get; private set; }
        public IndexPosition Max { get; private set; }

        public IndexPosition Primary { get; private set; }
        public IndexPosition Secondary { get; private set; }

        public void SetPrimary(IndexPosition p)
        {
            Primary = p;
            Secondary = p;
            CalculateMinMax();
        }

        public void SetSecondary(IndexPosition s)
        {
            Secondary = s;
            CalculateMinMax();
        }

        public void SetMinMax(IndexPosition min, IndexPosition max)
        {
            if (min.X > max.X) min.X = max.X;
            if (min.Y > max.Y) min.Y = max.Y;
            if (min.Z > max.Z) min.Z = max.Z;

            Min = min;
            Max = max;
        }

        public void Translate(IndexPosition move)
        {
            Min += move;
            Max += move;
        }

        public Vector3 Center()
        {
            return new Vector3(
                Min.X + (Max.X - Min.X) / 2f,
                Min.Y + (Max.Y - Min.Y) / 2f,
                Min.Z + (Max.Z - Min.Z) / 2f)
                * Block.CUBE_3D_SIZE;
        }

        public IndexPosition Size()
        {
            return new IndexPosition(
                Max.X - Min.X,
                Max.Y - Min.Y,
                Max.Z - Min.Z);
        }

        public bool Contains(IndexPosition pos)
        {
            return pos.X >= Min.X
                && pos.Y >= Min.Y
                && pos.Z >= Min.Z
                && pos.X <= Max.X
                && pos.Y <= Max.Y
                && pos.Z <= Max.Z;
        }

        void CalculateMinMax()
        {
            Min = new IndexPosition(
                Math.Min(Primary.X, Secondary.X),
                Math.Min(Primary.Y, Secondary.Y),
                Math.Min(Primary.Z, Secondary.Z));

            Max = new IndexPosition(
                Math.Max(Primary.X, Secondary.X),
                Math.Max(Primary.Y, Secondary.Y),
                Math.Max(Primary.Z, Secondary.Z));
        }
    }
}
