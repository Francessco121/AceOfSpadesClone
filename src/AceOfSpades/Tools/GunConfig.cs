/* GunConfig.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class GunConfig : ItemConfig
    {
        public int MagazineSize;
        public int BulletsPerShot = 1;
        public int MaxStoredMags;

        public float ReloadTime = 1.5f;

        public float PlayerDamage;
        public int BlockDamage;

        public float BulletSpread = 0.01f;
        public float AimBulletSpread = 0.005f;

        public float AimFOV = 60;
        public float AimMouseSensitivityScale = 0.5f;

        public float VerticalRecoil = 1;
        public float HorizontalRecoil = 0.3f;

        public float ModelKickback = 1;

        public GunAudioConfig PrimaryFireAudio;
        public GunAudioConfig ReloadAudio;
    }
}
