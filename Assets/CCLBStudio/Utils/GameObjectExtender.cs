using UnityEngine;

namespace CCLBStudio.Utils
{
    public static class GameObjectExtender
    {
        public static T GetOrAdd<T>(this GameObject go) where T : Component
        {
            return go.TryGetComponent(out T component) ? component : go.AddComponent<T>();
        }
        
        public static T GetOrAdd<T>(this Component comp) where T : Component
        {
            return comp.TryGetComponent(out T component) ? component : comp.gameObject.AddComponent<T>();
        }
    }
}
