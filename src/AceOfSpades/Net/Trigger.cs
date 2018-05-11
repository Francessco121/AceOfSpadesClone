using Dash.Engine;

/* Trigger.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    /// <summary>
    /// This is my trigger.
    /// </summary>
    public class Trigger
    {
        public byte Iterations;

        public void Activate(byte iterations = 1)
        {
            Iterations = (byte)MathHelper.Clamp(Iterations + iterations, 0, 255);
        }
    }
}
