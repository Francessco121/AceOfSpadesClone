using System;
using System.Collections.Concurrent;
using System.Net;

namespace Dash.Net
{
    enum RemoteChannelType : byte
    {
        Normal = 0, State = 1, Core = 2
    }

    public sealed class CoreRemoteChannel : RemoteChannelBase
    {
        internal CoreRemoteChannel(NetMessenger messenger, ushort id)
            : base(id, RemoteChannelType.Core, messenger)
        { }
    }

    public sealed class StateRemoteChannel : RemoteChannelBase
    {
        internal StateRemoteChannel(NetMessenger messenger, ushort id)
            : base(id, RemoteChannelType.State, messenger)
        { }
    }

    public sealed class RemoteChannel : RemoteChannelBase
    {
        internal RemoteChannel(NetMessenger messenger, ushort id)
            : base(id, RemoteChannelType.Normal, messenger)
        { }
    }

    public abstract class RemoteChannelBase
    {
        ushort nextCallbackId;
        ushort GetNextCallbackId()
        {
            ushort id;
            ushort start = nextCallbackId;
            do
            {
                id = nextCallbackId++;
                if (nextCallbackId == start)
                    throw new NetException(
                        "Failed to allocate callback id, there are too many! How do you pull that off?!?");
            } while (WaitingFunctions.ContainsKey(id));
            return id;
        }

        /// <summary>
        /// This remote channels Id
        /// </summary>
        public ushort Id { get; private set; }
        /// <summary>
        /// The NetMessenger associated with this channel
        /// </summary>
        public NetMessenger Messenger { get; private set; }
        /// <summary>
        /// All of this channels events
        /// </summary>
        public ConcurrentDictionary<string, RemoteEvent> Events { get; private set; }
        /// <summary>
        /// All of this channels functions
        /// </summary>
        public ConcurrentDictionary<string, RemoteFunction> Functions { get; private set; }

        internal ConcurrentDictionary<ushort, RemoteFunctionCallback> WaitingFunctions { get; private set; }
        internal RemoteChannelType Type { get; private set; }

        internal RemoteChannelBase(ushort id, RemoteChannelType type, NetMessenger messenger)
        {
            this.Id = id;
            this.Type = type;
            this.Messenger = messenger;

            Events = new ConcurrentDictionary<string, RemoteEvent>();
            Functions = new ConcurrentDictionary<string, RemoteFunction>();
            WaitingFunctions = new ConcurrentDictionary<ushort, RemoteFunctionCallback>();
        }

        #region Adding Remotes
        /// <summary>
        /// Adds a remote event.
        /// </summary>
        /// <param name="eventName">The name of the event to use.</param>
        /// <param name="evt">The RemoteEvent delegate method to add.</param>
        public void AddRemoteEvent(string eventName, RemoteEvent evt)
        {
            if (Events.ContainsKey(eventName))
                throw new InvalidOperationException(String.Format("Event \"{0}\" already exists!", eventName));

            Events.TryAdd(eventName, evt);
        }

        /// <summary>
        /// Adds a remote function.
        /// </summary>
        /// <param name="funcName">The name of the function.</param>
        /// <param name="function">The RemoteFunction delegate to add.</param>
        public void AddRemoteFunction(string funcName, RemoteFunction function)
        {
            if (Events.ContainsKey(funcName))
                throw new InvalidOperationException(String.Format("Function \"{0}\" already exists!", funcName));

            Functions.TryAdd(funcName, function);
        }
        #endregion

        #region Removing Remotes
        /// <summary>
        /// Removes a remote event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public bool RemoveRemoteEvent(string eventName)
        {
            RemoteEvent temp;
            return Events.TryRemove(eventName, out temp);
        }

        /// <summary>
        /// <para>Not Recommended.</para>
        /// Removes a remote function.
        /// </summary>
        /// <param name="funcName">The name of the function.</param>
        public bool RemoveRemoteFunction(string funcName)
        {
            RemoteFunction temp;
            return Functions.TryRemove(funcName, out temp);
        }
        #endregion

        #region FireEvent
        public void FireEventForAllConnections(string eventName, params object[] args)
        {
            FireEventForAllConnections(eventName, RemoteFlag.None, NetDeliveryMethod.Reliable, args);
        }

        public void FireEventForAllConnections(string eventName, RemoteFlag flags, params object[] args)
        {
            FireEventForAllConnections(eventName, flags, NetDeliveryMethod.Reliable, args);
        }

        public void FireEventForAllConnections(string eventName, NetBuffer data)
        {
            FireEventForAllConnections(eventName, RemoteFlag.None, NetDeliveryMethod.Reliable, data);
        }

        public void FireEventForAllConnections(string eventName, RemoteFlag flags, NetBuffer data)
        {
            FireEventForAllConnections(eventName, flags, NetDeliveryMethod.Reliable, data);
        }

