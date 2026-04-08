namespace ContextWeaver.Core;

/// <summary>
///     Data Transfer Object (DTO) que transporta los resultados de un análisis de archivo
///     entre las capas de la aplicación (analizadores → generadores de reportes).
///     Propiedades son <c>init</c>-only para prevenir mutación accidental post-construcción.
/// </summary>
public class FileAnalysisResult
{
    /// <summary>Gets or sets the relative path of the file from the root directory.</summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>Gets the total number of lines in the file.</summary>
    public int LinesOfCode { get; init; }

    /// <summary>Gets the full content of the file.</summary>
    public string CodeContent { get; init; } = string.Empty;

    /// <summary>Gets the detected language identifier (e.g. "csharp", "json").</summary>
    public string Language { get; init; } = "plaintext";

    /// <summary>Gets the specific metrics calculated for this file.</summary>
    public FileMetrics Metrics { get; init; } = new();

    /// <summary>Gets the list of namespaces imported via 'using' directives.</summary>
    public List<string> Usings { get; init; } = new();

    /// <summary>Gets the list of outgoing dependencies (e.g. "ThisClass --> OtherClass").</summary>
    public List<string> ClassDependencies { get; init; } = new();

    /// <summary>
    ///     Gets or sets the list of incoming dependencies (files that depend on this one).
    ///     Remains as <c>set</c> because it is populated in post-processing.
    /// </summary>
    public List<string> IncomingDependencies { get; set; } = new();

    /// <summary>Gets the list of type names defined in this file.</summary>
    public List<string> DefinedTypes { get; init; } = new();

    /// <summary>Gets a dictionary mapping type names to their kind (class, interface, etc.).</summary>
    public Dictionary<string, string> DefinedTypeKinds { get; init; } = new();

    /// <summary>Gets or sets the module name derived from the file path.</summary>
    public string ModuleName { get; set; } = "Root";

    /// <summary>Gets a dictionary mapping type names to their semantic details (modifiers, interfaces).</summary>
    public Dictionary<string, TypeSemantics> DefinedTypeSemantics { get; init; } = new();
}

/// <summary>
///     Encapsula detalles semánticos sobre un tipo definido.
/// </summary>
/// <param name="Modifiers">Lista de modificadores de acceso (public, static, etc.).</param>
/// <param name="Interfaces">Lista de interfaces implementadas o clases base.</param>
/// <param name="Attributes">Lista de atributos aplicados.</param>
public record TypeSemantics(
    List<string> Modifiers,
    List<string> Interfaces,
    List<string> Attributes
);
