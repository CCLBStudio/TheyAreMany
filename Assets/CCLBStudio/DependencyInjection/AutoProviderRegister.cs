using JetBrains.Annotations;
using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    /// <summary>
    /// Represents an abstract base class for facilitating automatic registration of MonoBehaviour-derived classes
    /// with a dependency injection system in Unity. Classes inheriting from <c>AutoProviderRegister</c>
    /// can be configured to automatically register their dependencies during specific Unity lifecycle events.
    /// </summary>
    /// <remarks>
    /// The class supports automatic registration during the <c>Awake</c>, <c>OnEnable</c>, or <c>Start</c> lifecycle events,
    /// determined by the <c>autoRegisterOn</c> property. Derived classes can override these lifecycle methods to implement
    /// custom behavior while ensuring they register with the injector.
    /// This class must be inherited to provide concrete implementations for dependency registrations.
    /// </remarks>
    public abstract class AutoProviderRegister : MonoBehaviour, IDependencyProvider
    {
        [SerializeField] private RegistrationEvent autoRegisterOn = RegistrationEvent.Awake;

        protected enum RegistrationEvent
        {
            [UsedImplicitly] None,
            Awake,
            OnEnable,
            Start
        }

        protected virtual void Awake()
        {
            if (autoRegisterOn == RegistrationEvent.Awake)
            {
                this.RegisterToInjector();
            }
        }

        protected virtual void OnEnable()
        {
            if (autoRegisterOn == RegistrationEvent.OnEnable)
            {
                this.RegisterToInjector();
            }
        }

        protected virtual void Start()
        {
            if (autoRegisterOn == RegistrationEvent.Start)
            {
                this.RegisterToInjector();
            }
        }
    }
}