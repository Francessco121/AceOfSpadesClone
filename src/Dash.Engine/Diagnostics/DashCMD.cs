using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using System.Diagnostics;

/* DashCMD.cs
 * Author: Ethan Lafrenais
 * Last Update: 12/12/2015
*/

namespace Dash.Engine.Diagnostics
{
    /// <summary>
    /// A command method called when a command is used in DashCMD.
    /// </summary>
    /// <param name="args">Arguments that were passed with the command.</param>
    public delegate void DashCMDCommand(string[] args);

    /// <summary>
    /// A wrapper for the console that provides a managed way of use.
    /// </summary>
    public static class DashCMD
    {
        /// <summary>
        /// Current version of DashCMD.
        /// </summary>
        public static readonly string Version = "1.5";

        /// <summary>
        /// Is there a console handle active?
        /// </summary>
        public static bool ConsoleHandleExists { get; private set; }

        /// <summary>
        /// Safley sets the console title
        /// </summary>
        public static string Title
        {
            get { return ConsoleHandleExists ? Console.Title : "Console"; }
            set { if (ConsoleHandleExists) Console.Title = value; }
        }

        public static bool PrependTimestamp = true;
        public static CultureInfo TimeCulture = new CultureInfo("en-US");

        static bool SupressTimestamps;
        static bool TryStarted;

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        static void TryStart(bool allocConsole)
        {
            if (TryStarted) return;
            TryStarted = true;

            // Only attempt the kernel32 method if this is windows.
            if (allocConsole && Environment.OSVersion.Platform == PlatformID.Win32NT)
                AllocConsole();

            try
            {
                // Check if the console has a handle
                Console.Clear();
                // If we didn't crash their, the handle exists.
                ConsoleHandleExists = true;

                outCs = new ConsoleStream(new MemoryStream());

                sw = new StreamWriter(outCs);
                sw.AutoFlush = true;

                outCs.OnConsoleWrite += outCs_OnConsoleWrite;

                OnMainScreen = true;

                // Setup default commands
                AddCommand("help", "Displays basic help for all commands.",
                    delegate (string[] args)
                    {
                        Console.WriteLine("Defined Commands ({0}):\n", commands.Count);

                        foreach (KeyValuePair<string, Command> pair in commands)
                            if (!pair.Value.hideInHelp)
                                Console.WriteLine(String.Format("{0}{1}", pair.Value.command.PadRight(20), pair.Value.help));

                        WriteImportant("\nYou can also use the argument --syntax or --? for each command, to view that commands syntax.\n");
                    });

                AddCommand("screens", "Displays all registered screens.",
                    delegate (string[] args)
                    {
                        Console.WriteLine("Defined Screens ({0}):\n", screens.Count);

                        foreach (KeyValuePair<string, DashCMDScreen> pair in screens)
                            Console.WriteLine(String.Format("{0}{1}", pair.Value.Name.PadRight(20), pair.Value.Description));

                        Console.WriteLine();
                    });

                AddCommand("screen",
                    "Switches to a screen.", "screen [screen name] (when name not provided, it goes back to main screen)",
                    delegate (string[] args)
                    {
                        string screenName = CombineArgs(args);
                        if (screens.ContainsKey(screenName))
                        {
                            OnMainScreen = false;
                            SwitchScreen(screens[screenName]);
                        }
                        else
                            WriteError(string.Format("Screen '{0}' is not defined.", screenName));
                    });

                AddCommand("cls", "Clears the screen.",
                    delegate (string[] args)
                    {
                        Console.Clear();
                        logLines.Clear();
                        top = 0;
                        WriteInputLine();
                    });

                AddCommand("allvars", "Prints a list of all available CVars.", "allvars [page]",
                    delegate (string[] args)
                    {
                        // Grab page number
                        int page = 0;
                        if (args.Length > 0)
                            int.TryParse(args[0], out page);

                        // Calculate number of pages
                        int maxLines = (Console.BufferHeight - 2);
                        double _numpages = (double)CVars.Count / (double)maxLines;
                        int numpages = (int)Math.Ceiling(_numpages) - 1;

                        // Check page number
                        if (page > numpages)
                            page = numpages;
                        if (page < 0)
                            page = 0;

                        // Start log
                        WriteImportant("-- -- CVars (Page: {0} of {1}) -- --", page, numpages);

                        if (CVars.Count != 0)
                        {
                            // Grab enumerator
                            var e = CVars.OrderBy(x => x.Key).GetEnumerator();
                            e.MoveNext();

                            // Skip pages if necessary
                            for (int i = 0; i < maxLines * page; i++)
                                e.MoveNext();

                            // Log current page
                            for (int i = maxLines * page; i < maxLines * (page + 1); i++)
                            {
                                KeyValuePair<string, CVar> var = e.Current;
                                WriteLine("{0}= {1}", var.Key.PadRight(20), var.Value.value);

                                if (!e.MoveNext())
                                    break;
                            }
                        }
                        else
                            WriteLine("There are no CVars defined!");

                        WriteLine("");
                    });

                //AddCommand("cmd-history", "Sets the length of the history.",
                //    "cmd-history <length [current window height - 300]>",
                //    delegate (string[] args)
                //    {
                //        if (isLinux)
                //        {
                //            WriteError("Command not available in Linux terminals.");
                //            return;
                //        }

                //        if (!AllowCMDHistory)
                //        {
                //            WriteError("Command not allowed.");
                //            return;
                //        }

                //        if (args.Length < 1)
                //        {
                //            WriteError("Wrong number of arguments");
                //            return;
                //        }

                //        int length = Convert.ToInt32(args[0]);

                //        if (length <= Console.WindowHeight || length > 300)
                //        {
                //            WriteError("Invalid length: must be between the current window height and 300");
                //            return;
                //        }
                //        else
                //        {
                //            Console.WindowTop = 0;
                //            Console.BufferHeight = length;
                //            WriteInputLine();
                //        }
                //    });

                //AddCommand("cmd-exit", "Exits DashCMD.",
                //    delegate (string[] args)
                //    {
                //        if (!AllowCMDExit)
                //        {
                //            WriteError("Command not allowed.");
                //            return;
                //        }

                //        StopListening();
                //    });

                // Mark all commands are core.
                List<string> keys = new List<string>(commands.Keys);
                foreach (string key in keys)
                {
                    Command rcmd = commands[key];
                    if (isLinux && key == "cmd-history")
                        rcmd.hideInHelp = true;
                    rcmd.core = true;
                    commands[key] = rcmd;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Properties
        /// <summary>
        /// Is it listening for input?
        /// </summary>
        public static bool Listening { get; private set; }
        /// <summary>
        /// Is it started?
        /// </summary>
        public static bool Started { get; private set; }
        /// <summary>
        /// Is it drawing the main screen?
        /// </summary>
        public static bool OnMainScreen { get; private set; }
        /// <summary>
        /// The active screen, null if main.
        /// </summary>
        public static DashCMDScreen ActiveScreen { get; private set; }
        /// <summary>
        /// Is the cmd-history command allowed?
        /// </summary>
        public static bool AllowCMDHistory
        {
            get { return allowCMDHistory; }
            set
            {
                string cmd = "cmd-history";
                if (commands.ContainsKey(cmd))
                {
                    Command rcmd = commands[cmd];
                    rcmd.hideInHelp = !value;
                    commands[cmd] = rcmd;
                }

                allowCMDHistory = value;
            }
        }
        private static bool allowCMDHistory = true;

        /// <summary>
        /// Is the cmd-exit command allowed?
        /// </summary>
        public static bool AllowCMDExit
        {
            get { return allowCMDExit; }
            set
            {
                string cmd = "cmd-exit";
                if (commands.ContainsKey(cmd))
                {
                    Command rcmd = commands[cmd];
                    rcmd.hideInHelp = !value;
                    commands[cmd] = rcmd;
                }

                allowCMDExit = value;
            }
        }
        private static bool allowCMDExit = true;
        #endregion

        #region Classes
        private struct Command
        {
            public string command;
            public string help;
            public string syntax;
            public bool hideInHelp;
            public bool core;
            public DashCMDCommand callback;

            internal Command(string command, string help, string syntax, DashCMDCommand callback, bool hideInHelp)
            {
                this.command = command;
                this.help = help;
                this.hideInHelp = hideInHelp;
                this.callback = callback;
                this.syntax = syntax;
                this.core = false;
            }
        }

        public class CVar
        {
            public Type dtype;
            public object value;

            public CVar(Type dtype, object value)
            {
                this.dtype = dtype;
                this.value = value;
            }
        }

        private class ConsoleStream : Stream
        {
            public event ConsoleStreamWrite OnConsoleWrite;
            public event ConsoleStreamRead OnConsoleRead;
            public event ConsoleStreamSeek OnConsoleSeek;

            public delegate void ConsoleStreamWrite(byte[] buffer, int offset, int count);
            public delegate void ConsoleStreamRead(int value, int offset, int count);
            public delegate void ConsoleStreamSeek(long newPos);

            private Stream inner;

            public ConsoleStream(Stream inner)
            {
                this.inner = inner;
            }

            public override bool CanRead
            {
                get { return inner.CanRead; }
            }

            public override bool CanSeek
            {
                get { return inner.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return inner.CanWrite; }
            }

            public override void Flush()
            {
                inner.Flush();
            }

            public override long Length
            {
                get { return inner.Length; }
            }

            public override long Position
            {
                get { return inner.Position; }
                set { inner.Position = value; }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int value = inner.Read(buffer, offset, count);

                if (OnConsoleRead != null)
                    OnConsoleRead(value, offset, count);

                return value;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long value = inner.Seek(offset, origin);

                if (OnConsoleSeek != null)
                    OnConsoleSeek(value);

                return value;
            }

            public override void SetLength(long value)
            {
                inner.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                inner.Write(buffer, offset, count);
                Flush();

                if (OnConsoleWrite != null)
                    OnConsoleWrite(buffer, offset, count);
            }
        }
        #endregion

        #region Fields
        public static Dictionary<string, CVar> CVars = new Dictionary<string, CVar>();

        static Dictionary<string, Command> commands = new Dictionary<string, Command>();
        static Dictionary<string, DashCMDScreen> screens = new Dictionary<string, DashCMDScreen>();

        static StreamWriter sw;
        static ConsoleStream outCs;
        static Thread consoleThread;

        public static int MaxSavedCommands = 30;
        static List<string> lastCommands = new List<string>();
        static int saveCommandI = 0;

        static int top = 0;
        static List<CLine> logLines = new List<CLine>();
        static StringBuilder typingCommand = new StringBuilder();
        static int typingCommandI;
        static bool isLinux = Environment.OSVersion.Platform == PlatformID.Unix;

        static int promptAnchor;

        static int MaxLogLines
        {
            get { return Console.BufferHeight - 10; }
        }
        #endregion

        #region Initialization/Stopping
        /// <summary>
        /// Attaches the wrapper to the Console, redirecting it's input and output.
        /// </summary>
        public static void Start(bool createConsole = false)
        {
            if (!TryStarted)
                TryStart(createConsole);

            if (!ConsoleHandleExists) return;
            //throw new Exception(
            //    "Cannot start DashCMD, no console handle exists! (Use DashCMD.ConsoleHandleExists to check this.)");

            sw.NewLine = Environment.NewLine;
            Console.SetOut(sw);
            Console.Clear();
            WriteImportant("Started DashCMD v{0}", Version);
            WriteInputLine();
        }

        /// <summary>
        /// Stops the wrapper completely.
        /// </summary>
        public static void Stop()
        {
            if (!ConsoleHandleExists) return;

            StopListening();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));

            //keyboardEvent(0x0058, 0, 0x0001, 0); // Send the final key press to end immediatly.
            stdWriteLine("Press any key to exit...");
        }

        /// <summary>
        /// Starts listening for input, blocks the calling thread until stopped.
        /// </summary>
        public static void Listen(bool async)
        {
            if (Listening)
                return;

            if (async)
            {
                consoleThread = new Thread(new ThreadStart(Listen));
                consoleThread.Name = "DashCMD Thread";
                consoleThread.IsBackground = true;
                consoleThread.Start();

                Listening = true;
            }
            else
                Listen();
        }

        /// <summary>
        /// Starts listening for input, blocks the calling thread until stopped.
        /// </summary>
        static void Listen()
        {
            if (!ConsoleHandleExists) return;

            Listening = true;

            while (Listening)
            {
                ConsoleKeyInfo keyinfo = Console.ReadKey(true);
                if (!OnMainScreen)
                {
                    if (keyinfo.Key == ConsoleKey.Escape)
                        SwitchScreen(null);
                    continue;
                }
                if (keyinfo.Key == ConsoleKey.Enter)
                {
                    string cmd = typingCommand.ToString();
                    typingCommandI = 0;
                    if (!string.IsNullOrWhiteSpace(cmd))
                        ExecuteCommand(typingCommand.ToString());
                    else
                    {
                        SupressTimestamps = true;
                        WriteStandard("");
                        SupressTimestamps = false;
                        typingCommand.Clear();
                    }
                }
                else if (keyinfo.Key == ConsoleKey.Backspace && typingCommand.Length > 0 && typingCommandI > 0)
                {
                    typingCommand.Remove(typingCommandI - 1, 1);
                    typingCommandI--;
                    WriteInputLine();
                }
                else if (keyinfo.Key == ConsoleKey.Delete && typingCommand.Length > 0 && typingCommandI < typingCommand.Length)
                {
                    typingCommand.Remove(typingCommandI, 1);
                    WriteInputLine();
                }
                else if (keyinfo.Key == ConsoleKey.UpArrow) // Cycle Up Last Commands
                {
                    if (saveCommandI != -1 && (lastCommands.Count == 0 || typingCommand.ToString() != lastCommands[saveCommandI]))
                        saveCommandI = Math.Max(-1, saveCommandI - 1);

                    if (saveCommandI + 1 < lastCommands.Count)
                        saveCommandI++;

                    if (saveCommandI >= 0 && saveCommandI < lastCommands.Count)
                    {
                        //stdWrite(String.Format("{0}{1}", new string('\b', typingCommand.Length), lastCommands[saveCommandI]));
                        typingCommand.Clear();
                        typingCommand.Append(lastCommands[saveCommandI]);
                        typingCommandI = typingCommand.Length;
                        WriteInputLine();
                    }
                }
                else if (keyinfo.Key == ConsoleKey.DownArrow) // Cycle Down Last Commands
                {
                    if (saveCommandI - 1 >= 0)
                        saveCommandI--;

                    if (saveCommandI >= 0 && saveCommandI < lastCommands.Count)
                    {
                        //stdWrite(String.Format("{0}{1}", new string('\b', typingCommand.Length), lastCommands[saveCommandI]));
                        typingCommand.Clear();
                        typingCommand.Append(lastCommands[saveCommandI]);
                        typingCommandI = typingCommand.Length;
                        WriteInputLine();
                    }

                    if (saveCommandI == 0)
                    {
                        typingCommand.Clear();
                        typingCommandI = 0;
                        WriteInputLine();
                    }
                }
                else if (keyinfo.Key == ConsoleKey.LeftArrow)
                {
                    if (typingCommandI > 0)
                    {
                        typingCommandI--;
                        SetCursorPos();
                    }
                }
                else if (keyinfo.Key == ConsoleKey.RightArrow)
                {
                    if (typingCommandI < typingCommand.Length)
                    {
                        typingCommandI++;
                        SetCursorPos();
                    }
                }
                else if (keyinfo.Key == ConsoleKey.Tab)
                {
                    SupressTimestamps = true;

                    string currentCommand = typingCommand.ToString();
                    if (!String.IsNullOrWhiteSpace(currentCommand))
                    {
                        var cvarSuggestions = CVars.Keys.Where(s => s.StartsWith(currentCommand));
                        var cmdSuggestions = commands.Keys.Where(s => s.StartsWith(currentCommand));
                        var suggestions = cvarSuggestions.Concat(cmdSuggestions);

                        if (suggestions.Count() == 1)
                        {
                            string suggestion = suggestions.First();

                            stdWrite(String.Format("{0}{1}", new string('\b', typingCommand.Length), suggestion));
                            typingCommand.Clear();
                            typingCommand.Append(suggestion);
                            typingCommandI = typingCommand.Length;
                        }
                        else if (suggestions.Count() != 0)
                        {
                            IEnumerator<string> e = suggestions.GetEnumerator();
                            int count = suggestions.Count();
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < count;)
                            {
                                sb.Clear();

                                for (int k = 0; k < 5 && i < count; k++, i++)
                                {
                                    if (e.MoveNext())
                                    {
                                        sb.Append(e.Current);
                                        sb.Append("\t");
                                    }
                                }

                                WriteStandard(sb.ToString());
                            }

                            WriteStandard("");
                        }
                    }

                    SupressTimestamps = false;
                }
                else if (!char.IsControl(keyinfo.KeyChar))
                {
                    typingCommand.Insert(typingCommandI++, keyinfo.KeyChar);
                    if (typingCommandI == typingCommand.Length)
                        stdWrite(keyinfo.KeyChar.ToString());
                    else
                        // Only rewrite input line if necessary
                        WriteInputLine();
                }
            }
        }

