using ContextWeaver.Analyzers;
using ContextWeaver.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Analyzers;

/// <summary>Tests for <see cref="GenericFileAnalyzer"/>.</summary>
public class GenericFileAnalyzerTests
{
    private readonly GenericFileAnalyzer _analyzer = new();

    // ─── CanAnalyze: Supported Extensions ───

    /// <summary>Verifies that supported extensions return true.</summary>
    /// <param name="extension">The file extension to test.</param>
    [Theory]
    [InlineData(".ts")]
    [InlineData(".js")]
    [InlineData(".html")]
    [InlineData(".css")]
    [InlineData(".scss")]
    [InlineData(".json")]
    [InlineData(".md")]
    [InlineData(".csproj")]
    [InlineData(".sln")]
    public void CanAnalyze_SupportedExtension_ReturnsTrue(string extension)
    {
        var file = new FileInfo($"test{extension}");
        _analyzer.CanAnalyze(file).Should().BeTrue();
    }

    // ─── CanAnalyze: Case Insensitive ───

    /// <summary>Verifies that extension matching is case-insensitive.</summary>
    /// <param name="extension">The file extension to test.</param>
    [Theory]
    [InlineData(".TS")]
    [InlineData(".Js")]
    [InlineData(".HTML")]
    public void CanAnalyze_UppercaseExtension_ReturnsTrue(string extension)
    {
        var file = new FileInfo($"test{extension}");
        _analyzer.CanAnalyze(file).Should().BeTrue();
    }

    // ─── CanAnalyze: Unsupported / Delegated ───

    /// <summary>Verifies that unsupported or delegated extensions return false.</summary>
    /// <param name="extension">The file extension to test.</param>
    [Theory]
    [InlineData(".cs")]   // Handled by CSharpFileAnalyzer
    [InlineData(".py")]
    [InlineData(".java")]
    [InlineData(".exe")]
    [InlineData(".dll")]
    [InlineData("")]
    public void CanAnalyze_UnsupportedExtension_ReturnsFalse(string extension)
    {
        var file = new FileInfo($"test{extension}");
        _analyzer.CanAnalyze(file).Should().BeFalse();
    }

    // ─── InitializeAsync ───

    // ─── InitializeAsync ───

    /// <summary>Verifies that initialization completes immediately.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task InitializeAsync_CompletesImmediately()
    {
        var task = _analyzer.InitializeAsync(Enumerable.Empty<FileInfo>());
        await task;
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ─── AnalyzeAsync: Language Detection ───

    /// <summary>Verifies that the correct language identifier is returned based on extension.</summary>
    /// <param name="extension">The file extension.</param>
    /// <param name="content">The file content.</param>
    /// <param name="expectedLanguage">The expected language identifier.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(".json", "{ \"key\": \"value\" }", "json")]
    [InlineData(".ts", "const x: number = 42;", "typescript")]
    [InlineData(".html", "<h1>Hello</h1>", "html")]
    [InlineData(".md", "# Title", "markdown")]
    [InlineData(".csproj", "<Project></Project>", "xml")]
    public async Task AnalyzeAsync_ByExtension_ReturnsCorrectLanguage(
        string extension, string content, string expectedLanguage)
    {
        using var tmp = new TempFile(extension, content);

        var result = await _analyzer.AnalyzeAsync(new FileInfo(tmp.Path));

        result.Should().NotBeNull();
        result.Language.Should().Be(expectedLanguage);
        result.CodeContent.Should().Be(content);
    }

    // ─── AnalyzeAsync: Line Counting ───

    /// <summary>Verifies that lines are counted correctly for multiline files.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task AnalyzeAsync_MultilineFile_CountsLinesCorrectly()
    {
        // 5 lines of text + trailing newline = Split('\n') produces 6 parts
        var content = "# Title\n\nParagraph 1\n\nParagraph 2\n";
        using var tmp = new TempFile(".md", content);

        var result = await _analyzer.AnalyzeAsync(new FileInfo(tmp.Path));

        result.LinesOfCode.Should().Be(6);
    }

    /// <summary>Verifies that single-line files are counted as 1 line.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task AnalyzeAsync_SingleLineFile_Returns1()
    {
        using var tmp = new TempFile(".json", "{}");

        var result = await _analyzer.AnalyzeAsync(new FileInfo(tmp.Path));

        result.LinesOfCode.Should().Be(1);
    }

    /// <summary>Verifies that empty files are counted as 1 line.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task AnalyzeAsync_EmptyFile_Returns1()
    {
        // Empty string split by '\n' produces 1 empty part
        using var tmp = new TempFile(".json", "");

        var result = await _analyzer.AnalyzeAsync(new FileInfo(tmp.Path));

        result.LinesOfCode.Should().Be(1);
    }
}
