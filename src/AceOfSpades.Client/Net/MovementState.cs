using Dash.Net;

/* MovementState.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class MovementState
    {
        public bool Sprint { get; private set; }
        public bool Crouch { get; private set; }
        public bool Walk { get; private set; }
        public bool Aiming { get; private set; }
        public bool Jump { get; private set; }
        public bool MoveForward { get; private set; }
        public bool MoveBackward { get; private set; }
        public bool MoveLeft { get; private set; }
        public bool MoveRight { get; private set; }

        public void FromByteFlag(ByteFlag movementFlag, bool isAiming)
        {
            Sprint = movementFlag.Get(0);
            Crouch = movementFlag.Get(1);
            Jump = movementFlag.Get(2);
            MoveForward = movementFlag.Get(3);
            MoveBackward = movementFlag.Get(4);
            MoveLeft = movementFlag.Get(5);
            MoveRight = movementFlag.Get(6);
            Walk = movementFlag.Get(7);
            Aiming = isAiming;
        }
    }
}
