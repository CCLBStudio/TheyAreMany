using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class FactoryB
    {
        public void Hello(string message = null)
        {
            Debug.Log($"{GetType().Name} says: {message}");
        }
    }
}