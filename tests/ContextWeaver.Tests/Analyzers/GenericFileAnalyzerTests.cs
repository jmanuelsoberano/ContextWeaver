using ContextWeaver.Analyzers;
using ContextWeaver.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Analyzers;

/// <summary>Pruebas para <see cref="GenericFileAnalyzer"/>.</summary>
public class GenericFileAnalyzerTests
{
    private readonly GenericFileAnalyzer _analyzer = new();

    // ─── CanAnalyze: Extensiones Soportadas ───

    /// <summary>Verifica que las extensiones soportadas retornan true.</summary>
    /// <param name="extension">La extensión de archivo a probar.</param>
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

    // ─── CanAnalyze: Insensible a Mayúsculas/Minúsculas ───

    /// <summary>Verifica que la coincidencia de extensiones no distinga entre mayúsculas y minúsculas.</summary>
    /// <param name="extension">La extensión de archivo a probar.</param>
    [Theory]
    [InlineData(".TS")]
    [InlineData(".Js")]
    [InlineData(".HTML")]
    public void CanAnalyze_UppercaseExtension_ReturnsTrue(string extension)
    {
        var file = new FileInfo($"test{extension}");
        _analyzer.CanAnalyze(file).Should().BeTrue();
    }

    // ─── CanAnalyze: No Soportado / Delegado ───

    /// <summary>Verifica que las extensiones no soportadas o delegadas retornan false.</summary>
    /// <param name="extension">La extensión de archivo a probar.</param>
    [Theory]
    [InlineData(".cs")]   // Manejado por CSharpFileAnalyzer
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

    /// <summary>Verifica que la inicialización se complete inmediatamente.</summary>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
    [Fact]
    public async Task InitializeAsync_CompletesImmediately()
    {
        var task = _analyzer.InitializeAsync(Enumerable.Empty<FileInfo>());
        await task;
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    // ─── AnalyzeAsync: Detección de Lenguaje ───

    /// <summary>Verifica que se retorne el identificador de lenguaje correcto basado en la extensión.</summary>
    /// <param name="extension">La extensión del archivo.</param>
    /// <param name="content">El contenido del archivo.</param>
    /// <param name="expectedLanguage">El identificador de lenguaje esperado.</param>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
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

    // ─── AnalyzeAsync: Conteo de Líneas ───

    /// <summary>Verifica que las líneas se cuenten correctamente para archivos multilínea.</summary>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
    [Fact]
    public async Task AnalyzeAsync_MultilineFile_CountsLinesCorrectly()
    {
        // 5 líneas de texto + nueva línea final = Split('\n') produce 6 partes
        var content = "# Title\n\nParagraph 1\n\nParagraph 2\n";
        using var tmp = new TempFile(".md", content);

        var result = await _analyzer.AnalyzeAsync(new FileInfo(tmp.Path));

        result.LinesOfCode.Should().Be(6);
    }

    /// <summary>Verifica que los archivos de una sola línea se cuenten como 1 línea.</summary>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
    [Fact]
    public async Task AnalyzeAsync_SingleLineFile_Returns1()
    {
        using var tmp = new TempFile(".json", "{}");

        var result = await _analyzer.AnalyzeAsync(new FileInfo(tmp.Path));

        result.LinesOfCode.Should().Be(1);
    }

    /// <summary>Verifica que los archivos vacíos se cuenten como 1 línea.</summary>
    /// <returns>Una <see cref="Task"/> que representa la operación asíncrona.</returns>
    [Fact]
    public async Task AnalyzeAsync_EmptyFile_Returns1()
    {
        // Cadena vacía dividida por '\n' produce 1 parte vacía
        using var tmp = new TempFile(".json", "");

        var result = await _analyzer.AnalyzeAsync(new FileInfo(tmp.Path));

        result.LinesOfCode.Should().Be(1);
    }
}
