using Dash.Engine;
using Dash.Engine.Graphics;

/* Shotgun.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class Shotgun : Gun
    {
        public Shotgun(ItemManager itemManager, MasterRenderer renderer)
            : base(itemManager, renderer)
        {
            ModelOffset = new Vector3(-3.15f, -3.6f, -0.5f);
            AimModelOffset = new Vector3(-0.495f, -3.8f, -0.5f);
            ThirdpersonScale = 0.7f;
            MuzzleFlashOffset = new Vector3(0.5f, 2.5f, 14.5f);
            LoadModel("Models/shotgun.aosm");
        }

        protected override GunConfig InitializeGunConfig()
        {
            return new GunConfig()
            {
                MagazineSize = 8,
                MaxStoredMags = 8,
                PlayerDamage = 12,
                BlockDamage = 1,
                BulletsPerShot = 10,
                IsPrimaryAutomatic = false,
                BulletSpread = 0.07f,
                AimBulletSpread = 0.06f,
                PrimaryFireDelay = 0.9f,
                HorizontalRecoil = 3.5f,
                VerticalRecoil = 4.5f,
                ModelKickback = 1.25f,
                AimFOVScale = 60f / 70f,
                AimMouseSensitivityScale = 0.9f,
                ReloadTime = 3f,
                PrimaryFireAudio = new GunAudioConfig
                {
                    LocalFilepath = "Weapons/Shotgun/fire-local.wav",
                    ReplicatedFilepath = "Weapons/Shotgun/fire.wav",
                    FarFilepath = "Weapons/Shotgun/fire-far.wav",
                    NearMaxDistance = 500f,
                    FarMinDistance = 500f,
                    FarMaxDistance = 800f,
                    LocalGain = 0.2f,
                    ReplicatedGain = 0.3f
                }
            };
        }
    }
}
