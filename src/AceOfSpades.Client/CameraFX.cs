using AceOfSpades.Characters;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Graphics;

/* CameraFX.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public class CameraFX
    {
        const float RECOIL_SMOOTH = 0.25f; // 4 iterations

        public bool IsShaking { get { return shakeFactorAnim.I < 1; } }

        Camera camera;
        Player player;

        Vector3Anim offsetAnim;
        FloatAnim shakeFactorAnim;
        FloatAnim rollAnim;

        float shakeFalloffRate;

        float recoilTimes;
        float recoilHFactor;
        float recoilVFactor;

        int bobI = 0;
        Vector3 lastPlayerPos;

        public CameraFX(Player player, Camera camera)
        {
            this.player = player;
            this.camera = camera;

            offsetAnim = new Vector3Anim();
            shakeFactorAnim = new FloatAnim();
            rollAnim = new FloatAnim();
        }

        public void ResetCamera()
        {
            camera.PreViewOffset = Vector3.Zero;
            camera.Roll = 0;
        }

        public void ShakeCamera(float duration, float factor)
        {
            shakeFalloffRate = 1f / duration;

            shakeFactorAnim.SnapTo(factor);
            shakeFactorAnim.SetTarget(0);
        }

        public void Recoil(float horizontal, float vertical)
        {
            if (!camera.SmoothCamera)
            {
                recoilHFactor = horizontal * RECOIL_SMOOTH;
                recoilVFactor = vertical * RECOIL_SMOOTH;
                recoilTimes = 1f / RECOIL_SMOOTH;
            }
            else
                camera.ApplyRecoil(vertical, horizontal);
        }

        Vector3 GetShake()
        {
            if (shakeFactorAnim.I == 1)
                return Vector3.Zero;
            else
            {
                float x = Maths.RandomRange(-shakeFactorAnim.Value, shakeFactorAnim.Value);
                float y = Maths.RandomRange(-shakeFactorAnim.Value, shakeFactorAnim.Value);
                float z = Maths.RandomRange(-shakeFactorAnim.Value, shakeFactorAnim.Value);

                return new Vector3(x, y, z);
            }
        }

        public void Update(float deltaTime)
        {
            Transform playerTransform = player.Transform;
            CharacterController cc = player.CharacterController;

            float distMoved = (playerTransform.Position - lastPlayerPos).Length;
            bool sprinting = distMoved > 0 && !Input.GetControl("Walk") 
                && !player.IsAiming && Input.GetControl("Sprint") && !cc.IsCrouching;

            rollAnim.SetTarget(sprinting ? player.StrafeDir * 0.75f : 0);

            if (sprinting && cc.IsGrounded && cc.IsMoving)
            {
                if (offsetAnim.I == 1)
                {
                    if (bobI == 0)
                    {
                        offsetAnim.SetTarget(new Vector3(0, 0.3f, 0));
                        bobI = 1;
                    }
                    else if (bobI == 1)
                    {
                        offsetAnim.SetTarget(new Vector3(0, -0.3f, 0));
                        bobI = 0;
                    }
                }
            }
            else if (offsetAnim.Target != Vector3.Zero)
            {
                bobI = 0;
                offsetAnim.SetTarget(Vector3.Zero);
            }

            offsetAnim.Step(deltaTime * 7);
            shakeFactorAnim.Step(deltaTime * shakeFalloffRate);
            rollAnim.Step(deltaTime * 10);

            if (recoilTimes >= 0)
                recoilTimes--;

            lastPlayerPos = playerTransform.Position;
        }

        public void Apply()
        {
            camera.PreViewOffset = offsetAnim.Value;
            camera.PostViewOffset = GetShake();
            camera.Roll = rollAnim.Value;

            if (recoilTimes >= 0)
                camera.ApplyRecoil(recoilVFactor, recoilHFactor);
        }
    }
}
