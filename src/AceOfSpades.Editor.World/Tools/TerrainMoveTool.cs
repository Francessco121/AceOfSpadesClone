using Dash.Engine;
using Dash.Engine.Graphics;

/* SelectTool.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World.Tools
{
    public class TerrainMoveTool : TerrainEditorTool
    {
        EditorSelectionBox startSelectionBox;
        VoxelTranslationHandles transHandles;
        bool canMove;
        Block[,,] copy;

        public TerrainMoveTool(EditorScreen screen, WorldEditor editor) 
            : base(screen, editor, EditorToolType.TerrainMove, Key.Number5)
        {
            startSelectionBox = new EditorSelectionBox();
            transHandles = new VoxelTranslationHandles(Renderer);
        }

        public override bool AllowUserSelecting()
        {
            return !transHandles.HasHold;
        }

        void MoveSelection()
        {
            int xSize = startSelectionBox.Max.X - startSelectionBox.Min.X + 1;
            int ySize = startSelectionBox.Max.Y - startSelectionBox.Min.Y + 1;
            int zSize = startSelectionBox.Max.Z - startSelectionBox.Min.Z + 1;

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        IndexPosition startPos = new IndexPosition(x, y, z) + startSelectionBox.Min;
                        IndexPosition endPos = new IndexPosition(x, y, z) + TerrainEditor.SelectionBox.Min;

                        IndexPosition cIndex, startBlock, endBlock;
                        GetLocalBlockCoords(startPos, out cIndex, out startBlock);

                        Chunk startChunk, endChunk;
                        if (Terrain.Chunks.TryGetValue(cIndex, out startChunk))
                        {
                            GetLocalBlockCoords(endPos, out cIndex, out endBlock);
                            if (Terrain.Chunks.TryGetValue(cIndex, out endChunk))
                            {
                                Block block = startChunk.GetBlockSafe(startBlock.X, startBlock.Y, startBlock.Z);
                                TerrainEditor.SetBlock(endChunk, block, endBlock);

                                if (!TerrainEditor.SelectionBox.Contains(startPos))
                                    TerrainEditor.SetBlock(startChunk, Block.AIR, startBlock);
                            }
                        }
                    }

            startSelectionBox.SetMinMax(TerrainEditor.SelectionBox.Min, TerrainEditor.SelectionBox.Max);
        }

        void CopySelection()
        {
            EditorSelectionBox selectionBox = TerrainEditor.SelectionBox;
            copy = new Block[
                        selectionBox.Max.Z - selectionBox.Min.Z + 1,
                        selectionBox.Max.Y - selectionBox.Min.Y + 1,
                        selectionBox.Max.X - selectionBox.Min.X + 1];

            for (int x = selectionBox.Min.X, _x = 0; x <= selectionBox.Max.X; x++, _x++)
                for (int y = selectionBox.Min.Y, _y = 0; y <= selectionBox.Max.Y; y++, _y++)
                    for (int z = selectionBox.Min.Z, _z = 0; z <= selectionBox.Max.Z; z++, _z++)
                    {
                        IndexPosition globalPos = new IndexPosition(x, y, z);
                        IndexPosition cIndex, bIndex;
                        GetLocalBlockCoords(globalPos, out cIndex, out bIndex);

                        Chunk chunk;
                        if (Terrain.Chunks.TryGetValue(cIndex, out chunk))
                            copy[_z, _y, _x] = chunk.GetBlockSafe(bIndex.X, bIndex.Y, bIndex.Z);
                    }
        }

        void PasteSelection()
        {
            EditorSelectionBox selectionBox = TerrainEditor.SelectionBox;
            IndexPosition origin = selectionBox.Min;

            IndexPosition min = origin;
            IndexPosition max = origin + new IndexPosition(
                    copy.GetLength(2) - 1,
                    copy.GetLength(1) - 1,
                    copy.GetLength(0) - 1);

            if (selectionBox.Size() == IndexPosition.Zero && (max - min) != IndexPosition.Zero)
            {
                min += new IndexPosition(0, 1, 0);
                max += new IndexPosition(0, 1, 0);
            }

            selectionBox.SetMinMax(min, max);

            for (int x = selectionBox.Min.X, _x = 0; x <= selectionBox.Max.X; x++, _x++)
                for (int y = selectionBox.Min.Y, _y = 0; y <= selectionBox.Max.Y; y++, _y++)
                    for (int z = selectionBox.Min.Z, _z = 0; z <= selectionBox.Max.Z; z++, _z++)
                    {
                        IndexPosition globalPos = new IndexPosition(x, y, z);
                        IndexPosition cIndex, bIndex;
                        GetLocalBlockCoords(globalPos, out cIndex, out bIndex);

                        Chunk chunk;
                        if (Terrain.Chunks.TryGetValue(cIndex, out chunk))
                            TerrainEditor.SetBlock(chunk, copy[_z, _y, _x], bIndex);
                    }
        }

        void DeleteSelection()
        {
            EditorSelectionBox selectionBox = TerrainEditor.SelectionBox;
            for (int x = selectionBox.Min.X, _x = 0; x <= selectionBox.Max.X; x++, _x++)
                for (int y = selectionBox.Min.Y, _y = 0; y <= selectionBox.Max.Y; y++, _y++)
                    for (int z = selectionBox.Min.Z, _z = 0; z <= selectionBox.Max.Z; z++, _z++)
                    {
                        IndexPosition globalPos = new IndexPosition(x, y, z);
                        IndexPosition cIndex, bIndex;
                        GetLocalBlockCoords(globalPos, out cIndex, out bIndex);

                        Chunk chunk;
                        if (Terrain.Chunks.TryGetValue(cIndex, out chunk))
                            TerrainEditor.SetBlock(chunk, Block.AIR, bIndex);
                    }
        }

        public override void Update(EditorWorldRaycastResult intersection, float deltaTime)
        {
            // Grab first because of how many times were using it
            EditorSelectionBox selectionBox = TerrainEditor.SelectionBox;

            if (Input.GetMouseButtonUp(MouseButton.Left))
                transHandles.LetGo();
            else if (Input.GetMouseButtonDown(MouseButton.Left))
                transHandles.TryGrab(Camera.Active);

            if (TerrainEditor.IsSelecting)
            {
                startSelectionBox.SetPrimary(selectionBox.Primary);
                startSelectionBox.SetSecondary(selectionBox.Secondary);
            }

            IndexPosition delta = transHandles.Update(Block.CUBE_SIZE, Camera.Active);
            if (!canMove) delta = IndexPosition.Zero;

            if (Input.IsControlHeld)
            {
                selectionBox.SetMinMax(selectionBox.Min, selectionBox.Max + delta);
                startSelectionBox.SetMinMax(startSelectionBox.Min, startSelectionBox.Max + delta);
            }
            else if (Input.IsAltHeld)
            {
                selectionBox.SetMinMax(selectionBox.Min + delta, selectionBox.Max);
                startSelectionBox.SetMinMax(startSelectionBox.Min + delta, startSelectionBox.Max);
            }
            else
                selectionBox.Translate(delta);

            transHandles.PositionToMinMax(selectionBox.Min, selectionBox.Max, Block.CUBE_SIZE, Block.CUBE_3D_SIZE);

            if (canMove)
            {
                if (transHandles.HasHold)
                {
                    if (Input.WrapCursor())
                        canMove = false;
                }
            }
            else
            {
                Camera.Active.Update(deltaTime);
                transHandles.ResetStartPos(Camera.Active);
                canMove = true;
            }

            if (Input.GetKeyDown(Key.Enter))
                MoveSelection();
            else if (Input.GetKeyDown(Key.Delete))
                DeleteSelection();

            if (Input.IsControlHeld)
            {
                if (Input.GetKeyDown(Key.C))
                    CopySelection();
                else if (Input.GetKeyDown(Key.X))
                {
                    CopySelection();
                    DeleteSelection();
                }
                else if (copy != null && Input.GetKeyDown(Key.V))
                    PasteSelection();
            }
        }

        public override void Draw(EditorWorldRaycastResult intersection)
        {
            Vector3 blockCoords = TerrainEditor.SelectionBox.Center();

            Vector3 scale = TerrainEditor.SelectionBox.Size().ToVector3() + Vector3.UnitScale;

            cursorCube.Position = blockCoords;
            cursorCube.VoxelObject.MeshScale = scale + new Vector3(0.01f, 0.01f, 0.01f);
            cursorCube.RenderAsWireframe = true;
            cursorCube.ColorOverlay = Color.Black;

            entRenderer.Batch(cursorCube);
            transHandles.Draw();
        }
    }
}
