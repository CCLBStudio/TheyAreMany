using CCLBStudio.DependencyInjection;
using UnityEngine;

namespace Services
{
    public abstract class AppService : ScriptableObject, IDependencyProvider
    {
        public virtual void Initialize()
        {
        }
        
        public virtual void OnAllServicesInitialized() { }

        public virtual void OnPostInitialize()  { }
    }
}
