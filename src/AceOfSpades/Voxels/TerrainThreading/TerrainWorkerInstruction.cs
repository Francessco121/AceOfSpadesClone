/* TerrainWorkerInstruction.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.TerrainThreading
{
    public class TerrainWorkerInstruction
    {
        public readonly Chunk Chunk;
        public readonly TerrainWorkerAction Action;

        public TerrainWorkerInstruction(Chunk chunk, TerrainWorkerAction action)
        {
            Chunk = chunk;
            Action = action;
        }
    }
}
