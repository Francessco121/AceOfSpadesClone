using System.Collections.Generic;

/* GUIElementMouseState.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Graphics.Gui
{
    class GUIElementMouseState
    {
        public bool MouseWasOver;
        public readonly HashSet<MouseButton> ButtonsDown;

        GUIElement element;

        public GUIElementMouseState(GUIElement element)
        {
            this.element = element;
            ButtonsDown = new HashSet<MouseButton>();
        }
    }
}
