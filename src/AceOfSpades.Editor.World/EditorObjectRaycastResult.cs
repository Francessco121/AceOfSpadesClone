using Dash.Engine;

namespace AceOfSpades.Editor.World
{
    public class EditorObjectRaycastResult : RaycastResult
    {
        public EditorObject EditorObject { get; }

        public EditorObjectRaycastResult(EditorObjectRaycastResult result) 
            : base(result)
        {
            EditorObject = result.EditorObject;
        }

        public EditorObjectRaycastResult(Ray ray) 
            : base(ray)
        { }

        public EditorObjectRaycastResult(EditorObject editorObject, Ray ray, bool intersects, 
            Vector3? intersectionPosition, float? intersectionDistance) 
            : base(ray, intersects, intersectionPosition, intersectionDistance)
        {
            EditorObject = editorObject;
        }
    }
}
