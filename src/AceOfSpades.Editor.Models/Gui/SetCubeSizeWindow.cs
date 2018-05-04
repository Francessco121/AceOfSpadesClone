using AceOfSpades.Editor.Gui;
using Dash.Engine;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;

namespace AceOfSpades.Editor.Models.Gui
{
    class SetCubeSizeWindow : GUIWindow
    {
        readonly EditorScreen screen;

        public SetCubeSizeWindow(GUISystem system, EditorScreen screen, GUITheme theme)
            : base(system, new UDim2(0, 300, 0, 150), "Set Cube Size", theme, true)
        {
            this.screen = screen;

            GUIForm form = new GUIForm(new UDim2(0, 5, 0, 25), new UDim2(1f, -10, 1f, -30), Theme);

            GUILabel sizeLabel;
            GUITextField sizeField;

            form.AddLabledTextField("Cube Size:", "1", new UDim2(0, 0, 0, 0), out sizeLabel, out sizeField, new UDim(0, 60));

            GUILabel noticeLabel = new GUILabel(new UDim2(0, 3, 0, 40), UDim2.Zero, "Must be a decimal between 0.1 and 24.",
                TextAlign.TopLeft, 
                font: theme.GetField<BMPFont>(null, "SmallFont"), 
                textColor: theme.GetField<Color>(Color.White, "Label.TextColor"),
                shadowColor: theme.GetField<Color?>(null, "Label.TextShadowColor"))
            { Parent = form };

            GUIButton cancelBtn = new GUIButton(new UDim2(1f, -100, 1f, -30), new UDim2(0, 100, 0, 30), "Cancel", theme)
            { Parent = form };
            cancelBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                    Visible = false;
            };

            GUIButton createBtn = new GUIButton(new UDim2(1f, -205, 1f, -30), new UDim2(0, 100, 0, 30), "Set Size", theme)
            { Parent = form };
            createBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                {
                    float size;
                    if (float.TryParse(sizeField.Text, out size))
                        size = MathHelper.Clamp(size, 0.1f, 24);

                    screen.Model.ChangeCubeSize(size);
                    screen.VoxelGrid.ChangeCubeSize(size);

                    Visible = false;
                }
            };

            AddTopLevel(form);
        }
    }
}
