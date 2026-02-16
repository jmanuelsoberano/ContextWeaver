namespace ContextWeaver.Reporters;

/// <summary>
///     Shared helper for PlantUML/Mermaid diagram generation.
///     Used by <see cref="Sections.DependencyGraphSection"/>,
///     <see cref="Sections.ModuleDiagramSection"/>, and
///     <see cref="Sections.FileContentSection"/>.
/// </summary>
internal static class DiagramHelper
{
    /// <summary>
    ///     Returns the PlantUML keyword and stereotype for a type name,
    ///     using the Roslyn-derived <paramref name="typeKindMap"/> for accuracy.
    /// </summary>
    public static (string Keyword, string Stereotype) GetPlantUMLMeta(
        string typeName, Dictionary<string, string> typeKindMap)
    {
        if (typeKindMap.TryGetValue(typeName, out var kind))
        {
            return kind switch
            {
                "interface" => ("interface", string.Empty),
                "enum" => ("enum", string.Empty),
                "record" => ("class", "<<record>>"),
                "struct" => ("class", "<<struct>>"),
                _ => ("class", string.Empty)
            };
        }

        // Fallback: heuristic detection
        if (typeName.Length > 1 && typeName.StartsWith('I') && char.IsUpper(typeName[1]))
            return ("interface", string.Empty);

        return ("class", string.Empty);
    }

    /// <summary>
    ///     Returns true if the given word is a C# type keyword.
    /// </summary>
    public static bool IsTypeKeyword(string? keyword)
    {
        return keyword is "class" or "interface" or "struct" or "record" or "enum";
    }
}
