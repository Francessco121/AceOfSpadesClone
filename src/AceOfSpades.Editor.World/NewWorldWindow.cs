using AceOfSpades.Editor.Gui;
using Dash.Engine;
using Dash.Engine.Graphics.Gui;
using System;

namespace AceOfSpades.Editor.World
{
    public class NewWorldWindow : GUIWindow
    {
        EditorScreen screen;

        public NewWorldWindow(GUISystem system, EditorScreen screen, GUITheme theme) 
            : base(system, new UDim2(0.4f, 0, 0.3f, 0), "New World", theme, true)
        {
            this.screen = screen;
            MinSize = new UDim2(0, 400, 0, 300);
            MaxSize = new UDim2(0, 575, 0, 475);

            GUIForm form = new GUIForm(new UDim2(0, 5, 0, 25), new UDim2(1f, -10, 1f, -30), Theme);

            GUILabel useNoiseLabel;
            GUICheckbox useNoiseCheckBox;

            form.AddLabledCheckbox("Use Noise:", false, UDim2.Zero, out useNoiseLabel, out useNoiseCheckBox);

            GUILabel sizeLabel = new GUILabel(new UDim2(0, 0, 0, 45), UDim2.Zero, "World Size:", TextAlign.TopLeft, Theme)
            { Parent = form };

            GUILabel xSizeLabel, ySizeLabel, zSizeLabel;
            GUITextField xSizeField, ySizeField, zSizeField;

            form.AddLabledTextField("X:", "8", new UDim2(0, 0, 0, 70),
                out xSizeLabel, out xSizeField, new UDim(0, 40));
            float sizeInputLength = xSizeField.Position.X.Offset + xSizeField.Size.X.Offset;
            form.AddLabledTextField("Y:", "3", new UDim2(0, sizeInputLength + 5, 0, 70),
                out ySizeLabel, out ySizeField, new UDim(0, 40));
            form.AddLabledTextField("Z:", "8", new UDim2(0, (sizeInputLength + 5) * 2, 0, 70),
                out zSizeLabel, out zSizeField, new UDim(0, 40));

            xSizeField.Label.TextAlign = TextAlign.Center;
            ySizeField.Label.TextAlign = TextAlign.Center;
            zSizeField.Label.TextAlign = TextAlign.Center;

            GUIButton cancelBtn = new GUIButton(new UDim2(1f, -100, 1f, -30), new UDim2(0, 100, 0, 30), "Cancel", theme)
            { Parent = form };
            cancelBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                    Visible = false;
            };

            GUIButton createBtn = new GUIButton(new UDim2(1f, -205, 1f, -30), new UDim2(0, 100, 0, 30), "Create", theme)
            { Parent = form };
            createBtn.OnMouseClick += (btn, mbtn) =>
            {
                if (mbtn == MouseButton.Left)
                {
                    int x = 1, y = 1, z = 1;
                    if (int.TryParse(xSizeField.Text, out x))
                        x = Math.Max(x, 0);
                    if (int.TryParse(ySizeField.Text, out y))
                        y = Math.Max(y, 0);
                    if (int.TryParse(zSizeField.Text, out z))
                        z = Math.Max(z, 0);

                    if (!useNoiseCheckBox.IsChecked)
                        screen.LoadNewFlatWorld(x, y, z);
                    else
                        screen.LoadNewWorld(x, y, z);

                    Visible = false;
                }
            };

            AddTopLevel(form);
        }
    }
}
