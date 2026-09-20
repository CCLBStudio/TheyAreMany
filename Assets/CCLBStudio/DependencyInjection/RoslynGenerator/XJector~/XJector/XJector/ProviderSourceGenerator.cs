using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace XJector;

[Generator]
public class ProviderSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsClassWithAttributes(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m != null);
        
        var assemblyName = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName);
        var combined = assemblyName.Combine(classDeclarations.Collect());
        
        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            Execute(source.Left, source.Right, spc);
        });
    }
    
    private static bool IsClassWithAttributes(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration 
               && classDeclaration.AttributeLists.Count > 0
               && classDeclaration.AttributeLists
                   .SelectMany(al => al.Attributes)
                   .Any(attr => attr.Name.ToString() is "Provide" or "ProvideAttribute");
    }
    
    private static ProviderClassInfo GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }
        
        string className = classSymbol.Name;
        string namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : classSymbol.ContainingNamespace.ToDisplayString();
        bool isPartial = classSymbol.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .Any(c => c.Modifiers.Any(m => m.Text == "partial"));

        ProviderKind kind = GetProviderKind(classSymbol);

        var provideAttribute = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "ProvideAttribute" or "Provide");

        int monoStrategy = GetIntArgument(provideAttribute, "MonoStrategy", 0);
        int registerOn = GetIntArgument(provideAttribute, "RegisterOn", 0);
        bool dontDestroyOnLoad = GetBoolArgument(provideAttribute, "DontDestroyOnLoad", true);
        string resourcesPath = GetStringArgument(provideAttribute, "ResourcesPath");

        string eventName = RegistrationEventName(registerOn);
        bool userDefinesEvent = classSymbol.GetMembers(eventName).OfType<IMethodSymbol>().Any();

        return new ProviderClassInfo(className, namespaceName, isPartial)
        {
            Kind = kind,
            MonoStrategy = monoStrategy,
            RegisterOn = registerOn,
            DontDestroyOnLoad = dontDestroyOnLoad,
            ResourcesPath = resourcesPath,
            EventName = eventName,
            UserDefinesEvent = userDefinesEvent
        };
    }

    private static ProviderKind GetProviderKind(INamedTypeSymbol classSymbol)
    {
        for (var baseType = classSymbol.BaseType; baseType != null; baseType = baseType.BaseType)
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

    private static string RegistrationEventName(int registerOn) => registerOn switch
    {
        1 => "OnEnable",
        2 => "Start",
        _ => "Awake"
    };

    private static int GetIntArgument(AttributeData attribute, string name, int defaultValue)
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

    private static bool GetBoolArgument(AttributeData attribute, string name, bool defaultValue)
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

    private static string GetStringArgument(AttributeData attribute, string name)
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
    
    private static void Execute(string assemblyName, ImmutableArray<ProviderClassInfo> fields, SourceProductionContext context)
        {
            if (fields.IsDefaultOrEmpty)
            {
                EmitReportForEmptyAssembly(assemblyName, context);
                return;
            }

            var groupedFields = fields.GroupBy(f => new { f.Namespace, f.ClassName, f.IsPartial });

            var generatedClasses = new List<string>();

            foreach (var group in groupedFields)
            {
                ProviderClassInfo info = group.First();

                // SO are handled by some Unity Editor code, so we don't generate code for them here.
                if (info.Kind == ProviderKind.ScriptableObject)
                {
                    continue;
                }

                // Security: if the user forgot "partial", we generate a Roslyn error
                if (!group.Key.IsPartial)
                {
                    var diagnostic = Diagnostic.Create(
                        new DiagnosticDescriptor("DI001", "Missing partial modifier", $"Class {group.Key.ClassName} must be 'partial' to use [Provide].", "DependencyInjection", DiagnosticSeverity.Error, true),
                        Location.None);
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }


                ICodeSource codeSource = CreateCodeText(info, context);
                if (codeSource == null)
                {
                    continue;
                }

                context.AddSource($"{group.Key.ClassName}_Provide.g.cs", SourceText.From(codeSource.GetCode(), Encoding.UTF8));

                string fullName = string.IsNullOrEmpty(group.Key.Namespace)
                    ? group.Key.ClassName
                    : $"{group.Key.Namespace}.{group.Key.ClassName}";
                generatedClasses.Add(fullName);
            }

            EmitReport(generatedClasses, context);
        }

    private static ICodeSource CreateCodeText(ProviderClassInfo info, SourceProductionContext context)
    {
        switch (info.Kind)
        {
            case ProviderKind.MonoBehaviour:
                if (info.MonoStrategy == 1)
                {
                    // Self-register: warn the user when their own lifecycle method needs to call XJectorRegister().
                    if (info.UserDefinesEvent)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor("DI003", "Forbidden lifecycle method override",
                                $"Class {info.ClassName} uses a [Provide] attribute specifying to perform the providing at '{info.EventName}' but already defines '{info.EventName}'. This is forbidden. Either remove your own '{info.EventName}' method, change the [Provide] attribute to use a different lifecycle event you don't already define, or derive from ProvidedMonoBehaviour.",
                                "DependencyInjection", DiagnosticSeverity.Error, true),
                            Location.None));

                        return null;
                    }

                    return new ProviderForMonoBehaviourSelfRegister(info.Namespace, info.ClassName, info.EventName, info.UserDefinesEvent);
                }

                return new ProviderForMonoBehaviourBootstrap(info.Namespace, info.ClassName, info.DontDestroyOnLoad);

            case ProviderKind.ScriptableObject:
                // if (string.IsNullOrEmpty(info.ResourcesPath))
                // {
                //     context.ReportDiagnostic(Diagnostic.Create(
                //         new DiagnosticDescriptor("DI002", "Missing Resources path",
                //             $"ScriptableObject {info.ClassName} must set [Provide(ResourcesPath = \"...\")] pointing to the asset to provide.",
                //             "DependencyInjection", DiagnosticSeverity.Error, true),
                //         Location.None));
                //     return null;
                // }
                //
                // return new ProviderForScriptableObject(info.Namespace, info.ClassName, info.ResourcesPath);

            default:
                return new ProviderForClassWithEmptyConstructor(info.Namespace, info.ClassName);
        }
    }
    
    private static void EmitReportForEmptyAssembly(string assemblyName, SourceProductionContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                "DI101",
                "XJector provider report",
                "No provider generated by XJector for assembly {0}",
                "DependencyInjection",
                DiagnosticSeverity.Warning,
                true),
            Location.None,
            assemblyName));
    }

        private static void EmitReport(List<string> generatedClasses, SourceProductionContext context)
        {
            if (generatedClasses.Count == 0) return;

            generatedClasses.Sort(System.StringComparer.Ordinal);

            // 1. Report visible in the build log (Unity Console window / MSBuild output)
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "DI100",
                    "XJector provider report",
                    "{0} generated provider(s) by XJector : {1}",
                    "DependencyInjection",
                    DiagnosticSeverity.Warning,
                    true),
                Location.None,
                generatedClasses.Count,
                string.Join(", ", generatedClasses)));
        }
}

public enum ProviderKind
{
    PlainClass,
    MonoBehaviour,
    ScriptableObject
}

public class ProviderClassInfo(string className, string @namespace, bool isPartial)
{
    public string ClassName { get; } = className;
    public string Namespace { get; } = @namespace;
    public bool IsPartial { get; } = isPartial;

    public ProviderKind Kind { get; set; } = ProviderKind.PlainClass;
    public int MonoStrategy { get; set; }
    public int RegisterOn { get; set; }
    public bool DontDestroyOnLoad { get; set; } = true;
    public string ResourcesPath { get; set; }
    public string EventName { get; set; } = "Awake";
    public bool UserDefinesEvent { get; set; }
}