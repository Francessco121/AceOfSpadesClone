using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.OpenGL;

/* DebugCube.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public class DebugCube : Entity
    {
        public DebugCube(Color4 color, float size)
        {
            VoxelObject = new DebugVOCube(color, size);
        }
    }

    public class DebugVOCube : VoxelObject
    {
        public DebugVOCube(Color4 color, float size)
            : base(size)
        {
            InitBlocks(1, 1, 1);

            Block b = Block.AIR;
            IndexPosition ipos = IndexPosition.Zero;
            meshBuilder.AddLeft(b, ipos, Vector3.Zero, color);
            meshBuilder.AddRight(b, ipos, Vector3.Zero, color);
            meshBuilder.AddBack(b, ipos, Vector3.Zero, color);
            meshBuilder.AddFront(b, ipos, Vector3.Zero, color);
            meshBuilder.AddTop(b, ipos, Vector3.Zero, color);
            meshBuilder.AddBottom(b, ipos, Vector3.Zero, color);

            CreateOrUpdateMesh(BufferUsageHint.StaticDraw);
        }
    }
}
