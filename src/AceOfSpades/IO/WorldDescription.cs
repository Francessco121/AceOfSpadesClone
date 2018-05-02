using System.Linq;
using System.Collections.Generic;

namespace AceOfSpades.IO
{
    public class WorldDescription
    {
        public FixedTerrain Terrain { get; }
        public ILookup<string, WorldObjectDescription> Objects { get; }

        public WorldDescription(FixedTerrain terrain)
        {
            Terrain = terrain;
            Objects = new List<WorldObjectDescription>().ToLookup(o => o.Tag);
        }

        public WorldDescription(FixedTerrain terrain, IEnumerable<WorldObjectDescription> objects)
        {
            Terrain = terrain;
            Objects = objects.ToLookup(o => o.Tag);
        }

        public WorldObjectDescription GetObjectByTag(string tag)
        {
            foreach (WorldObjectDescription desc in Objects)
                if (desc.Tag == tag)
                    return desc;

            return null;
        }

        public IList<WorldObjectDescription> GetObjectsByTag(string tag)
        {
            var objects = Objects[tag];
            if (objects != null)
                return objects.ToList();
            else
                return null;
        }
    }
}
