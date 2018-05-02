using System;

namespace AceOfSpades.Editor.World
{
    public class EditableField : Attribute
    {
        public string Name { get; }

        public EditableField(string name)
        {
            Name = name;
        }
    }
}
