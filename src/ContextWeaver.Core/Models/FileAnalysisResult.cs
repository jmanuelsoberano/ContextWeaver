namespace ContextWeaver.Core;

/// <summary>
///     Data Transfer Object (DTO) que transporta los resultados de un análisis de archivo
///     entre las capas de la aplicación (analizadores → generadores de reportes).
///     Propiedades son <c>init</c>-only para prevenir mutación accidental post-construcción.
/// </summary>
public class FileAnalysisResult
{
    /// <summary>Gets or sets obtiene o establece la ruta relativa del archivo desde el directorio raíz.</summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>Gets obtiene el número total de líneas en el archivo.</summary>
    public int LinesOfCode { get; init; }

    /// <summary>Gets obtiene el contenido completo del archivo.</summary>
    public string CodeContent { get; init; } = string.Empty;

    /// <summary>Gets obtiene el identificador de lenguaje detectado (ej. "csharp", "json").</summary>
    public string Language { get; init; } = "plaintext";

    /// <summary>Gets obtiene las métricas específicas calculadas para este archivo.</summary>
    public FileMetrics Metrics { get; init; } = new();

    /// <summary>Gets obtiene la lista de namespaces importados mediante directivas 'using'.</summary>
    public List<string> Usings { get; init; } = new();

    /// <summary>Gets obtiene la lista de dependencias salientes (ej. "EstaClase --> OtraClase").</summary>
    public List<string> ClassDependencies { get; init; } = new();

    /// <summary>
    ///     Gets or sets obtiene o establece la lista de dependencias entrantes (archivos que dependen de este).
    ///     Permanece como <c>set</c> porque se llena en post-procesamiento.
    /// </summary>
    public List<string> IncomingDependencies { get; set; } = new();

    /// <summary>Gets obtiene la lista de nombres de tipos definidos en este archivo.</summary>
    public List<string> DefinedTypes { get; init; } = new();

    /// <summary>Gets obtiene un diccionario mapeando nombres de tipos a su clase (class, interface, etc.).</summary>
    public Dictionary<string, string> DefinedTypeKinds { get; init; } = new();

    /// <summary>Gets obtiene el nombre del módulo derivado de la ruta del archivo.</summary>
    public string ModuleName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RelativePath))
                return "Root";
            var parts = RelativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[0] : "Root";
        }
    }

    /// <summary>Gets obtiene un diccionario mapeando nombres de tipos a sus detalles semánticos (modificadores, interfaces).</summary>
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
