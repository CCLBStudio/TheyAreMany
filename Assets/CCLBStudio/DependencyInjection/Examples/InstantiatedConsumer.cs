namespace CCLBStudio.DependencyInjection
{
    public class InstantiatedConsumer : SelfInjectedMonoBehaviour
    {
        [Inject] private ServiceA serviceA;
        [Inject] private ServiceB serviceB;

        [Inject]
        public void Hello()
        {
            serviceA.Initialize("Hello from InstantiatedConsumer");
            serviceB.Initialize("Hello from InstantiatedConsumer");
        }
    }
}