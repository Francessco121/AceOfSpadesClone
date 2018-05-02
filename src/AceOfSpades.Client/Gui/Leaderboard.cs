using AceOfSpades.Client.Net;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System.Collections.Generic;

namespace AceOfSpades.Client.Gui
{
    public class Leaderboard : GUIWindow
    {
        class PlayerFrame : GUIFrame
        {
            public NetworkPlayer NetPlayer { get; }

            GUILabel nameLabel;
            GUILabel scoreLabel;
            GUILabel pingLabel;
            bool isOurPlayer;

            public PlayerFrame(NetworkPlayer player, bool isOurPlayer, UDim2 position, UDim2 size, GUITheme theme) 
                : base(position, size, Image.CreateBlank(Color.White))
            {
                Theme = theme;
                NetPlayer = player;
                this.isOurPlayer = isOurPlayer;

                nameLabel = new GUILabel(UDim2.Zero, UDim2.Fill, "", TextAlign.Left, theme)
                { Parent = this };
                nameLabel.Font = Theme.GetField<BMPFont>(null, "Leaderboard.PlayerLabel.Font");
                nameLabel.TextPadding.X = 6;

                scoreLabel = new GUILabel(UDim2.Zero, new UDim2(1f, -50, 1f, 0), "0", TextAlign.Right, theme)
                { Parent = this };
                scoreLabel.Font = Theme.GetField<BMPFont>(null, "Leaderboard.PlayerLabel.Font");
                scoreLabel.TextPadding.Z = 6;

                pingLabel = new GUILabel(UDim2.Zero, UDim2.Fill, "0ms", TextAlign.Right, theme)
                { Parent = this };
                pingLabel.Font = Theme.GetField<BMPFont>(null, "Leaderboard.PlayerLabel.Font");
                pingLabel.TextPadding.Z = 6;

                SetName(player.Name);
                SetScore(player.Score);
            }

            public void SetAltColor(bool alt)
            {
                Color labelBackColor = isOurPlayer
                    ? new Color(62, 125, 221, 120)
                    : alt ? new Color(150, 150, 150, 30) : new Color(80, 80, 80, 30);

                Image.Color = labelBackColor;
            }

            public void SetName(string name)
            {
                nameLabel.Text = name;
            }

            public void SetScore(int score)
            {
                scoreLabel.Text = score.ToString();
            }

            public void SetPing(int ping)
            {
                pingLabel.Text = string.Format("{0}ms", ping);
            }
        }

        SnapshotNetComponent snapshotComponent;
        NetPlayerComponent netPlayerComponent;

        GUIFrame teamAFrame;
        GUIFrame teamBFrame;
        GUIFrame footerFrame;
        
        Dictionary<ushort, PlayerFrame> playerFrames;
        Gamemode gamemode;

        List<PlayerFrame> orderedTeamA;
        List<PlayerFrame> orderedTeamB;

        GUILabel teamAScoreLabel;
        GUILabel teamBScoreLabel;
        GUILabel gamemodeLabel;
        GUILabel gamemodeInfoLabel;

        BMPFont bigFont;

