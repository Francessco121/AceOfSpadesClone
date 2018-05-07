using System;
using System.Collections.Generic;

/* GameObject.cs
 * Ethan Lafrenais
*/

namespace Dash.Engine
{
    public class GameObject : IDisposable
    {
        public Transform Transform { get; }
        public Scene Scene { get; private set; }

        public bool IsEnabled = true;
        public bool IsDrawable = true;

        protected bool IsDisposed { get; private set; }

        InheritanceServiceContainer componentContainer;
        List<Component> components;

        public GameObject(Vector3 position)
            : this()
        {
            Transform.Position = position;
        }

        public GameObject()
        {
            Transform = new Transform(this);

            componentContainer = new InheritanceServiceContainer(typeof(Component));
            components = new List<Component>();
        }

        protected virtual void OnAddedToScene()
        {
            foreach (Component c in components)
                c.OnAddedToScene();
        }

        protected virtual void OnRemovedFromScene()
        {
            foreach (Component c in components)
                c.OnRemovedFromScene();
        }

        internal void OnAddedToScene(Scene scene)
        {
            if (scene == null)
                OnRemovedFromScene();

            Scene = scene;

            if (scene != null)
                OnAddedToScene();
        }

        Component FindComponent(Type type)
        {
            return componentContainer.GetService(type) as Component;
        }

        /// <summary>
        /// Adds and attaches a component to this game object.
        /// </summary>
        public void AddComponent(Component component)
        {
            Type type = component.GetType();
            if (HasComponent(type))
                throw new InvalidOperationException("GameObject already contains a component of type " + type.FullName);

            if (component.GameObject != null && component.GameObject != this)
                component.GameObject.RemoveComponent(type);
            
            componentContainer.AddService(type, component);
            components.Add(component);
            component.SetGameObject(this);
        }

        /// <summary>
        /// Gets the attached component by type.
        /// Returns null if type is not attached.
        /// </summary>
        public T GetComponent<T>()
            where T : Component
        {
            Component c = FindComponent(typeof(T));
            return c != null ? (T)c : null;
        }

        /// <summary>
        /// Gets the attached component by type.
        /// Returns null if type is not attached.
        /// </summary>
        public Component GetComponent(Type type)
        {
            return FindComponent(type);
        }

        /// <summary>
        /// Returns whether the specified component is
        /// attached to this game object.
        /// </summary>
        public bool HasComponent<T>()
            where T : Component
        {
            return FindComponent(typeof(T)) != null;
        }

        /// <summary>
        /// Returns whether the specified component is
        /// attached to this game object.
        /// </summary>
        public bool HasComponent(Type type)
        {
            return FindComponent(type) != null;
        }

        /// <summary>
        /// Attempts to retrieve the attached component by type.
        /// </summary>
        public bool TryGetComponent<T>(out T component)
            where T : Component
        {
            Component c = FindComponent(typeof(T));
            if (c != null)
            {
                component = (T)c;
                return true;
            }
            else
            {
                component = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to retrieve the attached component by type.
        /// </summary>
        public bool TryGetComponent(Type type, out Component component)
        {
            Component c = FindComponent(type);
            if (c != null)
            {
                component = c;
                return true;
            }
            else
            {
                component = null;
                return false;
            }
        }

        /// <summary>
        /// Removes a component by type.
        /// </summary>
        public void RemoveComponent<T>()
            where T : Component
        {
            T c = GetComponent<T>();
            if (c != null)
            {
                componentContainer.RemoveService(typeof(T));
                components.Remove(c);
                c.SetGameObject(null);
            }
        }

        /// <summary>
        /// Removes a component by type.
        /// </summary>
        public void RemoveComponent(Type type)
        {
            Component c = GetComponent(type);
            if (c != null)
            {
                componentContainer.RemoveService(type);
                components.Remove(c);
                c.SetGameObject(null);
            }
        }

        protected internal virtual void Update(float deltaTime)
        {
            foreach (Component c in components)
                if (c.IsEnabled)
                    c.Update(deltaTime);
        }

        protected internal virtual void Draw()
        {
            foreach (Component c in components)
                if (c.IsDrawable)
                    c.Draw();
        }

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                // Dispose of each component
                foreach (Component c in components)
                    c.Dispose();

                // Remove this game object from the scene
                if (Scene != null)
                    Scene.RemoveGameObject(this);

                IsDisposed = true;
            }
        }
    }
}
