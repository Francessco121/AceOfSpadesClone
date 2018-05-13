using AceOfSpades.Characters;
using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Graphics;

/* Item.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public abstract class Item : GameObject
    {
        public ItemType Type { get; }
        public ItemConfig Config { get; }
        public ItemManager Manager { get; }

        public float ThirdpersonScale { get; protected set; }
        public Vector3 ModelOffset { get; protected set; }
        public Vector3 ModelRotation { get; protected set; }

        protected Player OwnerPlayer { get { return Manager.OwnerPlayer; } }
        protected World World { get { return Manager.World; } }
        protected SimpleCamera Camera { get { return Manager.OwnerPlayer.GetCamera(); } }

        protected float primaryCooldown;
        protected float secondaryCooldown;

        protected VoxelRenderComponent Renderer { get; }

        public Item(ItemManager manager, ItemType type)
        {
            Manager = manager;
            Type = type;

            ThirdpersonScale = 1f;

            if (GlobalNetwork.IsClient)
            {
                Renderer = new VoxelRenderComponent();
                Renderer.OnlyRenderFor = RenderPass.Normal;
                AddComponent(Renderer);
            }

            Config = InitializeConfig();

            IsEnabled = false;
            IsDrawable = false;
        }

        public virtual bool CanEquip() { return true; }

        public virtual void OnEquip()
        {
            IsEnabled = true;
            IsDrawable = true;
        }

        public virtual void OnUnequip()
        {
            IsEnabled = false;
            IsDrawable = false;
        }

        public void PrimaryFire()
        {
            if (CanPrimaryFire())
            {
                primaryCooldown = Config.PrimaryFireDelay;
                OnPrimaryFire();
            }
        }

        public void ForcePrimaryFire()
        {
            OnPrimaryFire();
        }

        public virtual bool CanPrimaryFire()
        {
            return primaryCooldown <= 0;
        }

        public void SecondaryFire()
        {
            if (CanSecondaryFire())
            {
                secondaryCooldown = Config.SecondaryFireDelay;
                OnSecondaryFire();
            }
        }

        public virtual bool CanSecondaryFire()
        {
            return secondaryCooldown <= 0;
        }

        protected virtual void OnPrimaryFire() { }
        protected virtual void OnSecondaryFire() { }

        public virtual void OnReplicatedPrimaryFire() { }
        public virtual void OnReplicatedSecondaryFire() { }

        protected virtual ItemConfig InitializeConfig()
        {
            return new ItemConfig();
        }

        protected Matrix4 CalculateWorldMatrix(ItemViewbob viewbob)
        {
            SimpleCamera camera = OwnerPlayer.GetCamera();

            if (OwnerPlayer.IsRenderingThirdperson)
            {
                return Matrix4.CreateScale(ThirdpersonScale)
                    * Matrix4.CreateTranslation(0, 1.5f, -0.25f)
                    * Matrix4.CreateRotationX(MathHelper.ToRadians(ModelRotation.X))
                    * Matrix4.CreateRotationY(MathHelper.ToRadians(ModelRotation.Y))
                    * Matrix4.CreateRotationZ(MathHelper.ToRadians(viewbob.CurrentTilt + ModelRotation.Z))
                    * Matrix4.CreateTranslation(ModelOffset + viewbob.CurrentViewBob + new Vector3(-1.35f, 0, -viewbob.CurrentKickback + -2))
                    * Matrix4.CreateRotationX(MathHelper.ToRadians(camera.Pitch))
                    * Matrix4.CreateRotationY(MathHelper.ToRadians(-camera.Yaw) + MathHelper.Pi)
                    * Matrix4.CreateTranslation(OwnerPlayer.Transform.Position 
                        + new Vector3(0, OwnerPlayer.Size.Y / 2f - 1.5f, 0));
            }
            else
            {
                return Matrix4.CreateRotationX(MathHelper.ToRadians(ModelRotation.X + viewbob.CurrentSway.X))
                    * Matrix4.CreateRotationY(MathHelper.ToRadians(ModelRotation.Y + viewbob.CurrentSway.Y))
                    * Matrix4.CreateRotationZ(MathHelper.ToRadians(viewbob.CurrentTilt
                        + ModelRotation.Z + viewbob.CurrentSway.Y * 0.5f))
                    * Matrix4.CreateTranslation(ModelOffset + viewbob.CurrentViewBob + new Vector3(0, 0, -viewbob.CurrentKickback))
                    * Matrix4.CreateRotationX(MathHelper.ToRadians(camera.Pitch))
                    * Matrix4.CreateRotationY(MathHelper.ToRadians(-camera.Yaw) + MathHelper.Pi)
                    * Matrix4.CreateTranslation(camera.OffsetPosition);
            }
        }

        protected override void Update(float deltaTime)
        {
            if (primaryCooldown > 0)
                primaryCooldown -= deltaTime;

            if (secondaryCooldown > 0)
                secondaryCooldown -= deltaTime;

            base.Update(deltaTime);
        }

        public virtual void UpdateReplicated(float deltaTime) { }

        public virtual void Draw(ItemViewbob viewbob)
        {
            Renderer.RenderFront = !OwnerPlayer.IsRenderingThirdperson;
            Renderer.WorldMatrix = CalculateWorldMatrix(viewbob);
        }
    }
}
