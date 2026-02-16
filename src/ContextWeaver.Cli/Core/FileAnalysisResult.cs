namespace ContextWeaver.Core;

/// <summary>
///     BUENA PRÁCTICA: Data Transfer Object (DTO).
///     Esta clase es un POCO (Plain Old CLR Object) cuya única responsabilidad es transportar datos
///     entre las capas de la aplicación (desde los analizadores hasta los generadores de reportes).
///     PRINCIPIO DE DISEÑO: ALTA COHESIÓN.
///     La clase solo contiene datos relacionados con el resultado de un análisis de archivo.
///     No tiene lógica de negocio, lo que la hace cohesiva y fácil de entender.
/// </summary>
public class FileAnalysisResult
{
    public string RelativePath { get; set; } = string.Empty;
    public int LinesOfCode { get; set; }
    public string CodeContent { get; set; } = string.Empty;
    public string Language { get; set; } = "plaintext";
    public FileMetrics Metrics { get; set; } = new();

    // Nueva propiedad para los Usings, para tipado fuerte y fácil acceso.
    public List<string> Usings { get; set; } = new();

    // ✅ NUEVA PROPIEDAD: Para almacenar las dependencias de clase.
    // Guardaremos las relaciones en formato "ClaseOrigen -> ClaseDestino".
    public List<string> ClassDependencies { get; set; } = new();

    // ✅ NUEVA PROPIEDAD: Dependencias entrantes (Quién me usa).
    // Se llena en el post-procesamiento.
    public List<string> IncomingDependencies { get; set; } = new();

    // ✅ NUEVA PROPIEDAD: Tipos definidos en este archivo.
    public List<string> DefinedTypes { get; set; } = new();

    // ✅ NUEVA PROPIEDAD: Tipo de dato (class, interface, struct, record, enum).
    // Key: Nombre del tipo, Value: Kind.
    public Dictionary<string, string> DefinedTypeKinds { get; set; } = new();
    // ✅ NUEVA PROPIEDAD COMPUTADA: Centraliza la lógica de nombre de módulo.
    public string ModuleName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RelativePath)) return "Root";
            var parts = RelativePath.Replace('\\', '/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[0] : "Root";
        }
    }


    // ✅ NUEVA PROPIEDAD: Semántica enriquecida para Taxonomía (Modificadores, Interfaces, Atributos).
    public Dictionary<string, TypeSemantics> DefinedTypeSemantics { get; set; } = new();
}

public record TypeSemantics(
    List<string> Modifiers,
    List<string> Interfaces,
    List<string> Attributes
);