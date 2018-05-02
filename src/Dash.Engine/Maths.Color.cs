using Dash.Engine.Graphics;
using System;

/* Maths.Color.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public static partial class Maths
    {
        public static Color ToColor(this Color4 color)
        {
            return new Color(FloatToByteColor(color.R), FloatToByteColor(color.G), FloatToByteColor(color.B), FloatToByteColor(color.A));
        }

        public static Color4 ToColor4(this Color color)
        {
            return new Color4(color.R, color.G, color.B, color.A);
        }

        public static Vector3 RGBToVector3(byte r, byte g, byte b)
        {
            return new Vector3(r / 255f, g / 255f, b / 255f);
        }

        public static byte FloatToByteColor(float c)
        {
            return (byte)(c * 255);
        }

        public static float ByteToFloatColor(byte c)
        {
            return c / 255f;
        }

        public static void RGBToHSV(byte r, byte g, byte b, 
            out float hue, out float saturation, out float value)
        {
            float ri = r / 255f;
            float gi = g / 255f;
            float bi = b / 255f;

            float cMax = Math.Max(Math.Max(ri, gi), bi);
            float cMin = Math.Min(Math.Min(ri, gi), bi);
            float delta = cMax - cMin;

            hue = 0;
            saturation = 0;
            value = 0;

            if (delta == 0) hue = 0;
            else if (cMax == ri) hue = 60 * (((gi - bi) / delta) % 6);
            else if (cMax == gi) hue = 60 * (((bi - ri) / delta) + 2);
            else if (cMax == bi) hue = 60 * (((ri - gi) / delta) + 4);

            saturation = cMax == 0 ? 0 : (delta / cMax);
            value = cMax;

            if (hue < 0)
                hue += 360;
            if (hue > 360)
                hue -= 360;
        }

        public static Color HSVToRGB(float h, float s, float v)
        {
            float c = v * s;
            float h1 = h / 60f;
            float x = c * (1 - Math.Abs(h1 % 2 - 1));
            float m = v - c;

            float r = 0, g = 0, b = 0;

            if (h1 >= 0 && h1 < 1)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (h1 >= 1 && h1 < 2)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (h1 >= 2 && h1 < 3)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (h1 >= 3 && h1 < 4)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (h1 >= 4 && h1 < 5)
            {
                r = x;
                g = 0;
                b = c;
            }
            else if (h1 >= 5 && h1 < 6)
            {
                r = c;
                g = 0;
                b = x;
            }

            return new Color((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
        }
    }
}
