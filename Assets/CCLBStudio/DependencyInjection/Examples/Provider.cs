using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class Provider : AutoProviderRegister
    {
        [Provide]
        public ServiceA ProvideServiceA()
        {
            return new ServiceA();
        }
        
        [Provide]
        public ServiceB ProvideServiceB()
        {
            return new ServiceB();
        }
        
        [Provide]
        public FactoryA ProvideFactoryA()
        {
            return new FactoryA();
        }
        
        [Provide]
        public FactoryB ProvideFactoryB()
        {
            return new FactoryB();
        }
    }
}