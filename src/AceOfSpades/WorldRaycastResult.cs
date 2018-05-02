using Dash.Engine;

namespace AceOfSpades
{
    /// <summary>
    /// Represents a raycast result of the world,
    /// containing either a PlayerRaycastResult, 
    /// a TerrainRaycastResult, or neither if the
    /// ray missed.
    /// </summary>
    public class WorldRaycastResult : RaycastResult
    {
        public PlayerRaycastResult PlayerResult { get; }
        public TerrainRaycastResult TerrainResult { get; }

        public bool HitPlayer { get { return PlayerResult != null; } }
        public bool HitTerrain { get { return TerrainResult != null; } }

        public WorldRaycastResult(Ray ray) 
            : base(ray)
        { }

        public WorldRaycastResult(PlayerRaycastResult result)
            : base(result)
        {
            PlayerResult = result;
        }

        public WorldRaycastResult(TerrainRaycastResult result)
            : base(result)
        {
            TerrainResult = result;
        }
    }
}