        /// <summary>
        /// Stops listening for input.
        /// </summary>
        public static void StopListening()
        {
            if (!ConsoleHandleExists) return;
            Listening = false;
        }
        #endregion

        #region CVar Handling
        /// <summary>
        /// Attemps to retrieve a CVar.
        /// </summary>
        /// <typeparam name="T">The datatype of the CVar.</typeparam>
        /// <param name="name">The name of the CVar.</param>
        /// <returns>The CVar, as it's actual datatype.</returns>
        public static T GetCVar<T>(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CVar cvar;
            if (CVars.TryGetValue(name, out cvar))
                return (T)cvar.value;
            else
                throw new Exception(String.Format("CVar \"{0}\" does not exist!", name));
        }

        /// <summary>
        /// Trys to get a CVar.
        /// <para>Returns true if the CVar was found.</para>
        /// </summary>
        /// <typeparam name="T">The datatype of the CVar.</typeparam>
        /// <param name="name">The name of the CVar.</param>
        /// <param name="value">The value of the CVar.</param>
        /// <returns>Returns whether or not the CVar was found.</returns>
        public static bool TryGetCVar<T>(string name, out T value)
        {
            value = default(T);
            CVar cvar;

            bool success = CVars.TryGetValue(name, out cvar);
            value = (T)cvar.value;
            return success;
        }

