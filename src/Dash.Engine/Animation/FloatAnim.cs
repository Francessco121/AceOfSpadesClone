/* FloatAnim.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Animation
{
    public class FloatAnim : ValueAnim<float>
    {
        public override void Step(float deltaTime)
        {
            I = MathHelper.Clamp(I + deltaTime, 0, 1);
            Value = Interpolation.Linear(Start, Target, I);
        }
    }
}
