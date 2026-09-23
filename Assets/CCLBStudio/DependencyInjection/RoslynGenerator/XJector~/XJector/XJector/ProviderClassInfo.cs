namespace XJector;

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