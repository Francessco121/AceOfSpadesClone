using Dash.Engine.Diagnostics;
using System;
using System.Diagnostics;
using System.Threading;

/* SimulatedGame.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Server
{
    public abstract class SimulatedGame
    {
        /// <summary>
        /// The time between each frame (to target), in milliseconds.
        /// </summary>
        public int FrameDelay;

        public bool IsRunning
        {
            get { return threadRunning; }
        }

        /// <summary>
        /// The game loop's thread.
        /// </summary>
        protected Thread gameThread { get; private set; }
        /// <summary>
        /// Is the game running?
        /// </summary>
        protected bool gameRunning { get; private set; }
        /// <summary>
        /// Is the game loop thread running?
        /// </summary>
        protected bool threadRunning { get; private set; }

        int nextFrame;

        /// <summary>
        /// Creates a new NetGame.
        /// <para>Defaults framerate to 120FPS</para>
        /// </summary>
        public SimulatedGame()
        {
            FrameDelay = 1000 / 120;
        }

        /// <summary>
        /// Creates a new NetGame.
        /// </summary>
        /// <param name="targetFPS">The FPS to target.</param>
        public SimulatedGame(int targetFPS)
        {
            FrameDelay = 1000 / targetFPS;
        }

        /// <summary>
        /// Starts the game loop.
        /// </summary>
        public void Start(string threadName)
        {
            gameThread = new Thread(new ThreadStart(GameLoop));
            gameThread.Name = threadName;
            gameThread.IsBackground = true;
            gameThread.Start();
        }

        protected abstract void OnStopped();

        /// <summary>
        /// Stops the game loop.
        /// </summary>
        public virtual void Stop()
        {
            threadRunning = false;
        }

        void GameLoop()
        {
            threadRunning = true;
            gameRunning = true;

            nextFrame = Environment.TickCount + FrameDelay;
            int lastFrame = Environment.TickCount;
            int accumulator = 0;

            while (threadRunning)
            {
                int now = Environment.TickCount;
                int mdt = now - lastFrame;
                accumulator += mdt;

                if (accumulator > FrameDelay)
                {
                    Update(accumulator / 1000f);
                    accumulator = 0;
                }


                //if (now >= nextFrame)
                //{
                //    int milliDelta = now - (nextFrame - FrameDelay);
                //    nextFrame += FrameDelay;

                //    float dt = milliDelta / 1000f;
                //    float targetDt = FrameDelay / 1000f;

                //    int i = 0;
                //    while (dt >= targetDt)
                //    {
                //        Update(dt);
                //        dt -= targetDt;

                //        if (i >= 5)
                //        {
                //            DashCMD.WriteWarning("Server can't keep up! Skpping {0}s...", dt);
                //            nextFrame = Environment.TickCount + FrameDelay;
                //            break;
                //        }

                //        i++;
                //    }
                //}

                lastFrame = now;
                Thread.Sleep(1);
            }

            gameRunning = false;
            OnStopped();
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="deltaTime">The delta time since the last update frame. 
        /// (Targeted around 8.3 milliseconds)</param>
        protected abstract void Update(float deltaTime);
    }
}