        public void FireEventForAllConnections(string eventName, RemoteFlag flags, NetDeliveryMethod deliveryMethod,
            params object[] args)
        {
            foreach (NetConnection connection in Messenger.Connections.Values)
                FireEvent(eventName, connection, flags, deliveryMethod, args);
        }

        public void FireEventForAllConnections(string eventName, RemoteFlag flags, NetDeliveryMethod deliveryMethod,
            NetBuffer data)
        {
            foreach (NetConnection connection in Messenger.Connections.Values)
                FireEvent(eventName, connection, flags, deliveryMethod, data);
        }

        /// <summary>
        /// Fires an event for a connection.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="onConnection">The connection to fire the event for.</param>
        /// <param name="args">Any data associated with the event to be handled by the other application.</param>
        public void FireEvent(string eventName, NetConnection onConnection, params object[] args)
        {
            FireEvent(eventName, onConnection, RemoteFlag.None, NetDeliveryMethod.Reliable, args);
        }

        /// <summary>
        /// Fires an event for a connection.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="onConnection">The connection to fire the event for.</param>
        /// <param name="data">Data to write to the event call.</param>
        public void FireEvent(string eventName, NetConnection onConnection, NetBuffer data)
        {
            FireEvent(eventName, onConnection, RemoteFlag.None, NetDeliveryMethod.Reliable, data);
        }

        /// <summary>
        /// Fires an event for a connection.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="onConnection">The connection to fire the event for.</param>
        /// <param name="flags">Flags for the packet settings.</param>
        /// <param name="args">Any data associated with the event to be handled by the other application.</param>
        public void FireEvent(string eventName, NetConnection onConnection, RemoteFlag flags, params object[] args)
        {
            FireEvent(eventName, onConnection, flags, NetDeliveryMethod.Reliable, args);
        }

        /// <summary>
        /// Fires an event for a connection.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="onConnection">The connection to fire the event for.</param>
        /// <param name="flags">Flags for the packet settings.</param>
        /// <param name="data">Data to write to the event call.</param>
        public void FireEvent(string eventName, NetConnection onConnection, RemoteFlag flags, NetBuffer data)
        {
            FireEvent(eventName, onConnection, flags, NetDeliveryMethod.Reliable, data);
        }

        /// <summary>
        /// Fires an event for a connection.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="onConnection">The connection to fire the event for.</param>
        /// <param name="flags">Flags for the packet settings.</param>
        /// <param name="deliveryMethod">The way this packet will reach the connection.</param>
        /// <param name="args">Any data associated with the event to be handled by the other application.</param>
        public void FireEvent(string eventName, NetConnection onConnection, RemoteFlag flags,
            NetDeliveryMethod deliveryMethod, params object[] args)
        {
            NetBuffer data = new NetBuffer();
            // Write the data for the event
            for (int i = 0; i < args.Length; i++)
                data.WriteDynamic(args[i]);

            FireEvent(eventName, onConnection, flags, deliveryMethod, data, (ushort)args.Length);
        }

        /// <summary>
        /// Fires an event for a connection.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="onConnection">The connection to fire the event for.</param>
        /// <param name="flags">Flags for the packet settings.</param>
        /// <param name="deliveryMethod">The way this packet will reach the connection.</param>
        /// <param name="data">Data to write to the event call.</param>
        public void FireEvent(string eventName, NetConnection onConnection, RemoteFlag flags,
            NetDeliveryMethod deliveryMethod, NetBuffer data)
        {
            FireEvent(eventName, onConnection, flags, deliveryMethod, data, 0);
        }

        void FireEvent(string eventName, NetConnection onConnection, RemoteFlag flags,
            NetDeliveryMethod deliveryMethod, NetBuffer data, ushort numArgs)
        {
            // Create the event packet
            NetOutboundPacket firePacket = new NetOutboundPacket(deliveryMethod);
            firePacket.Type = NetPacketType.RemoteEvent;

            // Handle the flags
            firePacket.Encrypt = flags.HasFlag(RemoteFlag.Encrypt);
            if (flags.HasFlag(RemoteFlag.DontCompress))
                firePacket.Compression = NetPacketCompression.None;
            firePacket.SendImmediately = flags.HasFlag(RemoteFlag.SendImmediately);

            // Write the event header
            firePacket.Write((byte)Type);
            firePacket.Write(Id);
            firePacket.Write(eventName);
            firePacket.Write(numArgs);

            // Write the data for the event
            firePacket.WriteBytes(data.data, 0, data.Length);

            // Send the event packet
            onConnection.SendPacket(firePacket);
        }
        #endregion

        #region CallFunction
        /// <summary>
        /// Calls a function for a connection.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="onConnection">The connection to call the function for.</param>
        /// <param name="callback">The callback for the return value, sent by the called connection.</param>
        /// <param name="args">Any data associated with the function to be handled by the other application.</param>
        public bool CallFunction(string functionName, NetConnection onConnection, RemoteFunctionCallback callback,
            params object[] args)
        {
            return CallFunction(functionName, onConnection, callback, RemoteFlag.None, NetDeliveryMethod.Reliable, args);
        }

