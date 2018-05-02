using System;
using System.Diagnostics;
using System.IO;

/* ProgramExceptionHandler.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine.Diagnostics
{
    public delegate void DashExceptionEventHandler(DashExceptionHandlerEventArgs args);
    public class DashExceptionHandlerEventArgs : EventArgs
    {
        public readonly Exception Exception;

        public DashExceptionHandlerEventArgs(Exception e)
        {
            this.Exception = e;
        }
    }

    /// <summary>
    /// Provides simple exception handling for any application.
    /// </summary>
    public static class ProgramExceptionHandler
    {
        public static event DashExceptionEventHandler OnException;
        public static bool ErrorHandlerExists { get; private set; }

        public static void FireOnException(Exception e, params Tuple<object, object>[] data)
        {
            foreach (Tuple<object, object> tuple in data)
                e.Data.Add(tuple.Item1, tuple.Item2);

            if (OnException != null)
                OnException(new DashExceptionHandlerEventArgs(e));
        }

        /// <summary>
        /// Attaches the ProgramExceptionHandler.OnException event to the error-handler,
        /// and properly calls the error-handler from exceptions based on debugger status.
        /// </summary>
        public static void RunMainWithHandler(Action tryAction, Action finallyAction, Action shutdownAction)
        {
            ErrorHandlerExists = File.Exists("./ErrorHandler.exe");

            if (!ErrorHandlerExists)
                DashCMD.WriteWarning("[WARNING] ErrorHandler.exe does not exist!");

            OnException += (args) => HandleException(args.Exception, shutdownAction);

            if (Debugger.IsAttached || !ErrorHandlerExists)
            {
                // Exclude the catch if the debugger is attached, so visual studio can do its thing.
                try { tryAction(); }
                finally { finallyAction(); }
            }
            else
            {
                try { tryAction(); }
                catch (Exception e) { HandleException(e, shutdownAction); }
                finally { finallyAction(); }
            }
        }

        static void HandleException(Exception ex, Action shutdownAction)
        {
            if (!Debugger.IsAttached || ex.Data.Contains("glcontrol"))
            {
                // Get the command line arguments from the exception
                string cmdArgs = new ExceptionInfo(ex).ToCommandArgString();

                // Launch ErrorHandler.exe
                Process errorHandler = new Process();
                errorHandler.StartInfo.FileName = "ErrorHandler.exe";
                errorHandler.StartInfo.Arguments = cmdArgs;
                errorHandler.Start();

                // Shutdown application
                shutdownAction();
            }
        }
    }
}
