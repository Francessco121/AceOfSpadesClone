using Dash.Engine;
using Dash.Engine.Graphics.Gui;

/* GUIColorPickerWindow.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.Gui
{
    public class GUIColorPickerWindow : GUIWindow
    {
        public GUIColorPicker ColorPicker { get; }

        public GUIColorPickerWindow(GUISystem system, UDim2 size, GUITheme theme, 
            bool closable = true) 
            : base(system, size, "Color Picker", theme, closable)
        {
            ColorPicker = new GUIColorPicker(new UDim2(0, 5, 0, 25), new UDim2(1f, -10, 1f, -30), theme);
            AddTopLevel(ColorPicker);
        }
    }
}
