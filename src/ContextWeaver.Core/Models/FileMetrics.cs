namespace ContextWeaver.Core;

/// <summary>
///     Métricas tipadas para el resultado del análisis de un archivo.
///     Las propiedades son <c>init</c>-only para asegurar la inmutabilidad tras la construcción.
/// </summary>
public class FileMetrics
{
    /// <summary>Obtiene la complejidad ciclomática del archivo (solo C#).</summary>
    public int? CyclomaticComplexity { get; init; }

    /// <summary>Obtiene la profundidad máxima de anidamiento encontrada en el archivo (solo C#).</summary>
    public int? MaxNestingDepth { get; init; }

    /// <summary>Obtiene las firmas de API públicas extraídas por Roslyn (solo C#).</summary>
    public List<string> PublicApiSignatures { get; init; } = new();
}
