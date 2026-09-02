using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    /// Represents a dependency injection framework designed for Unity game development.
    /// The Injector class handles the registration of providers, automatic resolution
    /// of dependencies, and injection of dependencies into consumers. It extends from
    /// the MonoSingleton class, ensuring a global instance of the Injector is available
    /// during runtime.
    /// The Injector automatically scans the scene during the Unity Awake lifecycle phase
    /// for MonoBehaviours implementing the IDependencyProvider interface to register
    /// dependency providers. It also identifies and injects dependencies into fields,
    /// properties, and methods of MonoBehaviours annotated with the InjectAttribute.
    /// Key Responsibilities:
    /// - Discover and register dependency providers.
    /// - Resolve and inject dependencies into consumers.
    /// - Provide runtime methods for registering new providers and injecting new consumers.
    /// This class is essential in managing dependencies in Unity scenes to reduce coupling
    /// and improve modularization within the game application.
    [DefaultExecutionOrder(int.MinValue)]
    public class Injector : MonoSingleton<Injector>
    {
        #region Editor
        #if UNITY_EDITOR
        
        public static string AutoRegisterProperty => nameof(autoProviders);
        public static string AutoConsumersProperty => nameof(autoConsumers);
        
        #endif
        #endregion
        
        /// Indicates whether the Injector should automatically find and register
        /// all available dependency providers within the scene during the Awake phase.
        /// If set to true, the Injector scans all MonoBehaviours in the scene, identifies
        /// objects implementing the IDependencyProvider interface, and registers them
        /// for dependency injection. If set to false, automatic discovery and registration
        /// of providers is skipped, and providers must be manually registered.
        [Tooltip("If TRUE, the Injector will automatically find all available dependency providers and consumers (using FindObjectsByType). Providers will be registered and consumers will be injected.")]
        [SerializeField] private bool autoResolveProvidersAndConsumers = true;
        [Tooltip("Elements to be registered as providers. Note that instances should implement the IDependencyProvider interface.")]
        [SerializeField] private List<ScriptableObject> autoProviders;
        [Tooltip("Elements to be automatically injected.")]
        [SerializeField] private List<ScriptableObject> autoConsumers;
        
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private readonly Dictionary<Type, object> registry = new();
        private static readonly HashSet<IDependencyProvider> Providers = new();

        /// Initializes the Injector instance and performs dependency registration and injection.
        /// This method is called during the Unity Awake lifecycle phase. It scans all MonoBehaviours in the scene,
        /// identifies dependency providers implementing the IDependencyProvider interface, registers them, and
        /// injects dependencies into fields, properties, and methods of MonoBehaviours marked with the InjectAttribute.
        /// If any dependency cannot be resolved during injection, an exception may be thrown.
        protected override void Awake()
        {
            base.Awake();
            Providers.Clear();

            if (autoResolveProvidersAndConsumers)
            {
                var behaviours = FindMonoBehaviours();
                AutoFindAndRegisterProviders(behaviours);
                AutoFindAndInjectConsumers(behaviours);
            }

            foreach (var provider in autoProviders.Where(x => x).OfType<IDependencyProvider>())
            {
                provider.RegisterToInjector();
            }

            foreach (var consumer in autoConsumers.Where(x => x))
            {
                InjectNewConsumer(consumer);
            }
        }

        #region Injection Methods

        /// Injects dependencies into the fields, methods, and properties of the specified instance that are decorated with the InjectAttribute.
        /// This method resolves and assigns dependency instances to fields, invokes methods after resolving their parameters,
        /// and sets property values, all marked with the InjectAttribute in the provided object. If a dependency cannot be resolved
        /// for a field, method parameter, or property, an exception is thrown.
        /// <param name="target">
        /// The object that contains fields, methods, and/or properties to inject dependencies into.
        /// </param>
        private void Inject(object target)
        {
            InjectFields(target);
            InjectProperties(target);
            InjectMethods(target);
        }

        /// Injects a new MonoBehaviour consumer with dependencies.
        /// This method ensures that the specified MonoBehaviour receives all required dependency injections
        /// during runtime. It performs validation to confirm that the MonoBehaviour consumer is properly
        /// marked as injectable (using the InjectAttribute). If the consumer is not marked, the injection
        /// process is skipped, and an error message is logged.
        /// This method is restricted to play mode and will log an error if called otherwise.
        /// <param name="consumer">The MonoBehaviour instance that requires dependency injection. It must be
        /// marked with the InjectAttribute to be eligible for injection.</param>
        public static void InjectNewConsumer(MonoBehaviour consumer)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Inject is reserved for playmode only.");
                return;
            }

            if (!IsInjectable(consumer))
            {
                Debug.LogError($"Consumer {consumer.GetType().Name} is not marked with the InjectAttribute. Skipping.");
                return;
            }
            
            instance.Inject(consumer);
        }

        /// Injects dependencies for a newly created consumer object by using the dependency injection workflow.
        /// This method is specifically reserved for use during play mode and ensures the object is marked as an injectable consumer
        /// by verifying the presence of the InjectAttribute. If the object does not meet the requirements, an error is logged.
        /// Throws an error if used outside of play mode.
        /// <param name="obj">The object to be injected with its dependencies. Must be marked with the InjectAttribute and should be created or managed dynamically.</param>
        public static void InjectNewConsumer(object obj)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Inject is reserved for playmode only.");
                return;
            }

            if (!IsInjectable(obj))
            {
                Debug.LogError($"Consumer {obj.GetType().Name} is not marked with the InjectAttribute. Skipping.");
                return;
            }

            instance.Inject(obj);
        }

        /// Injects dependencies into the fields of the specified instance that are marked with the InjectAttribute.
        /// This method resolves and assigns dependency instances to all fields in the provided object that have the InjectAttribute.
        /// If a dependency cannot be resolved for a field, an exception is thrown.
        /// <param name="target">
        /// The object containing fields to inject dependencies into.
        /// </param>
        /// <exception cref="System.Exception">
        /// Thrown when a dependency for a field cannot be resolved.
        /// </exception>
        private void InjectFields(object target)
        {
            var type = target.GetType();

            var injectableFields = type.GetFields(Flags).Where(f => Attribute.IsDefined(f, typeof(InjectAttribute)));
            foreach (var field in injectableFields)
            {
                var fieldType = field.FieldType;
                var resolvedInstance = Resolve(fieldType);

                if (resolvedInstance == null)
                {
                    throw new Exception($"Failed to inject {fieldType.Name} into {type.Name}. Are you missing a provider ?");
                }

                field.SetValue(target, resolvedInstance);
            }
        }

        /// Injects dependencies into the properties of the specified instance that are marked with the InjectAttribute.
        /// This method resolves and assigns dependency instances to all properties in the provided object that have the InjectAttribute.
        /// If a dependency cannot be resolved for a property, an exception is thrown.
        /// <param name="target">
        /// The object containing properties to inject dependencies into.
        /// </param>
        /// <exception cref="System.Exception">
        /// Thrown when a dependency for a property cannot be resolved.
        /// </exception>
        private void InjectProperties(object target)
        {
            var type = target.GetType();
            
            var injectableProperties = type.GetProperties(Flags).Where(p => Attribute.IsDefined(p, typeof(InjectAttribute)));
            foreach (var property in injectableProperties)
            {
                var propertyType = property.PropertyType;
                var resolvedInstance = Resolve(propertyType);
                if (resolvedInstance == null)
                {
                    throw new Exception($"Failed to inject {propertyType.Name} into {type.Name}. Are you missing a provider ?");
                }
                
                property.SetValue(target, resolvedInstance);
                Debug.Log($"Property injected {propertyType.Name} into {type.Name}");
            }
        }

        /// Injects dependencies into methods of the specified target object.
        /// This method identifies all methods in the given target object that are marked with the InjectAttribute, resolves their parameters
        /// using the dependency resolution system, and invokes the methods with the resolved parameters. If any parameter cannot be
        /// resolved, an exception is thrown to indicate the failure of injection.
        /// <param name="target">The object whose methods will have parameters resolved and invoked.</param>
        /// <exception cref="Exception">Thrown if any dependency required for method injection cannot be resolved.</exception>
        private void InjectMethods(object target)
        {
            var type = target.GetType();
            
            var injectableMethods = type.GetMethods(Flags).Where(m => Attribute.IsDefined(m, typeof(InjectAttribute)));
            foreach (var method in injectableMethods)
            {
                var methodParameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var resolvedInstances = methodParameters.Select(Resolve).ToArray();
                if (resolvedInstances.Any(x => x == null))
                {
                    throw new Exception($"Failed to inject {type.Name}.{method.Name}. Are you missing a provider ?");
                }
                    
                Debug.Log($"Method {type.Name}.{method.Name} is about to be injected.");
                method.Invoke(target, resolvedInstances);
            }
        }

        /// Resolves an instance of the specified type from the dependency registry.
        /// This method attempts to retrieve an existing instance of the requested type that has been registered in the internal registry.
        /// <param name="type">The type of the instance to resolve.</param>
        /// <returns>The resolved instance of the specified type if found; otherwise, null.</returns>
        private object Resolve(Type type)
        {
            bool success = registry.TryGetValue(type, out var resolvedInstance);

            if (!success)
            {
                Debug.LogError($"No instance of {type.Name} found in registry");
            }
            
            return resolvedInstance;
        }

        #endregion

        #region Registration Methods

        /// Scans the provided array of MonoBehaviours to identify and register all instances implementing the IDependencyProvider interface.
        /// This method ensures that all dependency providers are systematically recognized and made available for dependency resolution.
        /// If no list is explicitly provided, all MonoBehaviours in the current scene will be scanned.
        /// <param name="behaviours">
        /// An optional array of MonoBehaviours to scan for dependency providers.
        /// If null, the method will automatically locate and use all MonoBehaviours in the scene.
        /// </param>
        private void AutoFindAndRegisterProviders(MonoBehaviour[] behaviours = null)
        {
            behaviours ??= FindMonoBehaviours();
            var providers = behaviours.OfType<IDependencyProvider>();
            foreach (var p in providers)
            {
                RegisterProvider(p);
            }
        }

        /// Identifies MonoBehaviours in the scene that are marked as injectable and performs dependency injection on them.
        /// This method automatically scans through the specified or discovered MonoBehaviours to find dependency
        /// consumers and injects the required dependencies into their fields, properties, and methods marked with the InjectAttribute.
        /// <param name="behaviours">
        /// An optional array of MonoBehaviours to scan for dependency consumers. If null, all active MonoBehaviours
        /// in the scene will be discovered and used for injection.
        /// </param>
        private void AutoFindAndInjectConsumers(MonoBehaviour[] behaviours = null)
        {
            behaviours ??= FindMonoBehaviours();
            var consumers = behaviours.Where(IsInjectable);
            foreach (var consumer in consumers)
            {
                Inject(consumer);
            }
        }

        /// Registers a dependency provider and its provided dependencies.
        /// This method scans all methods in the given provider that are decorated with the
        /// ProvideAttribute. For each method, the return type is interpreted as a dependency type, and
        /// the returned instance is registered in the internal dependency registry. If a method returns
        /// null or a duplicate dependency type is attempted to be registered, an exception will be thrown.
        /// <param name="provider">The dependency provider implementing the IDependencyProvider interface. This provider contains methods that provide
        /// instances of dependencies to be registered.</param>
        /// <exception cref="ArgumentException">Thrown if a method marked with the ProvideAttribute returns a null instance.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a duplicate dependency type is registered.</exception>
        private void RegisterProvider(IDependencyProvider provider)
        {
            if (Providers.Contains(provider))
            {
                Debug.LogError($"Provider {provider.GetType().Name} is already registered. Skipping.");
                return;
            }
            
            var methods = provider.GetType().GetMethods(Flags);
            foreach (var method in methods)
            {
                if(!Attribute.IsDefined(method, typeof(ProvideAttribute)))
                {
                    continue;
                }
                
                var returnType = method.ReturnType;

                if (registry.ContainsKey(returnType))
                {
                    Debug.LogError($"There is already an object of type {returnType.Name} registered in the registry. This provider will not be used.");
                    continue;
                }
                
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null)
                {
                    Providers.Add(provider);
                    registry.Add(returnType, providedInstance);
                }
                else
                {
                    throw new Exception($"Provider {provider.GetType().Name} returned null for method {returnType.Name}");
                }
            }
        }

        /// Notifies the Injector that a new dependency provider is available for registration.
        /// This method can only be used during play mode. It registers the provided dependency provider
        /// by invoking the RegisterProvider method internally. If called outside play mode, an error message
        /// will be logged and the method will not perform any registration.
        /// <param name="provider">The dependency provider implementing the IDependencyProvider interface to be registered.</param>
        public static void RegisterNewProvider(IDependencyProvider provider)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError($"NotifyNewProvider is reserved for playmode only.");
                return;
            }

            instance.RegisterProvider(provider);
        }

        #endregion

        #region Helper Methods

        /// Determines if a given MonoBehaviour instance is eligible for dependency injection.
        /// A MonoBehaviour is considered injectable if it contains at least one member (field, property, or method)
        /// that is decorated with the InjectAttribute.
        /// <param name="obj">The MonoBehaviour instance to examine for injectability.</param>
        /// <returns>True if the specified MonoBehaviour contains at least one member with the InjectAttribute; otherwise, false.</returns>
        private static bool IsInjectable(MonoBehaviour obj)
        {
            var members = obj.GetType().GetMembers(Flags);
            return members.Any(m => Attribute.IsDefined(m, typeof(InjectAttribute)));
        }

        /// Determines if the given object is injectable by checking if it has members marked with the InjectAttribute.
        /// This method inspects the object's type for any fields, properties, or methods that are decorated with the InjectAttribute
        /// to identify whether the object is eligible for dependency injection.
        /// <param name="obj">The object to check for injectability. This can be any object type.</param>
        /// <returns>True if the object has at least one member with the InjectAttribute, otherwise false.</returns>
        private static bool IsInjectable(object obj)
        {
            var members = obj.GetType().GetMembers(Flags);
            return members.Any(m => Attribute.IsDefined(m, typeof(InjectAttribute)));
        }

        /// Determines if the specified dependency provider is already registered with the Injector.
        /// <param name="provider">The instance of the dependency provider to check for registration.</param>
        /// <returns>Returns true if the provider is registered; otherwise, false.</returns>
        public static bool IsProviderRegistered(IDependencyProvider provider)
        {
            return Providers.Contains(provider);
        }

        private static MonoBehaviour[] FindMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>();
        }

        #endregion
    }
}