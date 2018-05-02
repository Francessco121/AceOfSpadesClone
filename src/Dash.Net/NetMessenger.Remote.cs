using System;
using System.Collections.Concurrent;
using System.Net;

/* NetMessenger.Remote.cs
 * Ethan Lafrenais
*/

namespace Dash.Net
{
    #region Delegates
    /// <summary>
    /// Represents a remote event.
    /// </summary>
    /// <param name="callingConnection">The connection that fired this event.</param>
    /// <param name="data">The data sent with the event.</param>
    /// <param name="numArgs">The number of values in the data sent with the event.</param>
    public delegate void RemoteEvent(NetConnection callingConnection, NetInboundPacket data, ushort numArgs);
    /// <summary>
    /// Represents a remote function.
    /// </summary>
    /// <param name="callingConnection">The connection that fired this event.</param>
    /// <param name="data">The data sent with the function.</param>
    /// <param name="numArgs">The number of values in the data sent with the function.</param>
    public delegate NetBuffer RemoteFunction(NetConnection callingConnection, NetInboundPacket data, ushort numArgs);

    /// <summary>
    /// Represents the callback of a called remote function.
    /// </summary>
    /// <param name="invokedConnection">The NetConnection invoked.</param>
    /// <param name="data">The return value in a buffer.</param>
    public delegate void RemoteFunctionCallback(NetConnection invokedConnection, NetInboundPacket data);
    #endregion

    /// <summary>
    /// Remote call settings.
    /// </summary>
    [Flags]
    public enum RemoteFlag
    {
        /// <summary>
        /// Do nothing extra to the packet.
        /// </summary>
        None = 0,
        /// <summary>
        /// Encrypt the remote's packet.
        /// <para>For functions, the return value will also be encrypted.</para>
        /// </summary>
        Encrypt = 1,
        /// <summary>
        /// Forces the packet to not be compressed.
        /// </summary>
        DontCompress = 2,
        /// <summary>
        /// Forces the packet to be sent immediately.
        /// </summary>
        SendImmediately = 4,
    }

    public abstract partial class NetMessenger
    {
        struct InboundRemote
        {
            public NetInboundPacket Packet;
            public RemoteChannelBase Channel;

            public InboundRemote(NetInboundPacket packet, RemoteChannelBase channel)
            {
                Packet = packet;
                Channel = channel;
            }
        }

        internal ConcurrentDictionary<ushort, RemoteChannel> Channels { get; private set; }
        internal ConcurrentDictionary<ushort, StateRemoteChannel> StateChannels { get; private set; }

        public CoreRemoteChannel GlobalChannel { get; private set; }
        internal CoreRemoteChannel HiddenChannel { get; private set; }

        ConcurrentQueue<InboundRemote> inboundRemotes;

        RemoteChannelBase GetRemoteChannelFromPacket(RemoteChannelType type, ushort channelId)
        {
            RemoteChannel channel;
            StateRemoteChannel stateChannel;
            if (type == RemoteChannelType.Core)
                return (channelId == GlobalChannel.Id) ? GlobalChannel : HiddenChannel;
            else if (type == RemoteChannelType.State)
            {
                if (!StateChannels.TryGetValue(channelId, out stateChannel))
                    NetLogger.LogError("Failed to retreive state channel of id {0}, it doesn't exist!", channelId);

                return stateChannel;
            }
            else
            {
                if (!Channels.TryGetValue(channelId, out channel))
                    NetLogger.LogError("Failed to retreive channel of id {0}, it doesn't exist!", channelId);

                return channel;
            }
        }

        void HandleRemotePacket(NetInboundPacket packet)
        {
            // Read the remote header
            RemoteChannelType type = (RemoteChannelType)packet.ReadByte();
            ushort channelId = packet.ReadUInt16();

            // Attempt to locate the channel
            RemoteChannelBase channel = GetRemoteChannelFromPacket(type, channelId);
            if (channel != null)
                inboundRemotes.Enqueue(new InboundRemote(packet, channel));
        }

        void ProcessInboundRemotes()
        {
            for (int i = 0; i < 100 && inboundRemotes.Count > 0; i++)
            {
                InboundRemote remote;
                if (inboundRemotes.TryDequeue(out remote))
                {
                    if (remote.Packet.Type == NetPacketType.RemoteEvent)
                        HandleRemoteEvent(remote.Packet, remote.Channel);
                    else if (remote.Packet.Type == NetPacketType.RemoteFunction)
                        HandleRemoteFunction(remote.Packet, remote.Channel);
                    else if (remote.Packet.Type == NetPacketType.RemoteFunctionResponse)
                        HandleRemoteFunctionResponse(remote.Packet, remote.Channel);
                }
            }
        }

