using ContextWeaver.Core;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Core;

public class FileAnalysisResultTests
{
    // ─── ModuleName (computed property) ───

    [Theory]
    [InlineData("Core/ClassA.cs", "Core")]
    [InlineData("Services/SettingsProvider.cs", "Services")]
    [InlineData("Analyzers/CSharpFileAnalyzer.cs", "Analyzers")]
    public void ModuleName_FileInSubdirectory_ReturnsFirstPathSegment(string path, string expectedModule)
    {
        var result = new FileAnalysisResult { RelativePath = path };
        result.ModuleName.Should().Be(expectedModule);
    }

    [Fact]
    public void ModuleName_FileAtRoot_ReturnsRoot()
    {
        var result = new FileAnalysisResult { RelativePath = "Program.cs" };
        result.ModuleName.Should().Be("Root");
    }

    [Fact]
    public void ModuleName_EmptyPath_ReturnsRoot()
    {
        var result = new FileAnalysisResult { RelativePath = "" };
        result.ModuleName.Should().Be("Root");
    }

    [Fact]
    public void ModuleName_WindowsStylePath_ReturnsFirstSegment()
    {
        var result = new FileAnalysisResult { RelativePath = "Core\\FileAnalysisResult.cs" };
        result.ModuleName.Should().Be("Core");
    }

    [Fact]
    public void ModuleName_DeeplyNestedPath_ReturnsTopLevel()
    {
        var result = new FileAnalysisResult { RelativePath = "Core/Models/SubFolder/File.cs" };
        result.ModuleName.Should().Be("Core");
    }

    // ─── Default Values ───

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

    // ─── Metrics (typed) ───

    [Fact]
    public void Metrics_DefaultInstance_HasNullNumericMetrics()
    {
        var result = new FileAnalysisResult();

        result.Metrics.Should().NotBeNull();
        result.Metrics.CyclomaticComplexity.Should().BeNull();
        result.Metrics.MaxNestingDepth.Should().BeNull();
        result.Metrics.PublicApiSignatures.Should().BeEmpty();
    }

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
