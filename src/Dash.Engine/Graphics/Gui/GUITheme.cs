using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Gui;
using System;
using System.Collections.Generic;

namespace Dash.Engine.Graphics.Gui
{
    public class GUITheme
    {
        public static GUITheme Empty
        {
            get { return new GUITheme(); }
        }

        public static GUITheme Basic
        {
            get
            {
                GUITheme t = new GUITheme();

                t.SetField("TextColor", Color.White);
                t.SetField("TextShadowColor", null);

                return t;
            }
        }

        Dictionary<string, object> fields;

        public GUITheme()
        {
            fields = new Dictionary<string, object>();
        }

        public void SetField(string key, object value)
        {
            if (fields.ContainsKey(key))
                fields[key] = value;
            else
                fields.Add(key, value);
        }

        public T GetField<T>(T fallback, params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                object ob;
                if (fields.TryGetValue(keys[i], out ob))
                {
                    try
                    {
                        T t = (T)ob;
                        return t;
                    }
                    catch (Exception) { }
                }
            }

            return fallback;
        }
    }
}
