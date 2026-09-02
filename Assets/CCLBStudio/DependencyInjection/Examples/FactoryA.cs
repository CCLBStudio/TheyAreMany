namespace CCLBStudio.DependencyInjection
{
    public class FactoryA
    {
        private ServiceA cachedServiceA;
        
        public ServiceA CreateServiceA()
        {
            return cachedServiceA ??= new ServiceA();
        }
    }
}