using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XJector.Extensions;

public static class SyntaxSymbolExtensions
{
    public static bool IsClassWithProvideAttribute(this SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration 
               && classDeclaration.AttributeLists.Count > 0
               && classDeclaration.AttributeLists
                   .SelectMany(al => al.Attributes)
                   .Any(attr => attr.Name.ToString() is "Provide" or "ProvideAttribute");
    }

    public static ProviderKind GetProviderKind(this INamedTypeSymbol typeSymbol)
    {
        for (var baseType = typeSymbol.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            switch (baseType.ToDisplayString())
            {
                case "UnityEngine.MonoBehaviour":
                    return ProviderKind.MonoBehaviour;
                case "UnityEngine.ScriptableObject":
                    return ProviderKind.ScriptableObject;
            }
        }

        return ProviderKind.PlainClass;
    }
    
    public static int GetIntArgument(this AttributeData attribute, string name, int defaultValue = 0)
    {
        if (attribute == null) return defaultValue;
        
        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Key == name && arg.Value.Value != null)
            {
                return System.Convert.ToInt32(arg.Value.Value);
            }
        }
        return defaultValue;
    }
    
    public static bool GetBoolArgument(this AttributeData attribute, string name, bool defaultValue = false)
    {
        if (attribute == null) return defaultValue;
        
        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Key == name && arg.Value.Value is bool b)
            {
                return b;
            }
        }
        return defaultValue;
    }
    
    public static string GetStringArgument(this AttributeData attribute, string name)
    {
        if (attribute == null) return null;
        
        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Key == name && arg.Value.Value is string s)
            {
                return s;
            }
        }
        return null;
    }
}