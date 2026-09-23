using System;
using System.Collections.Generic;
using System.Linq;

namespace XJector;

/// <summary>
/// Assembly names to skip entirely during XJector generation, loaded from a JSON additional file
/// (see <see cref="ProviderSourceGenerator"/>). Expected content is a flat JSON array of assembly
/// names, e.g. ["MyAssembly.Editor", "MyAssembly.Tests"].
/// Implements value equality so the incremental generator pipeline can cache downstream results
/// and only recompute when the ignored-assemblies content actually changes.
/// </summary>
public sealed class IgnoredAssembliesSettings : IEquatable<IgnoredAssembliesSettings>
{
    public static readonly IgnoredAssembliesSettings Empty = new(Array.Empty<string>());

    private readonly HashSet<string> _ignoredAssemblies;

    private IgnoredAssembliesSettings(IEnumerable<string> ignoredAssemblies)
    {
        _ignoredAssemblies = new HashSet<string>(ignoredAssemblies, StringComparer.Ordinal);
    }

    public bool IsIgnored(string assemblyName) => assemblyName != null && _ignoredAssemblies.Contains(assemblyName);

    public static IgnoredAssembliesSettings Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Empty;
        }

        List<string> names = ExtractStringArray(json);
        return names.Count == 0 ? Empty : new IgnoredAssembliesSettings(names);
    }

    // Minimal, dependency-free parser: the generator assembly is loaded in isolation by Unity/csc,
    // so we avoid pulling in a JSON library just to read a flat array of assembly names.
    private static List<string> ExtractStringArray(string json)
    {
        var names = new List<string>();

        int start = json.IndexOf('[');
        int end = json.LastIndexOf(']');
        if (start < 0 || end < 0 || end <= start)
        {
            return names;
        }

        string content = json.Substring(start + 1, end - start - 1);
        int i = 0;
        while (i < content.Length)
        {
            if (content[i] == '"')
            {
                int valueStart = i + 1;
                int valueEnd = content.IndexOf('"', valueStart);
                if (valueEnd < 0)
                {
                    break;
                }

                string value = content.Substring(valueStart, valueEnd - valueStart);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    names.Add(value);
                }

                i = valueEnd + 1;
            }
            else
            {
                i++;
            }
        }

        return names;
    }

    public bool Equals(IgnoredAssembliesSettings other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _ignoredAssemblies.SetEquals(other._ignoredAssemblies);
    }

    public override bool Equals(object obj) => Equals(obj as IgnoredAssembliesSettings);

    public override int GetHashCode()
    {
        int hash = 17;
        foreach (string name in _ignoredAssemblies.OrderBy(n => n, StringComparer.Ordinal))
        {
            hash = hash * 31 + name.GetHashCode();
        }

        return hash;
    }
}
