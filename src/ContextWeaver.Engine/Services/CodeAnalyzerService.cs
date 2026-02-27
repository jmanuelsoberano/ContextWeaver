using ContextWeaver.Core;
using ContextWeaver.Utilities;
using Microsoft.Extensions.Logging;

namespace ContextWeaver.Services;

/// <summary>
///     Coordina el proceso de análisis de código.
///     Orquesta la carga de configuración, descubrimiento de archivos, ejecución de analizadores
///     y generación de reportes.
/// </summary>
public sealed class CodeAnalyzerService
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
    /// <returns>Una task que representa la operación asíncrona.</returns>
    public async Task AnalyzeAndGenerateReport(DirectoryInfo directory, FileInfo outputFile, string format)
    {
        // 1. Obtener archivos
        var (files, config) = GetManagedFiles(directory);

        // 2. Analizar y generar reporte
        await AnalyzeFiles(files, directory, outputFile, format);
    }

    /// <summary>
    ///     Obtiene la lista de archivos gestionados por la configuración del proyecto.
    /// </summary>
    /// <param name="directory">Directorio raíz.</param>
    /// <returns>Una tupla con la lista de archivos y la configuración cargada.</returns>
    public (List<FileInfo> Files, AnalysisSettings Settings) GetManagedFiles(DirectoryInfo directory)
    {
        // 1. Cargar configuración
        var settings = _settingsProvider.LoadSettingsFor(directory);

        // 2. Encontrar y filtrar archivos
        var allFiles = directory.GetFiles("*.*", SearchOption.AllDirectories)
            .Where(f => !settings.ExcludePatterns.Any(p =>
                f.FullName.Contains(Path.DirectorySeparatorChar + p + Path.DirectorySeparatorChar)))
            .Where(f => settings.IncludedExtensions.Contains(f.Extension.ToLowerInvariant()))
            .ToList();

        return (allFiles, settings);
    }

    /// <summary>
    ///     Analiza una lista específica de archivos y genera el reporte.
    /// </summary>
    /// <param name="files">Lista de archivos a analizar.</param>
    /// <param name="directory">Directorio raíz (para rutas relativas).</param>
    /// <param name="outputFile">Archivo de salida.</param>
    /// <param name="format">Formato del reporte.</param>
    /// <param name="enabledSections">Nombres de secciones a incluir. Si es null, se incluyen todas.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AnalyzeFiles(IEnumerable<FileInfo> files, DirectoryInfo directory, FileInfo outputFile, string format, IEnumerable<string>? enabledSections = null)
    {
        var generator = _generators.FirstOrDefault(g => g.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
        if (generator == null)
        {
            _logger.LogError("El formato de salida '{Format}' no es soportado.", format);
            return;
        }

        var settings = _settingsProvider.LoadSettingsFor(directory);
        var fileList = files.ToList();

        // 3. Inicializar Analizadores (Pre-carga de contexto global)
        foreach (var analyzer in _analyzers)
        {
            await analyzer.InitializeAsync(fileList);
        }

        // 4. Analizar archivos (Paralelizado)
        var analysisResults = new System.Collections.Concurrent.ConcurrentBag<FileAnalysisResult>();

        await Parallel.ForEachAsync(fileList, async (file, ct) =>
        {
            var analyzer = _analyzers.FirstOrDefault(a => a.CanAnalyze(file));
            if (analyzer != null)
            {
                var result = await analyzer.AnalyzeAsync(file);

                var relativePath = file.FullName.Replace(directory.FullName, string.Empty)
                    .Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');

                result.RelativePath = relativePath;
                result.ModuleName = CalculateModuleName(relativePath, settings.WrapperDirectories);

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
        var reportContent = generator.Generate(directory, resultsList, instabilityMetrics, enabledSections);
        await File.WriteAllTextAsync(outputFile.FullName, reportContent);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Reporte en formato '{Format}' generado exitosamente en: {OutputPath}", format, outputFile.FullName);
        }
    }

    /// <summary>
    ///     Calcula el nombre del módulo basado en su ruta relativa y los directorios contenedores configurados.
    /// </summary>
    private static string CalculateModuleName(string relativePath, string[] wrapperDirectories)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return "Root";
        }

        var parts = relativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return "Root";
        }

        // Si el primer nivel es un directorio contenedor (ej. "src") y existe un subnivel (ej. "ContextWeaver.Core")
        // el módulo se considera la combinación de ambos.
        if (parts.Length > 1 && wrapperDirectories.Contains(parts[0], StringComparer.OrdinalIgnoreCase))
        {
            return $"{parts[0]}/{parts[1]}";
        }

        // De lo contrario, el módulo es el primer nivel.
        // Los archivos aislados en la raíz del repositorio se agrupan en "Root" (parts.Length == 1).
        return parts.Length > 1 ? parts[0] : "Root";
    }
}
