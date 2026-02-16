using ContextWeaver.Core;

namespace ContextWeaver.Reporters;

/// <summary>
///     Shared context passed to every <see cref="IReportSection"/>.
///     Built once by the orchestrator, avoiding redundant computation across sections.
/// </summary>
public record ReportContext(
    DirectoryInfo Directory,
    List<FileAnalysisResult> SortedResults,
    Dictionary<string, (int Ca, int Ce, double Instability)> InstabilityMetrics,
    Dictionary<string, string> TypeKindMap
);
