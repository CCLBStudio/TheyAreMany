using UnityEngine;

namespace CCLBStudio.Extensions
{
    public static class ObjectExtender
    {
        public static T GetComponentFromRoot<T>(this Component comp)
        {
            return comp ? comp.transform.root.GetComponentInChildren<T>() : default;
        }
        
        public static bool TryGetComponentFromRoot<T>(this Component comp, out T component)
        {
            if (!comp)
            {
                component = default;
                return false;
            }
            
            component = comp.transform.root.GetComponentInChildren<T>();
            return component != null;
        }
        
        public static bool TryGetComponentOnRoot<T>(this Component comp, out T component)
        {
            if (!comp)
            {
                component = default;
                return false;
            }
            
            component = comp.transform.root.GetComponent<T>();
            return component != null;
        }
    }
}
