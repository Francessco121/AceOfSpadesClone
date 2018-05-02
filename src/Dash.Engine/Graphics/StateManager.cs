using System;
using System.Collections.Generic;
using Dash.Engine.Graphics.OpenGL;
using Dash.Engine.Diagnostics;

namespace Dash.Engine.Graphics
{
    public static class StateManager
    {
        public static bool UsePointWireframe;
#if DEBUG
        public static bool CheckForErrors = true;
#else
        public static bool CheckForErrors = false;
#endif

        static readonly Dictionary<EnableCap, bool> states;
        static DepthFunction depthFunc = DepthFunction.Less;
        static bool wireFrameEnabled;

        static StateManager()
        {
            states = new Dictionary<EnableCap, bool>();

            // Flush any previous errors
            GL.GetError();

            // Locate each default state value
            foreach (EnableCap cap in Enum.GetValues(typeof(EnableCap)))
            {
                try
                {
                    if (!states.ContainsKey(cap))
                    {
                        bool isEnabled = GL.IsEnabled(cap);
                        if (GL.GetError() == ErrorCode.NoError)
                            states.Add(cap, isEnabled);
                    }
                }
                catch (Exception) { }
            }

            // So for some bizzare reason, this reports as being enabled at first,
            // but then gets disabled without this knowing.
            states[EnableCap.Blend] = false;

            // Flush any errors, not all of the enums checked above will work but they
            // won't affect the rest of the program.
            GL.GetError();
        }

        public static void DepthFunc(DepthFunction func)
        {
            if (func != depthFunc)
            {
                depthFunc = func;
                GL.DepthFunc(func);
            }
        }

        public static void ToggleWireframe(bool state)
        {
            if (state) EnableWireframe();
            else DisableWireframe();
        }

        public static void EnableWireframe()
        {
            if (wireFrameEnabled)
                return;

            wireFrameEnabled = true;
            GL.PolygonMode(MaterialFace.FrontAndBack, UsePointWireframe ? PolygonMode.Point : PolygonMode.Line);
        }

        public static void DisableWireframe(bool force = false)
        {
            if ((!force && MasterRenderer.Instance.GlobalWireframe) || !wireFrameEnabled)
                return;

            wireFrameEnabled = false;
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public static bool IsEnabled(EnableCap cap)
        {
            return states[cap];
        }

        public static void Toggle(EnableCap cap, bool state)
        {
            if (state) Enable(cap);
            else Disable(cap);
        }

        public static void Enable(EnableCap cap)
        {
            if (!states[cap])
            {
                if (CheckForErrors)
                    GLError.Begin();

                states[cap] = true;
                GL.Enable(cap);

                if (CheckForErrors)
                {
                    ErrorCode glError = GLError.End();
                    if (glError == ErrorCode.InvalidEnum)
                        throw new Exception(string.Format("Invalid glEnable Enum: {0}", cap));
                    else if (glError == ErrorCode.InvalidValue)
                        throw new Exception(string.Format("Invalid glEnable Value: {0}", cap));
                }
            }
        }

        public static void Disable(EnableCap cap)
        {
            if (states[cap])
            {
                if (CheckForErrors)
                    GLError.Begin();

                states[cap] = false;
                GL.Disable(cap);

                if (CheckForErrors)
                {
                    ErrorCode glError = GLError.End();
                    if (glError == ErrorCode.InvalidEnum)
                        throw new Exception(string.Format("Invalid glEnable Enum: {0}", cap));
                    else if (glError == ErrorCode.InvalidValue)
                        throw new Exception(string.Format("Invalid glEnable Value: {0}", cap));
                }
            }
        }
    }
}
