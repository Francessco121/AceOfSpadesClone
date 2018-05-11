using AceOfSpades.Net;
using Dash.Net;

/* NetConnectionSnapshotState.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class NetConnectionSnapshotState
    {
        public NetConnection Connection { get; }
        public SnapshotStats Stats { get; }

        public ushort OutboundSnapshotId;
        public ushort LastInboundSnapshotId;

        //public float TimeSinceLastSend;
        //public bool GotPacket = true; // Server will attempt to initiate the snapshot relaying

        public WorldSnapshot WorldSnapshot { get; }
        public ushort SnapshotId;

        public float RTT_TimeSinceLastSend;
        public float RoundTripTime;
        public bool MeasuringRTT;

        public bool Ready;

        public NetConnectionSnapshotState(SnapshotSystem snapshotSystem, NetConnection conn)
        {
            WorldSnapshot = new WorldSnapshot(snapshotSystem, conn);
            Connection = conn;
            Stats = new SnapshotStats();
        }
    }
}
