using Dash.Engine.Audio;
using System;

namespace AceOfSpades
{
    public class WorldAudioSource : IDisposable
    {
        readonly AudioSource audioSource;

        public WorldAudioSource(AudioSource audioSource)
        {
            this.audioSource = audioSource;

            audioSource.Play();
        }

        public bool IsDone()
        {
            return audioSource.State == ALSourceState.Stopped;
        }

        public void Dispose()
        {
            audioSource.Dispose();
        }
    }
}
