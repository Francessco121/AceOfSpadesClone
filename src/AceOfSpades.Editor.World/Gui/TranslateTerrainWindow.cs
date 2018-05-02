using AceOfSpades.Editor.Gui;
using Dash.Engine;
using Dash.Engine.Graphics.Gui;
using System;

namespace AceOfSpades.Editor.World.Gui
{
    public class TranslateTerrainWindow : GUIWindow
    {
        public event EventHandler<IndexPosition> OnApply;

        GUITextField xField;
        GUITextField yField;
        GUITextField zField;

        public TranslateTerrainWindow(GUISystem system, GUITheme theme) 
            : base(system, new UDim2(0.2f, 0, 0, 175), "Translate Terrain", theme)
        {
            MinSize = new UDim2(0.2f, 0, 0, 175);

            GUIForm form = new GUIForm(new UDim2(0, 5, 0, 25), new UDim2(1f, -5, 1f, -25), theme);

            form.AddLabledTextField("X:", "0", new UDim2(0, 0, 0, 0), out xField);
            form.AddLabledTextField("Y:", "0", new UDim2(0, 0, 0, 35), out yField);
            form.AddLabledTextField("Z:", "0", new UDim2(0, 0, 0, 70), out zField);

            GUIButton applyBtn = new GUIButton(new UDim2(0, 5, 1f, -40), new UDim2(0, 100, 0, 30), "Apply", theme);
            GUIButton cancelBtn = new GUIButton(new UDim2(1f, -105, 1f, -35), new UDim2(0, 100, 0, 30), "Cancel", theme);

            applyBtn.OnMouseClick += ApplyBtn_OnMouseClick;
            cancelBtn.OnMouseClick += CancelBtn_OnMouseClick;

            AddTopLevel(form, applyBtn, cancelBtn);
        }

        protected override void Shown()
        {
            xField.Text = "0";
            yField.Text = "0";
            zField.Text = "0";
            base.Shown();
        }

        private void CancelBtn_OnMouseClick(GUIButton btn, MouseButton button)
        {
            Visible = false;
        }

        private void ApplyBtn_OnMouseClick(GUIButton btn, MouseButton button)
        {
            int x, y, z;
            int.TryParse(xField.Text, out x);
            int.TryParse(yField.Text, out y);
            int.TryParse(zField.Text, out z);

            if (OnApply != null)
                OnApply(this, new IndexPosition(x, y, z));

            Visible = false;
        }
    }
}
