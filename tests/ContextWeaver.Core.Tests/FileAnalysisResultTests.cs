using ContextWeaver.Core;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Core.Tests;

/// <summary>Pruebas para <see cref="FileAnalysisResult"/>.</summary>
public class FileAnalysisResultTests
{
    // ─── Nombre del Módulo (propiedad calculada) ───

    /// <summary>Verifica que los archivos en subdirectorios se asignen al módulo correcto.</summary>
    /// <param name="path">La ruta relativa del archivo.</param>
    /// <param name="expectedModule">El nombre del módulo esperado.</param>
    [Theory]
    [InlineData("Core/ClassA.cs", "Core")]
    [InlineData("Services/SettingsProvider.cs", "Services")]
    [InlineData("Analyzers/CSharpFileAnalyzer.cs", "Analyzers")]
    public void ModuleName_FileInSubdirectory_ReturnsFirstPathSegment(string path, string expectedModule)
    {
        var result = new FileAnalysisResult { RelativePath = path };
        result.ModuleName.Should().Be(expectedModule);
    }

    /// <summary>Verifica que los archivos en la raíz retornen "Root" como nombre del módulo.</summary>
    [Fact]
    public void ModuleName_FileAtRoot_ReturnsRoot()
    {
        var result = new FileAnalysisResult { RelativePath = "Program.cs" };
        result.ModuleName.Should().Be("Root");
    }

    /// <summary>Verifica que una ruta vacía retorne "Root".</summary>
    [Fact]
    public void ModuleName_EmptyPath_ReturnsRoot()
    {
        var result = new FileAnalysisResult { RelativePath = string.Empty };
        result.ModuleName.Should().Be("Root");
    }

    /// <summary>Verifica que las rutas estilo Windows se manejen correctamente.</summary>
    [Fact]
    public void ModuleName_WindowsStylePath_ReturnsFirstSegment()
    {
        var result = new FileAnalysisResult { RelativePath = "Core\\FileAnalysisResult.cs" };
        result.ModuleName.Should().Be("Core");
    }

    /// <summary>Verifica que los archivos profundamente anidados retornen la carpeta de nivel superior como módulo.</summary>
    [Fact]
    public void ModuleName_DeeplyNestedPath_ReturnsTopLevel()
    {
        var result = new FileAnalysisResult { RelativePath = "Core/Models/SubFolder/File.cs" };
        result.ModuleName.Should().Be("Core");
    }

    // ─── Valores Por Defecto ───

    /// <summary>Verifica que una nueva instancia tenga los valores predeterminados esperados.</summary>
    [Fact]
    public void DefaultValues_NewInstance_HasExpectedDefaults()
    {
        var result = new FileAnalysisResult();

        result.RelativePath.Should().BeEmpty();
        result.LinesOfCode.Should().Be(0);
        result.CodeContent.Should().BeEmpty();
        result.Language.Should().Be("plaintext");
        result.Usings.Should().BeEmpty();
        result.ClassDependencies.Should().BeEmpty();
        result.IncomingDependencies.Should().BeEmpty();
        result.DefinedTypes.Should().BeEmpty();
        result.DefinedTypeKinds.Should().BeEmpty();
        result.DefinedTypeSemantics.Should().BeEmpty();
    }

    // ─── Métricas (tipadas) ───

    /// <summary>Verifica que las métricas se inicialicen como nulas/vacías por defecto.</summary>
    [Fact]
    public void Metrics_DefaultInstance_HasNullNumericMetrics()
    {
        var result = new FileAnalysisResult();

        result.Metrics.Should().NotBeNull();
        result.Metrics.CyclomaticComplexity.Should().BeNull();
        result.Metrics.MaxNestingDepth.Should().BeNull();
        result.Metrics.PublicApiSignatures.Should().BeEmpty();
    }

    /// <summary>Verifica que las métricas tipadas puedan establecerse y recuperarse correctamente.</summary>
    [Fact]
    public void Metrics_CanSetAndRetrieveTypedValues()
    {
        var result = new FileAnalysisResult
        {
            Metrics = new FileMetrics
            {
                CyclomaticComplexity = 5,
                MaxNestingDepth = 3,
                PublicApiSignatures = new List<string> { "- class Foo" }
            }
        };

        result.Metrics.CyclomaticComplexity.Should().Be(5);
        result.Metrics.MaxNestingDepth.Should().Be(3);
        result.Metrics.PublicApiSignatures.Should().ContainSingle().Which.Should().Be("- class Foo");
    }
}