        void HandleRemoteEvent(NetInboundPacket packet, RemoteChannelBase channel)
        {
            // Attempt to locate the event
            string eventName = packet.ReadString();
            RemoteEvent evt;
            if (channel.Events.TryGetValue(eventName, out evt))
            {
                // Call the event
                ushort numArgs = packet.ReadUInt16();
                evt(packet.Sender, packet, numArgs);
            }
            else
                NetLogger.LogError("Remote Event \"{0}\" was fired on a {1} channel with the id {2}, but it doesn't exist!",
                    eventName, channel.Type, channel.Id);
        }

        void HandleRemoteFunction(NetInboundPacket packet, RemoteChannelBase channel)
        {
            // Attempt to locate the function
            string funcName = packet.ReadString();
            RemoteFunction func;
            if (channel.Functions.TryGetValue(funcName, out func))
            {
                // Get the callback id
                ushort callbackId = packet.ReadUInt16();

                // Call the event
                ushort numArgs = packet.ReadUInt16();
                NetBuffer returnValue = func(packet.Sender, packet, numArgs);

                // Send the response
                NetOutboundPacket funcResponse = new NetOutboundPacket(NetDeliveryMethod.Reliable);
                funcResponse.Type = NetPacketType.RemoteFunctionResponse;
                funcResponse.Encrypt = packet.isEncrypted;

                // Write the normal remote header
                funcResponse.Write((byte)channel.Type);
                funcResponse.Write(channel.Id);
                funcResponse.Write(funcName);
                funcResponse.Write(callbackId);

                // Write the return value
                funcResponse.WriteBytes(returnValue.data, 0, returnValue.Length);

                // Send the response
                packet.Sender.SendPacket(funcResponse);
            }
            else
                NetLogger.LogError("Remote Function \"{0}\" was invoked on a {1} channel with the id {2}, but it doesn't exist!",
                    funcName, channel.Type, channel.Id);
        }

        void HandleRemoteFunctionResponse(NetInboundPacket packet, RemoteChannelBase channel)
        {
            // Attempt to locate the function callback
            string funcName = packet.ReadString();
            ushort callbackId = packet.ReadUInt16();
            RemoteFunctionCallback callback;
            if (channel.WaitingFunctions.TryGetValue(callbackId, out callback))
            {
                // Call the callback (hehe)
                callback(packet.Sender, packet);
                // Remove the callback from the waiting list
                channel.WaitingFunctions.TryRemove(callbackId, out callback);
            }
            else
                NetLogger.LogError(
                    "Received a Remote Function Response for function {0} on channel {1}, but we do not have a callback!",
                    funcName, channel.Id);
        }

        #region Getting/Adding/Removing Channels
        /// <summary>
        /// Adds and Creates RemoteChannel.
        /// </summary>
        /// <param name="channel">The channel to add.</param>
        public RemoteChannel CreateChannel(ushort id)
        {
            if (Channels.ContainsKey(id))
                throw new NetException("Failed to create RemoteChannel, the channel with the id {0} already exists!", id);

            RemoteChannel channel = new RemoteChannel(this, id);
            Channels.TryAdd(channel.Id, channel);
            return channel;
        }

        /// <summary>
        /// Removes a RemoteChannel.
        /// </summary>
        /// <param name="channel">The channel to remove.</param>
        public void RemoveChannel(RemoteChannel channel)
        {
            Channels.TryRemove(channel.Id, out channel);
        }

        /// <summary>
        /// Removes a RemoteChannel.
        /// </summary>
        /// <param name="id">The channel to remove's id</param>
        public void RemoveChannel(ushort id)
        {
            RemoteChannel temp;
            Channels.TryRemove(id, out temp);
        }

        /// <summary>
        /// Attempts to retrieve a RemoteChannel.
        /// </summary>
        /// <param name="id">The id of the channel.</param>
        /// <returns>The RemoteChannel if found, otherwise null.</returns>
        public RemoteChannel GetChannel(ushort id)
        {
            RemoteChannel channel;
            if (Channels.TryGetValue(id, out channel))
                return channel;
            else
                return null;

        }
        #endregion
    }
}
