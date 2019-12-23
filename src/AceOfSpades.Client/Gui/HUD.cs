using AceOfSpades.Characters;
using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using Dash.Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace AceOfSpades.Client.Gui
{
    public class HUD
    {
        class HitIndication
        {
            public Vector3 Origin;
            public float TimeLeft;

            public HitIndication(Vector3 origin)
            {
                Origin = origin;
                TimeLeft = 3f;
            }
        }

        class FeedItem : GUIFrame
        {
            public float TimeLeft;

            GUILabel leftLabel, rightLabel, centerLabel;

            public FeedItem(GUITheme theme, float height, int numFeed,
                string left, Color leftColor, string middle, string right, Color rightColor,
                BMPFont font)
                : base(UDim2.Zero, UDim2.Zero, image: null)
            {
                float leftLen = font.MeasureString(left).X;
                float midLen = font.MeasureString(middle).X;
                float rightLen = font.MeasureString(right).X;

                float fullLen = leftLen + midLen + rightLen + 20;
                float midX = ((leftLen + 10) + (fullLen - rightLen - 10)) / 2f;

                Position = new UDim2(1f, -fullLen - 10, 0, 20 + (height * numFeed));
                Size = new UDim2(0, fullLen, 0, 25);

                leftLabel = new GUILabel(UDim2.Zero, new UDim2(0, 1, 1f, 0), left, TextAlign.Left, leftColor, theme)
                { Parent = this };
                rightLabel = new GUILabel(new UDim2(1, 0, 0, 0), 
                    new UDim2(0, 1, 1f, 0), right, TextAlign.Right, rightColor, theme)
                { Parent = this };
                centerLabel = new GUILabel(new UDim2(0, midX, 0, 0), 
                    new UDim2(0, 1, 1f, 0), middle, TextAlign.Center, theme)
                { Parent = this };

                leftLabel.Font = font;
                rightLabel.Font = font;
                centerLabel.Font = font;

                TimeLeft = 5f;
            }

            public void ShiftY(float offset)
            {
                leftLabel.Position.Y.Offset += offset;
                rightLabel.Position.Y.Offset += offset;
                centerLabel.Position.Y.Offset += offset;
            }
        }

        const float INDICATOR_SPREAD = 140;
        const float INDICATOR_WIDTH = 32;
        const float INDICATOR_LENGTH = 140;
        const float INDICATOR_HALF_WIDTH = INDICATOR_WIDTH / 2f;
        const float INDICATOR_HALF_LENGTH = INDICATOR_LENGTH / 2f;

        const float HITMARKER_LIFETIME = 0.1f;

        const float INTEL_NOTIFICATION_BEFORE_FADE_DELAY = 4f;
        const float INTEL_NOTIFICATION_AFTER_FADE_DELAY = 1f;

        public Player Player;
        public bool ShowCharacterInformation;

        GUIArea area;
        MasterRenderer renderer;

        BMPFont font;
        GUILabel ammoLabel;
        GUILabel healthLabel;
        GUIFrame crosshair;
        GUIFrame hitmarker;
        GUIFrame intelInHand;
        GUILabel intelPickedUpNotification;

        static Texture palletTex;
        static Image crosshairImage;
        static Image hitmarkerImage;
        static Texture hurtRingTex;
        static Texture intelTex;

        List<HitIndication> hitIndications;
        List<FeedItem> feed;
        GUITheme theme;

        float hitmarkerTime;

        float intelPickupBeforeFadeTime;
        float intelPickupAfterFadeTime;
        bool showingIntelAnimation;
        bool hasIntel;

        World world;
        Gamemode gamemode;

        //bool showGameItems;

        public HUD(MasterRenderer renderer)
        {
            this.renderer = renderer;
            font = AssetManager.LoadFont("karmasuture-26");

            feed = new List<FeedItem>();
            hitIndications = new List<HitIndication>();

            if (palletTex == null)
            {
                palletTex = GLoader.LoadTexture("Textures/Gui/palette.png");
                Texture crosshairTex = GLoader.LoadTexture("Textures/Gui/crosshair.png");
                Texture hitmarkerTex = GLoader.LoadTexture("Textures/Gui/hitmarker.png");
                crosshairImage = new Image(crosshairTex);
                hitmarkerImage = new Image(hitmarkerTex);
                hurtRingTex = GLoader.LoadTexture("Textures/Gui/hurt-ring.png");
                intelTex = GLoader.LoadTexture("Textures/Gui/intel.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            }

            GUISystem gsys = renderer.Sprites.GUISystem;

            area = new GUIArea(gsys);
            area.ZIndex = -1;

            theme = GUITheme.Basic;
            theme.SetField("Font", font);
            theme.SetField("SmallFont", AssetManager.LoadFont("arial-bold-14"));
            theme.SetField("Label.TextColor", Color.White);
            theme.SetField("Label.TextShadowColor", new Color(0, 0, 0, 0.6f));

            healthLabel = new GUILabel(new UDim2(0, 40, 1, -20), UDim2.Zero, "Health: --", TextAlign.BottomLeft, theme);
            ammoLabel = new GUILabel(new UDim2(1, -50, 1, -25), UDim2.Zero, "", TextAlign.BottomRight, theme);
            crosshair = new GUIFrame(new UDim2(0.5f, -28, 0.5f, -28), new UDim2(0, 56, 0, 56), crosshairImage);
            hitmarker = new GUIFrame(new UDim2(0.5f, -43, 0.5f, -43), new UDim2(0, 86, 0, 86), hitmarkerImage);
            hitmarker.Visible = false;

            intelInHand = new GUIFrame(new UDim2(0.5f, -20, 0, 100), new UDim2(0, 40, 0, 40), new Image(intelTex));
            intelPickedUpNotification = new GUILabel(new UDim2(0.5f, 0, 0, 150), UDim2.Zero, 
                "You have picked up the intel!", TextAlign.TopCenter, theme);
            intelInHand.Visible = false;
            intelPickedUpNotification.Visible = false;

            crosshair.CapturesMouseClicks = false;
            hitmarker.CapturesMouseClicks = false;
            ammoLabel.CapturesMouseClicks = false;
            healthLabel.CapturesMouseClicks = false;

            area.AddTopLevel(ammoLabel, healthLabel, crosshair, hitmarker, intelInHand, intelPickedUpNotification);
        }

        public void SetWorld(World world)
        {
            this.world = world;
        }

        public void SetGamemode(Gamemode gamemode)
        {
            this.gamemode = gamemode;
        }

        public void Enable()
        {
            renderer.Sprites.Add(area);
            //showGameItems = true;
        }

        public void Disable()
        {
            intelInHand.Visible = false;
            intelPickedUpNotification.Visible = false;
            renderer.Sprites.Remove(area);
        }

        public void AddFeedItem(string left, string leftAssist, Color leftColor, string middle, string right, Color rightColor)
        {
            BMPFont font = theme.GetField<BMPFont>(null, "SmallFont");

            if (!string.IsNullOrWhiteSpace(leftAssist))
            {
                if (string.IsNullOrWhiteSpace(left))
                    left = right + " + " + leftAssist;
                else
                    left = left + " + " + leftAssist;
            }

            FeedItem item = new FeedItem(theme, 25, feed.Count,
                left, leftColor, middle, right, rightColor, font);
            feed.Add(item);
            area.AddTopLevel(item);
        }

        public void Update(float deltaTime)
        {
            if (Player == null || !ShowCharacterInformation)
            {
                ammoLabel.Visible = false;
                healthLabel.Visible = false;
                hitmarker.Visible = false;
                crosshair.Visible = false;

                if (Player == null)
                    return;
            }
            else
            {
                ammoLabel.Visible = true;
                healthLabel.Visible = true;
                hitmarker.Visible = true;
                crosshair.Visible = true;
            }

            ItemManager itemManager = Player.ItemManager;

            if (itemManager.SelectedItem != null)
            {
                if (itemManager.SelectedItem.Type.HasFlag(ItemType.Weapon))
                {
                    ammoLabel.Visible = ShowCharacterInformation;
                    Weapon wep = (Weapon)itemManager.SelectedItem;

                    if (wep.Type.HasFlag(ItemType.Gun))
                    {
                        Gun gun = (Gun)wep;
                        if (!gun.IsReloading)
                        {
                            int storedAmmo = GlobalNetwork.IsConnected ? gun.ServerStoredAmmo : gun.StoredAmmo;
                            int mag = GlobalNetwork.IsConnected ? gun.ServerMag : gun.CurrentMag;
                            ammoLabel.Text = string.Format("{0}/{1} | {2}", mag, gun.GunConfig.MagazineSize, storedAmmo);
                        }
                        else
                        {
                            int storedAmmo = GlobalNetwork.IsConnected ? gun.ServerStoredAmmo : gun.StoredAmmo;
                            ammoLabel.Text = string.Format("R/{0} | {1}", gun.GunConfig.MagazineSize, storedAmmo);
                        }
                    }
                    else if (wep.Type.HasFlag(ItemType.Grenade))
                        ammoLabel.Text = string.Format("Gx{0}", Player.NumGrenades);
                    else if (wep.Type.HasFlag(ItemType.Spade))
                        ammoLabel.Text = string.Format("Bx{0}", Player.NumBlocks);
                    else if (wep.Type.HasFlag(ItemType.MelonLauncher))
                        ammoLabel.Text = string.Format("Mx{0}", Player.NumMelons);
                    else
                        ammoLabel.Text = string.Format("--/-- | x--");
                }
                else if (itemManager.SelectedItem.Type.HasFlag(ItemType.BlockItem))
                {
                    BlockItem bitem = (BlockItem)itemManager.SelectedItem;

                    string text = string.Format("Bx{0}", Player.NumBlocks);
                    
                    ammoLabel.Text = text;
                    ammoLabel.Visible = ShowCharacterInformation;
                }
                else
                    ammoLabel.Visible = false;
            }
            else
                ammoLabel.Visible = false;

            healthLabel.Text = string.Format("Health: {0}", (int)Math.Max(Math.Ceiling(Player.Health), 0));

            for (int i = feed.Count - 1; i >= 0; i--)
            {
                FeedItem item = feed[i];
                item.TimeLeft -= deltaTime;

                if (item.TimeLeft < 0)
                {
                    feed.RemoveAt(i);
                    area.RemoveTopLevel(item);

                    for (int k = 0; k < feed.Count; k++)
                        feed[k].ShiftY(-25);
                }
            }

            foreach (Vector3 vec in Player.HitFeedbackPositions)
                hitIndications.Add(new HitIndication(vec));
            Player.HitFeedbackPositions.Clear();

            Camera camera = Camera.Active;
            Matrix4 mat = camera.ViewMatrix;

            for (int i = hitIndications.Count - 1; i >= 0; i--)
            {
                HitIndication indication = hitIndications[i];

                indication.TimeLeft -= deltaTime;
                if (indication.TimeLeft <= 0)
                    hitIndications.RemoveAt(i);
            }

            if (Player.HitPlayer)
            {
                Player.HitPlayer = false;
                hitmarkerTime = HITMARKER_LIFETIME;
            }

            hitmarker.Visible = hitmarkerTime > 0;

            if (hitmarkerTime > 0)
                hitmarkerTime -= deltaTime;

            //if (Input.GetControlDown("ToggleGameIcons") && (Player == null || Player.AllowUserInput))
            //    showGameItems = !showGameItems;

            if (gamemode != null)
            {
                CTFGamemode ctf = gamemode as CTFGamemode;
                if (ctf != null)
                {
                    if (Player != null && ctf.OurPlayerHasIntel && !hasIntel)
                    {
                        intelInHand.Image.Color = world.GetTeamColor(Player.Team == Team.A ? Team.B : Team.A);

                        hasIntel = true;
                        intelInHand.Position.Y.Offset = 100;
                        intelPickedUpNotification.TextColor.A = 255;
                        intelPickedUpNotification.TextShadowColor = new Color(0, 0, 0, 156);
                        intelInHand.Visible = true;
                        intelPickedUpNotification.Visible = true;

                        intelPickupBeforeFadeTime = INTEL_NOTIFICATION_BEFORE_FADE_DELAY;
                        intelPickupAfterFadeTime = INTEL_NOTIFICATION_AFTER_FADE_DELAY;
                        showingIntelAnimation = true;
                    }
                    else if ((Player == null || !ctf.OurPlayerHasIntel) && hasIntel)
                    {
                        hasIntel = false;
                        intelInHand.Visible = false;
                        intelPickedUpNotification.Visible = false;
                        intelInHand.Position.Y.Offset = 100;
                    }
                    else if (hasIntel && showingIntelAnimation)
                    {
                        if (intelPickupBeforeFadeTime > 0)
                            intelPickupBeforeFadeTime -= deltaTime;
                        else
                        {
                            if (intelPickupAfterFadeTime > 0)
                            {
                                intelPickupAfterFadeTime -= deltaTime;
                                float i = intelPickupAfterFadeTime / INTEL_NOTIFICATION_AFTER_FADE_DELAY;
                                intelPickedUpNotification.TextColor.A = (byte)(i * 255);
                                intelPickedUpNotification.TextShadowColor = new Color(intelPickedUpNotification.TextShadowColor.Value,
                                    ((byte)MathHelper.Clamp(i * 255 - 128, 0, 255)));
                                intelInHand.Position.Y.Offset = 25 + (75 * i);
                            }
                            else
                            {
                                intelPickedUpNotification.Visible = false;
                                intelInHand.Position.Y.Offset = 25;
                                showingIntelAnimation = false;
                            }
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            Camera camera = Camera.Active;
            Matrix4 mat = camera.ViewMatrix;

            Vector2 centerScreen = new Vector2(
                renderer.ScreenWidth / 2f,
                renderer.ScreenHeight / 2f);

            for (int i = hitIndications.Count - 1; i >= 0; i--)
            {
                HitIndication indication = hitIndications[i];

                if (indication.TimeLeft > 0)
                {
                    Vector3 dir = (camera.Position - indication.Origin) * mat;
                    float angle = dir.Xz.ToAngle();

                    Vector2 position = new Vector2(centerScreen.X, centerScreen.Y - INDICATOR_SPREAD);
                    position = Vector2.RotatePoint(centerScreen, position, -angle);

                    sb.Draw(hurtRingTex,
                        new Rectangle(
                            position.X - INDICATOR_HALF_LENGTH,
                            position.Y - INDICATOR_HALF_WIDTH,
                            INDICATOR_LENGTH, INDICATOR_WIDTH),
                        null, Color.White, angle,
                        new Vector2(INDICATOR_HALF_LENGTH, INDICATOR_HALF_WIDTH));
                }
            }

            if (Player != null)
            {
                ItemManager itemManager = Player.ItemManager;

                if (itemManager.SelectedItem != null && itemManager.SelectedItem.Type.HasFlag(ItemType.BlockItem))
                {
                    BlockItem bitem = (BlockItem)itemManager.SelectedItem;

                    float paletteX = renderer.ScreenWidth - 330;
                    float paletteY = renderer.ScreenHeight - 200;

                    for (int x = 0; x < BlockItem.PaletteWidth; x++)
                        for (int y = 0; y < BlockItem.PaletteHeight; y++)
                        {
                            Rectangle backClip;
                            Rectangle borderClip;
                            if (bitem.ColorX == x && bitem.ColorY == y)
                            {
                                backClip = new Rectangle(0, 16, 16, 16);
                                borderClip = new Rectangle(16, 16, 16, 16);
                            }
                            else
                            {
                                backClip = new Rectangle(0, 0, 16, 16);
                                borderClip = new Rectangle(16, 0, 16, 16);
                            }

                            if (!renderer.Gui.Hide)
                            {
                                renderer.Sprites.SpriteBatch.Draw(palletTex,
                                    new Rectangle(x * 15 + paletteX, y * 15 + paletteY, 22, 22),
                                    backClip,
                                    bitem.Colors[y, x]);

                                renderer.Sprites.SpriteBatch.Draw(palletTex,
                                    new Rectangle(x * 15 + paletteX, y * 15 + paletteY, 22, 22),
                                    borderClip,
                                    Color.White);
                            }
                        }
                }
            }
        }
    }
}
