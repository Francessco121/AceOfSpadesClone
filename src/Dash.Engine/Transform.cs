/* Transform.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public class Transform
    {
        public Matrix4 Matrix
        {
            get { return Matrix4.CreateTranslation(Position); }
        }

        public Vector3 Position;

        public GameObject GameObject { get; }

        public Transform(GameObject gameObject)
        {
            GameObject = gameObject;
        }
    }
}
