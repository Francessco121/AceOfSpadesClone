using System;
using System.Collections.Generic;
using System.Threading;

/* DashCMDScreen.cs
 * Author: Ethan Lafrenais
 * Last Update: 7/21/2015
*/

namespace Dash.Engine.Diagnostics
{
    /// <summary>
    /// Used for the draw calls of a DashCMDScreen
    /// </summary>
    /// <param name="screen">The screen of the section drawing.</param>
    public delegate void DashCMDScreenDraw(DashCMDScreen screen);

    internal struct CLine
    {
        public string text;
        public ConsoleColor fcolor;
        public ConsoleColor bcolor;

        public CLine(string text, ConsoleColor fcolor, ConsoleColor bcolor)
        {
            this.text = text;
            this.fcolor = fcolor;
            this.bcolor = bcolor;
        }
    }

    /// <summary>
    /// A screen for a DashCMD prompt.
    /// </summary>
    public class DashCMDScreen
    {
        /// <summary>
        /// Is this screen active?
        /// </summary>
        public bool IsActive { get; internal set; }
        /// <summary>
        /// Does this screen auto-draw when active?
        /// </summary>
        public bool AutoUpdate;
        /// <summary>
        /// Time in-between auto-draw calls.
        /// </summary>
        public int SleepTime = 1000;
        /// <summary>
        /// The name of this screen.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The description of this screen.
        /// </summary>
        public string Description;
        /// <summary>
        /// Default back color to write with.
        /// </summary>
        public ConsoleColor BackgroundColor;
        /// <summary>
        /// Default text color to write with.
        /// </summary>
        public ConsoleColor ForegroundColor;
        /// <summary>
        /// Parent DashCMD screen.
        /// </summary>
        public DashCMDScreen Screen;

        DashCMDScreenDraw drawCallback;
        Thread thread;

        /// <summary>
        /// Creates a new Screen.
        /// </summary>
        /// <param name="Name">The name of this screen.</param>
        /// <param name="Description">The description of this screen.</param>
        public DashCMDScreen(string Name, string Description, bool autoUpdate, DashCMDScreenDraw callback,
            ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            this.Name = Name;
            this.AutoUpdate = autoUpdate;
            this.Description = Description;
            this.drawCallback = callback;
            this.ForegroundColor = foreground;
            this.BackgroundColor = background;
        }

        #region Logging
        /// <summary>
        /// Writes text to the section.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        /// <param name="args">Arguments for the formatted string.</param>
        public void Write(string msg, params object[] args)
        {
            if (!DashCMD.ConsoleHandleExists) return;
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
            DashCMD.stdWrite(String.Format(msg, args));
        }

        /// <summary>
        /// Writes a line to the section.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        /// <param name="args">Arguments for the formatted string.</param>
        public void WriteLine(string msg, params object[] args)
        {
            if (!DashCMD.ConsoleHandleExists) return;
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
            DashCMD.stdWriteLine(String.Format(msg, args));
        }

        /// <summary>
        /// Writes text to the section.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="args">Arguments for the formatted string.</param>
        public void Write(string msg, ConsoleColor textColor, params object[] args)
        {
            if (!DashCMD.ConsoleHandleExists) return;
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = textColor;
            DashCMD.stdWrite(String.Format(msg, args));
        }

        /// <summary>
        /// Writes a line to the section.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="args">Arguments for the formatted string.</param>
        public void WriteLine(string msg, ConsoleColor textColor, params object[] args)
        {
            if (!DashCMD.ConsoleHandleExists) return;
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = textColor;
            DashCMD.stdWriteLine(String.Format(msg, args));
        }

        /// <summary>
        /// Writes text to the section.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backColor">Color of the background.</param>
        /// <param name="args">Arguments for the formatted string.</param>
        public void Write(string msg, ConsoleColor textColor, ConsoleColor backColor, params object[] args)
        {
            if (!DashCMD.ConsoleHandleExists) return;
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = textColor;
            DashCMD.stdWrite(String.Format(msg, args));
        }

        /// <summary>
        /// Writes a line to the section.
        /// </summary>
        /// <param name="msg">Message to write.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backColor">Color of the background.</param>
        /// <param name="args">Arguments for the formatted string.</param>
        public void WriteLine(string msg, ConsoleColor textColor, ConsoleColor backColor, params object[] args)
        {
            if (!DashCMD.ConsoleHandleExists) return;
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = textColor;
            DashCMD.stdWriteLine(String.Format(msg, args));
        }
        #endregion

        internal void Start()
        {
            if (IsActive) return;
            IsActive = true;

            thread = new Thread(new ThreadStart(Loop));
            thread.Name = String.Format("DashCMDScreen - {0}", Name);
            thread.IsBackground = true;
            thread.Start();
        }

        void Loop()
        {
            string txt = String.Format("Screen - {0}", Name);
            string spaces = new string(' ', (Console.BufferWidth / 2) - (txt.Length / 2));
            string titleLine = "(Esc to exit)".PadLeft(Console.BufferWidth);
            string title = String.Format("{0}{1}{2}{3}{4}",
                spaces, txt, spaces.Substring(0, spaces.Length - 1), Environment.NewLine, titleLine);
            bool firstStep = true;

            if (AutoUpdate)
            {
                do
                {
                    int top = Console.WindowTop;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Clear();
                    DashCMD.stdWrite(title);
                    Draw();
                    if (!firstStep)
                        Console.SetWindowPosition(Console.WindowLeft, top);
                    else
                    {
                        firstStep = false;
                        Console.SetWindowPosition(Console.WindowLeft, 0);
                    }
                    Thread.Sleep(SleepTime);
                } while (IsActive);
            }
            else
            {
                int top = Console.WindowTop;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Clear();
                DashCMD.stdWrite(title);
                Draw();
                Console.SetWindowPosition(Console.WindowLeft, 0);
            }
        }

        internal void Stop()
        {
            if (!IsActive) return;
            IsActive = false;
        }

        /// <summary>
        /// Draws the screen, can be called from the 
        /// outside but only functions when active!
        /// </summary>
        public void Draw()
        {
            if (!DashCMD.ConsoleHandleExists) return;

            if (!IsActive)
                return;

            drawCallback(this);
        }
    }
}
