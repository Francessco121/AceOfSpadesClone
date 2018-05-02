using System;
using System.Collections.Specialized;

namespace Dash.Engine.IO
{
    public class ConfigSection
    {
        public string Name { get; protected set; }
        public OrderedDictionary Children { get; protected set; }
        public ConfigSection Parent { get; protected set; }

        public ConfigSection(string name, ConfigSection parent)
        {
            Name = name;
            Parent = parent;
            Children = new OrderedDictionary();
        }

        /// <summary>
        /// Navigates to a value in the hierarchy.
        /// <para>Path is always relative to this section.</para>
        /// </summary>
        /// <example>
        /// string path = "../UserLocalConfigStore/Software/Valve/Steam/apps";
        /// ConfigSection apps = (ConfigSection)file.Navigate(path);
        /// </example>
        /// <param name="path">A hierarchy path where each section is seperate by a forward slash (/).</param>
        public object Navigate(string path)
        {
            string[] ops = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            ConfigSection currentSection = this;
            for (int i = 0; i < ops.Length; i++)
            {
                string op = ops[i];
                if (op == "..")
                    currentSection = currentSection.Parent;
                else if (op != ".")
                {
                    object obj = currentSection.Children[op];

                    if (obj == null)
                        return null;
                        //throw new InvalidOperationException(string.Format("Section {0} doesn't exist. ", path));

                    ConfigSection newSection = obj as ConfigSection;
                    if (newSection != null)
                        currentSection = newSection;
                    else if (i == ops.Length - 1)
                        return obj;
                    else
                        return null;
                        //throw new InvalidOperationException("Reached a key value pair before end of path.");
                }
            }

            return currentSection;
        }

        public dynamic this[int index]
        {
            get { return Children[index]; }
            set { Children[index] = value; }
        }

        public dynamic this[string name]
        {
            get { return Children[name]; }
            set { Children[name] = value; }
        }

        public string GetString(string name)
        {
            object ob = Children[name];
            if (ob != null)
                return ob as string;
            else
                return null;
        }

        public int? GetInteger(string name)
        {
            object ob = Children[name];
            string obs;
            if (ob != null && (obs = ob as string) != null)
            {
                int i;
                if (int.TryParse(obs, out i))
                    return i;
            }

            return null;
        }

        public float? GetFloat(string name)
        {
            object ob = Children[name];
            string obs;
            if (ob != null && (obs = ob as string) != null)
            {
                float f;
                if (float.TryParse(obs, out f))
                    return f;
            }

            return null;
        }

        public bool? GetBoolean(string name)
        {
            object ob = Children[name];
            string obs;
            if (ob != null && (obs = ob as string) != null)
            {
                bool b;
                if (bool.TryParse(obs, out b))
                    return b;
            }

            return null;
        }

        public T? GetEnum<T>(string name)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("ConfigSection.GetEnum expected enum type, got {0}", typeof(T)));

            object ob = Children[name];
            string obs;
            if (ob != null && (obs = ob as string) != null)
            {
                T t;
                if (Enum.TryParse(obs, true, out t))
                    return t;
            }

            return null;
        }

        public ConfigSection GetSection(string name)
        {
            object ob = Children[name];
            if (ob != null)
                return ob as ConfigSection;
            else
                return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
