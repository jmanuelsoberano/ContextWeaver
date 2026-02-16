namespace ContextWeaver.Reporters;

/// <summary>
///     Helper compartido para la generación de diagramas PlantUML/Mermaid.
///     Utilizado por <see cref="Sections.DependencyGraphSection"/>,
///     <see cref="Sections.ModuleDiagramSection"/>, y
///     <see cref="Sections.FileContentSection"/>.
/// </summary>
internal static class DiagramHelper
{
    /// <summary>
    ///     Devuelve la palabra clave y estereotipo de PlantUML para un nombre de tipo,
    ///     usando <paramref name="typeKindMap"/> derivado de Roslyn para mayor precisión.
    /// </summary>
    /// <param name="typeName">Nombre del tipo a verificar.</param>
    /// <param name="typeKindMap">Diccionario mapeando nombres de tipos a sus clases (class, interface, etc.).</param>
    /// <returns>Una tupla conteniendo la palabra clave y el estereotipo de PlantUML.</returns>
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

        // Fallback: detección heurística
        if (typeName.Length > 1 && typeName.StartsWith('I') && char.IsUpper(typeName[1]))
            return ("interface", string.Empty);

        return ("class", string.Empty);
    }

    /// <summary>
    ///     Devuelve true si la palabra dada es una palabra clave de tipo en C#.
    /// </summary>
    /// <param name="keyword">La palabra a verificar.</param>
    /// <returns>True si la palabra es una palabra clave de tipo; de lo contrario, false.</returns>
    public static bool IsTypeKeyword(string? keyword)
    {
        return keyword is "class" or "interface" or "struct" or "record" or "enum";
    }
}
