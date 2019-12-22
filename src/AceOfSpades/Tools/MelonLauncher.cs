using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Audio;
using Dash.Engine.Graphics;

/* MelonLauncher.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class MelonLauncher : Weapon
    {
        const float AIM_FOV_SCALE = 40f / 70f;
        const float AIM_MOUSE_SENSITIVITY_SCALE = 0.6f;

        Vector3 aimModelOffset;
        Vector3 normalModelOffset;
        FloatAnim fovAnim;
        Vector3Anim modelAnim;

        readonly AudioSource throwAudioSource;

        public MelonLauncher(ItemManager itemManager, MasterRenderer renderer) 
            : base(renderer, itemManager, ItemType.MelonLauncher)
        {
            ModelOffset = new Vector3(-3.15f, -4f, 0.5f);
            aimModelOffset = new Vector3(-0.75f, -2.35f, -6f);
            ThirdpersonScale = 0.7f;

            fovAnim = new FloatAnim();
            modelAnim = new Vector3Anim();

            LoadModel("Models/melon-launcher.aosm");

            if (!GlobalNetwork.IsServer)
            {
                if (!itemManager.IsReplicated)
                {
                    AudioBuffer throwAudioBuffer = AssetManager.LoadSound("Weapons/Grenade/throw.wav");

                    if (throwAudioBuffer != null)
                    {
                        throwAudioSource = new AudioSource(throwAudioBuffer);
                        throwAudioSource.IsSourceRelative = true;
                        throwAudioSource.Pitch = 1.5f;
                        throwAudioSource.Gain = 0.2f;
                    }
                }
            }
        }

        protected override ItemConfig InitializeConfig()
        {
            ItemConfig config = base.InitializeConfig();
            config.PrimaryFireDelay = 1f;

            return config;
        }

        public override bool CanEquip()
        {
            return OwnerPlayer.NumMelons > 0 && base.CanEquip();
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
                cam.FOV = cam.DefaultFOV;
                cam.FPSMouseSensitivity = cam.DefaultFPSMouseSensitivity;
            }

            ModelOffset = normalModelOffset;

            base.OnUnequip();
        }

        public override bool CanPrimaryFire()
        {
            return !OwnerPlayer.IsSprinting && base.CanPrimaryFire();
        }

        protected override void OnPrimaryFire()
        {
            if (GlobalNetwork.IsServer)
                return;

            if (OwnerPlayer.NumMelons > 0)
            {
                Matrix4 matrix;

                if (OwnerPlayer.IsRenderingThirdperson)
                {
                    matrix = Matrix4.CreateTranslation(1.8125f, -1.8125f, 0);
                }
                else
                {
                    if (OwnerPlayer.IsAiming)
                    {
                        matrix = Matrix4.CreateTranslation(1.8125f, -1.8125f, 0);
                    }
                    else
                    {
                        matrix = Matrix4.CreateTranslation(-1, -2.5f, 0);
                    }
                }

                matrix = matrix
                    * Matrix4.CreateRotationX(MathHelper.ToRadians(Camera.Pitch))
                    * Matrix4.CreateRotationY(MathHelper.ToRadians(-Camera.Yaw) + MathHelper.Pi);

                if (OwnerPlayer.IsRenderingThirdperson)
                {
                    matrix *= Matrix4.CreateTranslation(OwnerPlayer.Transform.Position 
                        + Dash.Engine.Graphics.Camera.Active.FirstPersonLockOffset);
                }
                else
                {
                    matrix *= Matrix4.CreateTranslation(Camera.Position);
                }

                Vector3 pos = matrix.ExtractTranslation();

                World.ShootMelon(OwnerPlayer, pos + Camera.ViewMatrix.Forward() * 2, Camera.LookVector);

                if (!GlobalNetwork.IsConnected)
                    OwnerPlayer.NumMelons--;

                throwAudioSource?.Play();
            }
        }

        public override bool CanSecondaryFire()
        {
            return true;
        }

        protected override void Update(float deltaTime)
        {
            if (GlobalNetwork.IsClient)
            {
                Camera cam = Dash.Engine.Graphics.Camera.Active;

                if (OwnerPlayer.IsAiming)
                {
                    modelAnim.SetTarget(aimModelOffset);
                    fovAnim.SetTarget(cam.DefaultFOV * AIM_FOV_SCALE);
                    cam.FPSMouseSensitivity = cam.DefaultFPSMouseSensitivity * AIM_MOUSE_SENSITIVITY_SCALE;
                }
                else
                {
                    modelAnim.SetTarget(normalModelOffset);
                    fovAnim.SetTarget(cam.DefaultFOV);
                    cam.FPSMouseSensitivity = cam.DefaultFPSMouseSensitivity;
                }

                modelAnim.Step(deltaTime * 12);
                fovAnim.Step(deltaTime * 12);
                cam.FOV = fovAnim.Value;
                ModelOffset = modelAnim.Value;
            }

            base.Update(deltaTime);
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                throwAudioSource?.Dispose();
            }

            base.Dispose();
        }
    }
}
