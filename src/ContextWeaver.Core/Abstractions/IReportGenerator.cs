namespace ContextWeaver.Core;

/// <summary>
///     Abstracción: Information Hiding (Parnas, 1972).
///     Esta interfaz oculta las decisiones de formato de reporte.
///     Para agregar un nuevo formato (XML, YAML, etc.), se implementa esta interfaz
///     sin alterar el servicio de análisis existente.
/// </summary>
public interface IReportGenerator
{
    string Format { get; }

    string Generate(DirectoryInfo directory, List<FileAnalysisResult> results,
        Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics);
}
