using Dash.Engine.Audio;
using System;

namespace AceOfSpades
{
    public class WorldAudioSource : IDisposable
    {
        readonly AudioSource audioSource;

        bool isDisposed;

        public WorldAudioSource(AudioSource audioSource)
        {
            this.audioSource = audioSource;
        }

        public void Play()
        {
            AL.Utils.CheckError();

            audioSource.Play();
        }

        public bool IsDone()
        {
            return audioSource.State == ALSourceState.Stopped;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                audioSource.Dispose();
            }
        }
    }
}
