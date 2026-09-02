using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public interface IDependencyProvider
    {
        public void RegisterToInjector()
        {
            if (Injector.IsProviderRegistered(this))
            {
                Debug.LogError($"Object of type {GetType().Name} is already registered to the injector.");
                return;
            }
            
            // Register to injector
            Injector.RegisterNewProvider(this);
        }
    }

    public static class DependencyProviderHelper
    {
        public static void RegisterToInjector(this IDependencyProvider provider)
        {
            provider.RegisterToInjector();
        }
    }
}