using System;

/* Component.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public abstract class Component : IDisposable
    {
        public GameObject GameObject { get; private set; }
        public Transform Transform { get { return GameObject.Transform; } }
        public Scene Scene { get { return GameObject.Scene; } }

        public bool IsEnabled = true;
        public bool IsDrawable = false;

        internal void SetGameObject(GameObject gameObject)
        {
            if (GameObject != null)
                OnDetached();

            GameObject = gameObject;

            if (gameObject != null)
                OnAttached();
        }

        internal protected virtual void OnAddedToScene() { }
        internal protected virtual void OnRemovedFromScene() { }

        protected virtual void OnAttached() { }
        protected virtual void OnDetached() { }

        internal protected virtual void Update(float deltaTime) { }
        internal protected virtual void Draw() { }

        public virtual void Dispose() { }
    }
}
