using System.Collections.Generic;
using System.IO;
using ContextWeaver.Core;
using ContextWeaver.Reporters;
using ContextWeaver.Reporters.Sections;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Engine.Tests.Reporters.Sections;

public class ModuleAdjacencyListSectionTests
{
    private readonly ModuleAdjacencyListSection _sut;

    public ModuleAdjacencyListSectionTests()
    {
        _sut = new ModuleAdjacencyListSection();
    }

    [Fact]
    public void Name_ShouldBeCorrect()
    {
        _sut.Name.Should().Be("🔗 Lista de Adyacencia de Módulos (YAML)");
    }

    [Fact]
    public void Description_ShouldBeCorrect()
    {
        _sut.Description.Should().Be("Dependencias entre módulos en formato estructurado (YAML)");
    }

    [Fact]
    public void IsRequired_ShouldBeFalse()
    {
        _sut.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void Render_WithNoModules_ShouldReturnEmptyString()
    {
        // Arrange
        var context = CreateContext(new List<FileAnalysisResult>());

        // Act
        var result = _sut.Render(context);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Render_WithModules_ShouldGenerateYaml()
    {
        // Arrange
        var results = new List<FileAnalysisResult>
        {
            new FileAnalysisResult
            {
                RelativePath = "Core/Config.cs",
                ModuleName = "Core",
                DefinedTypes = new List<string> { "Config" }
            },
            new FileAnalysisResult
            {
                RelativePath = "Api/Controller.cs",
                ModuleName = "Api",
                ClassDependencies = new List<string> { "Controller --> Config" }, // Api depends on Core
                DefinedTypes = new List<string> { "Controller" }
            },
            new FileAnalysisResult
            {
                RelativePath = "Api/Models.cs",
                ModuleName = "Api",
                ClassDependencies = new List<string> { "Models --> Auth" }, // Api depends on Shared
                DefinedTypes = new List<string> { "Models" }
            },
            new FileAnalysisResult
            {
                RelativePath = "Shared/Auth.cs",
                ModuleName = "Shared",
                ClassDependencies = new List<string> { "Auth --> Config" }, // Shared depends on Core
                DefinedTypes = new List<string> { "Auth" }
            }
        };

        var context = CreateContext(results);

        // Act
        var result = _sut.Render(context);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("# Lista de Adyacencia de Módulos");
        result.Should().Contain("```yaml");

        // Module 'Api' dependencies
        result.Should().Contain("Api:");
        result.Should().Contain("  - Core");
        result.Should().Contain("  - Shared");

        // Module 'Core' dependencies
        result.Should().Contain("Core:");
        result.Should().Contain("  []");

        // Module 'Shared' dependencies
        result.Should().Contain("Shared:");
        result.Should().Contain("  - Core");

        result.Should().Contain("```");
    }

    private static ReportContext CreateContext(List<FileAnalysisResult> results)
    {
        return new ReportContext(
            new DirectoryInfo("C:\\dummy"),
            results,
            new Dictionary<string, (int Ca, int Ce, double Instability)>(),
            new Dictionary<string, string>());
    }
}
