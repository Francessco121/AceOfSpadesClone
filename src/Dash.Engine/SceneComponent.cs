using System;

/* SceneComponent.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public abstract class SceneComponent : IDisposable
    {
        public Scene Scene { get; private set; }

        public bool IsEnabled = true;
        public bool IsDrawable = false;

        internal void SetScene(Scene scene)
        {
            if (Scene != null)
                OnRemovedFromScene();

            Scene = scene;

            if (scene != null)
                OnAddedToScene();
        }

        internal protected virtual void OnAddedToScene() { }
        internal protected virtual void OnRemovedFromScene() { }

        internal protected virtual void Update(float deltaTime) { }
        internal protected virtual void Draw() { }

        public virtual void Dispose() { }
    }
}