        /// <summary>
        /// Sets or Adds a CVar.
        /// </summary>
        /// <typeparam name="T">The datatype of the cvar.</typeparam>
        /// <param name="name">The cvar's name.</param>
        /// <param name="value">The cvar's value.</param>
        public static void SetCVar<T>(string name, T value)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");

            if (CVars.ContainsKey(name))
                CVars[name] = new CVar(CVars[name].dtype, value);
            else
                CVars.Add(name, new CVar(typeof(T), value));
        }

        /// <summary>
        /// Sets a CVar (infers datatype, slower than SetCVar<T>).
        /// </summary>
        /// <param name="name">The cvar's name.</param>
        /// <param name="value">The cvar's value.</param>
        static void SetCVar(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");

            if (CVars.ContainsKey(name))
                CVars[name] = new CVar(CVars[name].dtype, Convert.ChangeType(value, CVars[name].dtype));
            else
                throw new Exception(
                    String.Format("CVar \"{0}\" doesnt exist, and cannot be added! (To add, use AddCVar Or SetCVar<T>)", name));
        }

        /// <summary>
        /// Gets whether or not the specified CVar exists.
        /// </summary>
        /// <param name="name">The name of the CVar.</param>
        public static bool IsCVarDefined(string name)
        {
            return CVars.ContainsKey(name);
        }

