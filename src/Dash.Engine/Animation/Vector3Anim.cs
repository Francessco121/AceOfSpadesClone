/* Vector3Anim.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Animation
{
    public class Vector3Anim : ValueAnim<Vector3>
    {
        public override void Step(float deltaTime)
        {
            I = MathHelper.Clamp(I + deltaTime, 0, 1);
            Value = Interpolation.Lerp(Start, Target, I);
        }
    }
}
