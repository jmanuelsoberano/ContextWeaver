namespace ContextWeaver.Core;

/// <summary>
///     Abstracción: Information Hiding (Parnas, 1972).
///     Esta interfaz oculta las decisiones de formato de reporte.
///     Para agregar un nuevo formato (XML, YAML, etc.), se implementa esta interfaz
///     sin alterar el servicio de análisis existente.
/// </summary>
public interface IReportGenerator
{
    /// <summary>Gets the format identifier for this generator (e.g. "markdown").</summary>
    string Format { get; }

    /// <summary>
    ///     Genera el contenido del reporte basado en los resultados del análisis.
    /// </summary>
    /// <param name="directory">El directorio raíz que fue analizado.</param>
    /// <param name="results">La lista de resultados del análisis para cada archivo.</param>
    /// <param name="instabilityMetrics">Métricas de inestabilidad calculadas para los módulos.</param>
    /// <returns>El contenido del reporte generado como una cadena de texto.</returns>
    string Generate(DirectoryInfo directory, List<FileAnalysisResult> results,
        Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics);
}
