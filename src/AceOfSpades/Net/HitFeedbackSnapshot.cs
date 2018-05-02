using System.Collections.Generic;
using Dash.Net;
using Dash.Engine;

namespace AceOfSpades.Net
{
    /// <summary>
    /// Synchronizes 'hit feedback' with clients.
    /// Everytime a player is hit, the position of the origin
    /// of whatever caused the hit is sent through here.
    /// </summary>
    public class HitFeedbackSnapshot : CustomSnapshot
    {
        public List<Vector3> Hits { get; }

        public HitFeedbackSnapshot()
        {
            Hits = new List<Vector3>();
        }

        protected override void OnSerialize(NetBuffer buffer)
        {
            buffer.Write((ushort)Hits.Count);
            for (int i = 0; i < Hits.Count; i++)
            {
                Vector3 vec = Hits[i];
                buffer.Write(vec.X);
                buffer.Write(vec.Y);
                buffer.Write(vec.Z);
            }

            Hits.Clear();
        }

        protected override void OnDeserialize(NetBuffer buffer)
        {
            Hits.Clear();

            int numHits = buffer.ReadUInt16();
            for (int i = 0; i < numHits; i++)
            {
                Vector3 vec = new Vector3(
                    buffer.ReadFloat(),
                    buffer.ReadFloat(),
                    buffer.ReadFloat());

                Hits.Add(vec);
            }
        }
    }
}
