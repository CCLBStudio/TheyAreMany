using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public abstract class AutoScriptableProvider<T> : ScriptableObject, IDependencyProvider, IAutoProvider<T> where T : AutoScriptableProvider<T>
    {
        [ProvideLegacy]
        public virtual T Provide() => (T)this;
    }
}
