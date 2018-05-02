using Dash.Engine;
using System;

namespace AceOfSpades.Net
{
    /// <summary>
    /// Represents a client-fired bullet. Contains timing information
    /// that the server can use for rolling back other players to
    /// ensure the bullet is accurate to how the client saw it.
    /// </summary>
    public class NetworkBullet
    {
        public Vector3 Origin { get; }
        public float CameraYaw { get; }
        public float CameraPitch { get; }
        public int Ticks { get; }
        public int CreatedAt { get; }

        public NetworkBullet(Vector3 origin, float yaw, float pitch)
        {
            Origin = origin;
            CameraYaw = yaw;
            CameraPitch = pitch;
            Ticks = Environment.TickCount;
            CreatedAt = Environment.TickCount;
        }

        public NetworkBullet(Vector3 origin, float yaw, float pitch, int ticks)
        {
            Origin = origin;
            CameraYaw = yaw;
            CameraPitch = pitch;
            Ticks = ticks;
            CreatedAt = Environment.TickCount;
        }
    }
}
