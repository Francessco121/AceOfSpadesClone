using Dash.Engine;
using Dash.Engine.Audio;
using System;
using System.Collections.Generic;
using System.IO;

namespace AceOfSpades
{
    public class CyclicAudioSource : IDisposable
    {
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;

                if (!value)
                {
                    iterationTime = 0;
                }
            }
        }

        public float IterationLength { get; set; }

        readonly List<AudioSource> audioSources;

        bool isPlaying;

        float iterationTime;
        int i = 0;
        AudioSource currentSource;

        public CyclicAudioSource(string baseFilePath, int maxIterations, float iterationLength,
            bool relative = true, float maxDistance = 100f)
        {
            IterationLength = iterationLength;

            audioSources = new List<AudioSource>();

            string ext = Path.GetExtension(baseFilePath);
            string dir = Path.GetDirectoryName(baseFilePath);
            string fileName = Path.GetFileNameWithoutExtension(baseFilePath);

            for (int i = 0; i < maxIterations; i++)
            {
                AudioBuffer buffer = AssetManager.LoadSound($"{dir}/{fileName}{i + 1}{ext}");

                if (buffer != null)
                {
                    AudioSource source = new AudioSource(buffer);
                    source.IsSourceRelative = relative;
                    source.Gain = 0.4f;
                    source.MaxDistance = relative ? float.MaxValue : maxDistance;

                    audioSources.Add(source);
                }
            }
        }

        public void Update(float deltaTime, Vector3? position = null)
        {
            if (audioSources.Count == 0)
                return;

            if (currentSource == null || iterationTime <= 0)
            {
                if (IsPlaying)
                {
                    currentSource = audioSources[i++];

                    if (position.HasValue && currentSource.Position != position.Value)
                        currentSource.Position = position.Value;

                    currentSource.Play();

                    iterationTime += IterationLength;

                    if (i == audioSources.Count)
                        i = 0;
                }
            }
            else if (currentSource != null && iterationTime > 0)
            {
                iterationTime -= deltaTime;

                if (position.HasValue && currentSource.Position != position.Value)
                    currentSource.Position = position.Value;
            }
        }

        public void Dispose()
        {
            foreach (AudioSource source in audioSources)
                source.Dispose();
        }
    }
}
