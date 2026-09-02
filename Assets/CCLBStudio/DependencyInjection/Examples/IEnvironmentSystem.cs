using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public interface IEnvironmentSystem
    {
        IEnvironmentSystem ProvideEnvironmentSystem();
        void Initialize();
    }
}