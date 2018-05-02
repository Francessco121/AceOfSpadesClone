using System;
using System.Runtime.InteropServices;

/* AudioBuffer.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Audio
{
    /// <summary>
    /// Represents audio data.
    /// </summary>
    public abstract class AudioBuffer : IDisposable
    {
        public int BufferId { get; private set; }

        public byte[] Data { get; private set; }
        public ALFormat Format { get; private set; }
        public uint SampleRate { get; private set; }
        public float LengthSeconds { get; private set; }

        protected void GenerateBuffer(byte[] data, ALFormat format, uint sampleRate, float lengthSeconds)
        {
            if (data == null || data.Length == 0)
                throw new InvalidOperationException("Cannot generate sound buffer, no data has been set!");

            LengthSeconds = lengthSeconds;

            // Generate Sound Buffer
            BufferId = AL.GenBuffer();

            // Send data to sound card
            GCHandle data_ptr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try { AL.BufferData(BufferId, format, data_ptr.AddrOfPinnedObject(), data.Length, (int)sampleRate); }
            finally { data_ptr.Free(); }

            Data = data;
            Format = format;
            SampleRate = sampleRate;

            AL.Utils.CheckError();
        }

        public void Dispose()
        {
            AL.DeleteBuffer(BufferId);
        }
    }
}
