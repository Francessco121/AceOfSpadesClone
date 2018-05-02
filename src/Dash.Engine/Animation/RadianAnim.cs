/* RadianAnim.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Animation
{
    public class RadianAnim : ValueAnim<float>
    {
        public float DegreeValue { get { return MathHelper.ToDegrees(Value); } }

        public override void Step(float deltaTime)
        {
            I = MathHelper.Clamp(I + deltaTime, 0, 1);
            Value = Interpolation.LerpRadians(Start, Target, I);
        }
    }
}
