using Dash.Engine;
using Dash.Engine.Physics;

namespace AceOfSpades.Client.Net
{
    public class FakeServerPlayer : GameObject
    {
        public PhysicsBodyComponent PhysicsBody { get; }

        public FakeServerPlayer(Vector3 position, Vector3 size, float mass)
            : base(position)
        {
            PhysicsBody = new PhysicsBodyComponent(size, mass);
            AddComponent(PhysicsBody);
        }
    }
}
