using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Audio;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;

/* Grenade.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class Grenade : Weapon
    {
        readonly AudioSource throwAudioSource;

        public Grenade(ItemManager itemManager, MasterRenderer renderer) 
            : base(renderer, itemManager, ItemType.Grenade)
        {
            ModelOffset = new Vector3(-3.15f, -3f, 3);
            LoadModel("Models/grenade.aosm");

            if (!GlobalNetwork.IsServer)
            {
                if (!itemManager.IsReplicated)
                {
                    throwAudioSource = new AudioSource(AssetManager.LoadSound("Weapons/Grenade/Throw.wav"));
                    throwAudioSource.IsSourceRelative = true;
                }
            }
        }

        protected override ItemConfig InitializeConfig()
        {
            ItemConfig config = base.InitializeConfig();
            config.PrimaryFireDelay = 0.3f;
            config.SecondaryFireDelay = 0.3f;

            return config;
        }

        protected override void OnPrimaryFire()
        {
            ThrowGrenade(100);
            secondaryCooldown = Config.SecondaryFireDelay;
            base.OnPrimaryFire();
        }

        protected override void OnSecondaryFire()
        {
            ThrowGrenade(60);
            primaryCooldown = Config.PrimaryFireDelay;
            base.OnSecondaryFire();
        }

        void ThrowGrenade(float power)
        {
            if (GlobalNetwork.IsServer)
                return;

            if (OwnerPlayer.NumGrenades > 0)
            {
                Vector3 pos = OwnerPlayer.IsRenderingThirdperson
                    ? OwnerPlayer.Transform.Position + Dash.Engine.Graphics.Camera.Active.FirstPersonLockOffset
                    : Camera.Position;

                World.ThrowGrenade(OwnerPlayer, pos + Camera.LookVector * 2, Camera.LookVector, power);

                if (!GlobalNetwork.IsConnected)
                    OwnerPlayer.NumGrenades--;

                throwAudioSource?.Play();
            }
        }

        protected override void Draw()
        {
            if (OwnerPlayer.NumGrenades > 0)
                base.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (!IsDisposed)
            {
                throwAudioSource?.Dispose();
            }
        }
    }
}
