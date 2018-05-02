using System.Collections.Generic;

/* StartupArguments.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public class StartupArgument
    {
        public string Name;
        public string[] Values;
        public string Modifier;

        public StartupArgument(string name, string[] values, string modifier)
        {
            Name = name;
            Values = values;
            Modifier = modifier;
        }
    }

    public class StartupArguments
    {
        Dictionary<string, StartupArgument> args;

        public StartupArguments(string[] args)
        {
            this.args = new Dictionary<string, StartupArgument>();

            bool lookingForValues = false;
            List<string> values = new List<string>();
            string currentName = null;
            string currentMod = null;

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                string mod = GetArgModifier(a);

                if (mod == null && lookingForValues)
                    values.Add(a);
                else if (mod != null)
                {
                    lookingForValues = true;

                    if (currentName != null)
                    {
                        StartupArgument arg = new StartupArgument(currentName, values.ToArray(), currentMod);
                        this.args.Add(currentName, arg);
                        values.Clear();
                    }

                    currentName = a.Substring(1);
                    currentMod = mod;
                }
            }

            if (currentName != null)
            {
                StartupArgument arg = new StartupArgument(currentName, values.ToArray(), currentMod);
                this.args.Add(currentName, arg);
            }
        }

        string GetArgModifier(string a)
        {
            if (a.StartsWith("-")) return "-";
            else if (a.StartsWith("+")) return "+";
            else return null;
        }

        public bool IsArgSet(string name)
        {
            return args.ContainsKey(name);
        }

        public bool IsArgSet(string name, string modifier)
        {
            StartupArgument arg;
            if (args.TryGetValue(name, out arg))
                return arg.Modifier == modifier;
            else
                return false;
        }

        public StartupArgument GetArg(string name)
        {
            StartupArgument arg;
            if (args.TryGetValue(name, out arg))
                return arg;
            else
                return null;
        }

        public StartupArgument this[string name]
        {
            get { return GetArg(name); }
        }
    }
}
