using Dash.Engine;
using Dash.Engine.Graphics;

/* SMG.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class SMG : Gun
    {
        public SMG(ItemManager itemManager, MasterRenderer renderer)
            : base(itemManager, renderer)
        {
            ModelOffset = new Vector3(-3.15f, -4.35f, -0.5f);
            AimModelOffset = new Vector3(-0.495f, -4.6f, -0.5f);
            ThirdpersonScale = 0.7f;
            MuzzleFlashOffset = new Vector3(0.5f, 3, 10.75f);
            LoadModel("Models/smg.aosm");
        }

        protected override GunConfig InitializeGunConfig()
        {
            return new GunConfig()
            {
                MagazineSize = 30,
                MaxStoredMags = 6,
                PlayerDamage = 15f,
                BlockDamage = 1,
                IsPrimaryAutomatic = true,
                BulletSpread = 0.0085f,
                AimBulletSpread = 0.0045f,
                HorizontalRecoil = 0.2f,
                VerticalRecoil = 0.9f,
                PrimaryFireDelay = 0.1f,
                ModelKickback = 0.35f,
                AimFOV = 55,
                AimMouseSensitivityScale = 0.75f,
                ReloadTime = 3f,
                PrimaryFireAudio = new GunAudioConfig
                {
                    LocalFilepath = "Weapons/SMG/FireLocal.wav",
                    LocalGain = 0.5f,
                    ReplicatedFilepath = "Weapons/SMG/Fire.wav",
                    ReplicatedGain = 0.5f,
                    MaxDistance = 600
                },
                ReloadAudio = new GunAudioConfig
                {
                    LocalFilepath = "Weapons/SMG/ReloadLocal.wav"
                }
            };
        }
    }
}
