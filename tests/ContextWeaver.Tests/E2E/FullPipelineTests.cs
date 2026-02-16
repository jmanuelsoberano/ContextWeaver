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
    ///     Initializes a new instance of the <see cref="FullPipelineTests"/> class.
    ///     Configures the fixture directory and temporary output path.
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
        File.Exists(_outputPath).Should().BeTrue("el archivo de reporte debe ser creado");
        var content = await File.ReadAllTextAsync(_outputPath);
        content.Should().NotBeNullOrWhiteSpace();

        // Afirmar — las secciones clave están presentes
        content.Should().Contain("Análisis de Hotspots", "la sección de hotspot debe existir");
        content.Should().Contain("Análisis de Inestabilidad", "la sección de inestabilidad debe existir");
        content.Should().Contain("Estructura de Directorios", "el árbol de directorios debe existir");
        content.Should().Contain("## Archivo:", "las secciones de archivo deben existir");
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
        content.Should().Contain("Calculator.cs", "el archivo Calculator debe ser analizado");
        content.Should().Contain("MathService.cs", "el archivo MathService debe ser analizado");
        content.Should().Contain("CyclomaticComplexity", "las métricas de C# deben ser renderizadas");
        content.Should().Contain("csharp", "el lenguaje debe ser detectado como csharp");
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
        content.Should().Contain("config.json", "el archivo JSON debe ser analizado");
        content.Should().Contain("json", "el lenguaje debe ser detectado como json");
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
        content.Should().Contain("Calculator", "Calculator debe aparecer en el reporte");
        content.Should().Contain("MathService", "MathService debe aparecer en el reporte");
    }
}
