using System.Collections.Generic;

/* TerrainOperationBatch.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World
{
    public class TerrainOperationBatch
    {
        public int Count
        {
            get { return operations.Count; }
        }

        List<TerrainOperation> operations;

        public TerrainOperationBatch()
        {
            operations = new List<TerrainOperation>();
        }

        public void Add(TerrainOperation op)
        {
            operations.Add(op);
        }

        public void Clear()
        {
            operations.Clear();
        }

        public TerrainOperationBatch GenerateUndo()
        {
            TerrainOperationBatch redo = new TerrainOperationBatch();
            foreach (TerrainOperation op in operations)
                redo.Add(TerrainOperation.CreateUndoFor(op));

            return redo;
        }

        public void Apply()
        {
            foreach (TerrainOperation op in operations)
                op.Apply();
        }
    }
}
