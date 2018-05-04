﻿using AceOfSpades.Graphics;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Editor.Models.Tools
{
    class AddTool : EditorTool
    {
        readonly EntityRenderer entRenderer;

        static DebugCube cursorCube;

        public AddTool(EditorScreen screen, ModelEditor editor) 
            : base(screen, editor, EditorToolType.Add, Key.Number1)
        {
            entRenderer = Renderer.GetRenderer3D<EntityRenderer>();

            if (cursorCube == null)
                cursorCube = new DebugCube(Color4.White, 1f);
        }

        public override void Update(VoxelObjectRaycastResult intersection, float deltaTime)
        {
            if (GUISystem.HandledMouseInput)
                return;

            if (intersection.Intersects && Input.GetMouseButtonDown(MouseButton.Left))
            {
                Vector3 intersectionNormal = Maths.CubeSideToSurfaceNormal(intersection.IntersectionSide.Value);

                IndexPosition blockIndexPosition = new IndexPosition(
                    intersection.IntersectedBlockIndex.Value.X + (int)intersectionNormal.X,
                    intersection.IntersectedBlockIndex.Value.Y + (int)intersectionNormal.Y,
                    intersection.IntersectedBlockIndex.Value.Z + (int)intersectionNormal.Z);

                if (Screen.Model.IsBlockCoordInRange(blockIndexPosition))
                {
                    Color color = UI.ColorWindow.ColorPicker.Color;

                    Screen.Model.ChangeBlock(blockIndexPosition, new Block(Block.STONE.Material,
                        color.R, color.G, color.B));
                }
            }
        }

        public override void Draw(VoxelObjectRaycastResult intersection)
        {
            if (GUISystem.HandledMouseOver)
                return;

            if (intersection.Intersects)
            {
                cursorCube.Position = (intersection.IntersectedBlockIndex.Value * Screen.Model.CubeSize)
                    + (Maths.CubeSideToSurfaceNormal(intersection.IntersectionSide.Value) * Screen.Model.CubeSize);

                cursorCube.VoxelObject.MeshScale = new Vector3(Screen.Model.CubeSize);

                Color color = Screen.UI.ColorWindow.ColorPicker.Color;

                // TODO: We can't make the cube transparent, because we have no guarantee that it will render after the
                // editor model. If the cube renders first, it will only blend with the skybox.
                //color.A = 128;

                cursorCube.ColorOverlay = color;

                entRenderer.Batch(cursorCube);
            }
        }
    }
}
