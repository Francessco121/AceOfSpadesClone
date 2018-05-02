using Dash.Engine;
using Dash.Engine.Graphics;

/* VoxelTranslationHandle.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor
{
    public sealed class VoxelTranslationHandle : VoxelEditorObject
    {
        public VoxelTranslationHandle(int height, float cubeSize, Color color)
            : base(3, height, 3, cubeSize, true)
        {

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                    {
                        bool inMiddle = x == 1 && z == 1;
                        bool oneFromTop = y == Height - 2;
                        bool diag = x != 1 && z != 1;

                        if (!inMiddle && !oneFromTop || diag)
                            continue;

                        Blocks[z, y, x] = new Block(1, color.R, color.G, color.B);
                    }

            BuildMesh();
        }
    }
}