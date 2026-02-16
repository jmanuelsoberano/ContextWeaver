using ContextWeaver.Utilities;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Utilities;

/// <summary>Tests for <see cref="MarkdownHelper"/>.</summary>
public class MarkdownHelperTests
{
    // ─── Valid Inputs (consolidated into Theory) ───

    /// <summary>Verifies that valid inputs produce the expected anchor slugs.</summary>
    /// <param name="input">The input string to convert.</param>
    /// <param name="expected">The expected anchor slug.</param>
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

    /// <summary>Verifies that null or whitespace inputs return an empty string.</summary>
    /// <param name="input">The input string to check.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateAnchor_NullOrWhitespace_ReturnsEmpty(string? input)
    {
        MarkdownHelper.CreateAnchor(input!).Should().BeEmpty();
    }

    // ─── Edge Cases ───

    /// <summary>Verifies that accented characters are removed (normalized).</summary>
    [Fact]
    public void CreateAnchor_Accents_RemovesAccentedChars()
    {
        // Accented chars are stripped by regex [^a-z0-9\s-], leaving only ASCII
        MarkdownHelper.CreateAnchor("Módulo Análisis").Should().Be("mdulo-anlisis");
    }

    /// <summary>Verifies that strings consisting only of special characters return an empty string.</summary>
    [Fact]
    public void CreateAnchor_AllSpecialChars_ReturnsEmpty()
    {
        MarkdownHelper.CreateAnchor("@#$%^&*()").Should().BeEmpty();
    }

    /// <summary>Verifies that slashes in paths are removed, not preserved as separators.</summary>
    [Fact]
    public void CreateAnchor_PathLikeInput_RemovesSlashes()
    {
        // Slashes are non-alphanumeric, removed by regex
        MarkdownHelper.CreateAnchor("Core/FileAnalysis").Should().Be("corefileanalysis");
    }
}
