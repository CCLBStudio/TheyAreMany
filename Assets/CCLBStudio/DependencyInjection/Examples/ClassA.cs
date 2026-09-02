using JetBrains.Annotations;
using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class ClassA : MonoBehaviour
    {
        [Inject] private IEnvironmentSystem environmentSystem;
        private ServiceA serviceA;
        
        [Inject][UsedImplicitly]
        public void Initialize(ServiceA injectedServiceA)
        {
            Debug.Log($"Injecting {injectedServiceA.GetType().Name} into {GetType().Name}");
            serviceA = injectedServiceA;
        }

        private void Start()
        {
            serviceA.Initialize("Hello from ClassA");
            environmentSystem.Initialize();
        }
    }
}