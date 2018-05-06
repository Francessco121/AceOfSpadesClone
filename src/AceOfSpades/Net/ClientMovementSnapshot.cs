using Dash.Net;
using System.Collections.Generic;

namespace AceOfSpades.Net
{
    public class ClientMovementSnapshot : CustomSnapshot
    {
        public IReadOnlyList<NetworkClientMovement> Movements => movements;

        List<NetworkClientMovement> movements;
        NetworkClientMovement bufferedMovement;

        public ClientMovementSnapshot()
        {
            movements = new List<NetworkClientMovement>();
        }

        public NetworkClientMovement EnqueueMovement(NetworkClientMovement movement, float deltaTime)
        {
            NetworkClientMovement addedMovement = null;

            if (bufferedMovement != null)
            {
                bufferedMovement.Length = deltaTime;
                movements.Add(bufferedMovement);

                addedMovement = bufferedMovement;
            }

            bufferedMovement = movement;

            return addedMovement;
        }

        protected override void OnSerialize(NetBuffer buffer)
        {
            buffer.Write((ushort)movements.Count);

            for (int i = 0; i < movements.Count; i++)
            {
                NetworkClientMovement movement = movements[i];

                ByteFlag movementFlag = new ByteFlag();
                movementFlag.Set(0, movement.Sprint);
                movementFlag.Set(1, movement.Crouch);
                movementFlag.Set(2, movement.Jump);
                movementFlag.Set(3, movement.MoveForward);
                movementFlag.Set(4, movement.MoveBackward);
                movementFlag.Set(5, movement.MoveLeft);
                movementFlag.Set(6, movement.MoveRight);
                movementFlag.Set(7, movement.Walk);

                buffer.Write(movementFlag);

                buffer.Write(movement.CameraYaw);
                buffer.Write(movement.CameraPitch);

                buffer.Write(movement.Length);
                buffer.Write(movement.Sequence);
            }

            movements.Clear();
        }

        protected override void OnDeserialize(NetBuffer buffer)
        {
            movements.Clear();

            int numMovements = buffer.ReadUInt16();
            for (int i = 0; i < numMovements; i++)
            {
                NetworkClientMovement movement = new NetworkClientMovement();

                ByteFlag movementFlag = buffer.ReadByteFlag();
                movement.Sprint = movementFlag.Get(0);
                movement.Crouch = movementFlag.Get(1);
                movement.Jump = movementFlag.Get(2);
                movement.MoveForward = movementFlag.Get(3);
                movement.MoveBackward = movementFlag.Get(4);
                movement.MoveLeft = movementFlag.Get(5);
                movement.MoveRight = movementFlag.Get(6);
                movement.Walk = movementFlag.Get(7);

                movement.CameraYaw = buffer.ReadFloat();
                movement.CameraPitch = buffer.ReadFloat();

                movement.Length = buffer.ReadFloat();
                movement.Sequence = buffer.ReadUInt16();

                movements.Add(movement);
            }
        }
    }
}
