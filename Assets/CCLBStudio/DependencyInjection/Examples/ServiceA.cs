using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class ServiceA
    {
        public void Initialize(string message = null)
        {
            Debug.Log($"{GetType().Name} initialized with message: {message}");
        }
    }
}