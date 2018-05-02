using AceOfSpades.Client.Gui;
using AceOfSpades.Client.Net;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Net;

/* (Client)TDMGamemode.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public sealed class TDMGamemode : NetworkedGamemode
    {
        public int TeamAScore { get; private set; }
        public int TeamBScore { get; private set; }
        public int ScoreCap { get; private set; } = 100;

        CommandPost redPost, bluePost;

        public TDMGamemode(MultiplayerScreen screen) 
            : base(screen, GamemodeType.TDM)
        { }

        protected override void OnStarted()
        {
            NetChannel.AddRemoteEvent("Client_GamemodeInfo", R_GamemodeInfo);
            NetChannel.AddRemoteEvent("Client_UpdateScores", R_UpdateScores);

            objectComponent.AddInstantiationEvent("Client_CreateCommandPost", I_CreateCommandPost);

            base.OnStarted();
        }

        protected override void OnStopped()
        {
            NetChannel.RemoveRemoteEvent("Client_GamemodeInfo");
            NetChannel.RemoveRemoteEvent("Client_UpdateScores");

            objectComponent.RemoveInstantiationEvent("Client_CreateCommandPost");

            redPost = null;
            bluePost = null;

            TeamAScore = 0;
            TeamBScore = 0;
            ScoreCap = 100;

            base.OnStopped();
        }

        INetCreatable I_CreateCommandPost(ushort id, bool isAppOwner, NetBuffer data)
        {
            // Read the packet
            float x = data.ReadFloat();
            float y = data.ReadFloat();
            float z = data.ReadFloat();
            Team team = (Team)data.ReadByte();

            CommandPost post = new CommandPost(new Vector3(x, y, z), team);

            if (team == Team.A) redPost = post;
            else bluePost = post;

            return post;
        }

        void R_GamemodeInfo(NetConnection server, NetBuffer buffer, ushort numArgs)
        {
            ScoreCap = buffer.ReadUInt16();
        }

        void R_UpdateScores(NetConnection server, NetBuffer buffer, ushort numArgs)
        {
            TeamAScore = buffer.ReadInt16();
            TeamBScore = buffer.ReadInt16();
        }
    }
}
