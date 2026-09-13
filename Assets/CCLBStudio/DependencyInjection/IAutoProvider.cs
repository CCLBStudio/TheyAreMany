using CCLBStudio.DependencyInjection;
using UnityEngine;

public interface IAutoProvider<out T> where T : IDependencyProvider
{
    [ProvideLegacy]
    public T Provide();
}