        public Leaderboard(GUISystem gsys, GUITheme theme, NetPlayerComponent netPlayerComponent) 
            : base(gsys, new UDim2(0.65f, 0, 0.65f, 0), "Leaderboard", theme, false, false)
        {
            MinSize = new UDim2(0, 300, 0, 400);

            this.netPlayerComponent = netPlayerComponent;
            IsDraggable = false;

            playerFrames = new Dictionary<ushort, PlayerFrame>();
            orderedTeamA = new List<PlayerFrame>();
            orderedTeamB = new List<PlayerFrame>();

            bigFont = AssetManager.LoadFont("karmasuture-26");
            theme.SetField("Leaderboard.PlayerLabel.Font", AssetManager.LoadFont("arial-14"));

            teamAFrame = new GUIFrame(UDim2.Zero, new UDim2(0.5f, 0, 1f, 0), image: null);
            teamBFrame = new GUIFrame(new UDim2(0.5f, 0, 0, 0), new UDim2(0.5f, 0, 1f, 0), image: null);
            footerFrame = new GUIFrame(new UDim2(0, 0, 1f, -30), new UDim2(1f, 0, 0, 30), theme);

            gamemodeLabel = new GUILabel(UDim2.Zero, new UDim2(0.5f, 0, 1f, 0), "Current Gamemode: --", TextAlign.Left, theme)
            { Parent = footerFrame };
            gamemodeInfoLabel = new GUILabel(new UDim2(0.5f, 0, 0, 0), new UDim2(0.5f, 0, 1f, 0), "", TextAlign.Right, theme)
            { Parent = footerFrame };

            GUILabel teamALabel = new GUILabel(new UDim2(0, 0, 0, 0), new UDim2(1f, 0, 0, 40),
                "Red Team", TextAlign.Left, Theme)
            { Parent = teamAFrame };
            GUILabel teamBLabel = new GUILabel(new UDim2(0, 0, 0, 0), new UDim2(1f, 0, 0, 40),
                "Blue Team", TextAlign.Left, Theme)
            { Parent = teamBFrame };

            teamAScoreLabel = new GUILabel(new UDim2(1f, 0, 0, 0), new UDim2(0, 1, 0, 40),
                "0", TextAlign.Right, Theme)
            { Parent = teamAFrame };
            teamBScoreLabel = new GUILabel(new UDim2(1f, 0, 0, 0), new UDim2(0, 1, 0, 40),
                "0", TextAlign.Right, Theme)
            { Parent = teamBFrame };

            GUILabel teamAPlayerNameLabel = new GUILabel(new UDim2(0, 0, 0, 40), new UDim2(1f, 0, 0, 20), "Name", TextAlign.Left, theme)
            { Parent = teamAFrame };
            GUILabel teamAPlayerScoreLabel = new GUILabel(new UDim2(0, 0, 0, 40), new UDim2(1f, -50, 0, 20), "Score", TextAlign.Right, theme)
            { Parent = teamAFrame };
            GUILabel teamAPlayerPingLabel = new GUILabel(new UDim2(0, 0, 0, 40), new UDim2(1f, 0, 0, 20), "Ping", TextAlign.Right, theme)
            { Parent = teamAFrame };

            GUILabel teamBPlayerNameLabel = new GUILabel(new UDim2(0, 0, 0, 40), new UDim2(1f, 0, 0, 20), "Name", TextAlign.Left, theme)
            { Parent = teamBFrame };
            GUILabel teamBPlayerScoreLabel = new GUILabel(new UDim2(0, 0, 0, 40), new UDim2(1f, -50, 0, 20), "Score", TextAlign.Right, theme)
            { Parent = teamBFrame };
            GUILabel teamBPlayerPingLabel = new GUILabel(new UDim2(0, 0, 0, 40), new UDim2(1f, 0, 0, 20), "Ping", TextAlign.Right, theme)
            { Parent = teamBFrame };

            teamALabel.TextPadding.X = 6;
            teamBLabel.TextPadding.X = 6;
            teamAScoreLabel.TextPadding.Z = 6;
            teamBScoreLabel.TextPadding.Z = 6;

            teamAPlayerNameLabel.TextPadding.X = 6;
            teamAPlayerScoreLabel.TextPadding.Z = 6;
            teamAPlayerPingLabel.TextPadding.Z = 6;

            teamBPlayerNameLabel.TextPadding.X = 6;
            teamBPlayerScoreLabel.TextPadding.Z = 6;
            teamBPlayerPingLabel.TextPadding.Z = 6;

            teamALabel.BackgroundImage = Image.CreateBlank(new Color(186, 52, 52, 127));
            teamBLabel.BackgroundImage = Image.CreateBlank(new Color(39, 78, 194, 127));
            teamAPlayerNameLabel.BackgroundImage = theme.GetField<Image>(null, "Frame.Image");
            teamBPlayerNameLabel.BackgroundImage = theme.GetField<Image>(null, "Frame.Image");

            teamALabel.Font = bigFont;
            teamBLabel.Font = bigFont;
            teamAScoreLabel.Font = bigFont;
            teamBScoreLabel.Font = bigFont;

            teamAScoreLabel.ZIndex = 1;
            teamBScoreLabel.ZIndex = 1;
            teamAPlayerScoreLabel.ZIndex = 1;
            teamBPlayerScoreLabel.ZIndex = 1;
            teamAPlayerPingLabel.ZIndex = 1;
            teamBPlayerPingLabel.ZIndex = 1;

            AddTopLevel(teamAFrame, teamBFrame, footerFrame);
        }

        public void SetGamemode(Gamemode gamemode)
        {
            this.gamemode = gamemode;
        }

        protected override void Shown()
        {
            Center();
            base.Shown();
        }

