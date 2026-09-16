namespace CCLBStudio.DependencyInjection
{
    public partial class TestInjectedMonoBehaviour : InjectedMonoBehaviour
    {
        [Inject] private TestProvider _testProvider;
        [Inject] private TestProvider2 _testProvider2;
        [Inject] private DebugProvider _debugProvider;

        protected override void Start()
        {
            base.Start();
            _debugProvider.Print();
            _testProvider.Print();
            _testProvider2.Print();
        }
    }
}