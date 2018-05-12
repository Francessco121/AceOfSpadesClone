using Dash.Net;

/* GameVersion.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public enum GameVersionPhase : byte
    {
        PreAlpha,
        Alpha,
        Beta,
        ReleaseCandidate,
        Release
    }

    /// <summary>
    /// Represents a version of the application.
    /// Ex. Beta 4.5c
    /// </summary>
    public class GameVersion
    {
        public static readonly GameVersion Current = new GameVersion(GameVersionPhase.Alpha, 4, 0);

        public GameVersionPhase Phase { get; }
        public uint Major { get; }
        public uint Minor { get; }
        public char? Revision  { get; }

        public GameVersion(GameVersionPhase phase, uint major, uint minor, char? rev = null)
        {
            Phase = phase;
            Major = major;
            Minor = minor;
            Revision = rev;
        }

        public void Serialize(NetBuffer buffer)
        {
            Serialize(this, buffer);
        }

        public static void Serialize(GameVersion version, NetBuffer buffer)
        {
            buffer.Write((byte)version.Phase);
            buffer.Write(version.Major);
            buffer.Write(version.Minor);
            buffer.Write(version.Revision.HasValue);
            if (version.Revision.HasValue)
                buffer.Write(version.Revision.Value);
        }

        public static GameVersion Deserialize(NetBuffer buffer)
        {
            GameVersionPhase phase = (GameVersionPhase)buffer.ReadByte();
            uint major = buffer.ReadUInt32();
            uint minor = buffer.ReadUInt32();
            char? rev = null;
            if (buffer.ReadBool())
                rev = buffer.ReadChar();

            return new GameVersion(phase, major, minor, rev);
        }

        public override bool Equals(object obj)
        {
            GameVersion other = obj as GameVersion;
            if (other == null)
                return false;
            else
            {
                return Phase == other.Phase
                    && Major == other.Major
                    && Minor == other.Minor
                    && Revision == other.Revision;
            }
        }

        public override int GetHashCode()
        {
            return (int)(Major * 100 + Minor * 10 + (Revision.HasValue ? Revision.Value : 0)) 
                * (int)(Phase + 1);
        }

        public override string ToString()
        {
            string phase;
            switch (Phase)
            {
                case GameVersionPhase.PreAlpha:
                    phase = "Pre-alpha";
                    break;
                case GameVersionPhase.ReleaseCandidate:
                    phase = "RC";
                    break;
                default:
                    phase = Phase.ToString();
                    break;
            }

            return string.Format("{0} {1}.{2}{3}", phase, Major, Minor, 
                Revision.HasValue ? Revision.Value.ToString() : "");
        }
    }
}
