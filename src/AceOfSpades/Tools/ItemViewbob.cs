using AceOfSpades.Characters;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Graphics;

/* ItemViewbob.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class ItemViewbob
    {
        const int BOB_ORIGIN = 0;
        const int BOB_TOP1 = 1;
        const int BOB_LEFT = 2;
        const int BOB_TOP2 = 3;

        const float DISTMOD_SPRINT = 1;
        const float DISTMOD_NORMAL = 0.6f;
        const float DISTMOD_WALK = 0.2f;
        const float DISTMOD_CROUCH = 0.8f;

        const float SPEED_NORMAL = 5.5f;
        const float SPEED_WALK = 3.25f;
        const float SPEED_SPRINT = 7f;
        const float SPEED_CROUCH = 3f;

        const float SPRINT_TILT_YAW = 60;
        const float SPRINT_TILT_PITCH = 40;

        Vector3 bobOrigin = Vector3.Zero;
        Vector3 bobTop = new Vector3(0.25f, 0.4f, 0);
        Vector3 bobTopAiming = new Vector3(0.25f, 0.2f, 0);
        Vector3 bobLeft = new Vector3(0.5f, 0, 0);
        Vector3 bobOffset = new Vector3(-0.25f, 0, 0);
        Vector3 bobSprintOffset = new Vector3(-1, 0.1f, 1.9f);

        public Vector3 CurrentViewBob
        {
            get { return bobAnim.Value; }
        }

        public Vector3 CurrentSway
        {
            get { return swayAnim.Value; }
        }

        public float CurrentTilt
        {
            get { return tiltAnim.Value; }
        }

        public float CurrentKickback
        {
            get { return kickbackAnim.Value; }
        }

        FloatAnim tiltAnim;
        Vector3Anim bobAnim;
        Vector3Anim swayAnim;
        FloatAnim kickbackAnim;

        int bobState = 0;

        Player player;

        bool lastMoving;
        Vector3 lastPos;

        float lastYaw;
        float lastPitch;
        bool lastIsSprinting;
        bool lastIsAiming;
        ItemManager itemManager { get { return player.ItemManager; } }

        CharacterController cc;

        public ItemViewbob(Player player)
        {
            this.player = player;

            bobAnim = new Vector3Anim();
            tiltAnim = new FloatAnim();
            swayAnim = new Vector3Anim();
            kickbackAnim = new FloatAnim();

            lastYaw = Camera.Active.Yaw;
            lastPitch = Camera.Active.Pitch;

            cc = player.GetComponent<CharacterController>();
        }

        public void ApplyKickback(float amount)
        {
            kickbackAnim.SnapTo(kickbackAnim.Value + amount);
            kickbackAnim.SetTarget(0);
        }

        public void OnItemEquipped()
        {
            kickbackAnim.SnapTo(0);
        }

        float GetDistMod()
        {
            if (player.IsAiming)
                return DISTMOD_WALK;
            else if (cc.IsCrouching)
                return DISTMOD_CROUCH;
            else  if (Input.GetControl("Walk"))
                return DISTMOD_WALK;
            else if (Input.GetControl("Sprint"))
                return DISTMOD_SPRINT;
            else
                return DISTMOD_NORMAL;
        }

        float GetSpeed()
        {
            if (player.IsAiming)
                return SPEED_WALK;
            else if (cc.IsCrouching)
                return SPEED_CROUCH;
            else  if (Input.GetControl("Walk"))
                return SPEED_WALK;
            else if (Input.GetControl("Sprint"))
                return SPEED_SPRINT;
            else
                return SPEED_NORMAL;
        }

        public void Update(float deltaTime)
        {
            bool playerMoved = cc.IsMoving && Maths.Distance(lastPos, player.Transform.Position) > 0;
            bool heldItemIsGun = itemManager.SelectedItem != null && itemManager.SelectedItem.Type.HasFlag(ItemType.Gun);
            bool isSprinting = heldItemIsGun && player.IsSprinting && playerMoved;

            Vector3 useBobOffset = player.IsAiming ? bobOffset : isSprinting ? bobSprintOffset : Vector3.Zero;

            if (cc.IsMoving && Maths.Distance(lastPos, player.Transform.Position) > 0 && cc.IsGrounded)
            {
                if (!lastMoving || bobAnim.I == 1)
                {
                    // Switch state
                    if (bobState == BOB_ORIGIN)
                    {
                        bobState = BOB_TOP1;
                        bobAnim.SetTarget((bobTop + useBobOffset) * GetDistMod());
                    }
                    else if (bobState == BOB_TOP1)
                    {
                        bobState = BOB_LEFT;
                        bobAnim.SetTarget((bobLeft + useBobOffset) * GetDistMod());
                    }
                    else if (bobState == BOB_LEFT)
                    {
                        bobState = BOB_TOP2;
                        bobAnim.SetTarget((bobTop + useBobOffset) * GetDistMod());
                    }
                    else if (bobState == BOB_TOP2)
                    {
                        bobState = BOB_ORIGIN;
                        bobAnim.SetTarget(bobOrigin + useBobOffset * GetDistMod());
                    }
                }
            }
            else if (!cc.IsGrounded)
            {
                if (bobState != BOB_TOP1 || isSprinting != lastIsSprinting || lastIsAiming != player.IsAiming)
                {
                    bobState = BOB_TOP1;
                    Vector3 bob = player.IsAiming ? bobTopAiming : bobTop;
                    bobAnim.SetTarget((bob + useBobOffset) * GetDistMod());
                }
            }
            else
            {
                if (bobState != BOB_ORIGIN || bobAnim.Target != bobOrigin)
                {
                    // Return to origin
                    bobState = BOB_ORIGIN;
                    bobAnim.SetTarget(bobOrigin);
                }
            }

            if (player.IsStrafing)
            {
                float tilt = player.IsAiming ? 2 : 5;

                if (player.StrafeDir > 0)
                    tiltAnim.SetTarget(tilt);
                else
                    tiltAnim.SetTarget(-tilt);
            }
            else
            {
                if (tiltAnim.Target != 0)
                    tiltAnim.SetTarget(0);
            }

            Vector3 deltaPos = player.Transform.Position - lastPos;
            if (deltaPos.Y > 5)
                deltaPos.Y = 0.75f;

            float yawOffset = 0, pitchOffset = 0;
            if (isSprinting)
            {
                yawOffset = SPRINT_TILT_YAW;
                pitchOffset = SPRINT_TILT_PITCH;
            }

            float pitchSwayDamper = player.IsAiming ? 0.2f : 1f;
            float yawSwayDamper = player.IsAiming ? 0.7f : 1f;

            swayAnim.SetTarget(new Vector3(
                (lastPitch - Camera.Active.Pitch + (deltaPos.Y * 8) + pitchOffset) * pitchSwayDamper, 
                (Camera.Active.Yaw - lastYaw + yawOffset) * yawSwayDamper, 
                0));

            bobAnim.Step(deltaTime * GetSpeed());
            tiltAnim.Step(deltaTime * 5);
            swayAnim.Step(deltaTime * 8);
            kickbackAnim.Step(deltaTime * 8);

            lastMoving = cc.IsMoving;
            lastPos = player.Transform.Position;

            lastYaw = Camera.Active.Yaw;
            lastPitch = Camera.Active.Pitch;
            lastIsSprinting = isSprinting;
            lastIsAiming = player.IsAiming;
        }

        public void UpdateReplicated(float deltaTime)
        {
            kickbackAnim.Step(deltaTime * 8);
        }
    }
}
