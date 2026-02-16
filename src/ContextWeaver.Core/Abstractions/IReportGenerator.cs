namespace ContextWeaver.Core;

/// <summary>
///     Abstracción: Information Hiding (Parnas, 1972).
///     Esta interfaz oculta las decisiones de formato de reporte.
///     Para agregar un nuevo formato (XML, YAML, etc.), se implementa esta interfaz
///     sin alterar el servicio de análisis existente.
/// </summary>
public interface IReportGenerator
{
    /// <summary>Gets the format identifier for this generator (e.g., "markdown").</summary>
    string Format { get; }

    /// <summary>
    ///     Generates the report content based on the analysis results.
    /// </summary>
    /// <param name="directory">The root directory that was analyzed.</param>
    /// <param name="results">The list of analysis results for each file.</param>
    /// <param name="instabilityMetrics">Calculated instability metrics for modules.</param>
    /// <returns>The generated report content as a string.</returns>
    string Generate(DirectoryInfo directory, List<FileAnalysisResult> results,
        Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics);
}
