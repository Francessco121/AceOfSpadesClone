using System.Collections.Generic;

namespace AceOfSpades.Net
{
    public abstract class NestedSnapshot
    {
        public Snapshot Parent { get; }
        public string Name { get; }

        string index;
        List<string> fieldNames;
        string[] fieldNamesFinal;

        public NestedSnapshot(Snapshot parent)
        {
            Parent = parent;
            Name = GetType().Name;
            fieldNames = new List<string>();
        }

        public void InvalidateFields()
        {
            Parent.InvalidateFields(fieldNamesFinal);
        }

        public void Initialize(int? index)
        {
            if (index.HasValue)
                this.index = index.ToString();
            else
                this.index = null;

            OnInitialize();
            fieldNamesFinal = fieldNames.ToArray();
        }

        protected abstract void OnInitialize();

        public void Set<T>(string name, T value)
             where T : struct
        {
            Parent.Set(string.Format("{0}.{1}[{2}]", Name, name, index ?? ""), value);
        }

        public T Get<T>(string name)
            where T : struct
        {
            return Parent.Get<T>(string.Format("{0}.{1}[{2}]", Name, name, index ?? ""));
        }

        protected void AddField<T>(string name)
            where T : struct
        {
            string fname = string.Format("{0}.{1}[{2}]", Name, name, index ?? "");
            fieldNames.Add(fname);
            Parent.AddField<T>(fname);
        }
    }
}
