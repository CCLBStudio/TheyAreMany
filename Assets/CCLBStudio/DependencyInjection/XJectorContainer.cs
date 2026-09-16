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
            if(instance == null)
            {
                Debug.LogWarning($"[XJectorContainer] Attempted to provide a null instance of type {typeof(T)}.");
                return;
            }
            
            Debug.Log($"[XJectorContainer] Provide instance of type {typeof(T)}.");
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
        public void Print()
        {
            Debug.Log($"[XJectorContainer] Debug Print");
        }
    }
    
    [Provide]
    public partial class TestProvider2
    {
        public void Print()
        {
            Debug.Log($"[XJectorContainer] Prout");
        }
    }
}
