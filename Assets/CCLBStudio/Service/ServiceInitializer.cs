using CCLBStudio.DependencyInjection;
using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    [DefaultExecutionOrder(-10)]
    public class ServiceInitializer : MonoBehaviour
    {
        [SerializeField] private List<AppService> services;

        /// <summary>
        /// Initializes all AppService instances provided in the list and invokes their
        /// setup process.
        /// In this method:
        /// - The `Initialize` method of each AppService is called to register the service
        /// to the dependency injector.
        /// - After all services are initialized, the `OnAllServicesInitialized` method of
        /// each service is invoked to perform post-initialization setup or notify that
        /// all services are ready to use.
        /// This ensures that all AppServices are correctly initialized and integrated
        /// before any other system operations that depend on these services.
        /// </summary>
        private void Awake()
        {
            foreach (var service in services)
            {
                service.RegisterToInjector();
                service.Initialize();
            }
            
            foreach (var service in services)
            {
                service.OnAllServicesInitialized();
            }
        }
    }
}