        /// <summary>
        /// Calls a function for a connection.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="onConnection">The connection to call the function for.</param>
        /// <param name="callback">The callback for the return value, sent by the called connection.</param>
        /// <param name="flags">Flags for the packet settings.</param>
        /// <param name="args">Any data associated with the function to be handled by the other application.</param>
        public bool CallFunction(string functionName, NetConnection onConnection, RemoteFunctionCallback callback,
            RemoteFlag flags, params object[] args)
        {
            return CallFunction(functionName, onConnection, callback, flags, NetDeliveryMethod.Reliable, args);
        }

        /// <summary>
        /// Calls a function for a connection.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="onConnection">The connection to call the function for.</param>
        /// <param name="callback">The callback for the return value, sent by the called connection.</param>
        /// <param name="args">Any data associated with the function to be handled by the other application.</param>
        public bool CallFunction(string functionName, NetConnection onConnection, RemoteFunctionCallback callback,
            NetBuffer data)
        {
            return CallFunction(functionName, onConnection, callback, RemoteFlag.None, NetDeliveryMethod.Reliable, data);
        }

        /// <summary>
        /// Calls a function for a connection.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="onConnection">The connection to call the function for.</param>
        /// <param name="callback">The callback for the return value, sent by the called connection.</param>
        /// <param name="flags">Flags for the packet settings.</param>
        /// <param name="args">Any data associated with the function to be handled by the other application.</param>
        public bool CallFunction(string functionName, NetConnection onConnection, RemoteFunctionCallback callback,
            RemoteFlag flags, NetBuffer data)
        {
            return CallFunction(functionName, onConnection, callback, flags, NetDeliveryMethod.Reliable, data);
        }

        /// <summary>
        /// Calls a function for a connection.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="onConnection">The connection to call the function for.</param>
        /// <param name="callback">The callback for the return value, sent by the called connection.</param>
        /// <param name="flags">Flags for the packet settings.</param>
        /// <param name="deliveryMethod">The way this packet will reach the connection.</param>
        /// <param name="args">Any data associated with the function to be handled by the other application.</param>
        public bool CallFunction(string functionName, NetConnection onConnection, RemoteFunctionCallback callback,
            RemoteFlag flags, NetDeliveryMethod deliveryMethod, params object[] args)
        {
            NetBuffer data = new NetBuffer();
            // Write the data for the function
            for (int i = 0; i < args.Length; i++)
                data.WriteDynamic(args[i]);

            return CallFunc(functionName, onConnection, callback, flags, deliveryMethod, data, (ushort)args.Length);
        }

        /// <summary>
        /// Calls a function for a connection.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="onConnection">The connection to call the function for.</param>
        /// <param name="callback">The callback for the return value, sent by the called connection.</param>
        /// <param name="flags">Flags for the packet settings.</param>
        /// <param name="deliveryMethod">The way this packet will reach the connection.</param>
        /// <param name="args">Any data associated with the function to be handled by the other application.</param>
        public bool CallFunction(string functionName, NetConnection onConnection, RemoteFunctionCallback callback,
            RemoteFlag flags, NetDeliveryMethod deliveryMethod, NetBuffer data)
        {
            return CallFunc(functionName, onConnection, callback, flags, deliveryMethod, data, 0);
        }

        bool CallFunc(string functionName, NetConnection onConnection, RemoteFunctionCallback callback,
            RemoteFlag flags, NetDeliveryMethod deliveryMethod, NetBuffer data, ushort numArgs)
        {
            // Allocate Id
            ushort callbackId = GetNextCallbackId();

            // Create the function packet
            NetOutboundPacket funcPacket = new NetOutboundPacket(deliveryMethod);
            funcPacket.Type = NetPacketType.RemoteFunction;

            // Handle the flags
            funcPacket.Encrypt = flags.HasFlag(RemoteFlag.Encrypt);
            if (flags.HasFlag(RemoteFlag.DontCompress))
                funcPacket.Compression = NetPacketCompression.None;
            funcPacket.SendImmediately = flags.HasFlag(RemoteFlag.SendImmediately);

            // Write the function header
            funcPacket.Write((byte)Type);
            funcPacket.Write(Id);
            funcPacket.Write(functionName);
            funcPacket.Write(callbackId);
            funcPacket.Write(numArgs);

            // Write the data for the function
            funcPacket.WriteBytes(data.data, 0, data.Length);

            // Add the waiting function for the return value callback
            if (WaitingFunctions.TryAdd(callbackId, callback))
            {
                // Send the event packet
                onConnection.SendPacket(funcPacket);
                return true;
            }
            else
            {
                NetLogger.LogError("Failed to call function {0}, this function is already in the middle of being called!",
                    functionName);
                return false;
            }
        }
        #endregion
    }
}
