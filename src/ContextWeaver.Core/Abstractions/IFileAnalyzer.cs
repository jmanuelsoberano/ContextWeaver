namespace ContextWeaver.Core;

/// <summary>
///     Abstracción: Information Hiding (Parnas, 1972).
///     Esta interfaz oculta las decisiones de implementación de cada analizador de archivos.
///     Si mañana se agrega un analizador de Python, solo se implementa esta interfaz
///     sin modificar la lógica de orquestación existente.
/// </summary>
public interface IFileAnalyzer
{
    /// <summary>
    ///     Determina si este analizador puede procesar el archivo dado.
    /// </summary>
    /// <param name="file">Archivo a evaluar.</param>
    /// <returns>True si el archivo es soportado; de lo contrario, False.</returns>
    bool CanAnalyze(FileInfo file);

    /// <summary>
    ///     Realiza una pasada de inicialización sobre todos los archivos antes del análisis individual.
    ///     Útil para construir índices o mapas globales.
    /// </summary>
    /// <param name="files">Colección completa de archivos a analizar.</param>
    /// <returns>Una task que representa la inicialización asíncrona.</returns>
    Task InitializeAsync(IEnumerable<FileInfo> files);

    /// <summary>
    ///     Analiza un archivo individual y extrae métricas y metadatos.
    /// </summary>
    /// <param name="file">Archivo a analizar.</param>
    /// <returns>Resultado del análisis del archivo.</returns>
    Task<FileAnalysisResult> AnalyzeAsync(FileInfo file);
}
