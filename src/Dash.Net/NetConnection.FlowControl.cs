using System;

/* NetConnection.FlowControl.cs
 * Author: Ethan Lafrenais
 * Last Update: 12/6/15
*/

namespace Dash.Net
{
    public sealed partial class NetConnection
    {
        enum MTUStatus
        {
            /// <summary>
            /// Not yet found
            /// </summary>
            Unset = 0,
            /// <summary>
            /// Finding MTU
            /// </summary>
            Setting,
            /// <summary>
            /// MTU is current
            /// </summary>
            Set
        }

        public event EventHandler<int> OnMTUSet;
        internal bool MTUEventNeedsCall;

        /// <summary>
        /// Gets the current maximum packet size that can be sent to this connection.
        /// </summary>
        internal int MTU;

        const int MaxUDP_MTU = 5000;
        const int MTU_UnsetTry_Delay = 500;
        const int MTU_SetTry_Delay = 5000;
        const float MTU_TryInc = 1.25f;
        const float MTU_TryDec = 0.75f;

        internal bool MTUSet = false;
        MTUStatus mtu_status = MTUStatus.Unset;

        float nextMTUTest = 0;
        int lastMTUTest = 512;

        int avgPingI;
        int[] recentPings = new int[5];
        int lastAvgPing;
        int baseAvgPing = -1;

        readonly int[] normalSendRate = new int[] { 12, 79 }; // ~ 60 packets a second
        readonly int[] droppedSendRate = new int[] { 25, 39 }; // ~ 40 packets a second
        bool sendRateDropped;
        const int sendRateSwitchCooldownDelay = 5000;
        int sendRateSwitchCooldown;

        internal void RecalculateMTU()
        {
            lastMTUTest = (int)(lastMTUTest * MTU_TryDec);
            mtu_status = MTUStatus.Setting;
        }

        void FlowControlHeartbeat(int now)
        {
            MTUHeartbeat(now);
        }

        void RecalculateBasePing()
        {
            baseAvgPing = -1;
        }

        void AddRecentPing(int ping)
        {
            // Add the new ping
            recentPings[avgPingI++] = ping;

            // If we've reached the end of one ping collection
            if (avgPingI == recentPings.Length)
            {
                // Average the pings we collected
                AverageRecentPings();
                avgPingI = 0;

                // If we are calculating the base ping as well, set it
                if (baseAvgPing == -1)
                {
                    if (NetLogger.LogFlowControl)
                        NetLogger.LogVerbose("[FlowControl] Base Ping for {0} set to {1}", EndPoint, lastAvgPing);
                    baseAvgPing = lastAvgPing;
                }
                else if (NetTime.Now - sendRateSwitchCooldown >= 0)
                {
                    // If the ping increased too much, drop the send rate
                    if (lastAvgPing - baseAvgPing > 80)
                    {
                        sendRateDropped = true;
                        if (!config.DontApplyPingControl)
                        {
                            if (!NetLogger.IgnoreSendRateChanges)
                                NetLogger.LogWarning("[FlowControl] Dropped send rate for {0}", EndPoint);
                            chunkSendDelay = droppedSendRate[0];
                            PacketSendRate = droppedSendRate[1];
                        }
                        sendRateSwitchCooldown = NetTime.Now + sendRateSwitchCooldownDelay;
                    }
                    // If the ping returned to normal, try increasing the send rate
                    else if (sendRateDropped && lastAvgPing - baseAvgPing < 80)
                    {
                        sendRateDropped = false;
                        if (!config.DontApplyPingControl)
                        {
                            if (!NetLogger.IgnoreSendRateChanges)
                                NetLogger.Log("[FlowControl] Send rate set to normal for {0}", EndPoint);
                            chunkSendDelay = normalSendRate[0];
                            PacketSendRate = normalSendRate[1];
                        }
                        sendRateSwitchCooldown = NetTime.Now + sendRateSwitchCooldownDelay;
                    }
                    // If the average ping dropped, see if we can lower the base ping
                    else if (!sendRateDropped && lastAvgPing - baseAvgPing < -20)
                    {
                        if (NetLogger.LogFlowControl)
                            NetLogger.LogVerbose("[FlowControl] Attempting to increase base ping for {0}...", EndPoint);
                        RecalculateBasePing();
                    }
                }
            }
        }

        internal void FireMTUEvent()
        {
            if (OnMTUSet != null)
                OnMTUSet(this, MTU);
        }

        void AverageRecentPings()
        {
            int t = 0;
            for (int i = 0; i < recentPings.Length; i++)
                t += recentPings[i];
            lastAvgPing = t / recentPings.Length;
        }

        void MTUHeartbeat(int now)
        {
            if (mtu_status == MTUStatus.Unset)
            {
                // Prep next MTU test
                mtu_status = MTUStatus.Setting;
                nextMTUTest = now + MTU_UnsetTry_Delay + 1.5f + Stats.Ping;
                if (NetLogger.LogFlowControl)
                    NetLogger.LogVerbose("[FlowControl] Expanding MTU for {0}...", EndPoint);
            }
            else if (now >= nextMTUTest)
            {
                if (mtu_status == MTUStatus.Setting)
                {
                    // Try and send an MTU packet
                    if (lastMTUTest < NetMessenger.MAX_UDP_PACKET_SIZE && TrySendMTU(lastMTUTest))
                    {
                        // If it succeeds then continue
                        lastMTUTest = (int)(lastMTUTest * MTU_TryInc);
                        now += MTU_UnsetTry_Delay;
                    }
                    else
                    {
                        // If it fails, set the MTU and stop trying to expand it
                        if (lastMTUTest > NetMessenger.MAX_UDP_PACKET_SIZE)
                            lastMTUTest = NetMessenger.MAX_UDP_PACKET_SIZE;

                        // Use 75% of the MTU as the max chunk size, and cap it at 5000 bytes.
                        maxChunkSize = (int)Math.Min(MaxUDP_MTU, lastMTUTest * 0.75f);
                        MTU = maxChunkSize;
                        Stats.MTU = MTU;

                        MTUEventNeedsCall = true;

                        if (NetLogger.LogFlowControl)
                            NetLogger.LogVerbose("[FlowControl] MTU for {0} set to {1}", EndPoint, MTU);
                        mtu_status = MTUStatus.Set;
                        now += MTU_SetTry_Delay;
                    }
                }
                // No need.
                /*else if (status == MTUStatus.Set)
                {
                    int tryExpandAmount = Math.Min((int)(lastMTUTest * MTU_TryInc), MaxUDP_MTU);

                    if (TrySendMTU(tryExpandAmount))
                    {
                        lastMTUTest = (int)(lastMTUTest * MTU_TryInc);
                        now += MTU_UnsetTry_Delay;
                        MTU_Status = MTUStatus.Setting;
                        NetLogger.LogVerbose("Expanding MTU further for {0}...", EndPoint);
                    }
                    else
                        now += MTU_SetTry_Delay;
                }*/
            }
        }

        bool TrySendMTU(int size)
        {
            NetOutboundPacket msg = new NetOutboundPacket(NetDeliveryMethod.Unreliable);
            msg.Type = NetPacketType.MTUTest;
            msg.SendImmediately = true;
            msg.WriteBytes(new byte[size]);

            // Have to set the header manually since this isn't technically a legit packet
            msg.SetId(messenger.AllocatePacketId());
            msg.PrependHeader();

            return messenger.SendDataToSocket(msg.data, EndPoint, true);
        }
    }
}
