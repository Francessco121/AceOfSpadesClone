/* DeltaSnapshot.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Physics
{
    public class DeltaSnapshot
    {
        public Vector3 FinalPosition;
        public Vector3 StepPosition;
        public Vector3 FinalVelocity;
        public int DeltaPass;
        public bool IsGrounded;
        public float MaxStep;
        public bool Stepped;
        //public bool CanStep;
        //public float StepDist;
        //public HashSet<ICollider> Intersections;

        public DeltaSnapshot()
        {
            //Intersections = new HashSet<ICollider>();
        }

        public void Reset()
        {
            DeltaPass = 0;
            MaxStep = float.MaxValue;
            IsGrounded = false;
            //CanStep = false;
            //StepDist = 0;
            //Intersections.Clear();
        }
    }
}
