using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using XJector.Extensions;

namespace XJector;

[Generator]
public class ProviderSourceGenerator : IIncrementalGenerator
{
    private const string IgnoredAssembliesFileSuffix = ".XJector.additionalfile";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s.IsClassWithProvideAttribute(),
                transform: static (ctx, _) => ctx.GetSemanticTargetForGeneration())
            .Where(static m => m != null);

        var ignoredAssemblies = context.AdditionalTextsProvider
            .Where(static file => IsIgnoredAssembliesFile(file.Path))
            .Select(static (file, ct) => file.GetText(ct)?.ToString())
            .Collect()
            .Select(static (texts, _) => IgnoredAssembliesSettings.Parse(texts.Length > 0 ? texts[0] : null));

        var compilationAndSettings = context.CompilationProvider.Combine(ignoredAssemblies);

        var combined = compilationAndSettings.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            // Bail out before any grouping/diagnostics/code-generation work: this is the
            // earliest point at which the assembly identity is known in the pipeline.
            (Compilation compilation, IgnoredAssembliesSettings settings) = source.Left;
            if (settings.IsIgnored(compilation.AssemblyName))
            {
                return;
            }

            Execute(compilation, source.Right, spc);
        });
    }

    private static bool IsIgnoredAssembliesFile(string path) =>
        path != null && path.EndsWith(IgnoredAssembliesFileSuffix, System.StringComparison.OrdinalIgnoreCase);
    
    private static void Execute(Compilation compilation, ImmutableArray<ProviderClassInfo> fields, SourceProductionContext context)
        {
            string assemblyName = compilation.AssemblyName;
            
            Location reportLocation = compilation.SyntaxTrees.FirstOrDefault() is { } tree
                ? Location.Create(tree, new TextSpan(0, 0))
                : Location.None;

            if (fields.IsDefaultOrEmpty)
            {
                context.EmitEmptyAssemblyReport(assemblyName, reportLocation);
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

            context.EmitGeneratedProvidersReport(generatedClasses, reportLocation);
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
                return null;

            default:
                return new ProviderForClassWithEmptyConstructor(info.Namespace, info.ClassName);
        }
    }
}