namespace ContextWeaver.Core;

/// <summary>
///     Representa una relación de dependencia entre dos tipos del proyecto.
///     Centraliza el parseo y la serialización de dependencias que antes se duplicaba
///     en CodeAnalyzerService, InstabilityCalculator, y MarkdownReportGenerator.
/// </summary>
public record DependencyRelation(string Source, string Target, DependencyKind Kind)
{
    /// <summary>
    ///     Parsea una cadena de dependencia en formato Mermaid ("Source --> Target" o "Source -.-> Target").
    ///     Retorna null si la cadena no tiene el formato esperado.
    /// </summary>
    /// <param name="raw">Cadena de texto cruda que representa la dependencia.</param>
    /// <returns>Una nueva instancia de <see cref="DependencyRelation"/> o null si el formato es inválido.</returns>
    public static DependencyRelation? Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var kind = raw.Contains("-.->") ? DependencyKind.Inheritance : DependencyKind.Usage;
        var separator = kind == DependencyKind.Inheritance ? "-.->" : "-->";
        var parts = raw.Split(separator,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            return null;

        return new DependencyRelation(parts[0], parts[1], kind);
    }

    /// <summary>Serializa la relación a formato Mermaid graph TD.</summary>
    /// <returns>Cadena formateada para Mermaid.</returns>
    public string ToMermaid() => Kind == DependencyKind.Inheritance
        ? $"{Source} -.-> {Target}"
        : $"{Source} --> {Target}";

    /// <summary>Serializa la relación a formato PlantUML.</summary>
    /// <returns>Cadena formateada para PlantUML.</returns>
    public string ToPlantUml() => Kind == DependencyKind.Inheritance
        ? $"{Source} ..> {Target}"
        : $"{Source} --> {Target}";
}

/// <summary>
///     Enumera los tipos de relaciones de dependencia soportados.
/// </summary>
public enum DependencyKind
{
    /// <summary>Relación de uso directo (e.g., campo, propiedad, parámetro).</summary>
    Usage,

    /// <summary>Relación de herencia o implementación de interfaz.</summary>
    Inheritance
}
