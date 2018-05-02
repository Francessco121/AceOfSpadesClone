using AceOfSpades.Client.Gui;
using AceOfSpades.Client.Net;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Diagnostics;
using Dash.Net;

namespace AceOfSpades.Client
{
    public class CTFGamemode : NetworkedGamemode
    {
        public int TeamAScore { get; private set; }
        public int TeamBScore { get; private set; }
        public int ScoreCap { get; private set; } = 3;
        public bool OurPlayerHasIntel { get; private set; }

        CommandPost redPost, bluePost;
        Intel redIntel, blueIntel;

        public CTFGamemode(MultiplayerScreen screen)
            : base(screen, GamemodeType.CTF)
        { }

        protected override void OnStarted()
        {
            NetChannel.AddRemoteEvent("Client_GamemodeInfo", R_GamemodeInfo);
            NetChannel.AddRemoteEvent("Client_UpdateScores", R_UpdateScores);
            NetChannel.AddRemoteEvent("Client_IntelPickedUp", R_IntelPickedUp);
            NetChannel.AddRemoteEvent("Client_IntelDropped", R_IntelDropped);
            NetChannel.AddRemoteEvent("Client_IntelCaptured", R_IntelCaptured);
            NetChannel.AddRemoteEvent("Client_IntelReturned", R_IntelReturned);

            objectComponent.AddInstantiationEvent("Client_CreateIntel", I_CreateIntel);
            objectComponent.AddInstantiationEvent("Client_CreateCommandPost", I_CreateCommandPost);

            base.OnStarted();
        }

        protected override void OnStopped()
        {
            NetChannel.RemoveRemoteEvent("Client_GamemodeInfo");
            NetChannel.RemoveRemoteEvent("Client_UpdateScores");
            NetChannel.RemoveRemoteEvent("Client_IntelPickedUp");
            NetChannel.RemoveRemoteEvent("Client_IntelDropped");
            NetChannel.RemoveRemoteEvent("Client_IntelCaptured");
            NetChannel.RemoveRemoteEvent("Client_IntelReturned");

            objectComponent.RemoveInstantiationEvent("Client_CreateIntel");
            objectComponent.RemoveInstantiationEvent("Client_CreateCommandPost");

            redIntel = null;
            blueIntel = null;
            redPost = null;
            bluePost = null;

            TeamAScore = 0;
            TeamBScore = 0;
            ScoreCap = 3;

            OurPlayerHasIntel = false;

            base.OnStopped();
        }

        void R_IntelReturned(NetConnection server, NetBuffer data, ushort numArgs)
        {
            Team team = (Team)data.ReadByte();

            if (team == Team.A) redIntel.IsIconVisible = true;
            else blueIntel.IsIconVisible = true;

            OurPlayerHasIntel = false;

            string teamStr = team == Team.A ? "Red" : "Blue";
            Screen.ShowAnnouncement(string.Format("The {0} intel has been returned to their base!", teamStr), 5f, false);
        }

        void R_IntelCaptured(NetConnection server, NetBuffer data, ushort numArgs)
        {
            Team team = (Team)data.ReadByte();
            if (team == Team.A)
            {
                redIntel.Drop();
                redIntel.IsIconVisible = true;
            }
            else
            {
                blueIntel.Drop();
                blueIntel.IsIconVisible = true;
            }

            OurPlayerHasIntel = false;

            string teamStr = team == Team.A ? "Red" : "Blue";
            Screen.ShowAnnouncement(string.Format("The {0} intel has been captured!", teamStr), 5f, false);
        }

        void R_IntelPickedUp(NetConnection server, NetBuffer data, ushort numArgs)
        {
            Team team = (Team)data.ReadByte();
            ushort playerOwner = data.ReadUInt16();

            bool showedAltMessage = false;

            ClientPlayer owner;
            if (Players.TryGetValue(playerOwner, out owner))
            {
                if (team == Team.A)
                {
                    redIntel.ForcePickup(owner);
                    if (owner == OurPlayer)
                        redIntel.IsIconVisible = false;
                }
                else
                {
                    blueIntel.ForcePickup(owner);
                    if (owner == OurPlayer)
                        blueIntel.IsIconVisible = false;
                }

                if (owner == OurPlayer)
                {
                    OurPlayerHasIntel = true;
                    showedAltMessage = true;
                }
            }
            else
                DashCMD.WriteError("[CTFGamemode] Failed to replicate intel pickup! Intel owner {0} does not exist!", playerOwner);

            if (!showedAltMessage)
            {
                string teamStr = team == Team.A ? "Red" : "Blue";
                Screen.ShowAnnouncement(string.Format("The {0} intel has been picked up!", teamStr), 5f, false);
            }
        }

        void R_IntelDropped(NetConnection server, NetBuffer data, ushort numArgs)
        {
            Team team = (Team)data.ReadByte();
            if (team == Team.A)
            {
                redIntel.Drop();
                redIntel.IsIconVisible = true;
            }
            else
            {
                blueIntel.Drop();
                blueIntel.IsIconVisible = true;
            }

            OurPlayerHasIntel = false;

            string teamStr = team == Team.A ? "Red" : "Blue";
            Screen.ShowAnnouncement(string.Format("The {0} intel has been dropped!", teamStr), 5f, false);
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

        INetCreatable I_CreateIntel(ushort id, bool isAppOwner, NetBuffer data)
        {
            // Read the packet
            float x = data.ReadFloat();
            float y = data.ReadFloat();
            float z = data.ReadFloat();
            Team team = (Team)data.ReadByte();

            Intel intel = new Intel(new Vector3(x, y, z), team);

            if (team == Team.A) redIntel = intel;
            else blueIntel = intel;

            return intel;
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
