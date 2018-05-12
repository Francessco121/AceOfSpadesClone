using AceOfSpades.Graphics;
using AceOfSpades.Net;
using Dash.Engine;
using Dash.Engine.Audio;
using Dash.Engine.Graphics;
using Dash.Engine.Physics;
using System;

/* BlockItem.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Tools
{
    public class BlockItem : Item
    {
        public const float PLACE_RANGE = Block.CUBE_SIZE * 4;

        public const int PaletteWidth = 12;
        public const int PaletteHeight = 12;

        public Color[,] Colors { get; private set; }
        public int ColorX { get; private set; } = PaletteWidth - 1;
        public int ColorY { get; private set; }

        MasterRenderer renderer;
        DebugCube cube;
        static DebugCube cursorCube;
        public Color BlockColor { get; set; }

        bool holdingDown;
        IndexPosition startGlobalPosition;
        IndexPosition endGlobalPosition;
        IndexPosition globalMousePosition;
        bool mouseOverBlock;

        PhysicsBodyComponent ownerPlayerPhysicsBody;
        EntityRenderer entRenderer;

        readonly AudioSource buildAudioSource;

        public BlockItem(ItemManager itemManager, MasterRenderer renderer) 
            : base(itemManager, ItemType.BlockItem)
        {
            this.renderer = renderer;

            ModelOffset = new Vector3(-1.75f, -1.75f, 2.5f);
            ownerPlayerPhysicsBody = OwnerPlayer.GetComponent<PhysicsBodyComponent>();

            if (!GlobalNetwork.IsServer)
            {
                entRenderer = renderer.GetRenderer3D<EntityRenderer>();

                cube = new DebugCube(Color4.White, 1.5f);
                Renderer.VoxelObject = cube.VoxelObject;

                if (cursorCube == null)
                {
                    cursorCube = new DebugCube(Color4.White, Block.CUBE_SIZE);
                    cursorCube.RenderAsWireframe = true;
                    cursorCube.ApplyNoLighting = true;
                    cursorCube.OnlyRenderFor = RenderPass.Normal;
                }

                Colors = new Color[PaletteHeight, PaletteWidth];

                for (int y = 0; y < PaletteHeight; y++)
                    for (int x = 0; x < PaletteWidth; x++)
                    {
                        if (y == 0)
                            Colors[y, x] = Maths.HSVToRGB(0, 0, Math.Max(x / (float)PaletteWidth, 0.05f));
                        else
                        {
                            int halfPalette = PaletteWidth / 2;
                            if (x > halfPalette)
                                Colors[y, x] = Maths.HSVToRGB(
                                    (y - 1) / ((float)PaletteHeight - 1) * 360,
                                    1f - Math.Max((x - halfPalette) / (float)halfPalette, 0.05f),
                                    1f);
                            else
                                Colors[y, x] = Maths.HSVToRGB(
                                    (y - 1) / ((float)PaletteHeight - 1) * 360,
                                    1f,
                                    Math.Max(x / (float)halfPalette, 0.05f));
                        }
                    }

                BlockColor = Colors[ColorY, ColorX];

                if (!itemManager.IsReplicated)
                {
                    AudioBuffer buildAudioBuffer = AssetManager.LoadSound("Weapons/Block/build.wav");

                    if (buildAudioBuffer != null)
                    {
                        buildAudioSource = new AudioSource(buildAudioBuffer);
                        buildAudioSource.IsSourceRelative = true;
                        buildAudioSource.Gain = 0.5f;
                    }
                }
            }
        }

        protected override ItemConfig InitializeConfig()
        {
            ItemConfig config = base.InitializeConfig();
            config.SecondaryFireDelay = 0.4f;
            config.PrimaryFireDelay = 0.4f;
            config.IsPrimaryAutomatic = true;
            config.IsSecondaryAutomatic = false;
            return config;
        }

        public override void OnUnequip()
        {
            // Cancel placement
            holdingDown = false;
            base.OnUnequip();
        }

        public override bool CanSecondaryFire()
        {
            return base.CanSecondaryFire() && CanPrimaryFire();
        }

        protected override void OnPrimaryFire()
        {
            if (GlobalNetwork.IsServer || OwnerPlayer.NumBlocks <= 0)
                return;

            TerrainRaycastResult result = World.TerrainPhysics.Raycast(
                new Ray(Camera.Position, Camera.ViewMatrix.Forward()), true, PLACE_RANGE);

            primaryCooldown = 0;

            if (result.Intersects)
            {
                IndexPosition ipos = ShiftIPos(result.BlockIndex.Value, result.IntersectionCubeSide.Value);
                IndexPosition newChunkPos;
                Chunk.WrapBlockCoords(ipos.X, ipos.Y, ipos.Z, result.Chunk.IndexPosition, out ipos, out newChunkPos);

                Chunk chunkToMod;
                if (World.Terrain.Chunks.TryGetValue(newChunkPos, out chunkToMod))
                {
                    if (IsBlockPlacementSafe(ipos, newChunkPos))
                    {
                        buildAudioSource?.Play();

                        Block block = new Block(Block.CUSTOM.Data, BlockColor.R,
                                BlockColor.G, BlockColor.B);

                        if (!GlobalNetwork.IsConnected)
                            OwnerPlayer.NumBlocks--;

                        if (!GlobalNetwork.IsConnected)
                            chunkToMod.SetBlock(block, ipos);
                        else
                            World.SetBlock(chunkToMod.IndexPosition, ipos, block, true);

                        primaryCooldown = Config.PrimaryFireDelay;
                    }
                }
            }

            base.OnPrimaryFire();
        }

        protected override void OnSecondaryFire()
        {
            TerrainRaycastResult result = World.TerrainPhysics.Raycast(
                new Ray(Camera.Position, Camera.ViewMatrix.Forward()), true, PLACE_RANGE);

            if (result.Intersects)
            {
                IndexPosition ipos = ShiftIPos(result.BlockIndex.Value, result.IntersectionCubeSide.Value);
                IndexPosition newChunkPos;
                Chunk.WrapBlockCoords(ipos.X, ipos.Y, ipos.Z, result.Chunk.IndexPosition, out ipos, out newChunkPos);

                startGlobalPosition = Terrain.GetGlobalBlockCoords(newChunkPos, ipos);
                endGlobalPosition = startGlobalPosition;
                holdingDown = true;
            }

            base.OnSecondaryFire();
        }

        void PickColor()
        {
            TerrainRaycastResult result = World.TerrainPhysics.Raycast(
                new Ray(Camera.Position, Camera.ViewMatrix.Forward()), true, PLACE_RANGE);

            if (result.Intersects)
            {
                IndexPosition blockIndex = result.BlockIndex.Value;
                BlockColor = result.Chunk[blockIndex].GetColor();
            }
        }

        protected override void Update(float deltaTime)
        {
            if (GlobalNetwork.IsClient)
            {
                if (Input.GetControlDown("PickColor"))
                    PickColor();

                int ocx = ColorX, ocy = ColorY;
                if (Input.GetKeyDown(Key.Up) && ColorY > 0) ColorY--;
                if (Input.GetKeyDown(Key.Down) && ColorY < (PaletteHeight - 1)) ColorY++;
                if (Input.GetKeyDown(Key.Left) && ColorX > 0) ColorX--;
                if (Input.GetKeyDown(Key.Right) && ColorX < (PaletteWidth - 1)) ColorX++;

                if (ocx != ColorX || ocy != ColorY)
                    BlockColor = Colors[ColorY, ColorX];

                TerrainRaycastResult result = World.TerrainPhysics.Raycast(
                    new Ray(Camera.Position, Camera.ViewMatrix.Forward()), true, PLACE_RANGE);

                if (result.Intersects)
                {
                    IndexPosition ipos = ShiftIPos(result.BlockIndex.Value, result.IntersectionCubeSide.Value);
                    IndexPosition newChunkPos;
                    Chunk.WrapBlockCoords(ipos.X, ipos.Y, ipos.Z, result.Chunk.IndexPosition, out ipos, out newChunkPos);

                    globalMousePosition = Terrain.GetGlobalBlockCoords(newChunkPos, ipos);

                    mouseOverBlock = true;
                }
                else
                    mouseOverBlock = false;

                if (holdingDown)
                {
                    if (mouseOverBlock)
                        endGlobalPosition = globalMousePosition;

                    if (Input.GetControlUp("SecondaryFire"))
                    {
                        holdingDown = false;
                        primaryCooldown = Config.PrimaryFireDelay;

                        if (mouseOverBlock)
                        {
                            buildAudioSource?.Play();

                            Vector3 startWorld = startGlobalPosition * Block.CUBE_3D_SIZE;
                            Vector3 endWorld = endGlobalPosition * Block.CUBE_3D_SIZE;
                            Ray ray = new Ray(startWorld, endWorld - startWorld);
                            RayPlace(ray);
                        }
                    }
                }
            }

            base.Update(deltaTime);
        }

        void RayPlace(Ray ray)
        {
            IndexPosition lastCIndex = IndexPosition.Zero, lastBIndex = new IndexPosition(-1, -1, -1);

            IndexPosition cIndex, bIndex;
            Chunk chunk;

            // Add start
            Terrain.GetLocalBlockCoords(startGlobalPosition, out cIndex, out bIndex);

            if (OwnerPlayer.NumBlocks > 0 && World.Terrain.Chunks.TryGetValue(cIndex, out chunk))
            {
                if (IsBlockPlacementSafe(bIndex, cIndex))
                {
                    Block block = new Block(Block.CUSTOM.Data, BlockColor.R,
                            BlockColor.G, BlockColor.B);

                    OwnerPlayer.NumBlocks--;

                    if (!GlobalNetwork.IsConnected)
                        chunk.SetBlock(block, bIndex);
                    else
                        World.SetBlock(chunk.IndexPosition, bIndex, block, true);
                }
            }

            // Add middle
            for (int _i = 0; _i < 1000; _i++)
            {
                if (OwnerPlayer.NumBlocks <= 0)
                    break;

                float i = _i / 1000f;
                Vector3 worldPos = ray.Origin + ray.Direction * i;
                IndexPosition globalIndex = new IndexPosition(
                    (int)(worldPos.X / Block.CUBE_SIZE),
                    (int)(worldPos.Y / Block.CUBE_SIZE),
                    (int)(worldPos.Z / Block.CUBE_SIZE));

                Terrain.GetLocalBlockCoords(globalIndex, out cIndex, out bIndex);

                if (cIndex == lastCIndex && bIndex == lastBIndex
                    || globalIndex == startGlobalPosition || globalIndex == endGlobalPosition)
                    continue;

                lastCIndex = cIndex;
                lastBIndex = bIndex;

                if (World.Terrain.Chunks.TryGetValue(cIndex, out chunk))
                {
                    if (IsBlockPlacementSafe(bIndex, cIndex))
                    {
                        Block block = new Block(Block.CUSTOM.Data, BlockColor.R,
                                BlockColor.G, BlockColor.B);

                        OwnerPlayer.NumBlocks--;

                        if (!GlobalNetwork.IsConnected)
                            chunk.SetBlock(block, bIndex);
                        else
                            World.SetBlock(chunk.IndexPosition, bIndex, block, true);
                    }
                }
            }

            // Add end
            Terrain.GetLocalBlockCoords(endGlobalPosition, out cIndex, out bIndex);
            if (OwnerPlayer.NumBlocks > 0 && World.Terrain.Chunks.TryGetValue(cIndex, out chunk))
            {
                if (IsBlockPlacementSafe(bIndex, cIndex))
                {
                    Block block = new Block(Block.CUSTOM.Data, BlockColor.R,
                            BlockColor.G, BlockColor.B);

                    OwnerPlayer.NumBlocks--;

                    if (!GlobalNetwork.IsConnected)
                        chunk.SetBlock(block, bIndex);
                    else
                        World.SetBlock(chunk.IndexPosition, bIndex, block, true);
                }
            }
        }

        void RayDraw()
        {
            Vector3 startWorld = startGlobalPosition * Block.CUBE_3D_SIZE;
            Vector3 endWorld = endGlobalPosition * Block.CUBE_3D_SIZE;
            Ray ray = new Ray(startWorld, endWorld - startWorld);

            IndexPosition lastCIndex = IndexPosition.Zero, lastBIndex = new IndexPosition(-1, -1, -1);
            int bcount = 0;

            IndexPosition cIndex, bIndex;
            Chunk chunk;

            // Add start
            Terrain.GetLocalBlockCoords(startGlobalPosition, out cIndex, out bIndex);
            
            if (bcount < OwnerPlayer.NumBlocks && World.Terrain.Chunks.TryGetValue(cIndex, out chunk))
            {
                bcount++;
                entRenderer.Batch(cursorCube, Matrix4.CreateTranslation(startGlobalPosition * Block.CUBE_3D_SIZE));
            }

            // Add middle
            for (int _i = 0; _i < 1000; _i++)
            {
                if (bcount > OwnerPlayer.NumBlocks)
                    break;

                float i = _i / 1000f;
                Vector3 worldPos = ray.Origin + ray.Direction * i;
                IndexPosition globalIndex = new IndexPosition(
                    (int)(worldPos.X / Block.CUBE_SIZE),
                    (int)(worldPos.Y / Block.CUBE_SIZE),
                    (int)(worldPos.Z / Block.CUBE_SIZE));

                Terrain.GetLocalBlockCoords(globalIndex, out cIndex, out bIndex);

                if (cIndex == lastCIndex && bIndex == lastBIndex 
                    || globalIndex == startGlobalPosition || globalIndex == endGlobalPosition)
                    continue;

                lastCIndex = cIndex;
                lastBIndex = bIndex;

                if (World.Terrain.Chunks.TryGetValue(cIndex, out chunk))
                {
                    bcount++;
                    entRenderer.Batch(cursorCube, Matrix4.CreateTranslation(globalIndex * Block.CUBE_3D_SIZE));
                }
            }

            // Add end
            Terrain.GetLocalBlockCoords(endGlobalPosition, out cIndex, out bIndex);
            if (bcount < OwnerPlayer.NumBlocks && World.Terrain.Chunks.TryGetValue(cIndex, out chunk))
            {
                bcount++;
                entRenderer.Batch(cursorCube, Matrix4.CreateTranslation(endGlobalPosition * Block.CUBE_3D_SIZE));
            }
        }

        bool IsBlockPlacementSafe(IndexPosition blockPos, IndexPosition chunkPos)
        {
            Vector3 pos = Chunk.ChunkBlockToWorldCoords(chunkPos, blockPos);
            Vector3 halfSize = Block.CUBE_3D_SIZE / 2f;
            AxisAlignedBoundingBox aabb = new AxisAlignedBoundingBox(pos - halfSize, pos + halfSize);

            return !aabb.Intersects(ownerPlayerPhysicsBody.GetCollider());
        }

        IndexPosition ShiftIPos(IndexPosition ipos, CubeSide normal)
        {
            switch (normal)
            {
                case CubeSide.Back:
                    ipos.Z++; break;
                case CubeSide.Bottom:
                    ipos.Y--; break;
                case CubeSide.Front:
                    ipos.Z--; break;
                case CubeSide.Left:
                    ipos.X--; break;
                case CubeSide.Right:
                    ipos.X++; break;
                case CubeSide.Top:
                    ipos.Y++; break;
            }

            return ipos;
        }

        public override void Draw(ItemViewbob viewbob)
        {
            if (OwnerPlayer.NumBlocks > 0)
            {
                byte r = (byte)MathHelper.Clamp(BlockColor.R + 10, 0, 255);
                byte g = (byte)MathHelper.Clamp(BlockColor.G + 10, 0, 255);
                byte b = (byte)MathHelper.Clamp(BlockColor.B + 10, 0, 255);

                cursorCube.ColorOverlay = new Color(r, g, b);

                if (holdingDown)
                    RayDraw();
                else if (mouseOverBlock)
                    entRenderer.Batch(cursorCube, Matrix4.CreateTranslation(globalMousePosition * Block.CUBE_3D_SIZE));

                Renderer.ColorOverlay = BlockColor;

                base.Draw(viewbob);
            }
        }

        protected override void Draw()
        {
            if (primaryCooldown <= 0.1f)
                base.Draw();
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                buildAudioSource?.Dispose();
            }

            base.Dispose();
        }
    }
}
