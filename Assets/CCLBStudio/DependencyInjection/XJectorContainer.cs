using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CCLBStudio.DependencyInjection
{
    public class XJectorContainer
    {
        private static readonly Dictionary<Type, object> _registry = new();
        
        public static void Provide<T>(T instance)
        {
            AddToRegistry(typeof(T), instance);
        }
        
        public static void Provide(Type type, object instance)
        {
            AddToRegistry(type, instance);
        }
        
        private static void AddToRegistry(Type type, object instance)
        {
            if (instance is Object o && !o)
            {
                Debug.LogWarning($"[XJectorContainer] Attempted to add a destroyed UnityEngine.Object of type {type} to registry.");
                return;
            }
            
            if(instance == null)
            {
                Debug.LogWarning($"[XJectorContainer] Attempted to add a null instance of type {type} to registry.");
                return;
            }
            
            _registry[type] = instance;
            Debug.Log($"[XJectorContainer] Added object of type {type} to registry.");
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

        #region Editor
        #if UNITY_EDITOR
        
        [InitializeOnEnterPlayMode]
        private static void ClearRegistry()
        {
            _registry.Clear();
            Debug.Log($"[XJectorContainer] Cleared registry on entering play mode.");
        }
        
        #endif
        #endregion
    }
}
