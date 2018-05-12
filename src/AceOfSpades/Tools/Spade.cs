using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Audio;
using Dash.Engine.Graphics;

/* Spade.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class Spade : Weapon
    {
        public const float MODIFY_RANGE = Block.CUBE_SIZE * 4;
        const float PLAYER_DAMAGE = 40;

        const float cooldownBringBack = 0.125f;
        float cooldown;

        static DebugCube cursorCube;

        EntityRenderer entRenderer;
        IndexPosition globalMousePosition;
        bool mouseOverBlock;

        readonly AudioSource hitBlockAudioSource;
        readonly AudioSource missAudioSource;

        public Spade(ItemManager itemManager, MasterRenderer renderer) 
            : base(renderer, itemManager, ItemType.Spade)
        {
            ModelOffset = new Vector3(-3f, -5f, 3f);
            LoadModel("Models/spade.aosm");

            if (GlobalNetwork.IsClient)
            {
                entRenderer = renderer.GetRenderer3D<EntityRenderer>();

                if (cursorCube == null)
                {
                    cursorCube = new DebugCube(Color4.Black, Block.CUBE_SIZE);
                    cursorCube.RenderAsWireframe = true;
                    cursorCube.ApplyNoLighting = true;
                    cursorCube.OnlyRenderFor = RenderPass.Normal;
                }

                if (!itemManager.IsReplicated)
                {
                    AudioBuffer hitBlockAudioBuffer = AssetManager.LoadSound("Weapons/Spade/hit-block.wav");

                    if (hitBlockAudioBuffer != null)
                    {
                        hitBlockAudioSource = new AudioSource(hitBlockAudioBuffer);
                        hitBlockAudioSource.IsSourceRelative = true;
                        hitBlockAudioSource.Gain = 0.2f;
                    }

                    AudioBuffer missAudioBuffer = AssetManager.LoadSound("Weapons/Spade/miss.wav");

                    if (missAudioBuffer != null)
                    {
                        missAudioSource = new AudioSource(missAudioBuffer);
                        missAudioSource.IsSourceRelative = true;
                        missAudioSource.Gain = 0.5f;
                    }
                }
            }
        }

        protected override ItemConfig InitializeConfig()
        {
            return new ItemConfig()
            {
                IsPrimaryAutomatic = true,
                PrimaryFireDelay = 0.2f
            };
        }

        protected override void OnPrimaryFire()
        {
            if (GlobalNetwork.IsConnected)
            {
                World.FireBullet(OwnerPlayer, Camera.Position, Camera.LookVector, 
                    Vector3.Zero, 0, PLAYER_DAMAGE, MODIFY_RANGE);
            }
            
            TerrainRaycastResult result = World.TerrainPhysics.Raycast(
                new Ray(Camera.Position, Camera.LookVector), true, MODIFY_RANGE);

            if (result.Intersects)
            {
                hitBlockAudioSource?.Play();

                if (GlobalNetwork.IsServer || !GlobalNetwork.IsConnected)
                {
                    Block block = result.Chunk[result.BlockIndex.Value];

                    if (block.Health <= 3)
                        OwnerPlayer.NumBlocks++;

                    if (!GlobalNetwork.IsConnected)
                        result.Chunk.DamageBlock(result.BlockIndex.Value, 3);
                    else
                    {
                        byte newHealth = (byte)MathHelper.Clamp(block.Health - 3, 0, 255);

                        if (newHealth > 0)
                        {
                            block.Data.SetUpper(newHealth);
                            World.SetBlock(result.Chunk.IndexPosition, result.BlockIndex.Value, block, false);
                        }
                        else
                            World.SetBlock(result.Chunk.IndexPosition, result.BlockIndex.Value, Block.AIR, false);
                    }
                }
            }
            else
            {
                missAudioSource?.Play();
            }

            cooldown = Config.PrimaryFireDelay;
            base.OnPrimaryFire();
        }

        float GetTilt()
        {
            if (cooldown > cooldownBringBack)
                return Interpolation.Linear(15, 25, (cooldown - cooldownBringBack) / cooldownBringBack);
            else
                return Interpolation.Linear(25, 15, (cooldown - cooldownBringBack) / cooldownBringBack);
        }

        protected override void Update(float deltaTime)
        {
            ModelRotation = new Vector3(GetTilt(), -10, 0);

            if (cooldown > 0)
                cooldown -= deltaTime;

            TerrainRaycastResult result = World.TerrainPhysics.Raycast(
                new Ray(Camera.Position, Camera.LookVector), true, MODIFY_RANGE);

            if (result.Intersects)
            {
                globalMousePosition = Terrain.GetGlobalBlockCoords(result.Chunk.IndexPosition, result.BlockIndex.Value);
                mouseOverBlock = true;
            }
            else
                mouseOverBlock = false;

            base.Update(deltaTime);
        }

        protected override void Draw()
        {
            if (mouseOverBlock)
                entRenderer.Batch(cursorCube, Maths.CreateTransformationMatrix(globalMousePosition * Block.CUBE_3D_SIZE,
                    0, 0, 0, 1.01f));

            base.Draw();
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                hitBlockAudioSource?.Dispose();
                missAudioSource?.Dispose();
            }

            base.Dispose();
        }
    }
}
