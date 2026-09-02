using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class ClassB : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [Inject] public FactoryB FactoryB { get; set; }
        [Inject] private ServiceA serviceA;
        [Inject] private ServiceB serviceB;
        private FactoryA factoryA;
        
        [Inject][UsedImplicitly]
        public void Initialize(FactoryA injectedFactoryA)
        {
            Debug.Log($"Injecting {injectedFactoryA.GetType().Name} into {GetType().Name}");
            factoryA = injectedFactoryA;
        }
        
        private void Start()
        {
            serviceA.Initialize("Hello from ClassB");
            serviceB.Initialize("Hello from ClassB");
            factoryA.CreateServiceA().Initialize("ServiceA init from FactoryA");
            FactoryB.Hello("Hello from ClassB");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Instantiate(prefab);
            }
        }
    }
}