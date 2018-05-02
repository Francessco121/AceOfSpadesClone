using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using System;
using System.Collections.Generic;
using AceOfSpades.Graphics;

/* Gun.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public abstract class Gun : Weapon
    {
        public int CurrentMag;
        public int StoredAmmo;
        public GunConfig GunConfig { get; private set; }

        public Vector3 MuzzleFlashOffset { get; protected set; }

        public ushort ServerMag;
        public ushort ServerStoredAmmo;

        public bool IsReloading { get; private set; }

        float reloadTimeLeft;

        protected Vector3 AimModelOffset { get; set; }
        Vector3 normalModelOffset;
        FloatAnim fovAnim;
        Vector3Anim modelAnim;

        IMuzzleFlash muzzleFlash;

        public Gun(ItemManager itemManager, MasterRenderer renderer) 
            : base(renderer, itemManager, ItemType.Gun)
        {
            CurrentMag = GunConfig.MagazineSize;
            StoredAmmo = GunConfig.MagazineSize * GunConfig.MaxStoredMags;

            if (GlobalNetwork.IsClient && GlobalNetwork.IsConnected)
            {
                ServerMag = (ushort)CurrentMag;
                ServerStoredAmmo = (ushort)StoredAmmo;
            }

            fovAnim = new FloatAnim();
            modelAnim = new Vector3Anim();
            muzzleFlash = itemManager.GetMuzzleFlash();
        }

        public override bool CanEquip()
        {
            return ((GlobalNetwork.IsServer || !GlobalNetwork.IsConnected) && (StoredAmmo > 0 || CurrentMag > 0))
                || (ServerStoredAmmo > 0 || ServerMag > 0);
        }

        public override void OnEquip()
        {
            normalModelOffset = ModelOffset;
            if (GlobalNetwork.IsClient)
            {
                Camera cam = Dash.Engine.Graphics.Camera.Active;
                fovAnim.SnapTo(cam.FOV);
            }

            modelAnim.SnapTo(ModelOffset);
            base.OnEquip();
        }

        public override void OnUnequip()
        {
            if (GlobalNetwork.IsClient)
            {
                Camera cam = Dash.Engine.Graphics.Camera.Active;
                cam.FOV = 70;
                cam.FPSMouseSensitivity = cam.DefaultFPSMouseSensitivity;
            }

            IsReloading = false;
            ModelOffset = normalModelOffset;
            base.OnUnequip();
        }

        public override bool CanPrimaryFire()
        {
            return !IsReloading && !OwnerPlayer.IsSprinting && base.CanPrimaryFire();
        }

        public override bool CanSecondaryFire()
        {
            return !IsReloading;
        }

        protected override void OnPrimaryFire()
        {
            if (!GlobalNetwork.IsConnected || GlobalNetwork.IsServer)
            {
                if (CurrentMag > 0)
                {
                    for (int i = 0; i < GunConfig.BulletsPerShot; i++)
                    {
                        Vector3 recoil = GetRecoilJiggle(GunConfig.BulletSpread, GunConfig.BulletSpread);
                        World.FireBullet(OwnerPlayer, Camera.Position, Camera.LookVector, recoil, 
                            GunConfig.BlockDamage, GunConfig.PlayerDamage);
                    }

                    muzzleFlash.Show();
                    World.GunFired(GunConfig.VerticalRecoil, GetHorizontalCameraRecoil(GunConfig.HorizontalRecoil), GunConfig.ModelKickback);

                    if (!GlobalNetwork.IsServer || !DashCMD.GetCVar<bool>("ch_infammo"))
                        CurrentMag--;
                }
            }
            else if (GlobalNetwork.IsConnected && GlobalNetwork.IsClient)
            {
                if (ServerMag > 0)
                {
                    //for (int i = 0; i < GunConfig.BulletsPerShot; i++)
                    {
                        Vector3 recoil = GetRecoilJiggle(GunConfig.BulletSpread, GunConfig.BulletSpread);
                        World.FireBullet(OwnerPlayer, Camera.Position, Camera.LookVector, recoil,
                            GunConfig.BlockDamage, GunConfig.PlayerDamage);
                    }

                    muzzleFlash.Show();
                    World.GunFired(GunConfig.VerticalRecoil, GetHorizontalCameraRecoil(GunConfig.HorizontalRecoil), GunConfig.ModelKickback);
                }
            }
        }

        public void CancelReload()
        {
            IsReloading = false;
        }

        float GetHorizontalCameraRecoil(float range)
        {
            return (float)Maths.Random.NextDouble() * range * Maths.RandomSign(0.5f);
        }

        Vector3 GetRecoilJiggle(float hscatter, float vscatter)
        {
            float hrecoil = (float)Maths.Random.NextDouble() * hscatter * Maths.RandomSign(0.5f);
            float vrecoil = (float)Maths.Random.NextDouble() * vscatter * Maths.RandomSign(0.5f);

            return Camera.TransformNormalXY(new Vector3(hrecoil, vrecoil, 0));
        }

        public void Reload()
        {
            int stored = GlobalNetwork.IsClient && GlobalNetwork.IsConnected ? ServerStoredAmmo : StoredAmmo;
            int mag = GlobalNetwork.IsClient && GlobalNetwork.IsConnected ? ServerMag : CurrentMag;

            if (IsReloading || stored <= 0 || mag >= GunConfig.MagazineSize)
                return;

            reloadTimeLeft = GunConfig.ReloadTime;
            IsReloading = true;
        }

        void RefillAmmo()
        {
            int ammoNeeded = GunConfig.MagazineSize - CurrentMag;
            int ammoToTake = Math.Min(ammoNeeded, StoredAmmo);
            StoredAmmo -= ammoToTake;
            CurrentMag += ammoToTake;
        }

        protected abstract GunConfig InitializeGunConfig();
        protected override ItemConfig InitializeConfig()
        {
            return GunConfig = InitializeGunConfig();
        }

        protected override void Update(float deltaTime)
        {
            if (GlobalNetwork.IsClient)
            {
                Camera cam = Dash.Engine.Graphics.Camera.Active;

                if (OwnerPlayer.IsAiming)
                {
                    modelAnim.SetTarget(AimModelOffset);
                    fovAnim.SetTarget(GunConfig.AimFOV);
                    cam.FPSMouseSensitivity = cam.DefaultFPSMouseSensitivity * GunConfig.AimMouseSensitivityScale;
                }
                else
                {
                    modelAnim.SetTarget(normalModelOffset);
                    fovAnim.SetTarget(70);
                    cam.FPSMouseSensitivity = cam.DefaultFPSMouseSensitivity;
                }

                modelAnim.Step(deltaTime * 12);
                fovAnim.Step(deltaTime * 12);
                cam.FOV = fovAnim.Value;
                ModelOffset = modelAnim.Value;
            }

            if (IsReloading && reloadTimeLeft > 0)
                reloadTimeLeft -= deltaTime;
            else if (IsReloading && reloadTimeLeft <= 0)
            {
                IsReloading = false;
                RefillAmmo();
            }

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            if (!IsReloading)
                base.Draw();
        }
    }
}
