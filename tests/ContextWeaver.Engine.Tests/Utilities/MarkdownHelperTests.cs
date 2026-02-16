using ContextWeaver.Utilities;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Engine.Tests.Utilities;

/// <summary>Pruebas para <see cref="MarkdownHelper"/>.</summary>
public class MarkdownHelperTests
{
    // ─── Entradas Válidas (consolidado en Theory) ───

    /// <summary>Verifica que las entradas válidas produzcan los slugs de anclaje esperados.</summary>
    /// <param name="input">La cadena de entrada a convertir.</param>
    /// <param name="expected">El slug de anclaje esperado.</param>
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

    // ─── Nulo / Espacios en blanco ───

    /// <summary>Verifica que las entradas nulas o con espacios en blanco retornen una cadena vacía.</summary>
    /// <param name="input">La cadena de entrada a verificar.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateAnchor_NullOrWhitespace_ReturnsEmpty(string? input)
    {
        MarkdownHelper.CreateAnchor(input!).Should().BeEmpty();
    }

    // ─── Casos Borde ───

    /// <summary>Verifica que los caracteres acentuados se eliminen (normalización).</summary>
    [Fact]
    public void CreateAnchor_Accents_RemovesAccentedChars()
    {
        // Los caracteres acentuados son eliminados por regex [^a-z0-9\s-], dejando solo ASCII
        MarkdownHelper.CreateAnchor("Módulo Análisis").Should().Be("mdulo-anlisis");
    }

    /// <summary>Verifica que las cadenas que consisten solo en caracteres especiales retornen una cadena vacía.</summary>
    [Fact]
    public void CreateAnchor_AllSpecialChars_ReturnsEmpty()
    {
        MarkdownHelper.CreateAnchor("@#$%^&*()").Should().BeEmpty();
    }

    /// <summary>Verifica que las barras en las rutas se eliminen, no se preserven como separadores.</summary>
    [Fact]
    public void CreateAnchor_PathLikeInput_RemovesSlashes()
    {
        // Las barras no son alfanuméricas, eliminadas por regex
        MarkdownHelper.CreateAnchor("Core/FileAnalysis").Should().Be("corefileanalysis");
    }
}
