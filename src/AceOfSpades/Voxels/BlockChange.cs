using Dash.Engine;

namespace AceOfSpades
{
    public class BlockChange
    {
        public Block Block;
        public IndexPosition Position;
        public Chunk Chunk;

        public BlockChange(Chunk chunk, Block block, IndexPosition pos)
        {
            Block = block;
            Position = pos;
            Chunk = chunk;
        }
    }
}
