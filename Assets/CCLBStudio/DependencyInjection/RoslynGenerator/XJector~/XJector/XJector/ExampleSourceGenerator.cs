using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.CodeDom.Compiler;
using System.IO;

namespace XJector;

//[Generator]
public class ExampleSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (compilation, _) => compilation.AssemblyName);
        context.RegisterSourceOutput(assemblyName, (spc, currentAssemblyName) =>
        {
            spc.AddSource("ExampleSourceGenerator.g.cs", GenerateSource());

            if (currentAssemblyName == "Assembly-CSharp")
            {
            }
        });
    }

    SourceText GenerateSource()
    {
        using var sourceStream = new StringWriter();
        using var codeWriter = new IndentedTextWriter(sourceStream);
        
        codeWriter.WriteLine("using System;");
        codeWriter.WriteLine("namespace XJector {");
        codeWriter.Indent++;

        codeWriter.WriteLine("public static class ExampleSourceGenerated {");
        codeWriter.Indent++;

        codeWriter.WriteLine("public static string GetTestText()");
        codeWriter.WriteLine("{");
        codeWriter.Indent++;

        codeWriter.WriteLine("return \"This is from incremental generator - Generated at build time\";");

        codeWriter.Indent--;
        codeWriter.WriteLine("}");

        codeWriter.Indent--;
        codeWriter.WriteLine("}");

        codeWriter.Indent--;
        codeWriter.WriteLine("}");
        
        return SourceText.From(sourceStream.ToString(), Encoding.UTF8);
    }
}