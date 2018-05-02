using Dash.Engine.Graphics.Gui;
using System.Collections.Generic;

/* GUISystem.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    /// <summary>
    /// A GUI Area manager for a 2D environment.
    /// </summary>
    public class GUISystem
    {
        // TODO: This only supports one instance of GUISystem, making this a singleton might help.
        public static bool HandledMouseInput { get; private set; }
        public static bool HandledMouseOver { get; private set; }

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        SpriteBatch sb;
        List<GUIArea> areas;
        bool requireReSort;

        public GUISystem(SpriteBatch sb)
        {
            this.sb = sb;
            areas = new List<GUIArea>();
            ScreenWidth = sb.Width;
            ScreenHeight = sb.Height;
        }

        public void OnScreenResized(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;

            foreach (GUIArea area in areas)
                area.Resize(width, height);
        }

        public void Add(params GUIArea[] areas)
        {
            for (int i = 0; i < areas.Length; i++)
            {
                GUIArea area = areas[i];
                this.areas.Add(area);
                area.Resize(ScreenWidth, ScreenHeight);
            }

            requireReSort = true;
        }

        public void Remove(params GUIArea[] areas)
        {
            for (int i = 0; i < areas.Length; i++)
                this.areas.Remove(areas[i]);
            requireReSort = true;
        }

        public void Update(float deltaTime)
        {
            // Check for the need to resort areas by zindex
            for (int i = 0; i < areas.Count; i++)
            {
                GUIArea area = areas[i];

                if (area.ZIndexChanged)
                {
                    requireReSort = true;
                    area.ZIndexChanged = false;
                }
            }

            // Sort if needed
            if (requireReSort)
                areas.Sort(CompareZ);

            // Process mouse input in order by z
            bool clickHandled = false, mouseOverHandled = false;
            for (int i = areas.Count - 1; i >= 0; i--)
            {
                GUIArea area = areas[i];
                area.Update(deltaTime);

                if (area.Visible)
                    area.ProcessMouse(clickHandled, mouseOverHandled,
                        out clickHandled, out mouseOverHandled);
            }

            HandledMouseInput = clickHandled;
            HandledMouseOver = mouseOverHandled;
        }

        int CompareZ(GUIArea a, GUIArea b)
        {
            return a.ZIndex.CompareTo(b.ZIndex);
        }

        public void Draw()
        {
            for (int i = 0; i < areas.Count; i++)
            {
                GUIArea area = areas[i];
                if (area.Visible)
                    area.Draw(sb);
            }
        }
    }
}
