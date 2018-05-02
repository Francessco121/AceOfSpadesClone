using AceOfSpades.Characters;
using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;

namespace AceOfSpades.Client
{
    public class HUD : IDisposable
    {
        GUISurface2D surface;
        Player player;
        MasterRenderer renderer;

        DashFont font;
        GUILabel ammoLabel;
        GUILabel healthLabel;
        GUIImage crosshairDot;

        public HUD(MasterRenderer renderer, Player player, DashFont font)
        {
            this.renderer = renderer;
            this.player = player;
            this.font = font;

            surface = new GUISurface2D(renderer.ScreenWidth, renderer.ScreenHeight);
            renderer.Gui.Add(surface);

            GUITheme theme = new GUITheme()
            {
                Font = font,
                LabelTextColor = Color.White,
            };

            healthLabel = new GUILabel(new UDim2(0, 40, 1, -20), UDim2.Zero, theme, "Health: --", TextAlign.BottomLeft);
            ammoLabel = new GUILabel(new UDim2(1, -50, 1, -25), UDim2.Zero, theme, "", TextAlign.BottomRight);
            crosshairDot = new GUIImage(GUISurface.BlankTexture, new UDim2(0.5f, -1, 0.5f, -1), new UDim2(0, 2, 0, 2), theme);

            surface.Add(ammoLabel, healthLabel, crosshairDot);
        }

        public void Dispose()
        {
            renderer.Gui.Remove(surface);
        }

        public void Resize(int width, int height)
        {
            surface.OnResize(width, height);
        }

        public void Update(float deltaTime)
        {
            ItemManager itemManager = player.ItemManager;

            if (itemManager.SelectedItem != null)
            {
                if (itemManager.SelectedItem.Type.HasFlag(ItemType.Weapon))
                {
                    ammoLabel.Visible = true;
                    Weapon wep = (Weapon)itemManager.SelectedItem;

                    if (wep.Type.HasFlag(ItemType.Gun))
                    {
                        Gun gun = (Gun)wep;
                        int magCount = GlobalNetwork.IsConnected ? gun.ServerStoredMags : gun.StoredMags.Count;
                        int mag = GlobalNetwork.IsConnected ? gun.ServerMag : gun.CurrentMag.Count;
                        ammoLabel.Text = string.Format("{0}/{1} | x{2}", mag, gun.GunConfig.MagazineSize, magCount);
                    }
                    else if (wep.Type.HasFlag(ItemType.Grenade))
                        ammoLabel.Text = string.Format("Gx{0}", player.NumGrenades);
                    else if (wep.Type.HasFlag(ItemType.Spade))
                        ammoLabel.Text = string.Format("Bx{0}", player.NumBlocks);
                    else
                        ammoLabel.Text = string.Format("--/-- | x--");
                }
                else if (itemManager.SelectedItem.Type.HasFlag(ItemType.BlockItem))
                {
                    BlockItem bitem = (BlockItem)itemManager.SelectedItem;

                    string text = string.Format("Bx{0}", player.NumBlocks);
                    
                    ammoLabel.Text = text;
                    ammoLabel.Visible = true;

                    float paletteX = renderer.ScreenWidth - 220;
                    float paletteY = renderer.ScreenHeight - 115;

                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                            renderer.Gui.SpriteBatch.Draw(GUISurface.BlankTexture,
                                new Rectangle(x * 11 + paletteX, y * 11 + paletteY, 10, 10), bitem.Colors[y, x]);

                    renderer.Gui.SpriteBatch.Draw(GUISurface.BlankTexture,
                        new Rectangle(bitem.ColorX * 11 + paletteX + 2.5f, bitem.ColorY * 11 + paletteY + 2.5f, 5, 5), Color.White);
                }
                else
                    ammoLabel.Visible = false;
            }
            else
                ammoLabel.Visible = false;

            healthLabel.Text = string.Format("Health: {0}", player.Health);
        }
    }
}
