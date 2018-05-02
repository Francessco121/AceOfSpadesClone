using System;

/* TerrainWorkerError.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.TerrainThreading
{
    public class TerrainWorkerError
    {
        public Exception Exception { get; private set; }
        public TerrainWorkerInstruction Instruction { get; private set; }

        public TerrainWorkerError(Exception ex, TerrainWorkerInstruction inst)
        {
            Exception = ex;
            Instruction = inst;
        }

        public override string ToString()
        {
            return Exception.ToString();
        }
    }
}
