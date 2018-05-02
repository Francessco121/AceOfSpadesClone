using System;
using System.Diagnostics;
using System.Globalization;

/* NetLogger.cs
 * Author: Ethan Lafrenais
 * Last Update: 11/26/14
*/

namespace Dash.Net
{
    #region Enums
    /// <summary>
    /// A logging method used by the NetLogger.
    /// </summary>
    [Flags]
    public enum NetLoggerMethod
    {
        /// <summary>
        /// Tells the NetLogger to log to the Console.
        /// </summary>
        Console = 1,
        /// <summary>
        /// Tells the NetLogger to log to the output window in the current IDE.
        /// </summary>
        IDEOutput = 2,
        /// <summary>
        /// Tells the NetLogger to fire an event with the logged message.
        /// </summary>
        Event = 4
    }

    /// <summary>
    /// The type of log from the NetLogger.
    /// </summary>
    public enum NetLogType : byte
    {
        /// <summary>
        /// A debug log.
        /// </summary>
        Debug = 0,
        /// <summary>
        /// A verbose debug log.
        /// </summary>
        Verbose = 1,
        /// <summary>
        /// An important log.
        /// </summary>
        Important = 2,
        /// <summary>
        /// A warning log.
        /// </summary>
        Warning = 3,
        /// <summary>
        /// An error log.
        /// </summary>
        Error = 4
    }
    #endregion

    #region NetLog
    /// <summary>
    /// Handles a new NetLog.
    /// </summary>
    /// <param name="log">The NetLog that was just logged.</param>
    public delegate void NetLoggedHandler(NetLog log);

    /// <summary>
    /// A logged message
    /// </summary>
    public struct NetLog
    {
        /// <summary>
        /// The message of the log.
        /// </summary>
        public string Message;
        /// <summary>
        /// The type of log.
        /// </summary>
        public NetLogType Type;

        /// <summary>
        /// Creates a new NetLog.
        /// </summary>
        /// <param name="message">The log's message.</param>
        /// <param name="type">The log's type.</param>
        public NetLog(string message, NetLogType type)
        {
            this.Message = message;
            this.Type = type;
        }
    }
    #endregion

    /// <summary>
    /// Provides managed ways of writing debug messages.
    /// <para>Configurable.</para>
    /// </summary>
    public static class NetLogger
    {
        /// <summary>
        /// Log Debug messages or not.
        /// </summary>
        public static bool LogDebugs = true;
        /// <summary>
        /// Log Verbose messages or not.
        /// </summary>
        public static bool LogVerboses = false;
        /// <summary>
        /// Log Important messages or not.
        /// </summary>
        public static bool LogImportants = true;
        /// <summary>
        /// Log Warning messages or not.
        /// </summary>
        public static bool LogWarnings = true;
        /// <summary>
        /// Log Error messages or not.
        /// </summary>
        public static bool LogErrors = true;

        /// <summary>
        /// Logs every received packet as a verbose message.
        /// </summary>
        public static bool LogPacketReceives = false;
        /// <summary>
        /// Logs any reliable packets that are re-sent.
        /// </summary>
        public static bool LogReliableResends = false;
        public static bool LogAcks = false;
        public static bool LogPacketSends = false;
        public static bool MinimalPacketHeaderLogs = true;
        public static bool LogFlowControl = false;
        public static bool IgnoreSendRateChanges = false;
        public static bool LogPartials = false;
        public static bool SupressConnectionErrors = true;
        public static bool LogObjectStateChanges = false;
        public static bool LogAlreadyHandledAcks = false;
        public static bool LogAlreadyHandledPackets = false;

        static bool ChangeVerboseColor;

        /// <summary>
        /// Fired when a message is logged, 
        /// and the LogMethod contains the flag Event
        /// </summary>
        public static event NetLoggedHandler MessageLogged;

        /// <summary>
        /// The methods to use for message logging.
        /// <para>Can use multiple methods, combine with the bitwise operator.</para>
        /// <para>Ex. LogMethod = NetLoggerMethod.Console | NetLoggerMethod.Event</para>
        /// </summary>
        public static NetLoggerMethod LogMethod = NetLoggerMethod.Console;

