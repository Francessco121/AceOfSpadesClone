using AceOfSpades.Editor.World.WorldObjects;
using AceOfSpades.IO;
using Dash.Engine;
using Dash.Engine.Graphics;
using System;

namespace AceOfSpades.Editor.World
{
    public class EditorScreen
    {
        public string CurrentFile = null;

        public EditorUI UI { get; private set; }
        public MainWindow Window { get; private set; }
        public WorldEditor WorldEditor { get; private set; }
        public EditorWorld World { get; private set; }

        MasterRenderer renderer;

        public EditorScreen(MainWindow window, MasterRenderer renderer)
        {
            this.Window = window;
            this.renderer = renderer;

            World = new EditorWorld(this);
            UI = new EditorUI(renderer, this);
            WorldEditor = new WorldEditor(this);

            //LoadWorld("Content/Worlds/rockyc.aosw");
        }

        public void SaveWorld()
        {
            if (CurrentFile == null)
                throw new InvalidOperationException("File has not yet been saved!");

            if (World.Terrain != null)
                WorldIO.Save(CurrentFile, World.CreateDescription(), false);
        }

        public void SaveWorld(string filePath)
        {
            if (World.Terrain != null)
            {
                CurrentFile = filePath;
                Window.UpdateTitle(filePath);

                WorldDescription desc = World.CreateDescription();
                WorldIO.Save(filePath, desc, false);
            }
        }

        public void LoadWorld(string name)
        {
            World.UnloadTerrain();
            WorldDescription desc = WorldIO.Load(name, false);
            World.SetTerrain(desc.Terrain);
            WorldEditor.TerrainEditor.ClearUndoRedo();

            Window.UpdateTitle(name);
            CurrentFile = name;

            var commandposts = desc.GetObjectsByTag("CommandPost");
            foreach (WorldObjectDescription ob in commandposts)
            {
                Vector3 position = ob.GetVector3("Position");
                CommandPostObject editorCommandPost = new CommandPostObject(position);
                Team team = (Team)(ob.GetField<byte>("Team") ?? 0);
                editorCommandPost.Team = team;

                World.AddGameObject(editorCommandPost);
            }

            var intels = desc.GetObjectsByTag("Intel");
            foreach (WorldObjectDescription ob in intels)
            {
                Vector3 position = ob.GetVector3("Position");
                IntelObject editorIntel = new IntelObject(position);
                Team team = (Team)(ob.GetField<byte>("Team") ?? 0);
                editorIntel.Team = team;

                World.AddGameObject(editorIntel);
            }
        }

        public void LoadNewWorld(int x, int y, int z)
        {
            World.UnloadTerrain();
            FixedTerrain terrain = new FixedTerrain(renderer);
            terrain.Generate(x, y, z);
            World.SetTerrain(terrain);
            WorldEditor.TerrainEditor.ClearUndoRedo();

            Window.UpdateTitle(null);
            CurrentFile = null;
        }

        public void LoadNewFlatWorld(int x, int y, int z)
        {
            World.UnloadTerrain();
            FixedTerrain terrain = new FixedTerrain(renderer);
            terrain.GenerateFlat(x, y, z);
            World.SetTerrain(terrain);
            WorldEditor.TerrainEditor.ClearUndoRedo();

            Window.UpdateTitle(null);
            CurrentFile = null;
        }

        public void Update(float deltaTime)
        {
            WorldEditor.Update(deltaTime);
            World.Update(deltaTime);
            UI.Update(deltaTime);
        }

        public void Draw()
        {
            WorldEditor.Draw();
            World.Draw();
        }
    }
}
