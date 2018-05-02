using Dash.Engine;
using Dash.Engine.Graphics.Gui;

/* GUIForm.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.Gui
{
    public class GUIForm : GUIFrame
    {
        public float ElementPadding = 5;

        public GUIForm(UDim2 position, UDim2 size, GUITheme theme)
            : base(position, size, image: null)
        {
            Theme = theme;
        }

        public void AddLabledCheckbox(string labelText, bool checkboxDefaultState, UDim2 position,
            out GUILabel label, out GUICheckbox checkbox)
        {
            label = new GUILabel(position, UDim2.Zero, labelText, TextAlign.Left, Theme);
            Vector2 textSize = label.Font.MeasureString(labelText);
            label.Size = new UDim2(0, textSize.X, 0, textSize.Y + (ElementPadding * 2));

            UDim labelXPos = position.X + new UDim(0, textSize.X + ElementPadding);
            checkbox = new GUICheckbox(new UDim2(labelXPos, position.Y), label.Size.Y.Offset, Theme);
            checkbox.IsChecked = checkboxDefaultState;

            label.Parent = this;
            checkbox.Parent = this;
        }
        public void AddLabledTextField(string labelText, string defaultFieldText, UDim2 position,
            out GUITextField textField, UDim? fieldXLength = null)
        {
            GUILabel label;
            AddLabledTextField(labelText, defaultFieldText, position, out label, out textField, fieldXLength);
        }

        public void AddLabledTextField(string labelText, string defaultFieldText, UDim2 position,
            out GUILabel label, out GUITextField textField, UDim? fieldXLength = null)
        {
            label = new GUILabel(position, UDim2.Zero, labelText, TextAlign.Left, Theme);
            Vector2 textSize = label.Font.MeasureString(labelText);
            label.Size = new UDim2(0, textSize.X, 0, textSize.Y + (ElementPadding * 2));

            UDim labelXPos = position.X + new UDim(0, textSize.X + ElementPadding);
            if (!fieldXLength.HasValue)
                fieldXLength = new UDim(1f, -(textSize.X + ElementPadding) - ElementPadding);
            textField = new GUITextField(new UDim2(labelXPos, position.Y), 
                new UDim2(fieldXLength.Value, new UDim(0, label.Size.Y.Offset)), 
                defaultFieldText, TextAlign.Left, Theme);

            label.Parent = this;
            textField.Parent = this;
        }
    }
}
