/* Light.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics
{
    public class Light
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Color Color;
        public Vector3 Attenuation = new Vector3(1, 0, 0.1f);
        public float LightPower;
        public float Radius;
        public LightType Type;
        public bool Visible = true;

        public Light(Vector3 position, LightType type, float lightPower, Color color)
        {
            Position = position;
            Type = type;
            LightPower = lightPower;
            Color = color;
        }

        public Light(Vector3 position, LightType type, float lightPower, Color color, Vector3 attenuation)
        {
            Position = position;
            Type = type;
            LightPower = lightPower;
            Color = color;
            Attenuation = attenuation;
        }

        public Light(Vector3 position, LightType type, float lightPower, Color color, Vector3 attenuation, Vector3 direction)
        {
            Position = position;
            Direction = direction;
            Type = type;
            LightPower = lightPower;
            Color = color;
            Attenuation = attenuation;
        }
    }
}
