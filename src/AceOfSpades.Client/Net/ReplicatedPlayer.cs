using AceOfSpades.Net;
using AceOfSpades.Tools;
using Dash.Engine;
using Dash.Engine.Animation;
using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;

/* ReplicatedPlayer.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client.Net
{
    public class ReplicatedPlayer : ClientPlayer
    {
        Vector3Anim interpPos;
        FloatAnim yawAnim;
        FloatAnim pitchAnim;
        SimpleCamera camera;

        public ReplicatedPlayer(MasterRenderer renderer, World world, SimpleCamera camera, Vector3 position, Team team)
            : base(renderer, world, camera, position, team)
        {
            this.camera = camera;

            interpPos = new Vector3Anim();
            interpPos.SnapTo(position);

            yawAnim = new FloatAnim();
            pitchAnim = new FloatAnim();

            // This is fully server controlled
            ItemManager.DontUpdateItems = true;
            ItemManager.IsReplicated = true;
            CharacterController.IsEnabled = false;

            CreateStarterBackpack();
        }

        protected override void Update(float deltaTime)
        {
            if (StateInfo != null)
            {
                // Update viewbob
                Viewbob.UpdateReplicated(deltaTime);

                // Step each value animation
                float interpStep = DashCMD.GetCVar<float>("cl_interp_rep") * deltaTime;
                interpPos.Step(interpStep);
                yawAnim.Step(interpStep);
                pitchAnim.Step(interpStep);

                // Update each property affected by interpolation
                Transform.Position = interpPos.Value;

                camera.Yaw = yawAnim.Value;
                camera.Pitch = pitchAnim.Value;

                // Update our "fake" camera
                camera.Position = Transform.Position + new Vector3(0, Size.Y / 2f - 1.1f, 0);
                camera.Update(deltaTime);

                // Update flashlight
                flashlight.Position = camera.Position;
                flashlight.Direction = -camera.LookVector;

                // Update the item manager
                ItemManager.UpdateReplicated(deltaTime);
            }

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            if (StateInfo == null)
                return;

            CharacterController.Size.Y = CharacterController.IsCrouching 
                ? CharacterController.CrouchHeight 
                : CharacterController.NormalHeight;

            // Setup world matrix
            Renderer.WorldMatrix = Matrix4.CreateScale(VoxelObject.MeshScale)
                * Matrix4.CreateRotationY(MathHelper.ToRadians(-camera.Yaw))
                * Matrix4.CreateTranslation(Transform.Position);

            if (DrawCollider)
                debugRenderer.Batch(CharacterController.GetCollider());

            ItemManager.Draw(entRenderer, Viewbob);

            base.Draw();
        }

        public override void OnClientInbound(PlayerSnapshot snapshot)
        {
            ItemManager.Equip(snapshot.SelectedItem, forceEquip: true);
            ItemManager.MuzzleFlashIterations += snapshot.TimesShot;

            yawAnim.SetTarget(snapshot.CamYaw);
            pitchAnim.SetTarget(snapshot.CamPitch);
            interpPos.SetTarget(new Vector3(snapshot.X, snapshot.Y, snapshot.Z));

            flashlight.Visible = snapshot.IsFlashlightOn;
            
            CharacterController.IsCrouching = snapshot.IsCrouching;
        }
    }
}
