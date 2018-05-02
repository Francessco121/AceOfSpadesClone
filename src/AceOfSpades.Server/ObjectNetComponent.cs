using AceOfSpades.Net;
using Dash.Net;
using System;
using System.Collections.Generic;

/* (Server)ObjectNetComponent.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public class ObjectNetComponent : NetComponent
    {
        public event EventHandler<NetCreatableInfo> OnCreatableInstantiated;
        public event EventHandler<NetCreatableInfo> OnCreatableDestroyed;

        Dictionary<ushort, NetOutboundPacket> instPackets;
        NetCreatableCollection netObjects;
        SnapshotNetComponent snapshotComponent;

        ushort lastNetEntId = 1;

        public ObjectNetComponent(AOSServer server) 
            : base(server)
        {
            netObjects  = new NetCreatableCollection();
            instPackets = new Dictionary<ushort, NetOutboundPacket>();
        }

        public override void Initialize()
        {
            snapshotComponent = server.GetComponent<SnapshotNetComponent>();
            snapshotComponent.OnWorldSnapshotOutbound += SnapshotComponent_OnWorldSnapshotOutbound;
            base.Initialize();
        }

        private void SnapshotComponent_OnWorldSnapshotOutbound(object sender, WorldSnapshot e)
        {
            foreach (NetCreatableInfo info in netObjects.Entities.Values)
            {
                foreach (NetConnectionSnapshotState clientState in snapshotComponent.ConnectionStates.Values)
                {
                    INetEntity entity = (INetEntity)info.Creatable;
                    NetEntitySnapshot snapshot;
                    if (clientState.WorldSnapshot.NetEntityListSnapshot.TryGetEntitySnapshot(info.Id, out snapshot))
                        entity.OnServerOutbound(snapshot);
                }
            }
        }

        public void NetworkInstantiate(INetCreatable creatable, string instEventName,
            NetConnection clientOwner, params object[] args)
        {
            ushort netId = lastNetEntId++;
            if (netId == 0)
                netId++;

            NetCreatableInfo info = new NetCreatableInfo(clientOwner, creatable, netId, true);

            INetEntity entity = creatable as INetEntity;
            if (entity != null)
            {
                foreach (NetConnectionSnapshotState state in snapshotComponent.ConnectionStates.Values)
                    state.WorldSnapshot.NetEntityListSnapshot.AddNetEntity(info, entity);
            }

            creatable.OnNetworkInstantiated(info);

            if (OnCreatableInstantiated != null)
                OnCreatableInstantiated(this, info);

            netObjects.Add(netId, info);

            foreach (NetConnection conn in server.Connections.Values)
            {
                NetOutboundPacket packet = new NetOutboundPacket(NetDeliveryMethod.Reliable);
                packet.Write((byte)CustomPacketType.Instantiate);
                packet.Write(instEventName);
                packet.Write(netId);
                packet.Write(conn == clientOwner);

                for (int i = 0; i < args.Length; i++)
                    packet.WriteDynamic(args[i]);

                conn.SendPacket(packet);
            }

            NetOutboundPacket epacket = new NetOutboundPacket(NetDeliveryMethod.Reliable);
            epacket.Write((byte)CustomPacketType.Instantiate);
            epacket.Write(instEventName);
            epacket.Write(netId);
            epacket.Write(false);

            for (int i = 0; i < args.Length; i++)
                epacket.WriteDynamic(args[i]);

            instPackets.Add(netId, epacket);
        }

        public void NetworkDestroy(ushort id)
        {
            NetCreatableInfo creatableInfo;
            if (netObjects.Creatables.TryGetValue(id, out creatableInfo))
            {
                foreach (NetConnectionSnapshotState state in snapshotComponent.ConnectionStates.Values)
                    state.WorldSnapshot.NetEntityListSnapshot.RemoveNetEntitiy(id);

                netObjects.Remove(id);
                creatableInfo.Creatable.OnNetworkDestroy();

                if (OnCreatableDestroyed != null)
                    OnCreatableDestroyed(this, creatableInfo);
            }

            instPackets.Remove(id);

            NetOutboundPacket packet = new NetOutboundPacket(NetDeliveryMethod.Reliable);
            packet.Write((byte)CustomPacketType.Destroy);
            packet.Write(id);

            server.SendPacketToAll(packet);
        }

        public void SendInstantiationPackets(NetConnection to)
        {
            foreach (NetOutboundPacket packet in instPackets.Values)
                to.SendPacket(packet.Clone());

            foreach (NetCreatableInfo info in netObjects.Entities.Values)
            {
                NetConnectionSnapshotState state = snapshotComponent.ConnectionStates[to];
                state.WorldSnapshot.NetEntityListSnapshot.AddNetEntity(info, (INetEntity)info.Creatable);
            }
        }
    }
}
