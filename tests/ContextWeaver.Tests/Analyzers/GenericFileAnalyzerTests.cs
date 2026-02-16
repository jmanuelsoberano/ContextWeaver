using ContextWeaver.Analyzers;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Analyzers;

public class GenericFileAnalyzerTests
{
    private readonly GenericFileAnalyzer _analyzer = new();

    // ─── CanAnalyze ───

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

    [Theory]
    [InlineData(".TS")]
    [InlineData(".Js")]
    [InlineData(".HTML")]
    public void CanAnalyze_UppercaseExtension_ReturnsTrue(string extension)
    {
        var file = new FileInfo($"test{extension}");
        _analyzer.CanAnalyze(file).Should().BeTrue();
    }

    [Theory]
    [InlineData(".cs")]
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

    [Fact]
    public async Task InitializeAsync_CompletesImmediately()
    {
        var task = _analyzer.InitializeAsync(Enumerable.Empty<FileInfo>());
        await task; // Should not throw
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ─── AnalyzeAsync ───

    [Fact]
    public async Task AnalyzeAsync_JsonFile_ReturnsCorrectLanguageAndContent()
    {
        // Create a temp file
        var tempFile = Path.GetTempFileName();
        var jsonFile = Path.ChangeExtension(tempFile, ".json");
        File.Move(tempFile, jsonFile);

        try
        {
            var content = "{ \"key\": \"value\" }";
            await File.WriteAllTextAsync(jsonFile, content);

            var result = await _analyzer.AnalyzeAsync(new FileInfo(jsonFile));

            result.Should().NotBeNull();
            result.Language.Should().Be("json");
            result.CodeContent.Should().Be(content);
            result.LinesOfCode.Should().Be(1);
        }
        finally
        {
            File.Delete(jsonFile);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_MultilineFile_CountsLinesCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        var mdFile = Path.ChangeExtension(tempFile, ".md");
        File.Move(tempFile, mdFile);

        try
        {
            var content = "# Title\n\nParagraph 1\n\nParagraph 2\n";
            await File.WriteAllTextAsync(mdFile, content);

            var result = await _analyzer.AnalyzeAsync(new FileInfo(mdFile));

            result.Language.Should().Be("markdown");
            result.LinesOfCode.Should().Be(6); // 5 lines + trailing newline = 6 parts on split
        }
        finally
        {
            File.Delete(mdFile);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_TypeScriptFile_ReturnsTypescriptLanguage()
    {
        var tempFile = Path.GetTempFileName();
        var tsFile = Path.ChangeExtension(tempFile, ".ts");
        File.Move(tempFile, tsFile);

        try
        {
            await File.WriteAllTextAsync(tsFile, "const x: number = 42;");

            var result = await _analyzer.AnalyzeAsync(new FileInfo(tsFile));

            result.Language.Should().Be("typescript");
        }
        finally
        {
            File.Delete(tsFile);
        }
    }
}
