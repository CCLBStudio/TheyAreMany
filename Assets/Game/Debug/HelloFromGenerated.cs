using CCLBStudio.DependencyInjection;
using UnityEngine;

public partial class HelloFromGenerated : MonoBehaviour
{
    [Inject] private TestProvider _testProvider;
    
    void Start()
    {
        _testProvider.Print();
    }
}
