using Dash.Net;

namespace AceOfSpades.Net
{
    public class NetworkPlayerSnapshot : Snapshot
    {
        public NetworkPlayer NetPlayer { get; }

        public Team Team
        {
            get { return (Team)teamField.Value; }
            set { teamField.Value = (byte)value; }
        }

        public short Score
        {
            get { return (short)scoreField.Value; }
            set { scoreField.Value = value; }
        }

        public ushort Ping
        {
            get { return (ushort)pingField.Value; }
            set { pingField.Value = value; }
        }

        public ushort? CharacterId
        {
            get
            {
                if ((bool)hasCharacterField.Value)
                    return (ushort)characterIdField.Value;
                else
                    return null;
            }

            set
            {
                if (value == null)
                {
                    characterIdField.Value = (ushort)0;
                    hasCharacterField.Value = false;
                }
                else
                {
                    hasCharacterField.Value = true;
                    characterIdField.Value = value.Value;
                }
            }
        }

        SnapshotField teamField;
        SnapshotField scoreField;
        SnapshotField pingField;
        SnapshotField characterIdField;
        SnapshotField hasCharacterField;

        public NetworkPlayerSnapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection, NetworkPlayer netPlayer,
            bool dontAllocateId = false, bool dontAwait = false) 
            : base(snapshotSystem, otherConnection, dontAllocateId, dontAwait)
        {
            NetPlayer = netPlayer;

            teamField = AddPrimitiveField<byte>((byte)Team.None);
            scoreField = AddPrimitiveField<short>();
            pingField = AddPrimitiveField<ushort>();
            characterIdField = AddPrimitiveField<ushort>();
            hasCharacterField = AddPrimitiveField<bool>();

            Setup();
            EnableDeltaCompression(4);
        }

        public override string GetUniqueId()
        {
            return string.Format("NetworkPlayer_{0}", NetPlayer.Id);
        }

        public override void Serialize(NetBuffer buffer)
        {
            // Auto update snapshot
            Team = NetPlayer.Team;
            Score = (short)NetPlayer.Score;
            CharacterId = NetPlayer.CharacterId;
            Ping = (ushort)NetPlayer.Ping;

            base.Serialize(buffer);
        }

        public override void Deserialize(NetBuffer buffer)
        {
            base.Deserialize(buffer);

            // Auto update netplayer
            NetPlayer.Team = Team;
            NetPlayer.Score = Score;
            NetPlayer.CharacterId = CharacterId;
            NetPlayer.Ping = Ping;
        }
    }
}
