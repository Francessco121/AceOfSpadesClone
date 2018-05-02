using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;

/* TerraformTool.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World.Tools
{
    public class TerraformTool : TerrainEditorTool
    {
        float brushSize { get { return window.BrushSize; } }
        int riseHeight { get { return window.RiseHeight; } }
        int halfBrush;

        ColorGradient grassGradient = new ColorGradient(
               new Vector3(-512, 0, -512), new Vector3(512, 256, 512),
               Maths.RGBToVector3(0, 135, 16), Maths.RGBToVector3(33, 156, 22));

        ColorGradient stoneGradient = new ColorGradient(
                new Vector3(0, -64, 0), new Vector3(0, 512, 0),
                Maths.RGBToVector3(122, 122, 122), Maths.RGBToVector3(200, 200, 200));

        bool dragging;
        int dragHeight;
        int chunkDragHeight;

        TerraformWindow window;

        public TerraformTool(EditorScreen screen, WorldEditor editor) 
            : base(screen, editor, EditorToolType.Terraform, Key.Number6)
        { 
            window = new TerraformWindow(UI.GUISystem, UI.Theme);
            window.Visible = false;
            UI.GUISystem.Add(window);
        }

        public override void Equipped()
        {
            window.Visible = true;
            base.Equipped();
        }

        public override void Unequipped()
        {
            window.Visible = false;
            base.Unequipped();
        }

        public override bool AllowUserSelecting()
        {
            return false;
        }

        public override void Update(EditorWorldRaycastResult worldIntersection, float deltaTime)
        {
            halfBrush = (int)Math.Ceiling(brushSize / 2f);

            if (worldIntersection.HitTerrain && !GUISystem.HandledMouseInput)
            {
                TerrainRaycastResult intersection = worldIntersection.TerrainResult;

                if (Input.GetMouseButtonDown(MouseButton.Left))
                {
                    if (Input.IsControlHeld)
                    {
                        dragging = true;
                        dragHeight = intersection.BlockIndex.Value.Y;
                        chunkDragHeight = intersection.Chunk.IndexPosition.Y;
                    }
                }

                if (Input.GetMouseButtonUp(MouseButton.Left) && dragging)
                    dragging = false;
                else if (Input.GetMouseButtonUp(MouseButton.Left) || dragging)
                {
                    IndexPosition cIndex, bIndex;
                    if (dragging)
                    {
                        cIndex = new IndexPosition(intersection.Chunk.IndexPosition.X,
                            chunkDragHeight, intersection.Chunk.IndexPosition.Z);
                        bIndex = new IndexPosition(intersection.BlockIndex.Value.X, dragHeight,
                            intersection.BlockIndex.Value.Z);
                    }
                    else
                    {
                        cIndex = intersection.Chunk.IndexPosition;
                        bIndex = intersection.BlockIndex.Value;
                    }

                    IndexPosition globalBlock = GetGlobalBlockCoords(cIndex, bIndex);

                    for (int x = -halfBrush; x <= halfBrush; x++)
                        for (int y = -halfBrush; y <= halfBrush; y++)
                            for (int z = -halfBrush; z <= halfBrush; z++)
                            {
                                float dist = (new Vector3(x, y, z)).Length;
                                if (dist > halfBrush)
                                    continue;

                                float distPercent = (float)Math.Cos((dist / halfBrush) * MathHelper.PiOver2);
                                int offset = (int)Math.Round(distPercent * riseHeight);

                                IndexPosition globalPos = globalBlock + new IndexPosition(x, y - offset, z);
                                GetLocalBlockCoords(globalPos, out cIndex, out bIndex);

                                Chunk chunk;
                                if (Terrain.Chunks.TryGetValue(cIndex, out chunk) && chunk.IsBlockCoordInRange(bIndex))
                                {
                                    IndexPosition cIndex2, bIndex2;
                                    IndexPosition globalPos2 = globalBlock + new IndexPosition(x, y, z);
                                    GetLocalBlockCoords(globalPos2, out cIndex2, out bIndex2);

                                    Chunk chunk2;
                                    if (Terrain.Chunks.TryGetValue(cIndex2, out chunk2) && chunk2.IsBlockCoordInRange(bIndex2))
                                        TerrainEditor.SetBlock(chunk2, chunk[bIndex], bIndex2);
                                }
                            }

                    //ApplyActionToBrush(cIndex, bIndex,
                    //    (chunk, blockIndex, x, y, z, _) =>
                    //    {
                    //    //    int ay = 0;// Math.Abs(y);
                    //    //    float dist = (new Vector3(x + ay, ay, z + ay)).Length;
                    //    //    int offset = (int)Math.Round(((brushSize - dist) / brushSize) * riseHeight);

                    //    ////if (x == 0 && z == 0)
                    //    ////    y++;

                    //    //if (y > riseHeight)
                    //    //        return;

                    //    //    IndexPosition newBlockPos;
                    //    //    IndexPosition newChunkIndex;
                    //    //    Chunk.WrapBlockCoords(blockIndex.X, blockIndex.Y + offset, blockIndex.Z, chunk.IndexPosition,
                    //    //        out newBlockPos, out newChunkIndex);

                    //    //    if (Terrain.Chunks.TryGetValue(newChunkIndex, out chunk))
                    //    //    {
                    //    //        IndexPosition blockPos = new IndexPosition(x, y, z);
                    //    //        Vector3 coloroff = (blockPos + (newChunkIndex * Chunk.SIZE)) * Block.CUBE_SIZE;
                    //    //        Nybble2 data;
                    //    //        Color voxelColor;
                    //    //        if (y == riseHeight)
                    //    //        {
                    //    //            data = Block.GRASS.Data;
                    //    //            voxelColor = grassGradient.GetColor(coloroff);
                    //    //        }
                    //    //        else
                    //    //        {
                    //    //            data = Block.STONE.Data;
                    //    //            voxelColor = stoneGradient.GetColor(coloroff);
                    //    //        }

                    //    //        voxelColor = new Color(
                    //    //            (byte)MathHelper.Clamp(voxelColor.R + Maths.Random.Next(-3, 3), 0, 255),
                    //    //            (byte)MathHelper.Clamp(voxelColor.G + Maths.Random.Next(-3, 3), 0, 255),
                    //    //            (byte)MathHelper.Clamp(voxelColor.B + Maths.Random.Next(-3, 3), 0, 255));

                    //    //        if (chunk.GetBlockSafe(newBlockPos.X, newBlockPos.Y, newBlockPos.Z).Data.Value != data.Value)
                    //    //            Editor.SetBlock(chunk, new Block(data, voxelColor.R, voxelColor.G, voxelColor.B), newBlockPos);
                    //    //    }
                    //    });
                }
            }
        }

        void ApplyActionToBrush(IndexPosition chunkIndex, IndexPosition blockIndex, 
            Action<Chunk, IndexPosition, int, int, int, float> action)
        {
            IndexPosition globalBlock = GetGlobalBlockCoords(chunkIndex, blockIndex);

            for (int x = -halfBrush; x <= halfBrush; x++)
                for (int y = -halfBrush; y <= halfBrush; y++)
                    for (int z = -halfBrush; z <= halfBrush; z++)
                    {
                        float dist = (new Vector3(x, y, z)).Length;
                        if (dist > halfBrush)
                            continue;

                        IndexPosition globalPos = globalBlock + new IndexPosition(x, y, z);
                        IndexPosition cIndex, bIndex;
                        GetLocalBlockCoords(globalPos, out cIndex, out bIndex);

                        Chunk chunk;
                        if (Terrain.Chunks.TryGetValue(cIndex, out chunk))
                            action(chunk, bIndex, x, y, z, dist);
                    }
        }

        public override void Draw(EditorWorldRaycastResult worldIntersection)
        {
            if (worldIntersection.HitTerrain && !GUISystem.HandledMouseOver)
            {
                TerrainRaycastResult intersection = worldIntersection.TerrainResult;

                Vector3 blockCoords = TerrainEditor.IsSelecting
                   ? TerrainEditor.SelectionBox.Center()
                   : GetGlobalBlockCoords(intersection.Chunk.IndexPosition, intersection.BlockIndex.Value) * Block.CUBE_3D_SIZE;

                Vector3 scale = TerrainEditor.IsSelecting
                    ? TerrainEditor.SelectionBox.Size().ToVector3() + Vector3.UnitScale
                    : Vector3.UnitScale;

                cursorCube.Position = blockCoords;
                cursorCube.VoxelObject.MeshScale = scale + new Vector3(0.01f, 0.01f, 0.01f);
                cursorCube.RenderAsWireframe = true;
                cursorCube.ColorOverlay = Color.Black;

                entRenderer.Batch(cursorCube);
            }
        }
    }
}
