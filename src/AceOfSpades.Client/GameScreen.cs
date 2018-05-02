using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System.Collections.Generic;

/* (Client)GameScreen.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Client
{
    public abstract class GameScreen
    {
        public string Name { get; }

        protected MasterRenderer Renderer { get; }
        protected MainWindow Window { get; }
        protected GUISystem GUISystem { get; }
        protected GUIArea GUIArea { get; }
        protected HashSet<GUIWindowBase> Windows { get; }

        public GameScreen(MainWindow window, string name)
        {
            Name = name;
            Window = window;
            Renderer = window.Renderer;

            GUISystem = Renderer.Sprites.GUISystem;
            GUIArea = new GUIArea(GUISystem);
            GUISystem.Add(GUIArea);
            GUIArea.Visible = false;
            Windows = new HashSet<GUIWindowBase>();
        }

        public virtual void Load(object[] args)
        {
            OnLoad(args);
        }

        public virtual void Unload()
        {
            OnUnload();
        }

        protected virtual void OnLoad(object[] args)
        {
            GUIArea.Visible = true;
        }

        protected virtual void OnUnload()
        {
            GUIArea.Visible = false;
            foreach (GUIWindowBase win in Windows)
                win.Visible = false;
        }

        public virtual void OnScreenResized(int width, int height) { }

        public abstract void Update(float deltaTime);
        public abstract void Draw();
    }
}
