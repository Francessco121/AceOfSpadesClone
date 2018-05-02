/* ValueAnim.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Animation
{
    public abstract class ValueAnim<T>
    {
        public T Value { get; protected set; }
        public T Start { get; protected set; }
        public T Target { get; protected set; }
        public float I { get; protected set; }

        public ValueAnim()
        {
            I = 1;
        }

        public virtual void SetTarget(T target)
        {
            Start = Value;
            Target = target;
            I = 0;
        }

        public virtual void SnapTo(T value)
        {
            Value = value;
            Target = value;
            I = 1;
        }
            
        public abstract void Step(float deltaTime);
    }
}
