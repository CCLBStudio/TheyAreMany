using UnityEngine;

namespace CCLBStudio.Extensions
{
    public static class ObjectExtender
    {
        public static T GetComponentFromRoot<T>(this Component comp)
        {
            return comp ? comp.transform.root.GetComponentInChildren<T>() : default;
        }
    }
}
