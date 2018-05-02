using System;

/* NetMessengerConfig.cs
 * Author: Ethan Lafrenais
 * Last Update: 11/23/15
*/

namespace Dash.Net
{
    /// <summary>
    /// Represents the configuration for a NetMessenger.
    /// </summary>
    public abstract class NetMessengerConfig
    {
        /// <summary>
        /// Will this messenger automatically compress packets over the size of the CompressPacketAfter value?
        /// <para>Default: true</para>
        /// </summary>
        public bool AutoCompressPackets = true;
        /// <summary>
        /// The minimum size (in bytes) of a packet to be automatically compressed.
        /// <para>Minimum: 41, Default: 100</para>
        /// </summary>
        public int CompressPacketAfter
        {
            get { return compressPacketAfter; }
            set { compressPacketAfter = Math.Max(41, value); }
        }
        int compressPacketAfter = 100;

        /// <summary>
        /// Amount of time in milliseconds a connected messenger has to not respond before being disconnected.
        /// <para>Default: 10,000</para>
        /// </summary>
        public int ConnectionTimeout = 10000;

        #region Lag Simulation
        /// <summary>
        /// Will this messenger simulate latency with the SimulatedLatencyAmount variable?
        /// <para>Default: false</para>
        /// </summary>
        public bool SimulateLatency = false;
        /// <summary>
        /// In milliseconds, the delay in the sending of packets to a connection.
        /// </summary>
        public int SimulatedLatencyAmount = 0;

        /// <summary>
        /// Will this messenger simulate packet loss (in received messages)?
        /// <para>Default: false</para>
        /// </summary>
        public bool SimulatePacketLoss = false;
        /// <summary>
        /// From 0 to 1, the chance of packet loss in received messages.
        /// <para>Default: 0.1</para>
        /// </summary>
        public float SimulatedPacketLossChance = 0.1f;

        public bool DontApplyPingControl;
        #endregion

        #region Internal
        internal int PingDelay = 1500;
        internal int AckAwaitDelay = 1000;
        internal int MaxReliableSendAttempts = 20;
        internal int MaxMessageIdCache = 400;
        #endregion
    }

    /// <summary>
    /// Represents the configuration for a NetServer.
    /// </summary>
    public sealed class NetServerConfig : NetMessengerConfig
    {
        /// <summary>
        /// The maxmimum amount of connected clients.
        /// <para>Default: 12</para>
        /// </summary>
        public int MaxConnections = 12;

        /// <summary>
        /// The required password to connect to this server.
        /// <para>If null, no password is required.</para>
        /// <para>Default: null</para>
        /// </summary>
        public string Password = null;
    }

    /// <summary>
    /// Represents the configuration for a NetClient.
    /// </summary>
    public sealed class NetClientConfig : NetMessengerConfig
    {
        /// <summary>
        /// Max attempts to connect to a server.
        /// <para>Default: 3</para>
        /// </summary>
        public int MaxConnectionAttempts = 3;

        /// <summary>
        /// Maximum amount of time (in milliseconds) to wait for
        /// a connection to be completed with a server.
        /// </summary>
        public int ConnectionAttemptTimeout = 5000;
    }
}
