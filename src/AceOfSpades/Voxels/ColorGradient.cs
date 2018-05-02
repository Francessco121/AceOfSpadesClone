using Dash.Engine;
using Dash.Engine.Graphics;

/* ColorGradient.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class ColorGradient
    {
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }

        public Vector3 StartColor { get; set; }
        public Vector3 EndColor { get; set; }

        public bool ClampLerp = true;

        public ColorGradient(Vector3 startPos, Vector3 endPos, Vector3 startColor, Vector3 endColor)
        {
            StartPosition = startPos;
            EndPosition = endPos;
            StartColor = startColor;
            EndColor = endColor;
        }

        public Color GetColor(Vector3 position)
        {
            float l = Interpolation.InverseLerp(StartPosition, EndPosition, position);
            if (ClampLerp) l = MathHelper.Clamp(l, 0, 1);
            Vector3 colorLerp = Interpolation.Lerp(StartColor, EndColor, l);

            return new Color(colorLerp.X, colorLerp.Y, colorLerp.Z);
        }
    }
}
