/* AudioListener.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Audio
{
    public class AudioListener
    {
        public static AudioListener Active { get; private set; }

        public bool IsActive { get { return this == Active; } }

        /// <summary>
        /// Position of the listener.
        /// Default: Vector3.Zero
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                SetPosition();
            }
        }
        /// <summary>
        /// Velocity of the listener.
        /// Default: Vector3.Zero
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
            set
            {
                velocity = value;
                SetVelocity();
            }
        }
        /// <summary>
        /// At (forward) orientation of the listener.
        /// Default: Vector3.Forward
        /// </summary>
        public Vector3 OrientationAt
        {
            get { return orientationAt; }
        }
        /// <summary>
        /// Up orientation of the listener.
        /// Default: Vector3.Up
        /// </summary>
        public Vector3 OrientationUp
        {
            get { return orientationUp; }
        }

        /// <summary>
        /// Master gain. Should always be positive.
        /// Default: 1.0
        /// </summary>
        public float Gain
        {
            get { return gain; }
            set
            {
                gain = value;
                SetGain();
            }
        }
        /// <summary>
        /// Default: 1.0
        /// </summary>
        public float EfxMetersPerUnit
        {
            get { return efxMetersPerUnit; }
            set
            {
                efxMetersPerUnit = value;
                SetEfxMetersPerUnit();
            }
        }

        Vector3 position = Vector3.Zero;
        Vector3 velocity = Vector3.Zero;
        Vector3 orientationAt = Vector3.Forward;
        Vector3 orientationUp = Vector3.Up;
        float gain = 1;
        float efxMetersPerUnit = 1;

        float[] orientationBuffer = new float[6];

        public AudioListener() { }
        public AudioListener(Vector3 position)
        {
            Position = position;
        }

        public void MakeActive()
        {
            Active = this;

            SetPosition();
            SetVelocity();
            SetOrientation();
            SetGain();
            SetEfxMetersPerUnit();
        }

        public void SetOrientation(Vector3 at, Vector3 up)
        {
            orientationAt = at;
            orientationUp = up;

            SetOrientation();
        }

        void SetPosition()
        {
            if (IsActive)
                AL.Listener(ALListener3f.Position, position.X, position.Y, position.Z);
        }
        void SetVelocity()
        {
            if (IsActive)
                AL.Listener(ALListener3f.Velocity, velocity.X, velocity.Y, velocity.Z);
        }
        void SetOrientation()
        {
            if (IsActive)
            {
                orientationBuffer[0] = orientationAt.X;
                orientationBuffer[1] = orientationAt.Y;
                orientationBuffer[2] = orientationAt.Z;

                orientationBuffer[3] = orientationUp.X;
                orientationBuffer[4] = orientationUp.Y;
                orientationBuffer[5] = orientationUp.Z;

                AL.Listener(ALListenerfv.Orientation, ref orientationBuffer);
            }
        }
        void SetGain()
        {
            if (IsActive)
                AL.Listener(ALListenerf.Gain, gain);
        }
        void SetEfxMetersPerUnit()
        {
            if (IsActive)
                AL.Listener(ALListenerf.EfxMetersPerUnit, efxMetersPerUnit);
        }
    }
}

