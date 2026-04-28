using System.Reflection;
using System.Collections.Generic;

namespace WaylandClipboard;

public static class DiscoveryTools
{
    public static readonly string[] AssemblyPrefixes =
    {
        "FrooxEngine",
        "Elements."
    };

    public static readonly string[] Keywords =
    {
        "Inspector",
        "Font",
        "FontChain",
        "Text",
        "Style",
        "Theme",
        "Label"
    };

    public static int CountKeywordMatches(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        var count = 0;
        foreach (var keyword in Keywords)
        {
            if (value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                count++;
        }
        return count;
    }

    public static bool IsInterestingAssembly(Assembly asm)
    {
        var name = asm.GetName().Name;
        if (string.IsNullOrEmpty(name))
            return false;

        return AssemblyPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    public static int ScoreType(Type type)
    {
        var fullName = type.FullName ?? type.Name;
        var score = CountKeywordMatches(fullName);
        return score;
    }

    public static int ScoreMethod(MethodInfo method)
    {
        var score = CountKeywordMatches(method.Name);
        if (method.GetParameters().Any(p => CountKeywordMatches(p.ParameterType.Name) > 0))
            score += 1;
        if (method.ReturnType != null && CountKeywordMatches(method.ReturnType.Name) > 0)
            score += 1;
        return score;
    }
}
