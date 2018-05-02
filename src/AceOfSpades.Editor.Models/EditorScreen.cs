using AceOfSpades.Graphics;
using AceOfSpades.IO;
using Dash.Engine;
using Dash.Engine.Graphics;
using System;

namespace AceOfSpades.Editor.Models
{
    class EditorScreen
    {
        public string CurrentFile = null;

        public EditorUI UI { get; }
        public MainWindow Window { get; }
        public ModelEditor ModelEditor { get; }
        public VoxelEditorObject Model { get; private set; }

        readonly MasterRenderer renderer;
        readonly EntityRenderer entReneder;

        VoxelGridObject voxelGrid;

        public EditorScreen(MainWindow window, MasterRenderer renderer)
        {
            this.Window = window;
            this.renderer = renderer;

            entReneder = renderer.GetRenderer3D<EntityRenderer>();

            UI = new EditorUI(renderer, this);
            ModelEditor = new ModelEditor(this);

            LoadNewModel();
        }

        public void SaveModel()
        {
            if (CurrentFile == null)
                throw new InvalidOperationException("File has not yet been saved!");

            VoxelIO.Save(CurrentFile, Model);
        }

        public void SaveModel(string filePath)
        {
            VoxelIO.Save(filePath, Model);
        }

        public void LoadModel(string name)
        {
            VoxelObject tmpObj = null;
            if (VoxelIO.Load(name, out tmpObj))
            {
                Model = new VoxelEditorObject(tmpObj);
                Window.UpdateTitle(name);
                CurrentFile = name;

                if (voxelGrid != null)
                    voxelGrid.Dispose();

                voxelGrid = new VoxelGridObject(Model);
                Camera.Active.SetTarget(Model.CenterPosition);
            }
        }

        public void LoadNewModel()
        {
            Model = new VoxelEditorObject(new VoxelObject(1f));
            voxelGrid = new VoxelGridObject(Model);
            Camera.Active.Position = Vector3.Zero;
            Camera.Active.SetTarget(Model.CenterPosition);
            Window.UpdateTitle(null);
            CurrentFile = null;
        }

        public void Update(float deltaTime)
        {
            if (Model != null)
            {
                ModelEditor.Update(deltaTime);
            }

            UI.Update(deltaTime);

            

            //if (Model != null)
            //{
            //    Ray MouseRay = Camera.Active.MouseRay;
            //    IndexPosition blockNormalOffsetIndex = IndexPosition.Zero;
            //    IndexPosition MouseIndexPos = IndexPosition.Zero;
            //    CubeSide ModelSide = CubeSide.Bottom;
            //    Vector3 normal = Vector3.Zero;
            //    bool intersections = false;
            //    Color voxelColor = new Color(236, 157, 196);

            //    if (intersections = Model.RayIntersects(MouseRay, out MouseIndexPos, out ModelSide))
            //    {
            //        normal = Maths.CubeSideToSurfaceNormal(ModelSide);
            //        blockNormalOffsetIndex = new IndexPosition(
            //                MouseIndexPos.X + (int)normal.X,
            //                MouseIndexPos.Y + (int)normal.Y,
            //                MouseIndexPos.Z + (int)normal.Z);
            //    }
            //    if (Model.IsBlockCoordInRange(MouseIndexPos) && intersections)
            //    {
            //        if (Input.GetMouseButtonDown(MouseButton.Left))
            //        {
            //            Model.ChangeBlock(blockNormalOffsetIndex, new Block(1,
            //                voxelColor.R, voxelColor.G, voxelColor.B));
            //        }
            //    }
            //}

        }

        public void Draw()
        {
            if (voxelGrid != null)
            {
                entReneder.Batch(voxelGrid, Vector3.Zero);
            }

            if (Model != null)
            {
                entReneder.Batch(Model, Vector3.Zero);
            }

            ModelEditor.Draw();
        }
    }
}
