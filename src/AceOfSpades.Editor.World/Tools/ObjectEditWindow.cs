using AceOfSpades.Editor.Gui;
using Dash.Engine;
using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;
using System.Reflection;

/* ObjectEditWindow.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Editor.World.Tools
{
    public class ObjectEditWindow : GUIWindow
    {
        GUIForm form;
        List<GUIElement> elements;

        const float FIELD_HEIGHT = 25;
        const float FIELD_SPACING = 5;
        const float FIELD_Y_AREA = FIELD_HEIGHT + FIELD_SPACING;

        public ObjectEditWindow(GUISystem system, GUITheme theme) 
            : base(system, new UDim2(0.25f, 0, 0.25f, 0), "Edit <Object>", theme, false)
        {
            Position = new UDim2(0, 0, 0.75f, 0);
            MinSize = new UDim2(0, 200, 0, 250);
            MaxSize = new UDim2(0, 475, 0, 350);

            elements = new List<GUIElement>();

            form = new GUIForm(UDim2.Zero, new UDim2(1f, 0, 1f, 0), theme);
            AddTopLevel(form);

            Visible = false;
        }

        public void SetObject(EditorObject ob)
        {
            foreach (GUIElement element in elements)
                element.Parent = null;
            elements.Clear();

            if (ob != null)
            {
                Visible = true;
                Title = string.Format("Edit {0}", ob.EditorName);

                Type type = ob.GetType();
                foreach (var prop in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    foreach (var attr in prop.GetCustomAttributes(true))
                    {
                        EditableField field = attr as EditableField;
                        if (field != null)
                            TryCreateEditField(ob, prop, field);
                    }
                }
            }
            else
                Visible = false;
        }

        void TryCreateEditField(EditorObject ob, PropertyInfo prop, EditableField attr)
        {
            float y = elements.Count * FIELD_Y_AREA;
            UDim2 position = new UDim2(0, 0, 0, y + 25);

            GUILabel label = null;
            string labelText = string.Format("{0}:", attr.Name);

            Type type = prop.PropertyType;
            if (type == typeof(bool))
            {
                GUICheckbox checkbox;
                form.AddLabledCheckbox(labelText, (bool)prop.GetValue(ob), position, 
                    out label, out checkbox);
                elements.Add(checkbox);

                checkbox.OnCheckChanged += (sender, e) =>
                {
                    prop.SetValue(ob, e);
                };
            }
            else if (type == typeof(string))
            {
                GUITextField field;
                form.AddLabledTextField(labelText, (string)prop.GetValue(ob), position,
                    out label, out field);
                elements.Add(field);

                field.OnTextChanged += (sender, e) =>
                {
                    prop.SetValue(ob, e);
                };
            }

            if (label != null)
                elements.Add(label);
        }
    }
}
