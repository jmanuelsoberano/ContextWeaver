namespace ContextWeaver.Core;

/// <summary>
///     Typed metrics for a file analysis result.
///     Properties are <c>init</c>-only for immutability after construction.
/// </summary>
public class FileMetrics
{
    /// <summary>Gets obtiene la complejidad ciclomática del archivo (solo C#).</summary>
    public int? CyclomaticComplexity { get; init; }

    /// <summary>Gets obtiene la profundidad máxima de anidamiento encontrada en el archivo (solo C#).</summary>
    public int? MaxNestingDepth { get; init; }

    /// <summary>Gets obtiene las firmas de API públicas extraídas por Roslyn (solo C#).</summary>
    public List<string> PublicApiSignatures { get; init; } = new();
}
