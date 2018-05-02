using System;
using System.IO;

/* OggFile.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Audio
{
    public class OggFile : AudioBuffer
    {
        public OggFile(string filePath)
            : this(File.OpenRead(filePath))
        { }

        public OggFile(Stream stream)
        {
            byte[] data;
            ALFormat format;
            uint sampleRate;
            TimeSpan length;

            AL.Utils.LoadOgg(stream, out data, out format, out sampleRate, out length);

            GenerateBuffer(data, format, sampleRate, (float)length.TotalSeconds);

            stream.Dispose();
        }
    }
}
