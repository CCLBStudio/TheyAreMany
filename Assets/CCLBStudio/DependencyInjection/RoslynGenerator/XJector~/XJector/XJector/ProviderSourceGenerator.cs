using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XJector;

[Generator]
public class ProviderSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsProvidedMethod(node),
                transform: static (context, _) => context.Node as MethodDeclarationSyntax
                )
            .Where(static node => node != null);
        
        context.RegisterSourceOutput(syntaxProvider, (ctx, methodDeclaration) =>
        {
            if (IsProvidedMethod(methodDeclaration))
            {
                var d = Diagnostic.Create(
                    ProviderDiagnostic,
                    methodDeclaration.GetLocation(),
                    methodDeclaration.Identifier.Text
                    );
                ctx.ReportDiagnostic(d);
            }
        });
    }
    
    private static readonly DiagnosticDescriptor ProviderDiagnostic = new(
        id: "XJ001",
        title: "Provided Method Found",
        messageFormat: "Method '{0}' is marked with [Provide] attribute",
        category: "Debug",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static bool IsProvidedMethod(SyntaxNode node)
    {
        if(node is not MethodDeclarationSyntax methodDeclaration)
            return false;
        
        return methodDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "Provide");
    }
}