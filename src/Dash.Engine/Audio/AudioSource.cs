using System;

/* AudioSource.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Audio
{
    /// <summary>
    /// An AudioSource references an audio buffer and contains transformation data
    /// for this specific source, the single audio buffer lets multiple positions
    /// play the same sound without duplicating audio data.
    /// </summary>
    public class AudioSource : IDisposable
    {
        public int SourceId { get; }
        public AudioBuffer Buffer { get; }

        #region Properties
        /// <summary>
        /// Pitch multipler, always positive.
        /// Default: 1.0
        /// </summary>
        public float Pitch
        {
            get { return pitch; }
            set
            {
                pitch = value;
                AL.Source(SourceId, ALSourcef.Pitch, value);
            }
        }
        /// <summary>
        /// Source gain, always positive.
        /// Default: 1.0
        /// </summary>
        public float Gain
        {
            get { return gain; }
            set
            {
                gain = value;
                AL.Source(SourceId, ALSourcef.Gain, value);
            }
        }
        /// <summary>
        /// The maximum gain for this source.
        /// Default: 1.0
        /// </summary>
        public float MaxGain
        {
            get { return maxGain; }
            set
            {
                maxGain = value;
                AL.Source(SourceId, ALSourcef.MaxGain, value);
            }
        }
        /// <summary>
        /// The minimum gain for this source.
        /// Default: 0.0
        /// </summary>
        public float MinGain
        {
            get { return minGain; }
            set
            {
                minGain = value;
                AL.Source(SourceId, ALSourcef.MinGain, value);
            }
        }
        /// <summary>
        /// Used with the Inverse Clamped Distance Model
        /// to set the distance where there will no longer be
        /// any attenuation of the source.
        /// Default: float.MaxValue
        /// </summary>
        public float MaxDistance
        {
            get { return maxDist; }
            set
            {
                maxDist = value;
                AL.Source(SourceId, ALSourcef.MaxDistance, value);
            }
        }
        /// <summary>
        /// The rolloff rate for the source.
        /// Default: 1.0
        /// </summary>
        public float RolloffFactor
        {
            get { return rolloffFactor; }
            set
            {
                rolloffFactor = value;
                AL.Source(SourceId, ALSourcef.RolloffFactor, value);
            }
        }
        /// <summary>
        /// The distance under which the volume for the
        /// source would normally drop by half (before
        /// being influenced by RolloffFactor or
        /// MaxDistance).
        /// Default: 1.0
        /// </summary>
        public float ReferenceDistance
        {
            get { return refDist; }
            set
            {
                refDist = value;
                AL.Source(SourceId, ALSourcef.ReferenceDistance, value);
            }
        }
        /// <summary>
        /// The gain when outside the oriented cone.
        /// Default: 0.0
        /// </summary>
        public float ConeOuterGain
        {
            get { return coneOuterGain; }
            set
            {
                coneOuterGain = value;
                AL.Source(SourceId, ALSourcef.ConeOuterGain, value);
            }
        }
        /// <summary>
        /// The gain when inside the oriented cone. ??
        /// Default: 360
        /// </summary>
        public float ConeInnerAngle
        {
            get { return coneInnerAngle; }
            set
            {
                coneInnerAngle = value;
                AL.Source(SourceId, ALSourcef.ConeInnerAngle, value);
            }
        }
        /// <summary>
        /// Outer angle of the sound cone, in degrees.
        /// Default: 360
        /// </summary>
        public float ConeOuterAngle
        {
            get { return coneOuterAngle; }
            set
            {
                coneOuterAngle = value;
                AL.Source(SourceId, ALSourcef.ConeOuterAngle, value);
            }
        }

        /// <summary>
        /// Position of the source.
        /// Default: Vector3.Zero
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                AL.Source(SourceId, ALSource3f.Position, value.X, value.Y, value.Z);
            }
        }
        /// <summary>
        /// Velocity of the source.
        /// Default: Vector3.Zero
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
            set
            {
                velocity = value;
                AL.Source(SourceId, ALSource3f.Velocity, value.X, value.Y, value.Z);
            }
        }
        /// <summary>
        /// Direction of the source.
        /// Default: Vector3.Zero
        /// </summary>
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                AL.Source(SourceId, ALSource3f.Direction, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// Determines if the positions are relative to the
        /// listener.
        /// Default: false
        /// </summary>
        public bool IsSourceRelative
        {
            get { return isSourceRelative; }
            set
            {
                isSourceRelative = value;
                AL.Source(SourceId, ALSourceb.SourceRelative, value);
            }
        }
        /// <summary>
        /// Determines if the sound loops.
        /// Default: false
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
            set
            {
                isLooping = value;
                AL.Source(SourceId, ALSourceb.Looping, value);
            }
        }

        /// <summary>
        /// Get the current state of the source. 
        /// (Paused, Playing, Stopped, etc.)
        /// </summary>
        public ALSourceState State
        {
            get
            {
                int state;
                AL.GetSource(SourceId, ALGetSourcei.SourceState, out state);
                return (ALSourceState)state;
            }
        }

        float pitch = 1;
        float gain = 1;
        float maxDist = float.MaxValue;
        float rolloffFactor = 1;
        float refDist = 1;
        float minGain = 0;
        float maxGain = 1;
        float coneOuterGain = 0;
        float coneInnerAngle = 360;
        float coneOuterAngle = 360;

        Vector3 position;
        Vector3 velocity;
        Vector3 direction;

        bool isSourceRelative = false;
        bool isLooping = false;
        #endregion

        public AudioSource(AudioBuffer buffer)
        {
            if (buffer.Data == null)
                throw new InvalidOperationException("Cannot use audio buffer for source, the buffer has not been setup!");

            Buffer = buffer;

            // Generate source buffer
            SourceId = AL.GenSource();

            // Setup source
            AL.Source(SourceId, ALSourcei.Buffer, buffer.BufferId);

            AL.Utils.CheckError();
        }

        /// <summary>
        /// Plays/Resumes the source.
        /// </summary>
        public void Play()
        {
            AL.SourcePlay(SourceId);
        }

        /// <summary>
        /// Pauses the source.
        /// </summary>
        public void Pause()
        {
            AL.SourcePause(SourceId);
        }

        /// <summary>
        /// Stops the source, resetting the playback
        /// position to zero.
        /// </summary>
        public void Stop()
        {
            AL.SourceStop(SourceId);
        }

        /// <summary>
        /// Rewinds the source.
        /// </summary>
        public void Rewind()
        {
            AL.SourceRewind(SourceId);
        }

        public void Dispose()
        {
            AL.DeleteSource(SourceId);
        }
    }
}
