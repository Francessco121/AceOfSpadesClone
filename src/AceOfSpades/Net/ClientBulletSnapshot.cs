using Dash.Engine;
using Dash.Net;
using System;
using System.Collections.Generic;

namespace AceOfSpades.Net
{
    /// <summary>
    /// Synchronizes client-fired bullets, essentially allows clients
    /// to have control over firing bullets but the server still
    /// being authoritative over them.
    /// </summary>
    public class ClientBulletSnapshot : CustomSnapshot
    {
        List<NetworkBullet> bullets;

        public ClientBulletSnapshot()
        {
            bullets = new List<NetworkBullet>();
        }

        public void EnqueueBullet(NetworkBullet bullet)
        {
            bullets.Add(bullet);
        }

        public NetworkBullet[] GetBullets()
        {
            return bullets.ToArray();
        }

        protected override void OnSerialize(NetBuffer buffer)
        {
            buffer.Write((ushort)bullets.Count);

            for (int i = 0; i < bullets.Count; i++)
            {
                NetworkBullet bullet = bullets[i];
                buffer.Write(bullet.Origin.X);
                buffer.Write(bullet.Origin.Y);
                buffer.Write(bullet.Origin.Z);
                buffer.Write(bullet.CameraYaw);
                buffer.Write(bullet.CameraPitch);
                buffer.Write((ushort)(Environment.TickCount - bullet.Ticks));
            }

            bullets.Clear();
        }

        protected override void OnDeserialize(NetBuffer buffer)
        {
            bullets.Clear();

            int numBullets = buffer.ReadUInt16();
            for (int i = 0; i < numBullets; i++)
            {
                float x = buffer.ReadFloat();
                float y = buffer.ReadFloat();
                float z = buffer.ReadFloat();

                float camYaw = buffer.ReadFloat();
                float camPitch = buffer.ReadFloat();

                int tickDeltaTime = buffer.ReadUInt16();

                bullets.Add(new NetworkBullet(new Vector3(x, y, z), camYaw, camPitch, tickDeltaTime));
            }
        }
    }
}
