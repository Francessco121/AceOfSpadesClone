using Dash.Net;
using Dash.Engine;
using System.Collections.Generic;

namespace AceOfSpades.Net
{
    public class TerrainDeltaChange
    {
        public IndexPosition ChunkIndex;
        public IndexPosition BlockIndex;
        public Block Block;

        public TerrainDeltaChange(IndexPosition chunkIndex, IndexPosition blockIndex, Block block)
        {
            ChunkIndex = chunkIndex;
            BlockIndex = blockIndex;
            Block = block;
        }
    }

    /// <summary>
    /// Contains any changes made to the server's world,
    /// which is sent to each client through here.
    /// </summary>
    public class TerrainDeltaSnapshot : CustomSnapshot
    {
        public int LastByteSize;
        public HashSet<TerrainDeltaChange> ReceivedChanges;

        HashSet<BlockChange> changes;

        public TerrainDeltaSnapshot()
        {
            changes = new HashSet<BlockChange>();
            ReceivedChanges = new HashSet<TerrainDeltaChange>();
        }

        public void AddChange(BlockChange change)
        {
            changes.Add(change);
        }

        protected override void OnSerialize(NetBuffer buffer)
        {
            int size = buffer.Length;
            buffer.Write((ushort)changes.Count);
            foreach (BlockChange change in changes)
            {
                buffer.Write((short)change.Chunk.IndexPosition.X);
                buffer.Write((short)change.Chunk.IndexPosition.Y);
                buffer.Write((short)change.Chunk.IndexPosition.Z);
                buffer.Write((short)change.Position.X);
                buffer.Write((short)change.Position.Y);
                buffer.Write((short)change.Position.Z);
                buffer.Write(change.Block.R);
                buffer.Write(change.Block.G);
                buffer.Write(change.Block.B);
                buffer.Write(change.Block.Data.Value);
            }

            LastByteSize = buffer.Length - size;
            changes.Clear();
        }

        protected override void OnDeserialize(NetBuffer buffer)
        {
            ushort numTerrainChanges = buffer.ReadUInt16();

            for (int i = 0; i < numTerrainChanges; i++)
            {
                short cix = buffer.ReadInt16();
                short ciy = buffer.ReadInt16();
                short ciz = buffer.ReadInt16();

                short cx = buffer.ReadInt16();
                short cy = buffer.ReadInt16();
                short cz = buffer.ReadInt16();

                byte r = buffer.ReadByte();
                byte g = buffer.ReadByte();
                byte b = buffer.ReadByte();
                byte d = buffer.ReadByte();

                ReceivedChanges.Add(new TerrainDeltaChange(new IndexPosition(cix, ciy, ciz),
                    new IndexPosition(cx, cy, cz), new Block(new Nybble2(d), r, g, b)));
            }
        }
    }
}
