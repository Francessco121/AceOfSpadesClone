using Dash.Engine.Graphics.OpenGL;
using System;

namespace Dash.Engine.Graphics
{
    /// <summary>
    /// Helper class for catching opengl errors.
    /// </summary>
    public static class GLError
    {
        /// <summary>
        /// Global way to disable glErrorChecking for performance reasons.
        /// </summary>
        public static bool DisableChecking;

        /// <summary>
        /// Calls glGetError to clear any previous error.
        /// </summary>
        public static void Begin()
        {
            if (!DisableChecking)
            {
                ErrorCode error = GL.GetError();
                if (error != ErrorCode.NoError)
                    throw new Exception(string.Format("Uncaught opengl error before GLError.Begin: {0}", error));
            }
        }

        /// <summary>
        /// Returns glGetError.
        /// Returns NoError if error checking is disabled.
        /// </summary>
        public static ErrorCode End()
        {
            return DisableChecking ? ErrorCode.NoError : GL.GetError();
        }
    }
}
