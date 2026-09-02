using CCLBStudio.DependencyInjection;
using UnityEngine;

public interface IAutoProvider<out T> where T : IDependencyProvider
{
    [Provide]
    public T Provide();
}
