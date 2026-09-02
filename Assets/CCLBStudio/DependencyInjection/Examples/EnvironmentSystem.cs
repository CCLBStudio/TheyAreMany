using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class EnvironmentSystem : AutoProviderRegister, IEnvironmentSystem
    {
        [Provide]
        public IEnvironmentSystem ProvideEnvironmentSystem()
        {
            return this;
        }

        public void Initialize()
        {
            Debug.Log("Environment system initialized");
        }
    }
}