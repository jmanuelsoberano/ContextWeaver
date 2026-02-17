namespace ContextWeaver.Core;

/// <summary>
///     Métricas tipadas para el resultado del análisis de un archivo.
///     Las propiedades son <c>init</c>-only para asegurar la inmutabilidad tras la construcción.
/// </summary>
public class FileMetrics
{
    /// <summary>Gets the cyclomatic complexity of the file (C# only).</summary>
    public int? CyclomaticComplexity { get; init; }

    /// <summary>Gets the maximum nesting depth found in the file (C# only).</summary>
    public int? MaxNestingDepth { get; init; }

    /// <summary>Gets the public API signatures extracted by Roslyn (C# only).</summary>
    public List<string> PublicApiSignatures { get; init; } = new();
}
