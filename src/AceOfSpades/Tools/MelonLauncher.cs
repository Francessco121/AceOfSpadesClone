using System;
using Dash.Engine;
using Dash.Engine.Graphics;
using AceOfSpades.Net;
using Dash.Engine.Diagnostics;

/* MelonLauncher.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class MelonLauncher : Gun
    {
        public MelonLauncher(ItemManager itemManager, MasterRenderer renderer, int startAmmo) 
            : base(itemManager, renderer)
        {
            ModelOffset = new Vector3(-3.15f, -4f, 0.5f);
            AimModelOffset = new Vector3(-0.75f, -2.35f, -6f);
            ThirdpersonScale = 0.7f;
            LoadModel("Models/melon-launcher.aosm");

            CurrentMag = startAmmo;
        }

        public override void OnEquip()
        {
            base.OnEquip();
            CheckMag();
        }

        protected override GunConfig InitializeGunConfig()
        {
            return new GunConfig()
            {
                AimFOV = 40,
                AimMouseSensitivityScale = 0.6f,
                PrimaryFireDelay = 1f,
                MaxStoredMags = 0,
                MagazineSize = 2
            };
        }

        void CheckMag()
        {
            if (!GlobalNetwork.IsConnected || GlobalNetwork.IsServer)
            {
                if (CurrentMag == 0)
                    Manager.Equip(-1);
            }
            else
            {
                if (ServerMag == 0)
                    Manager.Equip(-1);
            }
        }

        protected override void Update(float deltaTime)
        {
            CheckMag();
            base.Update(deltaTime);
        }

        protected override void OnPrimaryFire()
        {
            if (!GlobalNetwork.IsConnected || GlobalNetwork.IsServer)
            {
                if (CurrentMag > 0)
                {
                    World.ShootMelon(OwnerPlayer, Camera.Position + Camera.ViewMatrix.Forward() * 2, Camera.ViewMatrix.Forward());

                    if (!GlobalNetwork.IsServer || !DashCMD.GetCVar<bool>("ch_infammo"))
                        CurrentMag--;
                }
            }
            else if (GlobalNetwork.IsConnected && GlobalNetwork.IsClient)
            {
                if (ServerMag > 0)
                    World.ShootMelon(OwnerPlayer, Camera.Position + Camera.ViewMatrix.Forward() * 2, Camera.ViewMatrix.Forward());
            }
        }
    }
}
