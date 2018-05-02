using Dash.Engine;
using System;

/* TerrainOperation.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World
{
    public class TerrainOperation
    {
        Chunk chunk;
        Block block;
        IndexPosition blockPos;

        public TerrainOperation(Chunk chunk, Block block, IndexPosition blockPos)
        {
            if (!chunk.IsBlockCoordInRange(blockPos))
                throw new ArgumentOutOfRangeException("blockPos", "Cannot create operation, block position is out of range!");

            this.chunk = chunk;
            this.block = block;
            this.blockPos = blockPos;
        }

        public void Apply()
        {
            chunk.SetBlock(block, blockPos);
        }

        public static TerrainOperation CreateUndoFor(TerrainOperation op)
        {
            return CreateUndoFor(op.chunk, op.blockPos);
        }

        public static TerrainOperation CreateUndoFor(Chunk chunk, IndexPosition blockPos)
        {
            if (!chunk.IsBlockCoordInRange(blockPos))
                throw new ArgumentOutOfRangeException("blockPos", "Cannot create undo operation, block position is out of range!");

            Block existing = chunk.GetBlockSafe(blockPos.X, blockPos.Y, blockPos.Z);
            return new TerrainOperation(chunk, existing, blockPos);
        }
    }
}
