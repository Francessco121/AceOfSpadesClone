using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using System;
using System.Collections.Generic;
using AceOfSpades.Graphics;
using Dash.Engine.Audio;

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

        readonly AudioSource fireAudioSource;
        readonly AudioSource fireFarAudioSource;
        readonly AudioSource reloadAudioSource;
        readonly AudioSource dryFireAudioSource;

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

            if (!GlobalNetwork.IsServer)
            {
                if (itemManager.IsReplicated)
                {
                    if (GunConfig.PrimaryFireAudio?.ReplicatedFilepath != null)
                    {
                        fireAudioSource = LoadAudioFromConfig(GunConfig.PrimaryFireAudio, replicated: true, far: false);
                    }

                    if (GunConfig.PrimaryFireAudio?.FarFilepath != null)
                    {
                        fireFarAudioSource = LoadAudioFromConfig(GunConfig.PrimaryFireAudio, replicated: true, far: true);
                    }

                    if (GunConfig.ReloadAudio?.ReplicatedFilepath != null)
                    {
                        reloadAudioSource = LoadAudioFromConfig(GunConfig.ReloadAudio, replicated: true);
                    }
                }
                else
                {
                    if (GunConfig.PrimaryFireAudio?.LocalFilepath != null)
                    {
                        fireAudioSource = LoadAudioFromConfig(GunConfig.PrimaryFireAudio);
                    }

                    if (GunConfig.ReloadAudio?.LocalFilepath != null)
                    {
                        reloadAudioSource = LoadAudioFromConfig(GunConfig.ReloadAudio);
                    }

                    AudioBuffer dryFireBuffer = AssetManager.LoadSound("Weapons/dry-fire.wav");

                    if (dryFireBuffer != null)
                    {
                        dryFireAudioSource = new AudioSource(dryFireBuffer);
                        dryFireAudioSource.IsSourceRelative = true;
                        dryFireAudioSource.Gain = 0.5f;
                    }
                }
            }
        }

        AudioSource LoadAudioFromConfig(GunAudioConfig config, bool replicated = false, bool far = false)
        {
            string filePath;
            if (replicated)
            {
                if (far)
                    filePath = config.FarFilepath;
                else
                    filePath = config.ReplicatedFilepath;
            }
            else
                filePath = config.LocalFilepath;

            AudioBuffer buffer = AssetManager.LoadSound(filePath);

            if (buffer == null)
                return null;

            AudioSource audioSource = new AudioSource(buffer);
            audioSource.Gain = replicated ? config.ReplicatedGain : config.LocalGain;

            if (replicated)
            {
                audioSource.MaxDistance = far ? config.FarMaxDistance : config.NearMaxDistance;
            }
            else
            {
                audioSource.IsSourceRelative = true;
            }

            return audioSource;
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

                reloadAudioSource?.Stop();
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

                    if (!GlobalNetwork.IsServer)
                        fireAudioSource?.Play();
                }
                else
                {
                    dryFireAudioSource?.Play();
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

                    fireAudioSource?.Play();
                }
                else
                {
                    dryFireAudioSource?.Play();
                }
            }
        }

        public override void OnReplicatedPrimaryFire()
        {
            if (fireFarAudioSource != null
                && (Dash.Engine.Graphics.Camera.Active.Position - OwnerPlayer.Transform.Position).Length >= GunConfig.PrimaryFireAudio.FarMinDistance)
            {
                fireFarAudioSource?.Play();
            }
            else
            {
                fireAudioSource?.Play();
            }

            base.OnReplicatedPrimaryFire();
        }

        public void OnReplicatedReload()
        {
            reloadAudioSource?.Play();
        }

        public void CancelReload()
        {
            IsReloading = false;
            reloadAudioSource?.Stop();
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

        public virtual void Reload()
        {
            int stored = GlobalNetwork.IsClient && GlobalNetwork.IsConnected ? ServerStoredAmmo : StoredAmmo;
            int mag = GlobalNetwork.IsClient && GlobalNetwork.IsConnected ? ServerMag : CurrentMag;

            if (IsReloading || stored <= 0 || mag >= GunConfig.MagazineSize)
                return;

            reloadTimeLeft = GunConfig.ReloadTime;
            IsReloading = true;

            reloadAudioSource?.Play();
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

        public override void UpdateReplicated(float deltaTime)
        {
            if (fireAudioSource != null && fireAudioSource.Position != OwnerPlayer.Transform.Position)
            {
                fireAudioSource.Position = OwnerPlayer.Transform.Position;
            }

            if (fireFarAudioSource != null && fireFarAudioSource.Position != OwnerPlayer.Transform.Position)
            {
                fireFarAudioSource.Position = OwnerPlayer.Transform.Position;
            }

            if (reloadAudioSource != null && reloadAudioSource.Position != OwnerPlayer.Transform.Position)
            {
                reloadAudioSource.Position = OwnerPlayer.Transform.Position;
            }

            base.UpdateReplicated(deltaTime);
        }

        protected override void Draw()
        {
            if (!IsReloading)
                base.Draw();
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                fireAudioSource?.Dispose();
                fireFarAudioSource?.Dispose();
                reloadAudioSource?.Dispose();
                dryFireAudioSource?.Dispose();
            }

            base.Dispose();
        }
    }
}
