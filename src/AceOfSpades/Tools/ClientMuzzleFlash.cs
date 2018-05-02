using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;

namespace AceOfSpades.Tools
{
    public class ClientMuzzleFlash : IMuzzleFlash
    {
        MasterRenderer renderer;
        Player ownerPlayer;
        SimpleCamera camera;

        DebugCube flashCube;
        Light light;
        int muzzleFlash;
        float replicatedMuzzleFlashCooldown;

        public ClientMuzzleFlash(MasterRenderer renderer, Player ownerPlayer)
        {
            this.renderer = renderer;
            this.ownerPlayer = ownerPlayer;
            camera = ownerPlayer.GetCamera();

            light = new Light(Vector3.Zero, LightType.Point, 3, Color.White, new Vector3(1, 0, 0.05f));
            light.Visible = false;
            renderer.Lights.Add(light);

            flashCube = new DebugCube(Color4.Yellow, 0.7f);
            flashCube.ApplyNoLighting = true;
        }

        public void Show()
        {
            muzzleFlash = 2;
            light.Visible = true;
        }

        public void Hide()
        {
            muzzleFlash = 0;
            light.Visible = false;
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
            if (muzzleFlash > 0)
            {
                muzzleFlash--;
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
