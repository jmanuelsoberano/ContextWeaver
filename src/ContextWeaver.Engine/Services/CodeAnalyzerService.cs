using ContextWeaver.Core;
using ContextWeaver.Utilities;
using Microsoft.Extensions.Logging;

namespace ContextWeaver.Services;

/// <summary>
///     Coordina el proceso de análisis de código.
///     Orquesta la carga de configuración, descubrimiento de archivos, ejecución de analizadores
///     y generación de reportes.
/// </summary>
public class CodeAnalyzerService
{
    private readonly IEnumerable<IFileAnalyzer> _analyzers;
    private readonly IEnumerable<IReportGenerator> _generators;
    private readonly ILogger<CodeAnalyzerService> _logger;
    private readonly SettingsProvider _settingsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CodeAnalyzerService"/> class.
    /// </summary>
    /// <param name="settingsProvider">Proveedor de configuración.</param>
    /// <param name="analyzers">Colección de analizadores de archivos disponibles.</param>
    /// <param name="generators">Colección de generadores de reportes disponibles.</param>
    /// <param name="logger">Logger para diagnósticos.</param>
    public CodeAnalyzerService(
        SettingsProvider settingsProvider,
        IEnumerable<IFileAnalyzer> analyzers,
        IEnumerable<IReportGenerator> generators,
        ILogger<CodeAnalyzerService> logger)
    {
        _settingsProvider = settingsProvider;
        _analyzers = analyzers;
        _generators = generators;
        _logger = logger;
    }

    /// <summary>
    ///     Ejecuta el pipeline completo de análisis y generación de reportes.
    /// </summary>
    /// <param name="directory">Directorio raíz a analizar.</param>
    /// <param name="outputFile">Archivo donde se escribirá el reporte.</param>
    /// <param name="format">Formato de salida deseado (ej. "markdown").</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AnalyzeAndGenerateReport(DirectoryInfo directory, FileInfo outputFile, string format)
    {
        var generator = _generators.FirstOrDefault(g => g.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
        if (generator == null)
        {
            _logger.LogError("El formato de salida '{Format}' no es soportado.", format);
            return;
        }

        // 1. Cargar configuración (responsabilidad delegada)
        var settings = _settingsProvider.LoadSettingsFor(directory);

        // 2. Encontrar y filtrar archivos
        var allFiles = directory.GetFiles("*.*", SearchOption.AllDirectories)
            .Where(f => !settings.ExcludePatterns.Any(p =>
                f.FullName.Contains(Path.DirectorySeparatorChar + p + Path.DirectorySeparatorChar)))
            .Where(f => settings.IncludedExtensions.Contains(f.Extension.ToLowerInvariant()))
            .ToList();

        // 3. Inicializar Analizadores (Pre-carga de contexto global)
        foreach (var analyzer in _analyzers)
        {
            await analyzer.InitializeAsync(allFiles);
        }

        // 4. Analizar archivos (Paralelizado)
        var analysisResults = new System.Collections.Concurrent.ConcurrentBag<FileAnalysisResult>();

        await Parallel.ForEachAsync(allFiles, async (file, ct) =>
        {
            var analyzer = _analyzers.FirstOrDefault(a => a.CanAnalyze(file));
            if (analyzer != null)
            {
                var result = await analyzer.AnalyzeAsync(file);
                result.RelativePath = file.FullName.Replace(directory.FullName, string.Empty)
                    .Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
                analysisResults.Add(result);
            }
        });

        // 4b. Post-procesamiento: Calcular Dependencias Entrantes (Reverse Index)
        var resultsList = analysisResults.ToList();
        var typeToFileMap = new Dictionary<string, FileAnalysisResult>();

        // Construir mapa Tipo -> Archivo
        foreach (var result in resultsList)
        {
            if (result.DefinedTypes != null)
            {
                foreach (var type in result.DefinedTypes)
                {
                    if (!typeToFileMap.ContainsKey(type))
                    {
                        typeToFileMap[type] = result;
                    }
                }
            }
        }

        // Llenar IncomingDependencies
        foreach (var result in resultsList)
        {
            if (result.ClassDependencies != null)
            {
                foreach (var dep in result.ClassDependencies)
                {
                    var relation = DependencyRelation.Parse(dep);
                    if (relation == null)
                        continue;

                    if (typeToFileMap.TryGetValue(relation.Target, out var targetFile))
                    {
                        if (!targetFile.IncomingDependencies.Contains(relation.Source))
                        {
                            targetFile.IncomingDependencies.Add(relation.Source);
                        }
                    }
                }
            }
        }

        // 5. Calcular inestabilidad (responsabilidad delegada)
        var instabilityMetrics = InstabilityCalculator.Calculate(resultsList);

        // 6. Generar y escribir el reporte
        var reportContent = generator.Generate(directory, resultsList, instabilityMetrics);
        await File.WriteAllTextAsync(outputFile.FullName, reportContent);

        _logger.LogInformation("Reporte en formato '{Format}' generado exitosamente en: {OutputPath}", format, outputFile.FullName);
    }
}
