using AceOfSpades.Net;
using Dash.Engine.Diagnostics;
using Dash.Net;
using System;
using System.Collections.Generic;

/* (Client)ObjectNetComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public delegate INetCreatable NetInstantiationCallback(ushort id, bool isAppOwner, NetBuffer data);

    public class ObjectNetComponent : NetComponent
    {
        public event EventHandler<NetCreatableInfo> OnCreatableInstantiated;
        public event EventHandler<NetCreatableInfo> OnCreatableDestroyed;

        public bool HoldAllPackets
        {
            get { return HoldInstantiationPackets && HoldDestroyPackets; }
            set
            {
                HoldInstantiationPackets = value;
                HoldDestroyPackets = value;
            }
        }
        public bool HoldInstantiationPackets;
        public bool HoldDestroyPackets;

        Queue<NetInboundPacket> heldInstantiationPackets;
        Queue<NetInboundPacket> heldDestroyPackets;

        Dictionary<string, NetInstantiationCallback> instCallbacks;
        NetCreatableCollection netObjects;
        SnapshotNetComponent snapshotComponent;

        public ObjectNetComponent(AOSClient client) 
            : base(client)
        {
            instCallbacks = new Dictionary<string, NetInstantiationCallback>();
            netObjects    = new NetCreatableCollection();

            heldInstantiationPackets = new Queue<NetInboundPacket>();
            heldDestroyPackets       = new Queue<NetInboundPacket>();
        }

        public override void Initialize()
        {
            snapshotComponent = client.GetComponent<SnapshotNetComponent>();
            snapshotComponent.OnWorldSnapshotInbound += SnapshotComponent_OnWorldSnapshotInbound;
            base.Initialize();
        }

        private void SnapshotComponent_OnWorldSnapshotInbound(object sender, WorldSnapshot e)
        {
            foreach (NetCreatableInfo info in netObjects.Entities.Values)
            {
                NetEntitySnapshot snapshot;
                if (snapshotComponent.WorldSnapshot.NetEntityListSnapshot.TryGetEntitySnapshot(info.Id, out snapshot))
                {
                    INetEntity entity = info.Creatable as INetEntity;
                    entity.OnClientInbound(snapshot);
                }
            }
        }

        public void AddInstantiationEvent(string name, NetInstantiationCallback callback)
        {
            instCallbacks.Add(name, callback);
        }

        public bool RemoveInstantiationEvent(string name)
        {
            return instCallbacks.Remove(name);
        }

        public override void OnDisconnected(NetConnection connection, string reason, bool lostConnection)
        {
            netObjects.Clear();

            base.OnDisconnected(connection, reason, lostConnection);
        }

        public override bool HandlePacket(NetInboundPacket packet, CustomPacketType type)
        {
            if (type == CustomPacketType.Instantiate)
            {
                if (HoldInstantiationPackets)
                    heldInstantiationPackets.Enqueue(packet);
                else
                    HandleInstantiationPacket(packet);
            }
            else if (type == CustomPacketType.Destroy)
            {
                if (HoldDestroyPackets)
                    heldDestroyPackets.Enqueue(packet);
                else
                    HandleDestroyPacket(packet);
            }
            else
                return false;

            return true;
        }

        public override void Update(float deltaTime)
        {
            if (!HoldInstantiationPackets)
            {
                while (heldInstantiationPackets.Count > 0)
                    HandleInstantiationPacket(heldInstantiationPackets.Dequeue());
            }

            if (!HoldDestroyPackets)
            {
                while (heldDestroyPackets.Count > 0)
                    HandleDestroyPacket(heldDestroyPackets.Dequeue());
            }

            base.Update(deltaTime);
        }

        void HandleInstantiationPacket(NetInboundPacket packet)
        {
            string eventName = packet.ReadString();
            ushort id = packet.ReadUInt16();
            bool isOwner = packet.ReadBool();

            NetInstantiationCallback callback;
            if (instCallbacks.TryGetValue(eventName, out callback))
            {
                if (netObjects.Creatables.ContainsKey(id))
                {
                    DashCMD.WriteError("[ObjectNC] Creatable with id {0} is already instantiated!", id);
                    return;
                }

                //DashCMD.WriteLine("[ObjectNC] Instantiating creatable with id {0}...", id);

                INetCreatable creatable = callback(id, isOwner, packet);
                NetCreatableInfo info = new NetCreatableInfo(packet.Sender, creatable, id, isOwner);
                netObjects.Add(id, info);

                INetEntity entity = creatable as INetEntity;
                if (entity != null && snapshotComponent.WorldSnapshot != null)
                {
                    NetEntityListSnapshot entList = snapshotComponent.WorldSnapshot.NetEntityListSnapshot;
                    entList.AddNetEntity(info, entity);
                }

                creatable.OnNetworkInstantiated(info);

                if (OnCreatableInstantiated != null)
                    OnCreatableInstantiated(this, info);
            }
            else
                DashCMD.WriteError("[ObjectNC] Received instantiation for unknown type: {0}", eventName);
        }

        void HandleDestroyPacket(NetInboundPacket packet)
        {
            ushort id = packet.ReadUInt16();
            NetCreatableInfo creatableInfo;
            if (netObjects.Creatables.TryGetValue(id, out creatableInfo))
            {
                //DashCMD.WriteLine("[ObjectNC] Destroying creatable with id {0}...", id);

                NetEntityListSnapshot entList = snapshotComponent.WorldSnapshot.NetEntityListSnapshot;
                entList.RemoveNetEntitiy(id);

                netObjects.Remove(id);
                creatableInfo.Creatable.OnNetworkDestroy();

                if (OnCreatableDestroyed != null)
                    OnCreatableDestroyed(this, creatableInfo);
            }
            else
                DashCMD.WriteError("[ObjectNC] Received destroy for unknown creatable, id: {0}", id);
        }
    }
}
