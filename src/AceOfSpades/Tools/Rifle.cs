using Dash.Engine;
using Dash.Engine.Graphics;

/* Rifle.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class Rifle : Gun
    {
        public Rifle(ItemManager itemManager, MasterRenderer renderer)
            : base(itemManager, renderer)
        {
            ModelOffset = new Vector3(-3.15f, -3.6f, -0.5f);
            AimModelOffset = new Vector3(-0.495f, -3.1f, -0.5f);
            ThirdpersonScale = 0.7f;
            MuzzleFlashOffset = new Vector3(0.5f, 2, 14);
            LoadModel("Models/rifle.aosm");
        }

        protected override GunConfig InitializeGunConfig()
        {
            return new GunConfig()
            {
                MagazineSize = 8,
                MaxStoredMags = 5,
                PlayerDamage = 30,
                BlockDamage = 2,
                IsPrimaryAutomatic = false,
                PrimaryFireDelay = 0.3f,
                BulletSpread = 0.001f,
                AimBulletSpread = 0,
                HorizontalRecoil = 0.4f,
                VerticalRecoil = 1.75f,
                ModelKickback = 0.7f,
                AimFOV = 35,
                AimMouseSensitivityScale = 0.35f,
                ReloadTime = 3f,
                PrimaryFireAudio = new GunAudioConfig
                {
                    LocalFilepath = "Weapons/Rifle/fire-local.wav",
                    ReplicatedFilepath = "Weapons/Rifle/fire.wav",
                    FarFilepath = "Weapons/Rifle/fire-far.wav",
                    NearMaxDistance = 500,
                    FarMinDistance = 500,
                    FarMaxDistance = 1200,
                    LocalGain = 0.2f,
                    ReplicatedGain = 0.3f
                },
                ReloadAudio = new GunAudioConfig
                {
                    LocalFilepath = "Weapons/Rifle/reload-local.wav",
                    LocalGain = 0.2f,
                    ReplicatedFilepath = "Weapons/Rifle/reload.wav",
                    ReplicatedGain = 0.3f,
                    NearMaxDistance = 50f
                }
            };
        }
    }
}
