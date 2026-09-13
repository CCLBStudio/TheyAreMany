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
    
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProvideAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideLegacyAttribute : Attribute
    {
    }
}