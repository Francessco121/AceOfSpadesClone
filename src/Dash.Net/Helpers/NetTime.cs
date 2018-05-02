using System;

/* NetTime.cs
 * Author: Ethan Lafrenais
 * Last Update: 2/24/15
*/

namespace Dash.Net
{
    /// <summary>
    /// Helper class for time.
    /// </summary>
    public static class NetTime
    {
        /// <summary>
        /// Gets the current time in milliseconds of the application.
        /// </summary>
        public static int Now { get { return Environment.TickCount; } }
    }
}
