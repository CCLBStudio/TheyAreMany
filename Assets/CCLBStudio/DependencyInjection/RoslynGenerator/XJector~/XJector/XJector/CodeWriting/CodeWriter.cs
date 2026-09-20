using System.CodeDom.Compiler;
using System.IO;

namespace XJector;

public class CodeWriter
{
    public int Indent {get => _textWriter.Indent; set => _textWriter.Indent = value;}
    private readonly IndentedTextWriter _textWriter;
    private bool _hasNameSpace = false;
    
    public CodeWriter()
    {
        var sourceStream = new StringWriter();
        _textWriter = new IndentedTextWriter(sourceStream);
    }

    public CodeWriter WriteLine(string line)
    {
        _textWriter.WriteLine(line);
        return this;
    }
    
    public CodeWriter OpenBlock()
    {
        _textWriter.WriteLine("{");
        _textWriter.Indent++;
        return this;
    }
    
    public CodeWriter CloseBlock()
    {
        _textWriter.Indent--;
        _textWriter.WriteLine("}");
        return this;
    }

    public CodeWriter InBlock(params string[] lines)
    {
        OpenBlock();
        foreach (var line in lines)
        {
            WriteLine(line);
        }
        CloseBlock();
        return this;
    }
    
    public CodeWriter InBlock(params IInstruction[] instructions)
    {
        OpenBlock();
        foreach (var instruction in instructions)
        {
            instruction.Write(this);
        }
        CloseBlock();
        return this;
    }
    
    public CodeWriter Comment(string comment)
    {
        _textWriter.WriteLine($"// {comment}");
        return this;
    }

    public CodeWriter Usings(params string[] nameSpaces)
    {
        foreach (var nameSpace in nameSpaces)
        {
            _textWriter.WriteLine($"using {nameSpace};");
        }
        
        return this;
    }

    public CodeWriter OpenNamespace(string nameSpace)
    {
        if (nameSpace == string.Empty)
        {
            _hasNameSpace = false;
            return this;
        }

        _hasNameSpace = true;
        _textWriter.WriteLine($"namespace {nameSpace}");
        _textWriter.WriteLine("{");
        _textWriter.Indent++;
        return this;
    }
    
    public CodeWriter CloseNamespace()
    {
        if (!_hasNameSpace)
        {
            return this;
        }
        
        _textWriter.Indent--;
        _textWriter.WriteLine("}");
        return this;
    }
    
    public ClassDeclaration OpenClass(string className)
    {
        var classDeclaration = new ClassDeclaration(className, this);
        return classDeclaration;
    }
    
    public CodeWriter CloseClass()
    {
        return CloseBlock();
    }
    
    public MethodWriter Method(string methodName)
    {
        var methodWriter = new MethodWriter(methodName, this);
        return methodWriter;
    }
    
    public CodeWriter Attributes(params string[] attributes)
    {
        foreach (var attribute in attributes)
        {
            _textWriter.WriteLine($"[{attribute}]");
        }
        
        return this;
    }
    
    public string Build()
    {
        _textWriter.Flush();
        var result = _textWriter.InnerWriter.ToString();
        _textWriter.Close();
        return result;
    }
}