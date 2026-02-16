using ContextWeaver.Utilities;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Utilities;

public class MarkdownHelperTests
{
    // ─── Valid Inputs (consolidated into Theory) ───

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Core -- Module", "core-module")]
    [InlineData("Step 1 - Analysis", "step-1-analysis")]
    [InlineData("  Hello  ", "hello")]
    [InlineData("C# File Analysis!", "c-file-analysis")]
    public void CreateAnchor_ValidInput_ReturnsExpectedAnchor(string input, string expected)
    {
        MarkdownHelper.CreateAnchor(input).Should().Be(expected);
    }

    // ─── Null / Whitespace ───

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateAnchor_NullOrWhitespace_ReturnsEmpty(string? input)
    {
        MarkdownHelper.CreateAnchor(input!).Should().BeEmpty();
    }

    // ─── Edge Cases ───

    [Fact]
    public void CreateAnchor_Accents_RemovesAccentedChars()
    {
        // Accented chars are stripped by regex [^a-z0-9\s-], leaving only ASCII
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
        // Slashes are non-alphanumeric, removed by regex
        MarkdownHelper.CreateAnchor("Core/FileAnalysis").Should().Be("corefileanalysis");
    }
}
