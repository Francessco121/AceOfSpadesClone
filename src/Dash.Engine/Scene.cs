using System;
using System.Collections.Generic;

namespace Dash.Engine
{
    public class Scene : IDisposable
    {
        protected List<GameObject> gameObjects { get; private set; }
        InheritanceServiceContainer componentContainer;
        List<SceneComponent> components;

        public Scene()
        {
            gameObjects = new List<GameObject>();
            componentContainer = new InheritanceServiceContainer(typeof(SceneComponent));
            components = new List<SceneComponent>();
        }

        public void AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
            gameObject.OnAddedToScene(this);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            gameObjects.Remove(gameObject);
            gameObject.OnAddedToScene(null);
        }

        public GameObject[] GetGameObjects()
        {
            return gameObjects.ToArray();
        }

        SceneComponent FindComponent(Type type)
        {
            return componentContainer.GetService(type) as SceneComponent;
        }

        /// <summary>
        /// Adds a component to this scene.
        /// </summary>
        public void AddComponent(SceneComponent component)
        {
            Type type = component.GetType();
            if (HasComponent(type))
                throw new InvalidOperationException("GameObject already contains a component of type " + type.FullName);

            if (component.Scene != null && component.Scene != this)
                component.Scene.RemoveComponent(type);

            componentContainer.AddService(type, component);
            components.Add(component);
            component.SetScene(this);
        }

        /// <summary>
        /// Gets the component by type.
        /// Returns null if type is not added.
        /// </summary>
        public T GetComponent<T>()
            where T : SceneComponent
        {
            SceneComponent c = FindComponent(typeof(T));
            return c != null ? (T)c : null;
        }

        /// <summary>
        /// Gets the component by type.
        /// Returns null if type is not added.
        /// </summary>
        public SceneComponent GetComponent(Type type)
        {
            return FindComponent(type);
        }

        /// <summary>
        /// Returns whether the specified component is
        /// attached to this scene.
        /// </summary>
        public bool HasComponent<T>()
            where T : SceneComponent
        {
            return FindComponent(typeof(T)) != null;
        }

        /// <summary>
        /// Returns whether the specified component is
        /// attached to this scene.
        /// </summary>
        public bool HasComponent(Type type)
        {
            return FindComponent(type) != null;
        }

        /// <summary>
        /// Attempts to retrieve the attached component by type.
        /// </summary>
        public bool TryGetComponent<T>(out T component)
            where T : SceneComponent
        {
            SceneComponent c = FindComponent(typeof(T));
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
        public bool TryGetComponent(Type type, out SceneComponent component)
        {
            SceneComponent c = FindComponent(type);
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
            where T : SceneComponent
        {
            T c = GetComponent<T>();
            if (c != null)
            {
                componentContainer.RemoveService(typeof(T));
                components.Remove(c);
                c.SetScene(null);
            }
        }

        /// <summary>
        /// Removes a component by type.
        /// </summary>
        public void RemoveComponent(Type type)
        {
            SceneComponent c = GetComponent(type);
            if (c != null)
            {
                componentContainer.RemoveService(type);
                components.Remove(c);
                c.SetScene(null);
            }
        }

        public virtual void Update(float deltaTime)
        {
            foreach (SceneComponent c in components)
                if (c.IsEnabled)
                    c.Update(deltaTime);

            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObject go = gameObjects[i];
                if (go.IsEnabled)
                    go.Update(deltaTime);
            }
        }

        public virtual void Draw()
        {
            foreach (SceneComponent c in components)
                if (c.IsDrawable)
                    c.Draw();

            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObject go = gameObjects[i];
                if (go.IsDrawable)
                    go.Draw();
            }
        }

        public virtual void Dispose()
        {
            // Dispose of each component
            foreach (SceneComponent c in components)
                c.Dispose();

            // Dispose of each game object
            GameObject[] gameObjects = this.gameObjects.ToArray();
            for (int i = 0; i < gameObjects.Length; i++)
                gameObjects[i].Dispose();
        }
    }
}