        /// <summary>
        /// Adds a CVar.
        /// </summary>
        /// <typeparam name="T">The datatype of the cvar.</typeparam>
        /// <param name="name">The cvar's name.</param>
        /// <param name="value">The cvar's value.</param>
        //public static void AddCVar<T>(string name, T value)
        //{
        //    if (name == null)
        //        throw new ArgumentNullException("name");
        //    if (value == null)
        //        throw new ArgumentNullException("value");

        //    if (CVars.ContainsKey(name))
        //        throw new Exception(String.Format("CVar \"{0}\" already exists!", name));
        //    else
        //        CVars.Add(name, new CVar(typeof(T), value));
        //}

        /// <summary>
        /// Removes a CVar.
        /// </summary>
        /// <param name="name">The name of the CVar</param>
        public static void RemoveCVar(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (CVars.ContainsKey(name))
                CVars.Remove(name);
            else
                throw new Exception(String.Format("Failed to remove CVar \"{0}\", it does not exist!", name));
        }
        #endregion

        #region Writing
        /// <summary>
        /// Writes a line of text.
        /// </summary>
        /// <param name="msg">Text to write.</param>
        /// <param name="color">Color of the text.</param>
        public static void WriteLine(string msg, ConsoleColor color, params object[] args)
        {
            if (!ConsoleHandleExists) return;

            if (PrependTimestamp && !SupressTimestamps)
                msg = string.Format("[{0}] {1}", DateTime.Now.ToString(TimeCulture), msg);

            Console.ForegroundColor = color;
            if (args.Length > 0)
                Console.WriteLine(String.Format(msg, args));
            else
                Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Writes text.
        /// </summary>
        /// <param name="msg">Text to write.</param>
        /// <param name="color">Color of the text.</param>
        public static void Write(string msg, ConsoleColor color, params object[] args)
        {
            if (!ConsoleHandleExists) return;
            Console.ForegroundColor = color;
            Console.Write(String.Format(msg, args));
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Writes a line of text.
        /// </summary>
        public static void WriteLine(object obj)
        {
            if (!ConsoleHandleExists) return;

            string msg;
            if (PrependTimestamp && !SupressTimestamps)
                msg = string.Format("[{0}] {1}", DateTime.Now.ToString(TimeCulture), obj.ToString());
            else
                msg = obj.ToString();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Writes a line of text.
        /// </summary>
        /// <param name="msg">Text to write.</param>
        public static void WriteLine(string msg, params object[] args)
        {
            if (!ConsoleHandleExists) return;

            if (PrependTimestamp && !SupressTimestamps)
                msg = string.Format("[{0}] {1}", DateTime.Now.ToString(TimeCulture), msg);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(String.Format(msg, args));
        }

        /// <summary>
        /// Writes text.
        /// </summary>
        /// <param name="msg">Text to write.</param>
        public static void Write(string msg, params object[] args)
        {
            if (!ConsoleHandleExists) return;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(String.Format(msg, args));
        }

        /// <summary>
        /// Writes a standard white message.
        /// </summary>
        /// <param name="msg">Text to write.</param>
        public static void WriteStandard(string msg, params object[] args)
        {
            WriteLine(String.Format(msg, args), ConsoleColor.White);
        }

        /// <summary>
        /// Writes an important message.
        /// </summary>
        /// <param name="msg">Text to write.</param>
        public static void WriteImportant(string msg, params object[] args)
        {
            WriteLine(String.Format(msg, args), ConsoleColor.Cyan);
        }

        /// <summary>
        /// Writes a warning message.
        /// </summary>
        /// <param name="msg">Text to write.</param>
        public static void WriteWarning(string msg, params object[] args)
        {
            WriteLine(String.Format(msg, args), ConsoleColor.Yellow);
        }

        /// <summary>
        /// Writes an error message.
        /// </summary>
        /// <param name="msg">Text to write.</param>
        public static void WriteError(string msg, params object[] args)
        {
            WriteLine(String.Format(msg, args), ConsoleColor.Red);
        }

        /// <summary>
        /// Writes an exception error message.
        /// </summary>
        /// <param name="e">Exception to write.</param>
        public static void WriteError(Exception e)
        {
            WriteLine(e.ToString(), ConsoleColor.Red);
        }
        #endregion

        #region Command Handling
        /// <summary>
        /// Adds a command with this cmd.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="callback">Method to call when the command is used.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public static void AddCommand(string command, DashCMDCommand callback)
        {
            if (commands.ContainsKey(command))
                throw new ArgumentException("Command already registered!");

            commands.Add(command, new Command(command,
                "",
                command,
                callback,
                true));
        }

        /// <summary>
        /// Adds a command with this cmd.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="help">The help message to show when the help command is used.</param>
        /// <param name="callback">Method to call when the command is used.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public static void AddCommand(string command, string help, DashCMDCommand callback)
        {
            if (commands.ContainsKey(command))
                throw new ArgumentException("Command already registered!");

            commands.Add(command, new Command(command,
                !String.IsNullOrWhiteSpace(help) ? help : "",
                command,
                callback,
                String.IsNullOrWhiteSpace(help)));
        }

        /// <summary>
        /// Adds a command with this cmd.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="help">The help message to show when the help command is used.</param>
        /// <param name="syntax">The syntax message to show when the --syntax argument 
        /// is used with this command.</param>
        /// <param name="callback">Method to call when the command is used.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public static void AddCommand(string command, string help, string syntax, DashCMDCommand callback)
        {
            if (commands.ContainsKey(command))
                throw new ArgumentException(String.Format("Command already registered: {0}", command));

            commands.Add(command, new Command(command,
                !String.IsNullOrWhiteSpace(help) ? help : "",
                !String.IsNullOrWhiteSpace(syntax) ? syntax : command,
                callback,
                String.IsNullOrWhiteSpace(help)));
        }

        /// <summary>
        /// Removes a command.
        /// </summary>
        /// <param name="command">The command to unregister.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public static void RemoveCommand(string command)
        {
            if (commands.ContainsKey(command))
                if (!commands[command].core)
                    commands.Remove(command);
                else
                    throw new ArgumentException("Cannot unregister a core command!");
            else
                throw new ArgumentException(String.Format("Command does not exist: {0}", command));

        }

        /// <summary>
        /// Gets whether or not the specified command is defined.
        /// </summary>
        /// <param name="command">The name of the command</param>
        public static bool IsCommandDefined(string command)
        {
            return commands.ContainsKey(command);
        }

        /// <summary>
        /// Combines the arguments in the list to one string seperate by spaces.
        /// </summary>
        /// <param name="args">The Arguments to combine.</param>
        /// <returns>The combined string.</returns>
        public static string CombineArgs(string[] args)
        {
            return CombineArgs(args, ' ', 0, args.Length);
        }

        /// <summary>
        /// Combines the arguments in the list to one string.
        /// </summary>
        /// <param name="args">The Arguments to combine.</param>
        /// <param name="seperateChar">Character to seperate them with.</param>
        /// <param name="start">Starting position to start combing.</param>
        /// <param name="count">How many to combine.</param>
        /// <returns>The combined string.</returns>
        public static string CombineArgs(string[] args, char seperateChar, int start, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < count; i++)
            {
                sb.Append(args[i]);
                if (i + 1 < args.Length)
                    sb.Append(seperateChar);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="command">The command (with parameters) to execute.</param>
        public static void ExecuteCommand(string command)
        {
            saveCommandI = -1;

            if (lastCommands.Count == 0 || lastCommands[0] != command)
            {
                lastCommands.Insert(0, command);

                if (lastCommands.Count > MaxSavedCommands)
                    lastCommands.RemoveAt(lastCommands.Count - 1);
            }

            List<string> cmds = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool start = false;
            for (int i = 0; i < command.Length; i++)
            {
                string c = command.Substring(i, 1);
                if (!String.IsNullOrWhiteSpace(c))
                    start = true;

                if (c == ";")
                {
                    cmds.Add(sb.ToString());
                    sb.Clear();
                    start = false;
                }
                else if (start)
                    sb.Append(c);
            }

            cmds.Add(sb.ToString());

            foreach (string cmd in cmds)
                InternalExecuteCommand(cmd);
        }

        static void InternalExecuteCommand(string command)
        {
            // Supress timestamps when in command execution context
            SupressTimestamps = true;

            typingCommand.Clear();
            Command rcmd;

            // Get the actual command with its arguments
            string cmd;
            List<string> args = ParseCommand(command, out cmd);

            // Find the command.
            if (commands.TryGetValue(cmd, out rcmd))
            {
                if (args.Count >= 1 && (args[0].ToLower() == "--syntax" || args[0].ToLower() == "--?" || args[0].ToLower() == "/?"))
                    ShowSyntax(rcmd);
                else
                    try
                    {
                        rcmd.callback(args.ToArray());
                        WriteInputLine();
                    }
                    catch (Exception e) { WriteError(e.ToString()); }
            }
            else
            {
                if (args.Count > 0)
                    if (TryModVar(cmd, args[0]))
                        WriteStandard("{0} -> {1}", cmd, args[0]);
                    else
                        WriteError("Failed to change {0} to {1}. (Wrong datatype?)", cmd, args[0]);
                else if (args.Count == 0 && CVars.ContainsKey(cmd))
                    WriteStandard("{0} = {1}", cmd, CVars[cmd].value);
                else
                    WriteError("Command not defined: {0}", cmd);
            }

            SupressTimestamps = false;
        }

        static bool TryModVar(string cvar, object val)
        {
            if (CVars.ContainsKey(cvar))
            {
                try
                {
                    SetCVar(cvar, val);
                    return true;
                }
                catch (Exception)
                {
                    //LogError(e);
                    return false;
                }
            }
            else
                return false;
        }

        static List<string> ParseCommand(string cmd, out string trimmedCmd)
        {
            List<string> args = new List<string>();
            int fs = cmd.IndexOf(" ");

            if (fs == -1)
                trimmedCmd = cmd; // No args exist, so just set the trimmed cmd.
            else
            {
                // Start searching for all of the arguments.
                trimmedCmd = cmd.Substring(0, fs);
                string pArgs = cmd.Substring(fs + 1);
                int nextSpace = -1;

                while ((nextSpace = pArgs.IndexOf(" ")) != -1)
                {
                    string arg = pArgs.Substring(0, nextSpace);

                    // Strip out "'s if used at the beginning and end
                    //if (arg.Substring(0, 1) == "\"" && arg.Substring(arg.Length - 1, 1) == "\"")
                    //    arg = arg.Remove(0, 1).Remove(arg.Length - 2, 1);

                    args.Add(arg);
                    pArgs = pArgs.Substring(nextSpace + 1);
                }

                // Strip out the final arg's "'s if used at the beginning and end
                // if (pArgs.Substring(0, 1) == "\"" && pArgs.Substring(pArgs.Length - 1, 1) == "\"")
                //    pArgs = pArgs.Remove(0, 1).Remove(pArgs.Length - 2, 1);

                args.Add(pArgs);
            }

            return args;
        }

        public static void ShowSyntax(string commandName)
        {
            Command cmd;
            if (commands.TryGetValue(commandName, out cmd))
                ShowSyntax(cmd);
            else
                WriteError("Failed to display syntax. Command '{0}' is not defined!", commandName);
        }

        static void ShowSyntax(Command rcmd)
        {
            WriteStandard(String.Format("Syntax: {0}", rcmd.syntax));
        }
        #endregion

        #region Screen Handling
        /// <summary>
        /// Adds a screen with this CMD.
        /// </summary>
        /// <param name="screen">The screen to register.</param>
        public static void AddScreen(DashCMDScreen screen)
        {
            if (screens.ContainsValue(screen))
                throw new ArgumentException(String.Format("Screen already registered: {0}", screen.Name));
            else
                screens.Add(screen.Name, screen);

        }

        /// <summary>
        /// Removes a screen with this CMD.
        /// </summary>
        /// <param name="screen">The screen to register.</param>
        public static void RemoveScreen(DashCMDScreen screen)
        {
            if (screens.ContainsValue(screen))
                throw new ArgumentException(String.Format("Screen not registered: {0}", screen.Name));
            else
                screens.Remove(screen.Name);
        }

        static void SwitchScreen(DashCMDScreen screen)
        {
            if (!ConsoleHandleExists) return;
            if (ActiveScreen != null)
                ActiveScreen.Stop();

            if (screen != null)
            {
                OnMainScreen = false;
                Console.Clear();
                ActiveScreen = screen;
                Console.CursorVisible = false;
                screen.Start();
            }
            else
            {
                Console.CursorVisible = true;
                OnMainScreen = true;
                ActiveScreen = null;
                Console.ResetColor();

                // Try-catch is for when coming out of a screen back to
                // a lot of messages. It takes CMD so long to actually write
                // them that the lines list changes causing a collection
                // modified exception.
                try { WriteLogScreen(); }
                catch (Exception) { }
            }
        }
        #endregion

        #region Core Console Handling
        static void outCs_OnConsoleWrite(byte[] buffer, int offset, int count)
        {
            if (isLinux && top == 0)
                top = 1;

            string strnl = Console.OutputEncoding.GetString(buffer, offset, count);
            string str = Regex.Replace(strnl, "\n|\r", "");

            logLines.Add(new CLine(strnl, Console.ForegroundColor, Console.BackgroundColor));
            if (logLines.Count > MaxLogLines)
                logLines.RemoveAt(0);

            if (OnMainScreen)
            {
                int newlines = 0;

                for (int i = offset; i < count; i++)
                    if (i < count - (Environment.NewLine.Length - 1))
                        if (Console.OutputEncoding.GetString(buffer, i, Environment.NewLine.Length) == Environment.NewLine)
                        {
                            newlines++;
                            i += (Environment.NewLine.Length - 1);
                        }
                        else if (Console.OutputEncoding.GetString(buffer, i, 1) == "\n")
                            newlines++;

                int t = SafePosSet(0, top++);

                if (isLinux && t == top - 1)
                    ClearLine(' ', false);
                else if (!isLinux && t == top - 1)
                    ClearLine(' ', false);
                Console.OpenStandardOutput().Write(buffer, offset, count);

                newlines += (int)Math.Floor(str.Length / (double)Console.BufferWidth);
                top += newlines - 1;
                WriteInputLine();
            }
            else
                top++;
        }

        static void WriteLogScreen()
        {
            if (!ConsoleHandleExists) return;
            Console.Clear();
            foreach (CLine line in logLines)
            {
                Console.ForegroundColor = line.fcolor;
                Console.BackgroundColor = line.bcolor;
                stdWrite(line.text);
            }

            if (!isLinux)
            {
                top = Console.CursorTop;
                WriteInputLine();
            }
        }

        internal static void ClearLine(char clearChar = ' ', bool isLog = true)
        {
            if (!ConsoleHandleExists) return;
            if (isLog)
                logLines.RemoveAt(Console.CursorTop);

            Console.SetCursorPosition(0, Console.CursorTop);
            stdWrite(new string(clearChar, Console.BufferWidth));

            if (Console.CursorTop > 0 && !isLinux)
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            else if (Console.CursorTop > 0 && isLinux)
                Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        internal static void stdWrite(string text)
        {
            if (!ConsoleHandleExists) return;
            byte[] bytes = Console.OutputEncoding.GetBytes(text);
            Console.OpenStandardOutput().Write(bytes, 0, bytes.Length);
        }

        internal static void stdWriteLine(string text)
        {
            if (!ConsoleHandleExists) return;
            byte[] bytes = Console.OutputEncoding.GetBytes(String.Format("{0}{1}", text, Environment.NewLine));
            Console.OpenStandardOutput().Write(bytes, 0, bytes.Length);
        }

        internal static int SafePosSet(int x, int y)
        {
            if ((!isLinux && y >= (Console.BufferHeight - 2)) || (isLinux && y >= (Console.BufferHeight - 1)))
            {
                if (!isLinux)
                    y = (Console.BufferHeight - 2);
                else
                    y = (Console.BufferHeight - 1);
            }

            Console.SetCursorPosition(x, y);

            return y;
        }

        internal static void SafePosBottomSet(int x, int y)
        {
            if (!ConsoleHandleExists) return;
            if (y >= Console.BufferHeight)
            {
                y = (Console.BufferHeight - 1);
            }

            Console.SetCursorPosition(x, y);
        }

        static void WriteInputLine()
        {
            if (!ConsoleHandleExists) return;
            Console.ForegroundColor = ConsoleColor.White;
            int t = SafePosSet(0, top);
            bool shifted = false;

            if (t != top)
            {
                SafePosBottomSet(0, top + 1);
                top = t;
                shifted = true;
            }
            else
                SafePosBottomSet(0, top);

            if (!isLinux)
                ClearLine(' ', false);
            else if (isLinux && shifted)
            {
                ClearLine(' ', false);
                SafePosBottomSet(0, top - 1);
            }
            else
            {
                ClearLine(' ', false);
                SafePosBottomSet(0, top);
            }

            if (isLinux && shifted)
                stdWriteLine("\n");

            promptAnchor = Console.CursorTop;
            stdWrite(String.Format("#> {0}", typingCommand));
            SetCursorPos();
        }

        static void SetCursorPos()
        {
            int promptLength = 3 + typingCommand.Length;
            int bufferWidth = Console.BufferWidth;

            int x = (typingCommandI + 3) % bufferWidth;
            int y = (typingCommandI + 3) / bufferWidth;

            SafePosSet(x, promptAnchor + y);
        }
        #endregion
    }
}
