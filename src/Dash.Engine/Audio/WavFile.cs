using System.IO;

namespace Dash.Engine.Audio
{
    public class WavFile : AudioBuffer
    {
        public uint AverageBytesPerSecond { get; }
        public uint ChunkSize { get; }
        public short BytesPerSample { get; }
        public short BitsPerSample { get; }

        public WavFile(string filePath)
            : this(File.OpenRead(filePath))
        { }

        public WavFile(Stream stream)
        {
            byte[] data;
            ALFormat format;
            uint sampleRate;
            uint averageBytesPerSecond;
            uint chunkSize;
            short bytesPerSample;
            short bitsPerSample;

            AL.Utils.LoadWavExt(stream, out data, out chunkSize, out format, out sampleRate, 
                out averageBytesPerSecond, out bytesPerSample, out bitsPerSample);

            GenerateBuffer(data, format, sampleRate, data.Length / (float)bytesPerSample);

            ChunkSize = chunkSize;
            AverageBytesPerSecond = averageBytesPerSecond;
            BytesPerSample = bytesPerSample;
            BitsPerSample = bitsPerSample;

            stream.Dispose();
        }
    }
}
