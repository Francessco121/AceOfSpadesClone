using AceOfSpades.Editor.Gui;
using Dash.Engine;
using Dash.Engine.Graphics.Gui;

/* TerraformWindow.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World.Tools
{
    public class TerraformWindow : GUIWindow
    {
        public int BrushSize { get; private set; } = 20;
        public int RiseHeight { get; private set; } = 4;

        public TerraformWindow(GUISystem system, GUITheme theme) 
            : base(system, new UDim2(0.25f, 0, 0.25f, 0), "Terraform Options", theme, false)
        {
            Position = new UDim2(0, 0, 0.75f, 0);
            MinSize = new UDim2(0, 200, 0, 250);
            MaxSize = new UDim2(0, 475, 0, 350);

            GUIForm form = new GUIForm(UDim2.Zero, new UDim2(1f, 0, 1f, 0), theme);

            GUILabel brushSizeLabel;
            GUITextField brushSizeField;
            form.AddLabledTextField("Brush Size:", BrushSize.ToString(), new UDim2(0, 5, 0, 25),
                out brushSizeLabel, out brushSizeField);
            brushSizeField.OnTextChanged += BrushSizeField_OnTextChanged;

            GUILabel riseHeightLabel;
            GUITextField riseHeightField;
            form.AddLabledTextField("Rise Height:", RiseHeight.ToString(), new UDim2(0, 5, 0, 30 + brushSizeLabel.Size.Y.Offset),
                out riseHeightLabel, out riseHeightField);
            riseHeightField.OnTextChanged += RiseHeightField_OnTextChanged;

            AddTopLevel(form);
        }

        private void RiseHeightField_OnTextChanged(GUITextField field, string text)
        {
            int riseHeight;
            if (int.TryParse(text, out riseHeight))
                RiseHeight = riseHeight;
        }

        private void BrushSizeField_OnTextChanged(GUITextField field, string text)
        {
            int brushSize;
            if (int.TryParse(text, out brushSize))
                BrushSize = brushSize;
        }
    }
}
