using System;

/* NetException.cs
 * Author: Ethan Lafrenais
 * Last Update: 4/28/15
*/

namespace Dash.Net
{
    /// <summary>
    /// Represents an exception from the DashNet library.
    /// </summary>
    [Serializable]
    public class NetException : Exception
    {
        internal static readonly NetException PastBuffer =
            new NetException("Attempt to read past buffer. Byte array smaller than attempted read/write positions.");

        internal static readonly NetException Unknown =
            new NetException("DashNet encountered an unknown error.");

        internal static readonly NetException WriteOnly =
            new NetException("This NetBuffer is Write Only.");

        internal static readonly NetException ReadOnly =
            new NetException("This NetBuffer is Read Only.");

        /// <summary>
        /// Creates a new empty-messaged NetException.
        /// </summary>
        public NetException() : base() { }
        /// <summary>
        /// Creates a new NetException.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public NetException(string message) : base(message) { }
        public NetException(string message, params object[] args) : base(string.Format(message, args)) { }
        /// <summary>
        /// Creates a new NetException from an inner exception.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="inner">The inner exception.</param>
        public NetException(string message, Exception inner) : base(message, inner) { }


        /// <summary>
        /// Shows the exception if showMessage is true.
        /// </summary>
        /// <param name="showMessage">Conditional to show it or not.</param>
        /// <param name="message">The exception message.</param>
        internal static void Assert(bool showMessage, string message)
        {
            if (showMessage)
                throw new NetException(message);
        }

        /// <summary>
        /// Shows the exception if showMessage is true.
        /// </summary>
        /// <param name="showMessage">Conditional to show it or not.</param>
        /// <param name="toThrow">The exception to throw.</param>
        internal static void Assert(bool showMessage, NetException toThrow)
        {
            if (showMessage)
                throw toThrow;
        }
    }
}
