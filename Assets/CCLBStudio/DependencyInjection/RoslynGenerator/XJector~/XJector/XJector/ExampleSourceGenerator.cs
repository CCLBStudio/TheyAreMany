using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace XJector;

[Generator]
public class ExampleSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        
    }

    public void Execute(GeneratorExecutionContext context)
    {
        MemoryStream sourceStream = new();
        StreamWriter sourceStreamWriter = new(sourceStream, Encoding.UTF8);
        IndentedTextWriter codeWriter = new(sourceStreamWriter);
        
        codeWriter.WriteLine("using System;");
        codeWriter.WriteLine("namespace XJector {");
        codeWriter.Indent++;
        
        codeWriter.WriteLine("public static class ExampleSourceGenerated {");
        codeWriter.Indent++;
        
        codeWriter.WriteLine("public static string GetTestText()");
        codeWriter.WriteLine("{");
        codeWriter.Indent++;
        
        codeWriter.WriteLine("return \"This is from source generator - Generated at build time\";");
        
        codeWriter.Indent--;
        codeWriter.WriteLine("}");
        
        codeWriter.Indent--;
        codeWriter.WriteLine("}");
        
        codeWriter.Indent--;
        codeWriter.WriteLine("}");
        
        sourceStreamWriter.Flush();
        
        context.AddSource("ExampleSourceGenerator.g.cs", SourceText.From(sourceStream, Encoding.UTF8, canBeEmbedded:true));
    }
}