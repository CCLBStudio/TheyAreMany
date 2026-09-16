using CCLBStudio.DependencyInjection;
using Sirenix.OdinInspector;
using UnityEngine;
//using XJector;

public partial class HelloFromGenerated : MonoBehaviour
{
    public GameObject toInstantiate;
    
    [Inject] private TestProvider _testProvider;
    [Inject] private TestProvider2 _testProvider2;
    [Inject] private DebugProvider _debugProvider;
    
    void Start()
    {
        //Debug.Log(ExampleSourceGenerated.GetTestText());
        _debugProvider.Print();
        _testProvider.Print();
        _testProvider2.Print();
    }

    [Button]
    public void Spawn()
    {
        Instantiate(toInstantiate);
    }
}
