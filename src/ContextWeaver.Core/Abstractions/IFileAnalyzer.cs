namespace ContextWeaver.Core;

/// <summary>
///     Abstracción: Information Hiding (Parnas, 1972).
///     Esta interfaz oculta las decisiones de implementación de cada analizador de archivos.
///     Si mañana se agrega un analizador de Python, solo se implementa esta interfaz
///     sin modificar la lógica de orquestación existente.
/// </summary>
public interface IFileAnalyzer
{
    bool CanAnalyze(FileInfo file);
    Task InitializeAsync(IEnumerable<FileInfo> files);
    Task<FileAnalysisResult> AnalyzeAsync(FileInfo file);
}
