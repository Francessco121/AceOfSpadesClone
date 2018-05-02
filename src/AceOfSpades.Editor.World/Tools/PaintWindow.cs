using AceOfSpades.Editor.Gui;
using Dash.Engine;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Editor.World.Tools
{
    public class PaintWindow : GUIWindow
    {
        public int Grain { get; private set; } = 0;

        public PaintWindow(GUISystem system, GUITheme theme) 
            : base(system, new UDim2(0.25f, 0, 0.25f, 0), "Paint Options", theme, false)
        {
            Position = new UDim2(0, -10, 0.7f, -10);
            MinSize = new UDim2(0, 200, 0, 250);
            MaxSize = new UDim2(0, 475, 0, 350);

            GUIForm form = new GUIForm(UDim2.Zero, new UDim2(1f, 0, 1f, 0), theme);

            GUILabel grainLabel;
            GUITextField grainField;
            form.AddLabledTextField("Grainy Factor:", Grain.ToString(), new UDim2(0, 5, 0, 25),
                out grainLabel, out grainField);
            grainField.OnTextChanged += GrainField_OnTextChanged;

            AddTopLevel(form);
        }

        private void GrainField_OnTextChanged(GUITextField field, string text)
        {
            int grain;
            if (int.TryParse(text, out grain))
                Grain = grain;
        }
    }
}
