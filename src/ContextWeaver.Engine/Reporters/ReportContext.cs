using ContextWeaver.Core;

namespace ContextWeaver.Reporters;

/// <summary>
///     Contexto compartido pasado a cada <see cref="IReportSection"/>.
///     Construido una vez por el orquestador, evitando cálculos redundantes entre secciones.
/// </summary>
public record ReportContext(
    DirectoryInfo Directory,
    List<FileAnalysisResult> SortedResults,
    Dictionary<string, (int Ca, int Ce, double Instability)> InstabilityMetrics,
    Dictionary<string, string> TypeKindMap
);
