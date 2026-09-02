using CCLBStudio.DependencyInjection;
using Services;

namespace Scripts.Services
{
    public abstract class AutoProvidedService<T> : AppService, IAutoProvider<T> where T : AutoProvidedService<T>
    {
        [Provide]
        public T Provide() => (T)this;
    }
}