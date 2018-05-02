using Dash.Engine.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Threading;

/* TerrainWorker.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.TerrainThreading
{
    public class TerrainWorker : IDisposable
    {
        public bool IsBusy
        {
            get { return processing; }
        }

        public int WorkCount
        {
            get { return queue.Count; }
        }

        public int ErrorCount
        {
            get { return errors.Count; }
        }

        Terrain terrain;
        ConcurrentQueue<TerrainWorkerInstruction> queue;
        ConcurrentBag<TerrainWorkerError> errors;

        Thread thread;
        ManualResetEvent resetEvent;
        bool running;
        bool processing;
        bool disposed;

        public TerrainWorker()
        {
            resetEvent = new ManualResetEvent(false);

            thread = new Thread(new ThreadStart(ThreadLoop));
            thread.Name = "TerrainWorker";
            thread.IsBackground = true;
            thread.Start();

            queue = new ConcurrentQueue<TerrainWorkerInstruction>();
            errors = new ConcurrentBag<TerrainWorkerError>();
        }

        public void SetTerrain(Terrain terrain)
        {
            this.terrain = terrain;
        }

        public void Enqueue(Chunk chunk, TerrainWorkerAction action)
        {
            if (disposed)
                throw new ObjectDisposedException("TerrainWorker", "Cannot enqueue work, this TerrainWorker has been disposed!");

            queue.Enqueue(new TerrainWorkerInstruction(chunk, action));
            resetEvent.Set();
        }

        public TerrainWorkerError[] GetErrors()
        {
            return errors.ToArray();
        }

        void ThreadLoop()
        {
            running = true;

            while (running)
            {
                resetEvent.WaitOne();
                processing = true;

                while (queue.Count > 0)
                {
                    TerrainWorkerInstruction inst;
                    if (queue.TryDequeue(out inst))
                    {
                        try
                        {
                            if (inst.Action == TerrainWorkerAction.Populate)
                            {
                                inst.Chunk.BuildTerrain();
                                inst.Chunk.State = ChunkState.Unshaped;
                            }
                            else if (inst.Action == TerrainWorkerAction.Shape)
                            {

                                inst.Chunk.ShapeTerrain();
                                inst.Chunk.State = ChunkState.Unbuilt;
                            }
                            else if (inst.Action == TerrainWorkerAction.BuildMesh)
                            {
                                inst.Chunk.BuildMesh();
                                inst.Chunk.State = ChunkState.MeshReady;
                                terrain.MeshReadyChunks.Enqueue(inst.Chunk);
                            }
                            else
                                throw new Exception("Terrain worker given unsupported action: " + inst.Action.ToString());
                        }
                        catch (Exception ex)
                        {
                            DashCMD.WriteError("[TerrainWorker:DoWork] Caught Exception: {0}", ex);
                            errors.Add(new TerrainWorkerError(ex, inst));
                        }
                    }
                }

                resetEvent.Reset();
                processing = false;
            }
        }

        public void Dispose()
        {
            disposed = true;
            running = false;
        }
    }
}
