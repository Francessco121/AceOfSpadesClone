using Dash.Engine.Diagnostics;

/* Gamemode.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public abstract class Gamemode
    {
        public GamemodeType Type { get; }
        public bool IsActive { get; protected set; }

        public Gamemode(GamemodeType type)
        {
            Type = type;
        }

        public virtual void Start()
        {
            DashCMD.WriteImportant("[Gamemode] Starting '{0}'...", Type);
            OnStarted();
        }

        public virtual void Stop()
        {
            DashCMD.WriteImportant("[Gamemode] Stopping '{0}'...", Type);
            OnStopped();
        }

        protected virtual void OnStarted() { }
        protected virtual void OnStopped() { }

        public virtual void Update(float deltaTime) { }
    }
}
