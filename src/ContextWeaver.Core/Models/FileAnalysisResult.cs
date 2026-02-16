namespace ContextWeaver.Core;

/// <summary>
///     Data Transfer Object (DTO) que transporta los resultados de un análisis de archivo
///     entre las capas de la aplicación (analizadores → generadores de reportes).
///     Propiedades son <c>init</c>-only para prevenir mutación accidental post-construcción.
/// </summary>
public class FileAnalysisResult
{
    public string RelativePath { get; set; } = string.Empty;
    public int LinesOfCode { get; init; }
    public string CodeContent { get; init; } = string.Empty;
    public string Language { get; init; } = "plaintext";
    public FileMetrics Metrics { get; init; } = new();

    public List<string> Usings { get; init; } = new();
    public List<string> ClassDependencies { get; init; } = new();

    /// <summary>
    ///     Dependencias entrantes (quién me usa).
    ///     Permanece como <c>set</c> porque se llena en post-procesamiento
    ///     después de construir el objeto (en <c>CodeAnalyzerService</c>).
    /// </summary>
    public List<string> IncomingDependencies { get; set; } = new();

    public List<string> DefinedTypes { get; init; } = new();
    public Dictionary<string, string> DefinedTypeKinds { get; init; } = new();

    /// <summary>Centraliza la lógica de nombre de módulo.</summary>
    public string ModuleName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RelativePath))
                return "Root";
            var parts = RelativePath.Replace('\\', '/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[0] : "Root";
        }
    }

    public Dictionary<string, TypeSemantics> DefinedTypeSemantics { get; init; } = new();
}

public record TypeSemantics(
    List<string> Modifiers,
    List<string> Interfaces,
    List<string> Attributes
);
