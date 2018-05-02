using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Context;
using System;
using System.Collections.Generic;

/* MouseState.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public class MouseState
    {
        public static MouseState Empty
        {
            get { return new MouseState(); }
        }

        public static MouseButton[] AllButtons { get; }

        public int X { get; private set; }
        public int Y { get; private set; }

        /// <summary>
        /// Gets the X cursor coordinate clamped between the window
        /// left and right bounds. Useful for when the cursor mode is set
        /// to 'captured'.
        /// </summary>
        public int ClampedX { get; private set; }
        /// <summary>
        /// Gets the Y cursor coordinate clamped between the window
        /// top and bottom bounds. Useful for when the cursor mode is set
        /// to 'captured'.
        /// </summary>
        public int ClampedY { get; private set; }

        public int ScrollX { get; private set; }
        public int ScrollY { get; private set; }

        public Dictionary<MouseButton, bool> Buttons { get; private set; }

        static MouseState()
        {
            Array _allBtns = Enum.GetValues(typeof(MouseButton));
            AllButtons = new MouseButton[_allBtns.Length];
            for (int i = 0; i < _allBtns.Length; i++)
                AllButtons[i] = (MouseButton)_allBtns.GetValue(i);
        }

        private MouseState()
        {
            Buttons = new Dictionary<MouseButton, bool>(AllButtons.Length);
            for (int i = 0; i < AllButtons.Length; i++)
                Buttons[AllButtons[i]] = false;
        }

        public MouseState(GameWindow window)
        {
            GlfwWindowPtr ptr = window.GetPointer();

            Buttons = new Dictionary<MouseButton, bool>(AllButtons.Length);
            for (int i = 0; i < AllButtons.Length; i++)
            {
                MouseButton b = AllButtons[i];
                Buttons[b] = Glfw.GetMouseButton(ptr, b);
            }

            double _x, _y;
            Glfw.GetCursorPos(ptr, out _x, out _y);

            X = (int)_x;
            Y = (int)_y;

            ClampedX = MathHelper.Clamp(X, 0, window.Width);
            ClampedY = MathHelper.Clamp(Y, 0, window.Height);

            // MouseState must rely on Input for scrolling
            // since the scroll wheel values can only be
            // accessed in a callback.
            ScrollX = Input.ScrollX;
            ScrollY = Input.ScrollY;
        }

        public bool IsMouseButtonDown(MouseButton btn)
        {
            return Buttons[btn];
        }

        public bool this[MouseButton btn]
        {
            get { return Buttons[btn]; }
        }
    }
}
