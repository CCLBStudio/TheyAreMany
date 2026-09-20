using System;

namespace CCLBStudio.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class InjectLegacyAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectAttribute : Attribute
    {
    }
    
    /// <summary>
    /// Strategy used to provide a MonoBehaviour instance to the container.
    /// </summary>
    public enum MonoProvideStrategy
    {
        /// <summary>The generator creates a dedicated GameObject and adds the component to it.</summary>
        CreateGameObject,
        /// <summary>The MonoBehaviour registers itself during a Unity lifecycle event (see <see cref="ProvideAttribute.RegisterOn"/>).</summary>
        SelfRegister
    }

    /// <summary>
    /// Unity lifecycle event on which a self-registering provider registers itself.
    /// </summary>
    public enum ProviderRegistrationEvent
    {
        Awake,
        OnEnable,
        Start
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProvideAttribute : Attribute
    {
        /// <summary>
        /// For MonoBehaviour providers, controls how the instance is created/registered.
        /// Ignored for plain classes and ScriptableObjects.
        /// </summary>
        public MonoProvideStrategy MonoStrategy { get; set; } = MonoProvideStrategy.CreateGameObject;

        /// <summary>
        /// For MonoBehaviour providers using <see cref="MonoProvideStrategy.SelfRegister"/>,
        /// the lifecycle event on which the instance registers itself.
        /// </summary>
        public ProviderRegistrationEvent RegisterOn { get; set; } = ProviderRegistrationEvent.Awake;

        /// <summary>
        /// For ScriptableObject providers, the path (relative to a <c>Resources</c> folder, without extension)
        /// of the asset to load and provide to the container. Required for ScriptableObjects.
        /// </summary>
        public string ResourcesPath { get; set; }

        /// <summary>
        /// For MonoBehaviour providers using <see cref="MonoProvideStrategy.CreateGameObject"/>,
        /// whether the created GameObject survives scene loads.
        /// </summary>
        public bool DontDestroyOnLoad { get; set; } = true;
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideLegacyAttribute : Attribute
    {
    }
}