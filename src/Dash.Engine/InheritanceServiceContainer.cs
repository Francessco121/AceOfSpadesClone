using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace Dash.Engine
{
    /// <summary>
    /// A service container that supports inheritence
    /// with the keys of each service.
    /// </summary>
    public class InheritanceServiceContainer : IServiceContainer
    {
        class Service
        {
            public Type RootType
            {
                get { return Types[0]; }
            }

            public readonly object Object;
            public readonly Type[] Types;

            public Service(Type type, object obj, Type lowestType)
            {
                Object = obj;

                List<Type> types = new List<Type>();
                while (type != null && type != lowestType)
                {
                    types.Add(type);
                    type = type.BaseType;
                }

                Types = types.ToArray();
            }
        }

        Dictionary<Type, Service> services;
        Type lowestType;

        public InheritanceServiceContainer(Type lowestType)
        {
            this.lowestType = lowestType;
            services = new Dictionary<Type, Service>();
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            if (serviceType != lowestType && !serviceType.IsSubclassOf(lowestType))
                throw new ArgumentException(
                    "Cannot add service, specified type does not implement the lowest type supported by this ServiceContainer.");
            if (services.ContainsKey(serviceType))
                throw new InvalidOperationException("ServiceContainer already contains type, or base of type.");

            Service service = new Service(serviceType, serviceInstance, lowestType);

            for (int i = 0; i < service.Types.Length; i++)
                services.Add(service.Types[i], service);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            object obj = callback(this, serviceType);
            AddService(serviceType, obj);
        }

        public object GetService(Type serviceType)
        {
            Service service;
            if (services.TryGetValue(serviceType, out service))
                return service.Object;
            else
                return null;
        }

        public void RemoveService(Type serviceType)
        {
            Service service;
            if (services.TryGetValue(serviceType, out service))
            {
                if (service.RootType != serviceType)
                    throw new InvalidOperationException("ServiceContainer can only remove root service types!");

                for (int i = 0; i < service.Types.Length; i++)
                    services.Remove(service.Types[i]);
            }
        }

        #region Unsupported Methods
        void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            throw new NotSupportedException();
        }

        void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
        {
            throw new NotSupportedException();
        }

        void IServiceContainer.RemoveService(Type serviceType, bool promote)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
