using Dash.Engine;
using Dash.Engine.Physics;

namespace AceOfSpades
{
    public class PhysicsBlock : GameObject
    {
        public Block Block { get; set; }
        public IndexPosition BlockPos { get; set; }
        public Chunk Chunk { get; set; }

        public PhysicsBlock(Block block, Vector3 position, IndexPosition ipos, Chunk chunk) 
            : base(position)
        {
            PhysicsBodyComponent physicsBody = new PhysicsBodyComponent(Block.CUBE_3D_SIZE);
            AddComponent(physicsBody);

            Block = block;
            BlockPos = ipos;
            Chunk = chunk;
            physicsBody.IsStatic = true;
            physicsBody.CanCollideWithTerrain = false;
        }
    }
}
