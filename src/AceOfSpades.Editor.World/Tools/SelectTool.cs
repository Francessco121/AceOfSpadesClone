using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Editor.World.Tools
{
    public class SelectTool : EditorTool
    {
        VoxelTranslationHandles transHandles;
        EditorObject selectedObject;
        DebugRenderer debugRenderer;
        bool canMove;

        ObjectEditWindow window;

        public SelectTool(EditorScreen screen, WorldEditor editor) 
            : base(screen, editor, EditorToolType.Select, Key.Number1)
        {
            debugRenderer = Renderer.GetRenderer3D<DebugRenderer>();
            transHandles = new VoxelTranslationHandles(Renderer);

            window = new ObjectEditWindow(UI.GUISystem, UI.Theme);
            window.Visible = false;
            UI.GUISystem.Add(window);
        }

        public override void Unequipped()
        {
            window.SetObject(null);
            base.Unequipped();
        }

        public override bool AllowUserSelecting()
        {
            return !transHandles.HasHold;
        }

        public override void Update(EditorWorldRaycastResult worldIntersection, float deltaTime)
        {
            if (Input.GetMouseButtonUp(MouseButton.Left))
                transHandles.LetGo();
            else if (Input.GetMouseButtonDown(MouseButton.Left) 
                && !GUISystem.HandledMouseOver && selectedObject != null)
                transHandles.TryGrab(Camera.Active);

            if (!GUISystem.HandledMouseOver && Input.GetMouseButtonDown(MouseButton.Left) && !transHandles.HasHold)
            {
                if (worldIntersection.HitEditorObject)
                {
                    EditorObjectRaycastResult intersection = worldIntersection.EditorObjectResult;
                    selectedObject = intersection.EditorObject;
                }
                else
                    selectedObject = null;

                window.SetObject(selectedObject);
            }

            if (selectedObject != null)
            {
                Vector3 delta = transHandles.Update(Camera.Active, 16);
                if (!canMove) delta = Vector3.Zero;

                AxisAlignedBoundingBox aabb = selectedObject.GetCollider();
                transHandles.PositionToMinMax(aabb.Min, aabb.Max, Vector3.Zero);

                if (canMove)
                {
                    if (transHandles.HasHold)
                    {
                        if (Input.WrapCursor())
                            canMove = false;
                    }
                }
                else
                {
                    Camera.Active.Update(deltaTime);
                    transHandles.ResetStartPos(Camera.Active);
                    canMove = true;
                }

                selectedObject.Transform.Position += delta;
            }
        }

        public override void Draw(EditorWorldRaycastResult intersection)
        {
            if (selectedObject != null)
            {
                debugRenderer.Batch(selectedObject.GetCollider());
                transHandles.Draw();
            }
        }
    }
}
