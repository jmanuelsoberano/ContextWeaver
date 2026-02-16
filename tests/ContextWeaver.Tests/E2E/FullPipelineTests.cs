using ContextWeaver.Analyzers;
using ContextWeaver.Core;
using ContextWeaver.Interfaces;
using ContextWeaver.Reporters;
using ContextWeaver.Services;
using ContextWeaver.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ContextWeaver.Tests.E2E;

/// <summary>
///     End-to-end tests that exercise the full analysis pipeline:
///     SettingsProvider → Analyzers → InstabilityCalculator → MarkdownReportGenerator.
/// </summary>
public class FullPipelineTests : IDisposable
{
    private readonly DirectoryInfo _fixtureDir;
    private readonly string _outputPath;

    public FullPipelineTests()
    {
        // Walk up from the assembly output (bin/Debug/net8.0) to the test project root,
        // then into Fixtures/SampleProject. This avoids the "bin" exclude pattern.
        var assemblyDir = Path.GetDirectoryName(typeof(FullPipelineTests).Assembly.Location)!;
        var testProjectDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
        _fixtureDir = new DirectoryInfo(Path.Combine(testProjectDir, "Fixtures", "SampleProject"));
        _outputPath = Path.Combine(Path.GetTempPath(), $"contextweaver_e2e_{Guid.NewGuid()}.md");
    }

    public void Dispose()
    {
        if (File.Exists(_outputPath))
            File.Delete(_outputPath);
    }

    private CodeAnalyzerService BuildService()
    {
        var settingsProvider = new SettingsProvider(
            NullLogger<SettingsProvider>.Instance);
        var instabilityCalculator = new InstabilityCalculator();
        var analyzers = new IFileAnalyzer[]
        {
            new CSharpFileAnalyzer(NullLogger<CSharpFileAnalyzer>.Instance),
            new GenericFileAnalyzer()
        };
        var generators = new IReportGenerator[] { new MarkdownReportGenerator() };

        return new CodeAnalyzerService(
            settingsProvider,
            instabilityCalculator,
            analyzers,
            generators,
            NullLogger<CodeAnalyzerService>.Instance);
    }

    [Fact]
    public async Task AnalyzeAndGenerateReport_SampleProject_ProducesValidMarkdown()
    {
        // Arrange
        var service = BuildService();
        var outputFile = new FileInfo(_outputPath);

        // Act
        await service.AnalyzeAndGenerateReport(_fixtureDir, outputFile, "markdown");

        // Assert — file was produced
        File.Exists(_outputPath).Should().BeTrue("the report file should be created");
        var content = await File.ReadAllTextAsync(_outputPath);
        content.Should().NotBeNullOrWhiteSpace();

        // Assert — key sections are present
        content.Should().Contain("Análisis de Hotspots", "hotspot section should exist");
        content.Should().Contain("Análisis de Inestabilidad", "instability section should exist");
        content.Should().Contain("Directory Structure", "directory tree should exist");
        content.Should().Contain("## File:", "file sections should exist");
    }

    [Fact]
    public async Task AnalyzeAndGenerateReport_SampleProject_ContainsCSharpAnalysis()
    {
        // Arrange
        var service = BuildService();
        var outputFile = new FileInfo(_outputPath);

        // Act
        await service.AnalyzeAndGenerateReport(_fixtureDir, outputFile, "markdown");
        var content = await File.ReadAllTextAsync(_outputPath);

        // Assert — C# specific content
        content.Should().Contain("Calculator.cs", "Calculator file should be analyzed");
        content.Should().Contain("MathService.cs", "MathService file should be analyzed");
        content.Should().Contain("CyclomaticComplexity", "C# metrics should be rendered");
        content.Should().Contain("csharp", "language should be detected as csharp");
    }

    [Fact]
    public async Task AnalyzeAndGenerateReport_SampleProject_ContainsNonCSharpFiles()
    {
        // Arrange
        var service = BuildService();
        var outputFile = new FileInfo(_outputPath);

        // Act
        await service.AnalyzeAndGenerateReport(_fixtureDir, outputFile, "markdown");
        var content = await File.ReadAllTextAsync(_outputPath);

        // Assert — JSON file included
        content.Should().Contain("config.json", "JSON file should be analyzed");
        content.Should().Contain("json", "language should be detected as json");
    }

    [Fact]
    public async Task AnalyzeAndGenerateReport_SampleProject_ContainsDependencyInfo()
    {
        // Arrange
        var service = BuildService();
        var outputFile = new FileInfo(_outputPath);

        // Act
        await service.AnalyzeAndGenerateReport(_fixtureDir, outputFile, "markdown");
        var content = await File.ReadAllTextAsync(_outputPath);

        // Assert — dependency relationships detected
        content.Should().Contain("Calculator", "Calculator should appear in report");
        content.Should().Contain("MathService", "MathService should appear in report");
    }
}
