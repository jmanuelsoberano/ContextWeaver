using ContextWeaver.Analyzers;
using ContextWeaver.Core;
using ContextWeaver.Reporters;
using ContextWeaver.Services;
using ContextWeaver.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ContextWeaver.Tests.E2E;

/// <summary>
///     Pruebas de extremo a extremo que ejercitan el pipeline completo de análisis:
///     SettingsProvider → Analyzers → InstabilityCalculator → MarkdownReportGenerator.
/// </summary>
public class FullPipelineTests : IDisposable
{
    private readonly DirectoryInfo _fixtureDir;
    private readonly string _outputPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullPipelineTests"/> class.
    ///     Inicializa una nueva instancia de la clase <see cref="FullPipelineTests"/>.
    ///     Configura el directorio de fixtures y la ruta de salida temporal.
    /// </summary>
    public FullPipelineTests()
    {
        // Subir desde la salida del ensamblado (bin/Debug/net8.0) a la raíz del proyecto de pruebas,
        // luego a Fixtures/SampleProject. Esto evita el patrón de exclusión "bin".
        var assemblyDir = Path.GetDirectoryName(typeof(FullPipelineTests).Assembly.Location)!;
        var testProjectDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
        _fixtureDir = new DirectoryInfo(Path.Combine(testProjectDir, "Fixtures", "SampleProject"));
        _outputPath = Path.Combine(Path.GetTempPath(), $"contextweaver_e2e_{Guid.NewGuid()}.md");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (File.Exists(_outputPath))
            File.Delete(_outputPath);
    }

    private CodeAnalyzerService BuildService()
    {
        var settingsProvider = new SettingsProvider(
            NullLogger<SettingsProvider>.Instance);
        var analyzers = new IFileAnalyzer[]
        {
            new CSharpFileAnalyzer(NullLogger<CSharpFileAnalyzer>.Instance),
            new GenericFileAnalyzer()
        };
        var generators = new IReportGenerator[] { new MarkdownReportGenerator() };

        return new CodeAnalyzerService(
            settingsProvider,
            analyzers,
            generators,
            NullLogger<CodeAnalyzerService>.Instance);
    }

    /// <summary>
    ///     Verifica que el pipeline completo produce un archivo markdown válido.
    /// </summary>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
    [Fact]
    public async Task AnalyzeAndGenerateReport_SampleProject_ProducesValidMarkdown()
    {
        // Organizar
        var service = BuildService();
        var outputFile = new FileInfo(_outputPath);

        // Actuar
        await service.AnalyzeAndGenerateReport(_fixtureDir, outputFile, "markdown");

        // Afirmar — el archivo fue producido
        File.Exists(_outputPath).Should().BeTrue("the report file should be created");
        var content = await File.ReadAllTextAsync(_outputPath);
        content.Should().NotBeNullOrWhiteSpace();

        // Afirmar — las secciones clave están presentes
        content.Should().Contain("Análisis de Hotspots", "hotspot section should exist");
        content.Should().Contain("Análisis de Inestabilidad", "instability section should exist");
        content.Should().Contain("Directory Structure", "directory tree should exist");
        content.Should().Contain("## File:", "file sections should exist");
    }

    /// <summary>
    ///     Verifica que el análisis específico de C# confirma la presencia de archivos C# y métricas.
    /// </summary>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
    [Fact]
    public async Task AnalyzeAndGenerateReport_SampleProject_ContainsCSharpAnalysis()
    {
        // Organizar
        var service = BuildService();
        var outputFile = new FileInfo(_outputPath);

        // Actuar
        await service.AnalyzeAndGenerateReport(_fixtureDir, outputFile, "markdown");
        var content = await File.ReadAllTextAsync(_outputPath);

        // Afirmar — contenido específico de C#
        content.Should().Contain("Calculator.cs", "Calculator file should be analyzed");
        content.Should().Contain("MathService.cs", "MathService file should be analyzed");
        content.Should().Contain("CyclomaticComplexity", "C# metrics should be rendered");
        content.Should().Contain("csharp", "language should be detected as csharp");
    }

    /// <summary>
    ///     Verifica que los archivos no-C# (como JSON) son analizados vía GenericFileAnalyzer.
    /// </summary>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
    [Fact]
    public async Task AnalyzeAndGenerateReport_SampleProject_ContainsNonCSharpFiles()
    {
        // Organizar
        var service = BuildService();
        var outputFile = new FileInfo(_outputPath);

        // Actuar
        await service.AnalyzeAndGenerateReport(_fixtureDir, outputFile, "markdown");
        var content = await File.ReadAllTextAsync(_outputPath);

        // Afirmar — archivo JSON incluido
        content.Should().Contain("config.json", "JSON file should be analyzed");
        content.Should().Contain("json", "language should be detected as json");
    }

    /// <summary>
    ///     Verifica que la información de dependencias es extraída y reportada correctamente.
    /// </summary>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
    [Fact]
    public async Task AnalyzeAndGenerateReport_SampleProject_ContainsDependencyInfo()
    {
        // Organizar
        var service = BuildService();
        var outputFile = new FileInfo(_outputPath);

        // Actuar
        await service.AnalyzeAndGenerateReport(_fixtureDir, outputFile, "markdown");
        var content = await File.ReadAllTextAsync(_outputPath);

        // Afirmar — relaciones de dependencia detectadas
        content.Should().Contain("Calculator", "Calculator should appear in report");
        content.Should().Contain("MathService", "MathService should appear in report");
    }
}
