using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;

namespace AceOfSpades.Tools
{
    public class ClientMuzzleFlash : IMuzzleFlash
    {
        const float MUZZLE_FLASH_COOLDOWN = 0.15f;
        const float MUZZLE_FLASH_LIGHT_POWER = 1f;

        MasterRenderer renderer;
        Player ownerPlayer;
        SimpleCamera camera;

        DebugCube flashCube;
        Light light;
        float muzzleFlashTime;
        float replicatedMuzzleFlashCooldown;

        public ClientMuzzleFlash(MasterRenderer renderer, Player ownerPlayer)
        {
            this.renderer = renderer;
            this.ownerPlayer = ownerPlayer;
            camera = ownerPlayer.GetCamera();

            light = new Light(Vector3.Zero, LightType.Point, 0f, Color.White, new Vector3(1, 0, 0.1f));
            light.Visible = false;
            renderer.Lights.Add(light);

            flashCube = new DebugCube(Color4.Yellow, 0.7f);
            flashCube.ApplyNoLighting = true;
        }

        public void Show()
        {
            muzzleFlashTime = MUZZLE_FLASH_COOLDOWN;
            light.Visible = true;
        }

        public void Hide()
        {
            muzzleFlashTime = 0;
            light.Visible = false;
        }

        public void Update(float deltaTime)
        {
            if (muzzleFlashTime > 0)
                muzzleFlashTime -= deltaTime;
        }

        public bool UpdateReplicated(Gun gun, int flashIterations, float deltaTime)
        {
            if (replicatedMuzzleFlashCooldown <= 0 && flashIterations > 0)
            {
                replicatedMuzzleFlashCooldown = gun.GunConfig.PrimaryFireDelay;

                Show();
                return true;
            }
            else
            {
                replicatedMuzzleFlashCooldown -= deltaTime;
                return false;
            }
        }

        public void Render(Gun gun, EntityRenderer entRenderer, ItemViewbob viewbob)
        {
            if (muzzleFlashTime > 0)
            {
                Matrix4 flashMatrix;

                if (ownerPlayer.IsRenderingThirdperson)
                {
                    flashMatrix =
                        Matrix4.CreateTranslation(gun.MuzzleFlashOffset)
                        * Matrix4.CreateScale(gun.ThirdpersonScale)
                        * Matrix4.CreateTranslation(0, 1.5f, -0.25f)
                        * Matrix4.CreateRotationZ(MathHelper.ToRadians(viewbob.CurrentTilt))
                        * Matrix4.CreateTranslation(gun.ModelOffset + viewbob.CurrentViewBob 
                            + new Vector3(-1.35f, 0, -viewbob.CurrentKickback + -2))
                        * Matrix4.CreateRotationX(MathHelper.ToRadians(camera.Pitch))
                        * Matrix4.CreateRotationY(MathHelper.ToRadians(-camera.Yaw) - MathHelper.Pi)
                        * Matrix4.CreateTranslation(ownerPlayer.Transform.Position 
                            + new Vector3(0, ownerPlayer.Size.Y / 2f - 1.5f, 0));
                }
                else
                {
                    flashMatrix =
                        Matrix4.CreateTranslation(gun.MuzzleFlashOffset)
                        * Matrix4.CreateRotationX(MathHelper.ToRadians(viewbob.CurrentSway.X))
                        * Matrix4.CreateRotationY(MathHelper.ToRadians(viewbob.CurrentSway.Y))
                        * Matrix4.CreateRotationZ(MathHelper.ToRadians(viewbob.CurrentTilt
                            + viewbob.CurrentSway.Y * 0.5f))
                        * Matrix4.CreateTranslation(gun.ModelOffset + viewbob.CurrentViewBob 
                        + new Vector3(0, 0, -viewbob.CurrentKickback))
                        * Matrix4.CreateRotationX(MathHelper.ToRadians(camera.Pitch))
                        * Matrix4.CreateRotationY(MathHelper.ToRadians(-camera.Yaw) - MathHelper.Pi)
                        * Matrix4.CreateTranslation(camera.OffsetPosition);
                }

                light.Position = flashMatrix.ExtractTranslation();
                light.LightPower = (muzzleFlashTime / MUZZLE_FLASH_COOLDOWN) * MUZZLE_FLASH_LIGHT_POWER;

                flashCube.RenderFront = !ownerPlayer.IsRenderingThirdperson;
                entRenderer.Batch(flashCube, flashMatrix);
            }
            else
                light.Visible = false;
        }

        public void Dispose()
        {
            renderer.Lights.Remove(light);
        }
    }
}
