using System.Collections.Generic;
using System.Linq;

namespace XJector;

public class MethodWriter(string methodName, CodeWriter codeWriter)
{
    private string _accessModifier = "public";
    private bool _isStatic;
    private string _returnType = "void";
    private readonly List<string> _attributes = new();
    private string[] _parameters;
    //private readonly List<string> _instructions = new();
    private readonly List<IInstruction> _instructions = new();
    private bool _built;
    
    public MethodWriter Public()
    {
        _accessModifier = "public";
        return this;
    }

    public MethodWriter Private()
    {
        _accessModifier = "private";
        return this;
    }
    
    public MethodWriter Protected()
    {
        _accessModifier = "protected";
        return this;
    }
    
    public MethodWriter Static(bool isStatic = true)
    {
        _isStatic = isStatic;
        return this;
    }

    public MethodWriter ReturnType(string returnType)
    {
        _returnType = returnType;
        return this;
    }
    
    public MethodWriter Attribute(string attribute)
    {
        _attributes.Add(attribute);
        return this;
    }

    public MethodWriter Attributes(params string[] attributes)
    {
        _attributes.AddRange(attributes);
        return this;
    }

    public MethodWriter Instruction(string instruction)
    {
        _instructions.Add(new OneLineInstruction(instruction));
        return this;
    }
    
    public MethodWriter If(string condition, string[] instructions)
    {
        var instructionObjects = new IInstruction[instructions.Length];
        for (int i = 0; i < instructions.Length; i++)
        {
            instructionObjects[i] = new OneLineInstruction(instructions[i]);
        }
        
        _instructions.Add(new IfInstruction(condition, instructionObjects));
        return this;
    }

    public MethodWriter Instructions(params string[] instructions)
    {
        foreach (var i in instructions)
        {
            _instructions.Add(new OneLineInstruction(i));
        }

        return this;
    }

    public MethodWriter Parameters(params string[] parameters)
    {
        _parameters = parameters;
        return this;
    }

    public CodeWriter Build()
    {
        if (_built) return codeWriter;
        _built = true;

        if (_attributes != null)
        {
            codeWriter.Attributes(_attributes.ToArray());
        }

        var staticModifier = _isStatic ? "static " : "";
        var parameters = _parameters != null ? string.Join(", ", _parameters) : "";

        codeWriter.WriteLine($"{_accessModifier} {staticModifier}{_returnType} {methodName}({parameters})");
        codeWriter.WriteLine("{");
        codeWriter.Indent++;
        foreach (var instruction in _instructions)
        {
            instruction.Write(codeWriter);
        }
        codeWriter.Indent--;
        codeWriter.WriteLine("}");
        return codeWriter;
    }
}