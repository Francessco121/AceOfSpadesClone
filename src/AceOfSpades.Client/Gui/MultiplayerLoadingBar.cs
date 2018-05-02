using AceOfSpades.Client.Net;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Client.Gui
{
    public class MultiplayerLoadingBar : GUIWindow
    {
        GUILabel statusLabel;
        GUIFrame bar;
        Handshake handshake;

        FloatAnim barAnim;
        FloatAnim byteAnim;

        int bytesTotal;
        bool downloading;

        int chunksTotal;
        FixedTerrain terrain;

        public MultiplayerLoadingBar(GUISystem system, GUITheme theme) 
            : base(system, new UDim2(0.8f, 0, 0, 120), "", theme, false)
        {
            IsDraggable = false;

            barAnim = new FloatAnim();
            byteAnim = new FloatAnim();

            statusLabel = new GUILabel(new UDim2(0.5f, 0, 0.4f, 0), UDim2.Zero, "0/0 bytes", theme);
            bar = new GUIFrame(new UDim2(0, 5, 1f, -35), new UDim2(0, -10, 0, 30), Image.Blank);

            AddTopLevel(statusLabel, bar);
        }

        public void SetHandshake(Handshake handshake)
        {
            Title = "Downloading world...";
            this.handshake = handshake;
            Update(0, 0);
            handshake.OnTerrainProgressReported += Update;
        }

        public void ClearAndShow()
        {
            downloading = true;
            Title = "Awaiting Server...";
            bar.Size.X.Scale = 0;
            bytesTotal = 0;
            statusLabel.Text = "";
            Visible = true;
        }

        public void SwitchToTerrainLoading(FixedTerrain terrain)
        {
            downloading = false;
            Title = "Loading terrain...";
            bar.Size.X.Scale = 0;
            this.terrain = terrain;
            chunksTotal = terrain.Width * terrain.Height * terrain.Depth;
        }

        void Update(int bytesDownloaded, int bytesTotal)
        {
            this.bytesTotal = bytesTotal;

            if (bytesTotal == 0)
            {
                barAnim.SnapTo(0);
                byteAnim.SnapTo(0);
            }
            else
            {
                float percent = (float)bytesDownloaded / bytesTotal;
                barAnim.SetTarget(percent);
                byteAnim.SetTarget(bytesDownloaded);

                if (bytesDownloaded == bytesTotal)
                    handshake.OnTerrainProgressReported -= Update;
            }
        }

        public override void Update(float deltaTime)
        {
            if (downloading)
            {
                if (bytesTotal > 0)
                {
                    barAnim.Step(deltaTime * 10);
                    byteAnim.Step(deltaTime * 10);

                    bar.Size.X.Scale = barAnim.Value;
                    statusLabel.Text = string.Format("{0}/{1} bytes", (int)byteAnim.Value, bytesTotal);
                }
            }
            else if (terrain != null)
            {
                barAnim.SetTarget((float)(chunksTotal - terrain.UnfinishedChunks) / chunksTotal);
                barAnim.Step(deltaTime * 10);

                bar.Size.X.Scale = barAnim.Value;
                statusLabel.Text = string.Format("{0}/{1} chunks loaded", chunksTotal - terrain.UnfinishedChunks, chunksTotal);

                if (terrain.UnfinishedChunks == 0)
                {
                    terrain = null;
                    Visible = false;
                }
            }

            base.Update(deltaTime);
        }
    }
}
