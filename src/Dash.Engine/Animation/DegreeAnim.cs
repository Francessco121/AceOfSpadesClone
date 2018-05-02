/* DegreeAnim.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Animation
{
    public class DegreeAnim : ValueAnim<float>
    {
        public float RadianValue { get { return MathHelper.ToRadians(Value); } }

        public override void Step(float deltaTime)
        {
            I = MathHelper.Clamp(I + deltaTime, 0, 1);
            Value = Interpolation.LerpDegrees(Start, Target, I);
        }
    }
}
