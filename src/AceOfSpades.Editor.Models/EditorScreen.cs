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
        public VoxelGridObject VoxelGrid { get; private set; }
        public bool RenderGrid { get; set; } = true;

        readonly MasterRenderer renderer;
        readonly EntityRenderer entReneder;

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

                if (VoxelGrid != null)
                    VoxelGrid.Dispose();

                VoxelGrid = new VoxelGridObject(Model);
                Camera.Active.SetTarget(Model.CenterPosition);
            }
        }

        public void LoadNewModel()
        {
            Model = new VoxelEditorObject(20, 20, 20, 1f);
            VoxelGrid = new VoxelGridObject(Model);
            Camera.Active.Position = Vector3.Zero;
            Camera.Active.SetTarget(Model.CenterPosition);
            Window.UpdateTitle(null);
            CurrentFile = null;
        }

        public void UpdateGrid()
        {
            if (VoxelGrid != null)
            {
                VoxelGrid.Dispose();

                VoxelGrid = new VoxelGridObject(Model);
            }
        }

        public void Update(float deltaTime)
        {
            if (Model != null)
            {
                ModelEditor.Update(deltaTime);
            }

            UI.Update(deltaTime);
        }

        public void Draw()
        {
            if (VoxelGrid != null && RenderGrid)
            {
                entReneder.Batch(VoxelGrid, Vector3.Zero);
            }

            if (Model != null)
            {
                entReneder.Batch(Model, Vector3.Zero);
            }

            ModelEditor.Draw();
        }
    }
}
