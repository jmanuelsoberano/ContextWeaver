using ContextWeaver.Utilities;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Utilities;

public class MarkdownHelperTests
{
    [Fact]
    public void CreateAnchor_SimpleText_ReturnsLowercaseKebab()
    {
        MarkdownHelper.CreateAnchor("Hello World").Should().Be("hello-world");
    }

    [Fact]
    public void CreateAnchor_SpecialCharacters_RemovesNonAlphanumeric()
    {
        MarkdownHelper.CreateAnchor("C# File Analysis!").Should().Be("c-file-analysis");
    }

    [Fact]
    public void CreateAnchor_MultipleDashes_CollapsesToSingle()
    {
        MarkdownHelper.CreateAnchor("Core -- Module").Should().Be("core-module");
    }

    [Fact]
    public void CreateAnchor_LeadingTrailingSpaces_Trims()
    {
        MarkdownHelper.CreateAnchor("  Hello  ").Should().Be("hello");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateAnchor_NullOrWhitespace_ReturnsEmpty(string? input)
    {
        MarkdownHelper.CreateAnchor(input!).Should().BeEmpty();
    }

    [Fact]
    public void CreateAnchor_WithNumbers_PreservesNumbers()
    {
        MarkdownHelper.CreateAnchor("Step 1 - Analysis").Should().Be("step-1-analysis");
    }

    [Fact]
    public void CreateAnchor_Accents_RemovesAccentedChars()
    {
        // Accented chars are removed by the regex [^a-z0-9\s-]
        MarkdownHelper.CreateAnchor("Módulo Análisis").Should().Be("mdulo-anlisis");
    }

    [Fact]
    public void CreateAnchor_AllSpecialChars_ReturnsEmpty()
    {
        MarkdownHelper.CreateAnchor("@#$%^&*()").Should().BeEmpty();
    }

    [Fact]
    public void CreateAnchor_PathLikeInput_RemovesSlashes()
    {
        MarkdownHelper.CreateAnchor("Core/FileAnalysis").Should().Be("corefileanalysis");
    }
}
