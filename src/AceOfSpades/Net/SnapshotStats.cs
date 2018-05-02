using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AceOfSpades.Net
{
    public class SnapshotStats
    {
        public int PacketHeader;
        public int Acks;
        public int PlayerData;
        public int TerrainData;

        public int Total
        {
            get { return PacketHeader + Acks + PlayerData + TerrainData; }
        }
    }
}