        public override void Update(float deltaTime)
        {
            if (AOSClient.Instance != null)
            {
                if (snapshotComponent == null)
                    snapshotComponent = AOSClient.Instance.GetComponent<SnapshotNetComponent>();

                WorldSnapshot worldSnapshot = snapshotComponent.WorldSnapshot;

                if (worldSnapshot != null)
                {
                    foreach (NetworkPlayer player in netPlayerComponent.NetPlayers)
                    {
                        if (player.Team == Team.None)
                            continue;

                        PlayerFrame frame;
                        if (playerFrames.TryGetValue(player.Id, out frame))
                        {
                            if (frame.Parent == teamAFrame && player.Team == Team.B
                                || frame.Parent == teamBFrame && player.Team == Team.A)
                            {
                                RemovePlayer(frame);
                                AddPlayer(player);
                            }
                        }
                    }

                    int totalAScore = 0, totalBScore = 0;

                    foreach (NetworkPlayer player in netPlayerComponent.NetPlayers)
                    {
                        if (player.Team == Team.None)
                            continue;

                        if (player.Team == Team.A) totalAScore += player.Score;
                        if (player.Team == Team.B) totalBScore += player.Score;

                        PlayerFrame frame;
                        if (playerFrames.TryGetValue(player.Id, out frame))
                        {
                            frame.SetScore(player.Score);
                            frame.SetName(player.Name);
                            frame.SetPing(player.Ping);
                        }
                        else
                            AddPlayer(player);
                    }

                    foreach (PlayerFrame frame in playerFrames.Values)
                        if (!netPlayerComponent.HasNetPlayer(frame.NetPlayer.Id))
                        {
                            RemovePlayer(frame);
                            break;
                        }

                    AlignAndSortLists();
                    UpdatePrimaryScores(totalAScore, totalBScore);
                }

                gamemodeLabel.Text = string.Format("Current Gamemode: {0}",
                    gamemode != null ? gamemode.Type.ToString() : "--");
            }

            base.Update(deltaTime);
        }

        void UpdatePrimaryScores(int totalAScore, int totalBScore)
        {
            if (gamemode != null)
            {
                if (gamemode.Type == GamemodeType.TDM)
                {
                    TDMGamemode tdm = (TDMGamemode)gamemode;
                    teamAScoreLabel.Text = tdm.TeamAScore.ToString();
                    teamBScoreLabel.Text = tdm.TeamBScore.ToString();
                    gamemodeInfoLabel.Text = string.Format("Playing until {0} kills", tdm.ScoreCap);
                }
                else if (gamemode.Type == GamemodeType.CTF)
                {
                    CTFGamemode ctf = (CTFGamemode)gamemode;
                    teamAScoreLabel.Text = ctf.TeamAScore.ToString();
                    teamBScoreLabel.Text = ctf.TeamBScore.ToString();
                    gamemodeInfoLabel.Text = string.Format("Playing until {0} captures", ctf.ScoreCap);
                }
                else
                {
                    teamAScoreLabel.Text = totalAScore.ToString();
                    teamBScoreLabel.Text = totalBScore.ToString();
                    gamemodeInfoLabel.Text = "";
                }
            }
            else
            {
                teamAScoreLabel.Text = totalAScore.ToString();
                teamBScoreLabel.Text = totalBScore.ToString();
                gamemodeInfoLabel.Text = "";
            }
        }

        int CompareFrame(PlayerFrame a, PlayerFrame b)
        {
            return b.NetPlayer.Score - a.NetPlayer.Score;
        }

        void AlignAndSortLists()
        {
            orderedTeamA.Sort(CompareFrame);
            orderedTeamB.Sort(CompareFrame);

            for (int y = 0; y < orderedTeamA.Count; y++)
            {
                PlayerFrame frame = orderedTeamA[y];
                frame.Position.Y.Offset = 60 + 30 * y;
                frame.SetAltColor(y % 2 == 0);
            }

            for (int y = 0; y < orderedTeamB.Count; y++)
            {
                PlayerFrame frame = orderedTeamB[y];
                frame.Position.Y.Offset = 60 + 30 * y;
                frame.SetAltColor(y % 2 == 0);
            }
        }

        void AddPlayer(NetworkPlayer player)
        {
            PlayerFrame pFrame = new PlayerFrame(player, netPlayerComponent.OurNetPlayerId == player.Id,
                UDim2.Zero, new UDim2(1f, 0, 0, 30), Theme)
            { Parent = player.Team == Team.A ? teamAFrame : teamBFrame };

            playerFrames.Add(player.Id, pFrame);

            if (player.Team == Team.A) orderedTeamA.Add(pFrame);
            if (player.Team == Team.B) orderedTeamB.Add(pFrame);
        }

        void RemovePlayer(PlayerFrame frame)
        {
            frame.Parent = null;

            orderedTeamA.Remove(frame);
            orderedTeamB.Remove(frame);
            playerFrames.Remove(frame.NetPlayer.Id);
        }
    }
}
