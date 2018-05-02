using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using System;

/* TerrainEditorTool.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World
{
    public abstract class TerrainEditorTool : EditorTool
    {
        protected TerrainEditor TerrainEditor { get; }
        protected FixedTerrain Terrain { get { return Screen.World.Terrain; } }

        protected EntityRenderer entRenderer { get; }
        protected static DebugCube cursorCube { get; private set; }

        public TerrainEditorTool(EditorScreen screen, WorldEditor editor,
            EditorToolType type, Key keyBind)
            : base(screen, editor, type, keyBind)
        {
            TerrainEditor = Editor.TerrainEditor;
            entRenderer = Renderer.GetRenderer3D<EntityRenderer>();

            if (cursorCube == null)
                cursorCube = new DebugCube(Color4.White, Block.CUBE_SIZE);
        }

        public virtual IndexPosition GetRayIntersectionIndex(TerrainRaycastResult rayIntersection)
        {
            return GetGlobalBlockCoords(rayIntersection.Chunk.IndexPosition, rayIntersection.BlockIndex.Value);
        }

        protected bool ShiftPositionByNormal(Chunk chunk, IndexPosition bpos, CubeSide normal,
            out Chunk newChunk, out IndexPosition newBlockPos)
        {
            Vector3 vecNormal = Maths.CubeSideToSurfaceNormal(normal);
            IndexPosition indexNormal = new IndexPosition((int)vecNormal.X, (int)vecNormal.Y, (int)vecNormal.Z);

            newBlockPos = bpos + indexNormal;
            IndexPosition newChunkIndex;
            Chunk.WrapBlockCoords(newBlockPos.X, newBlockPos.Y, newBlockPos.Z, chunk.IndexPosition,
                out newBlockPos, out newChunkIndex);

            return Terrain.Chunks.TryGetValue(newChunkIndex, out newChunk);
        }

        protected IndexPosition GetGlobalBlockCoords(IndexPosition chunkPos, IndexPosition blockPos)
        {
            return FixedTerrain.GetGlobalBlockCoords(chunkPos, blockPos);
        }

        protected void GetLocalBlockCoords(IndexPosition global, out IndexPosition chunkPos, out IndexPosition blockPos)
        {
            FixedTerrain.GetLocalBlockCoords(global, out chunkPos, out blockPos);
        }

        protected void ApplyActionToSelection(EditorSelectionBox box, Action<Chunk, IndexPosition> action)
        {
            for (int x = box.Min.X; x <= box.Max.X; x++)
                for (int y = box.Min.Y; y <= box.Max.Y; y++)
                    for (int z = box.Min.Z; z <= box.Max.Z; z++)
                    {
                        IndexPosition globalPos = new IndexPosition(x, y, z);
                        IndexPosition cIndex, bIndex;
                        GetLocalBlockCoords(globalPos, out cIndex, out bIndex);

                        Chunk chunk;
                        if (Terrain.Chunks.TryGetValue(cIndex, out chunk))
                            action(chunk, bIndex);
                    }
        }
    }
}
