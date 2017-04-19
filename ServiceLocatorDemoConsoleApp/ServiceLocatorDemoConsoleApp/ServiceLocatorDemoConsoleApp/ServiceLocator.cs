using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLocatorDemoConsoleApp
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, Type> _mapping = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, Object> _resources = new Dictionary<Type, object>();
        private static object lockobj = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <param name="instance"></param>
        public static void Add<TClass>(object instance) where TClass : class
        {
            Add(typeof(TClass), instance);
        }

        public static void Add(Type typeofInstance, object instance)
        {
            if (typeofInstance == null)
            {
                throw new ArgumentNullException(nameof(typeofInstance));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (!(typeofInstance.IsInstanceOfType(instance)))
            {
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "Resource does not implement supplied interface: {0}", typeofInstance.FullName));
            }
            lock (lockobj)
            {
                if (_resources.ContainsKey(typeofInstance))
                {
                    throw new ArgumentException(
              string.Format(CultureInfo.InvariantCulture,
"Resource is already existing : {0}", typeofInstance.FullName));
                }
                _resources[typeofInstance] = instance;
            }
        }

        public static TClass Get<TClass>() where TClass : class
        {
            return Get(typeof(TClass)) as TClass;
        }
        public static object Get(Type typeOfInstance)
        {
            if (typeOfInstance == null)
                throw new ArgumentNullException("typeOfInstance");
            object resource;
            lock (lockobj)
            {
                _resources.TryGetValue(typeOfInstance, out resource);
                //if (!_resources.TryGetValue(typeOfInstance,out resource))
                //{
                //    //throw new ResourceNotFoundException(typeOfInstance.FullName);
                //}
            }
            return resource;
        }
        public static bool TryGet<TClass>(out TClass resource) where TClass : class
        {
            bool isFound = false;
            resource = null;
            object target;
            lock (lockobj)
            {
                if (_resources.TryGetValue(typeof(TClass), out target))
                {
                    resource = target as TClass;
                    isFound = true;
                }
            }
            return isFound;
        }
        public static void RegisterType<TClass>() where TClass : class, new()
        {
            lock (lockobj)
            {
                _mapping[typeof(TClass)] = typeof(TClass);
            }
        }
        public static void RegisterType<TFrom, TTo>()
            where TFrom : class
            where TTo : TFrom, new()
        {
            lock (lockobj)
            {
                _mapping[typeof(TFrom)] = typeof(TTo);
                _mapping[typeof(TTo)] = typeof(TTo);
            }
        }
        public static bool IsRegistered<TClass>() where TClass : class
        {
            lock (lockobj)
            {
                return _mapping.ContainsKey(typeof(TClass));
            }
        }
        public static TClass Resolve<TClass>() where TClass : class
        {
            TClass resource = default(TClass);
            bool isExisting = TryGet<TClass>(out resource);
            if (!isExisting)
            {
                ConstructorInfo constructor = null;
                lock (lockobj)
                {
                    if (!_mapping.ContainsKey(typeof(TClass)))
                    {
                        throw new Exception(
             string.Format(CultureInfo.InvariantCulture,
"Cannot find the target type : {0}", typeof(TClass).FullName));

                    }
                    Type concrete = _mapping[typeof(TClass)];
                    constructor = concrete.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
                    if (constructor == null)
                    {
                        throw new Exception(
              string.Format(CultureInfo.InvariantCulture,
"Public constructor is missing for type : {0}", typeof(TClass).FullName));
                    }
                }
                Add<TClass>((TClass)constructor.Invoke(null));
            }
            return Get<TClass>();
        }


        public static void Remove<TClass>()
        {
            Teardown(typeof(TClass));
        }
        public static void Remove(Type typeOfInstance)
        {
            if (typeOfInstance == null)
                throw new ArgumentNullException("typeOfInstance");

            lock (lockobj)
            {
                _resources.Remove(typeOfInstance);
            }
        }
        public static void Teardown<TClass>()
        {
            Teardown(typeof(TClass));
        }
        public static void Teardown(Type typeOfInstance)
        {
            if (typeOfInstance == null)
                throw new ArgumentNullException("typeOfInstance");

            lock (lockobj)
            {
                _resources.Remove(typeOfInstance);
                _mapping.Remove(typeOfInstance);
            }
        }
        public static void Clear()
        {
            lock (lockobj)
            {
                _resources.Clear();
                _mapping.Clear();
            }
        }


    }
}
