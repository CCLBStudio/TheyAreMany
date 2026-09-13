using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class XJectorContainer
    {
        private static readonly Dictionary<Type, object> _registry = new();
        
        public static void Provide<T>(T instance)
        {
            _registry[typeof(T)] = instance;
            Debug.Log($"[XJectorContainer] Added object of type {typeof(T)} to registry.");
        }

        public static T Resolve<T>()
        {
            if (_registry.TryGetValue(typeof(T), out var instance))
            {
                return (T)instance;
            }

            Debug.LogWarning($"[XJectorContainer] No object of type {typeof(T)} found in registry.");
            return default;
        }
    }
    
    [Provide]
    public partial class TestProvider
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // public static void Provide()
        // {
        //     XJectorContainer.Provide(new TestProvider());
        // }

        public void Print()
        {
            Debug.Log($"[XJectorContainer] Debug Print");
        }
    }
}
