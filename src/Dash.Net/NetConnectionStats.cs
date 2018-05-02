/* NetConnectionStats.cs
 * Author: Ethan Lafrenais
 * Last Update: 9/13/15
*/

namespace Dash.Net
{
    /// <summary>
    /// Represents the current statistics for a NetConnection.
    /// </summary>
    public class NetConnectionStats
    {
        /// <summary>
        /// The number of packets sent to this connection.
        /// </summary>
        public long PacketsSent { get; internal set; }
        /// <summary>
        /// The number of packets sent to this connection.
        /// </summary>
        public long PhysicalPacketsSent { get; internal set; }
        /// <summary>
        /// The number of physical packets sent to this connection a second. 
        /// (doesn't count cunked packets individual pieces).
        /// </summary>
        public int PhysicalPacketsSentPerSecond { get; internal set; }
        /// <summary>
        /// The "real" number of packets sent to this connection a second.
        /// (counts the insides of chunked packets).
        /// </summary>
        public int PacketsSentPerSecond { get; internal set; }
        /// <summary>
        /// The number of packets received from this connection.
        /// </summary>
        public long PacketsReceived { get; internal set; }
        /// <summary>
        /// The number of packets received from this connection.
        /// </summary>
        public long PhysicalPacketsReceived { get; internal set; }
        /// <summary>
        /// The number of packets received from this connection a second.
        /// (does not count the insides of chunked packets).
        /// </summary>
        public int PhysicalPacketsReceivedPerSecond { get; internal set; }
        /// <summary>
        /// The number of packets "actually" received from this connection a second.
        /// (counts the insides of chunked packets).
        /// </summary>
        public int PacketsReceivedPerSecond { get; internal set; }
        /// <summary>
        /// The number of packets lost, being sent to this connection.
        /// </summary>
        public int PacketsLost { get; internal set; }
        /// <summary>
        /// The highest the ping has ever been to this connection.
        /// </summary>
        public int HighestPing { get; internal set; }
        /// <summary>
        /// The current ping to this connection.
        /// </summary>
        public int Ping { get; internal set; }
        /// <summary>
        /// The lowest the ping has ever been to this connection.
        /// </summary>
        public int LowestPing { get; internal set; }
        /// <summary>
        /// The current Maximum Transmission Unit for this connection. (Max packet size).
        /// </summary>
        public int MTU { get; internal set; }

        public NetConnectionStats()
        {
            HighestPing = int.MinValue;
            LowestPing = int.MaxValue;
        }
    }
}
