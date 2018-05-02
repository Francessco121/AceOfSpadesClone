using Dash.Engine;

namespace AceOfSpades.Editor.World
{
    public class EditorWorldRaycastResult : RaycastResult
    {
        public EditorObjectRaycastResult EditorObjectResult { get; }
        public TerrainRaycastResult TerrainResult { get; }

        public bool HitEditorObject { get { return EditorObjectResult.Intersects; } }
        public bool HitTerrain { get { return TerrainResult.Intersects; } }

        public EditorWorldRaycastResult(Ray ray)
            : base(ray)
        {
            EditorObjectResult = new EditorObjectRaycastResult(ray);
            TerrainResult = new TerrainRaycastResult(ray);
        }

        public EditorWorldRaycastResult(EditorObjectRaycastResult result)
            : base(result)
        {
            EditorObjectResult = result;
            TerrainResult = new TerrainRaycastResult(result.Ray);
        }

        public EditorWorldRaycastResult(TerrainRaycastResult result)
            : base(result)
        {
            TerrainResult = result;
            EditorObjectResult = new EditorObjectRaycastResult(result.Ray);
        }
    }
}
