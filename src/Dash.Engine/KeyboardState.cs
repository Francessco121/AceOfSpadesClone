using Dash.Engine.Diagnostics;
using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Context;
using System;
using System.Collections.Generic;

/* KeyboardState.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public class KeyboardState
    {
        public static KeyboardState Empty
        {
            get { return new KeyboardState(); }
        }

        static Key[] allKeys;

        Dictionary<Key, bool> keys;

        static KeyboardState()
        {
            Array _allKeys = Enum.GetValues(typeof(Key));
            allKeys = new Key[_allKeys.Length];
            for (int i = 0; i < _allKeys.Length; i++)
                allKeys[i] = (Key)_allKeys.GetValue(i);
        }

        private KeyboardState()
        {
            keys = new Dictionary<Key, bool>(allKeys.Length);
            for (int i = 0; i < allKeys.Length; i++)
                keys[allKeys[i]] = false;
        }

        public KeyboardState(GameWindow window)
            : this()
        {
            GlfwWindowPtr ptr = window.GetPointer();

            keys = new Dictionary<Key, bool>(allKeys.Length);
            for (int i = 0; i < allKeys.Length; i++)
            {
                Key k = allKeys[i];
                keys[k] = Glfw.GetKey(ptr, k);
            }
        }

        public bool IsKeyDown(Key key)
        {
            return keys[key];
        }

        public bool IsKeyUp(Key key)
        {
            return !keys[key];
        }

        public bool this[Key key]
        {
            get { return keys[key]; }
        }
    }
}
