using System.IO;

namespace AceOfSpades.IO
{
    public interface IWorldFileIO
    {
        void Save(Stream stream, WorldDescription desc);
        WorldDescription Load(Stream stream);
    }
}
