using ContextWeaver.Core;
using ContextWeaver.Interfaces;
using ContextWeaver.Utilities;

namespace ContextWeaver.Services;

public class CodeAnalyzerService
{
    private readonly IEnumerable<IFileAnalyzer> _analyzers;
    private readonly IEnumerable<IReportGenerator> _generators;
    private readonly InstabilityCalculator _instabilityCalculator;
    private readonly SettingsProvider _settingsProvider;

    public CodeAnalyzerService(
        SettingsProvider settingsProvider,
        InstabilityCalculator instabilityCalculator,
        IEnumerable<IFileAnalyzer> analyzers,
        IEnumerable<IReportGenerator> generators)
    {
        _settingsProvider = settingsProvider;
        _instabilityCalculator = instabilityCalculator;
        _analyzers = analyzers;
        _generators = generators;
    }

    public async Task AnalyzeAndGenerateReport(DirectoryInfo directory, FileInfo outputFile, string format)
    {
        var generator = _generators.FirstOrDefault(g => g.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
        if (generator == null)
        {
            Console.Error.WriteLine($"Error: El formato de salida '{format}' no es soportado.");
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
                result.RelativePath = file.FullName.Replace(directory.FullName, "")
                    .Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
                analysisResults.Add(result);
            }
        });

        // 5. Calcular inestabilidad (responsabilidad delegada)
        var resultsList = analysisResults.ToList();
        var instabilityMetrics = _instabilityCalculator.Calculate(directory.Name, resultsList);

        // 6. Generar y escribir el reporte
        var reportContent = generator.Generate(directory, resultsList, instabilityMetrics);
        await File.WriteAllTextAsync(outputFile.FullName, reportContent);

        Console.WriteLine($"✅ Reporte en formato '{format}' generado exitosamente en: {outputFile.FullName}");
    }
}