namespace AceOfSpades.Net
{
    public interface INetEntity : INetCreatable
    {
        NetEntitySnapshot CreateSnapshot(NetCreatableInfo info, SnapshotSystem snapshotSystem);
        void OnServerOutbound(NetEntitySnapshot snapshot);
        void OnClientInbound(NetEntitySnapshot snapshot);
    }
}
