namespace XJector;

public class ClassDeclaration(string className, CodeWriter codeWriter)
{
    private string _className = className;
    private CodeWriter _codeWriter = codeWriter;
    private string _accessModifier = "public";
    private bool _isStatic;
    private bool _isPartial;
    private string _baseClass;
    private string[] _interfaces;
    private bool _built = false;
    
    public ClassDeclaration Public()
    {
        _accessModifier = "public";
        return this;
    }
    
    public ClassDeclaration Private()
    {
        _accessModifier = "private";
        return this;
    }
    
    public ClassDeclaration Protected()
    {
        _accessModifier = "protected";
        return this;
    }

    public ClassDeclaration Static(bool isStatic = true)
    {
        _isStatic = isStatic;
        return this;
    }
    
    public ClassDeclaration Partial(bool isPartial = true)
    {
        _isPartial = isPartial;
        return this;
    }
    
    public ClassDeclaration BaseClass(string baseClass)
    {
        _baseClass = baseClass;
        return this;
    }
    
    public ClassDeclaration Interfaces(params string[] interfaces)
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