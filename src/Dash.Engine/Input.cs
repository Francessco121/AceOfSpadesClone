using Dash.Engine.Graphics;
using Dash.Engine.Graphics.Context;
using System;
using System.Collections.Generic;
using System.Text;

/* Input.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public static class Input
    {
        public class InputBind
        {
            Key[] keys;
            MouseButton[] mbtns;

            string toStr;

            internal InputBind(Key[] keys, MouseButton[] mbtns)
            {
                this.keys = keys;
                this.mbtns = mbtns;

                StringBuilder sb = new StringBuilder();
                if (keys != null)
                    for (int i = 0; i < keys.Length; i++)
                    {
                        sb.AppendFormat("{0}", keys[i].ToString());

                        if (i < keys.Length - 1 || mbtns != null && mbtns.Length > 0)
                            sb.Append(", ");
                    }

                if (mbtns != null)
                    for (int i = 0; i < mbtns.Length; i++)
                    {
                        MouseButton mbtn = mbtns[i];

                        if (mbtn == MouseButton.Left)
                            sb.Append("Left Mouse");
                        else if (mbtn == MouseButton.Right)
                            sb.Append("Right Mouse");
                        else if (mbtn == MouseButton.Middle)
                            sb.Append("Middle Mouse");
                        else
                            sb.AppendFormat("Mouse {0}", mbtn.ToString());

                        if (i < mbtns.Length - 1)
                            sb.Append(", ");
                    }

                toStr = sb.ToString();
            }

            public bool Get() { return GetState(GetKey, GetMouseButton); }
            public bool GetDown() { return GetState(GetKeyDown, GetMouseButtonDown); }
            public bool GetUp() { return GetState(GetKeyUp, GetMouseButtonUp); }

            bool GetState(Func<Key, bool> keyMethod, Func<MouseButton, bool> mMethod)
            {
                for (int i = 0; i < keys.Length; i++)
                    if (keyMethod(keys[i]))
                        return true;

                for (int i = 0; i < mbtns.Length; i++)
                    if (mMethod(mbtns[i]))
                        return true;

                return false;
            }

            public override string ToString()
            {
                return toStr;
            }
        }

        public static int CursorX
        {
            get { return ms.X; }
            set { SetCursorPos(value, CursorY); }
        }
        public static int CursorY
        {
            get { return ms.Y; }
            set { SetCursorPos(CursorX, value); }
        }
        public static int ClampedCursorX { get { return ms.ClampedX; } }
        public static int ClampedCursorY { get { return ms.ClampedY; } }
        public static int CursorDeltaX { get; private set; }
        public static int CursorDeltaY { get; private set; }

        public static Vector2i CursorPosition
        {
            get { return new Vector2i(CursorX, CursorY); }
        }

        public static int ScrollX { get; private set; }
        public static int ScrollY { get; private set; }
        public static int ScrollDeltaX { get; private set; }
        public static int ScrollDeltaY { get; private set; }

        public static bool IsControlHeld { get { return GetKey(Key.LeftControl) || GetKey(Key.RightControl); } }
        public static bool IsAltHeld { get { return GetKey(Key.LeftAlt) || GetKey(Key.RightAlt); } }
        public static bool IsShiftHeld { get { return GetKey(Key.LeftShift) || GetKey(Key.RightShift); } }

        public static bool GetKey(Key key) { return kb[key]; }
        public static bool GetKeyDown(Key key) { return kb[key] && !lkb[key]; }
        public static bool GetKeyUp(Key key) { return !kb[key] && lkb[key]; }

        public static bool GetMouseButton(MouseButton button) { return ms[button]; }
        public static bool GetMouseButtonDown(MouseButton button) { return ms[button] && !lms[button]; }
        public static bool GetMouseButtonUp(MouseButton button) { return !ms[button] && lms[button]; }

        public static bool GetControl(string bindName) { return Binds[bindName].Get(); }
        public static bool GetControlDown(string bindName) { return Binds[bindName].GetDown(); }
        public static bool GetControlUp(string bindName) { return Binds[bindName].GetUp(); }

        public static bool IsBound(string name) { return Binds.ContainsKey(name); }
        public static bool Unbind(string name) { return Binds.Remove(name); }
        public static void Bind(string name, params Key[] keys) { Bind(name, keys, new MouseButton[0]); }
        public static void Bind(string name, params MouseButton[] mbtns) { Bind(name, new Key[0], mbtns); }
        public static void Bind(string name, Key[] keys, MouseButton[] mbtns)
        {
            if (Binds.ContainsKey(name))
                Binds[name] = new InputBind(keys, mbtns);
            else
                Binds.Add(name, new InputBind(keys, mbtns));
        }

        public static bool IsCursorVisible
        {
            get { return window.CursorMode == CursorMode.CursorNormal; }
            set { if (value != IsCursorVisible) window.CursorMode = value ? CursorMode.CursorNormal : CursorMode.CursorHidden; }
        }

        public static bool IsCursorLocked
        {
            get { return isCursorLocked; }
            set { isCursorLocked = value; }
        }
        static bool isCursorLocked;

        public static Key[] GetPressedKeys() { return heldKeys.ToArray(); }

        public static KeyboardState CurrentKeyboardState { get { return kb; } }
        public static KeyboardState LastKeyboardState { get { return lkb; } }
        public static MouseState CurrentMouseState { get { return ms; } }
        public static MouseState LastMouseState { get { return lms; } }

        public static Dictionary<string, InputBind> Binds { get; private set; }
        static List<Key> heldKeys;

        static KeyboardState kb = KeyboardState.Empty;
        static KeyboardState lkb = KeyboardState.Empty;
        static MouseState ms = MouseState.Empty;
        static MouseState lms = MouseState.Empty;
        static GameWindow window;

        static bool windowWasFocused;
        static int lastScrollX, lastScrollY;

        public static KeyboardState GetKeyboardState()
        {
            return new KeyboardState(window);
        }

        public static MouseState GetMouseState()
        {
            return new MouseState(window);
        }

        public static bool WrapCursor()
        {
            bool wrapped = false;
            if (CursorX <= 0)
            {
                SetCursorPos(window.Width - 2, CursorY);
                wrapped = true;
            }
            if (CursorY <= 0)
            {
                SetCursorPos(CursorX, window.Height - 2);
                wrapped = true;
            }
            if (CursorX >= window.Width - 1)
            {
                SetCursorPos(1, CursorY);
                wrapped = true;
            }
            if (CursorY >= window.Height - 1)
            {
                SetCursorPos(CursorX, 1);
                wrapped = true;
            }

            return wrapped;
        }

        public static void SetCursorPos(int x, int y)
        {
            Glfw.SetCursorPos(window.GetPointer(), x, y);
        }

        static Input()
        {
            Binds = new Dictionary<string, InputBind>();
            heldKeys = new List<Key>();
        }

        internal static void Initialize(GameWindow window)
        {
            if (Input.window != null)
                throw new InvalidOperationException("Input can only be registered with one game window!");

            Input.window = window;

            window.OnScroll += OnScroll;
            window.OnKeyAction += OnKey;
        }

        internal static void Begin()
        {
            KeyboardState _kb = new KeyboardState(window);
            MouseState _ms = new MouseState(window);

            kb = window.HasFocus ? _kb : KeyboardState.Empty;
            ms = window.HasFocus ? _ms : MouseState.Empty;

            if (window.HasFocus)
            {
                if (!windowWasFocused)
                {
                    // Fixes cursor/scroll jumping when re-entering window
                    CursorDeltaX = 0;
                    CursorDeltaY = 0;
                    lastScrollX = 0;
                    lastScrollY = 0;
                }
                else
                {
                    /*if (!isCursorLocked)
                    {
                        CursorDeltaX = ms.X - lms.X;
                        CursorDeltaY = ms.Y - lms.Y;
                    }
                    else*/
                    {
                        CursorDeltaX = ms.X - (window.Width / 2);
                        CursorDeltaY = ms.Y - (window.Height / 2);
                    }

                    ScrollDeltaX = lastScrollX;
                    ScrollDeltaY = lastScrollY;
                    lastScrollX = 0;
                    lastScrollY = 0;
                }
            }
            else
            {
                CursorDeltaX = 0;
                CursorDeltaY = 0;
                ScrollDeltaX = 0;
                ScrollDeltaY = 0;
            }

            windowWasFocused = window.HasFocus;
        }

        internal static void End()
        {
            if (windowWasFocused)
            {
                lkb = kb;
                lms = ms;

                if (isCursorLocked)
                    SetCursorPos(window.Width / 2, window.Height / 2);
            }
        }

        static void OnScroll(GameWindow _, int x, int y)
        {
            ScrollX = x;
            ScrollY = y;

            lastScrollX = x;
            lastScrollY = y;
        }

        static void OnKey(GameWindow _, Key key, int scancode, KeyAction action, KeyModifiers modifiers)
        {
            if (action == KeyAction.Press)
                heldKeys.Add(key);
            else if (action == KeyAction.Release)
                heldKeys.Remove(key);
        }
    }
}
