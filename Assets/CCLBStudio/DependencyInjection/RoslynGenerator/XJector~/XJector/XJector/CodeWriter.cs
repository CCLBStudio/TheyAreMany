using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace XJector;

public class CodeWriter
{
    public int Indent {get => _textWriter.Indent; set => _textWriter.Indent = value;}
    private readonly IndentedTextWriter _textWriter;
    public enum AccessModifier { Public, Private, Protected, Internal }
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
    
    public CodeWriter WriteComment(string comment)
    {
        _textWriter.WriteLine($"// {comment}");
        return this;
    }

    public CodeWriter WriteUsings(params string[] nameSpaces)
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
    
    public MethodWriter WriteMethod(string methodName)
    {
        var methodWriter = new MethodWriter(methodName, this);
        return methodWriter;
    }
    
    public CodeWriter WriteAttributes(params string[] attributes)
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

public class ClassDeclaration(string className, CodeWriter codeWriter)
{
    private string _className = className;
    private CodeWriter _codeWriter = codeWriter;
    private string _accessModifier;
    private bool _isStatic;
    private bool _isPartial;
    private string _baseClass;
    private string[] _interfaces;
    private bool _built = false;

    public ClassDeclaration WithAccessModifier(CodeWriter.AccessModifier accessModifier)
    {
        _accessModifier = accessModifier.ToString().ToLower();
        return this;
    }

    public ClassDeclaration WithStatic(bool isStatic)
    {
        _isStatic = isStatic;
        return this;
    }
    
    public ClassDeclaration WithPartial(bool isPartial)
    {
        _isPartial = isPartial;
        return this;
    }
    
    public ClassDeclaration WithBaseClass(string baseClass)
    {
        _baseClass = baseClass;
        return this;
    }
    
    public ClassDeclaration WithInterfaces(params string[] interfaces)
    {
        _interfaces = interfaces;
        return this;
    }
    
    public CodeWriter Build()
    {
        if (_built) return _codeWriter;
        _built = true;
        
        var staticModifier = _isStatic ? "static " : "";
        string partialModifier = _isPartial ? "partial " : "";
        var baseClassDeclaration = string.IsNullOrEmpty(_baseClass) ? "" : $" : {_baseClass}";
        string interfaceStart = string.IsNullOrEmpty(_baseClass) ? " : " : ", ";
        var interfacesDeclaration = _interfaces != null && _interfaces.Length > 0 
            ? $"{interfaceStart}{string.Join(", ", _interfaces)}" : "";

        _codeWriter.WriteLine($"{_accessModifier} {staticModifier}{partialModifier}class {_className}{baseClassDeclaration}{interfacesDeclaration}");
        _codeWriter.WriteLine("{");
        _codeWriter.Indent++;
        return _codeWriter;
    }
}

public class MethodWriter(string methodName, CodeWriter codeWriter)
{
    private string _methodName = methodName;
    private CodeWriter _codeWriter = codeWriter;
    private string _accessModifier = "public";
    private bool _isStatic;
    private string _returnType = "void";
    private string[] _attributes;
    private string[] _parameters;
    private List<string> _instructions = new List<string>();
    private bool _built = false;

    public MethodWriter WithAccessModifier(CodeWriter.AccessModifier accessModifier)
    {
        _accessModifier = accessModifier.ToString().ToLower();
        return this;
    }

    public MethodWriter WithStatic(bool isStatic)
    {
        _isStatic = isStatic;
        return this;
    }

    public MethodWriter WithReturnType(string returnType)
    {
        _returnType = returnType;
        return this;
    }

    public MethodWriter WithAttributes(params string[] attributes)
    {
        _attributes = attributes;
        return this;
    }

    public MethodWriter WithInstruction(string instruction)
    {
        if(!instruction.EndsWith(";"))
        {
            instruction += ";";
        }
        _instructions.Add(instruction);
        return this;
    }
    
    public MethodWriter WithInstructions(params string[] instructions)
    {
        for(int i = 0; i < instructions.Length; i++)
        {
            if(!instructions[i].EndsWith(";"))
            {
                instructions[i] += ";";
            }
            _instructions.Add(instructions[i]);
        }
        return this;
    }

    public MethodWriter WithParameters(params string[] parameters)
    {
        _parameters = parameters;
        return this;
    }

    public CodeWriter Build()
    {
        if (_built) return _codeWriter;
        _built = true;

        if (_attributes != null)
        {
            _codeWriter.WriteAttributes(_attributes);
        }

        var staticModifier = _isStatic ? "static " : "";
        var parameters = _parameters != null ? string.Join(", ", _parameters) : "";

        _codeWriter.WriteLine($"{_accessModifier} {staticModifier}{_returnType} {_methodName}({parameters})");
        _codeWriter.WriteLine("{");
        _codeWriter.Indent++;
        foreach (var instruction in _instructions)
        {
            _codeWriter.WriteLine(instruction);
        }
        _codeWriter.Indent--;
        _codeWriter.WriteLine("}");
        return _codeWriter;
    }
}