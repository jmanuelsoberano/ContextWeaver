using ContextWeaver.Core;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Core.Tests;

/// <summary>Pruebas para <see cref="FileAnalysisResult"/>.</summary>
public class FileAnalysisResultTests
{
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