        private static object lk = new object();
        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="type">The type of message to write.</param>
        public static void Write(string message, NetLogType type)
        {
            lock (lk)
            {
                // Console Log
                if (LogMethod.HasFlag(NetLoggerMethod.Console))
                {
                    ConsoleColor lastColor = Console.ForegroundColor;
                    Console.ForegroundColor = LogTypeToConsoleColor(type);
                    Console.WriteLine(message);
                    Console.ForegroundColor = lastColor;
                }

                // IDE Output Log
                if (LogMethod.HasFlag(NetLoggerMethod.IDEOutput))
                    Debug.WriteLine(message);

                // Event Log
                if (LogMethod.HasFlag(NetLoggerMethod.Event) && MessageLogged != null)
                    MessageLogged(new NetLog(message, type));
            }
        }

        static ConsoleColor LogTypeToConsoleColor(NetLogType type)
        {
            if (type == NetLogType.Debug)
                return ConsoleColor.White;
            else if (type == NetLogType.Verbose)
            {
                ChangeVerboseColor = !ChangeVerboseColor;
                return ChangeVerboseColor ? ConsoleColor.Gray : ConsoleColor.DarkGray;
            }
            else if (type == NetLogType.Important)
                return ConsoleColor.Green;
            else if (type == NetLogType.Warning)
                return ConsoleColor.Yellow;
            else if (type == NetLogType.Error)
                return ConsoleColor.Red;
            else
                return ConsoleColor.White;
        }

        // Log

        /// <summary>
        /// Logs a debug message to the console.
        /// </summary>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void Log(string message, params object[] args)
        {
            if (LogDebugs)
                Write(String.Format(message, args), NetLogType.Debug);
        }

        /// <summary>
        /// Logs a debug message to the console.
        /// </summary>
        /// <param name="condition">Actually write the message or not.</param>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void Log(bool condition, string message, params object[] args)
        {
            if (condition && LogDebugs)
                Write(String.Format(message, args), NetLogType.Debug);
        }

        // LogVerbose

        /// <summary>
        /// Logs a verbose message to the console.
        /// </summary>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogVerbose(string message, params object[] args)
        {
            if (LogVerboses)
                Write(String.Format(message, args), NetLogType.Verbose);
        }

        /// <summary>
        /// Logs a verbose message to the console.
        /// </summary>
        /// <param name="condition">Actually write the message or not.</param>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogVerbose(bool condition, string message, params object[] args)
        {
            if (condition && LogVerboses)
                Write(String.Format(message, args), NetLogType.Verbose);
        }

        // LogImportant

        /// <summary>
        /// Logs an important message to the console.
        /// </summary>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogImportant(string message, params object[] args)
        {
            if (LogImportants)
                Write(String.Format(message, args), NetLogType.Important);
        }

        /// <summary>
        /// Logs an important message to the console.
        /// </summary>
        /// <param name="condition">Actually write the message or not.</param>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogImportant(bool condition, string message, params object[] args)
        {
            if (condition && LogImportants)
                Write(String.Format(message, args), NetLogType.Important);
        }

        // LogWarning

        /// <summary>
        /// Logs a warning to the console.
        /// </summary>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogWarning(string message, params object[] args)
        {
            if (LogWarnings)
                Write(String.Format(message, args), NetLogType.Warning);
        }

        /// <summary>
        /// Logs a warning to the console.
        /// </summary>
        /// <param name="condition">Actually write the message or not.</param>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogWarning(bool condition, string message, params object[] args)
        {
            if (condition && LogWarnings)
                Write(String.Format(message, args), NetLogType.Warning);
        }

        // LogError

        /// <summary>
        /// Logs an error to the console.
        /// </summary>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogError(string message, params object[] args)
        {
            if (LogErrors)
                Write(String.Format(message, args), NetLogType.Error);
        }

        /// <summary>
        /// Logs an error to the console.
        /// </summary>
        /// <param name="condition">Actually write the message or not.</param>
        /// <param name="message">Formatted string.</param>
        /// <param name="args">Format arguments.</param>
        public static void LogError(bool condition, string message, params object[] args)
        {
            if (condition && LogErrors)
                Write(String.Format(message, args), NetLogType.Error);
        }

        /// <summary>
        /// Logs an exception to the console.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        public static void LogError(Exception exception)
        {
            if (LogErrors)
                Write(exception.ToString(), NetLogType.Error);
        }
    }
}
