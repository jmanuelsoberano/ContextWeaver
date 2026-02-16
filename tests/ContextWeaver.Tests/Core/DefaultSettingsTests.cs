using ContextWeaver.Core;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Core;

/// <summary>Pruebas para <see cref="DefaultSettings"/>.</summary>
public class DefaultSettingsTests
{
    /// <summary>Verifica que la configuración por defecto no sea nula.</summary>
    [Fact]
    public void Get_ReturnsNonNullSettings()
    {
        var settings = DefaultSettings.Get();
        settings.Should().NotBeNull();
    }

    /// <summary>Verifica que .cs esté incluido en las extensiones por defecto.</summary>
    [Fact]
    public void Get_IncludedExtensions_ContainsCSharp()
    {
        var settings = DefaultSettings.Get();
        settings.IncludedExtensions.Should().Contain(".cs");
    }

    /// <summary>Verifica que todas las extensiones esperadas estén incluidas.</summary>
    [Fact]
    public void Get_IncludedExtensions_ContainsAllExpected()
    {
        var settings = DefaultSettings.Get();
        settings.IncludedExtensions.Should().Contain(new[]
        {
            ".cs", ".csproj", ".sln", ".json", ".ts", ".html", ".scss", ".css", ".md"
        });
    }

    /// <summary>Verifica que bin y obj estén excluidos por defecto.</summary>
    [Fact]
    public void Get_ExcludePatterns_ContainsBinAndObj()
    {
        var settings = DefaultSettings.Get();
        settings.ExcludePatterns.Should().Contain("bin");
        settings.ExcludePatterns.Should().Contain("obj");
    }

    /// <summary>Verifica que node_modules esté excluido por defecto.</summary>
    [Fact]
    public void Get_ExcludePatterns_ContainsNodeModules()
    {
        var settings = DefaultSettings.Get();
        settings.ExcludePatterns.Should().Contain("node_modules");
    }

    /// <summary>Verifica que se retorne una nueva instancia cada vez.</summary>
    [Fact]
    public void Get_ReturnsFreshInstance_EachCall()
    {
        var a = DefaultSettings.Get();
        var b = DefaultSettings.Get();
        a.Should().NotBeSameAs(b, "cada llamada debe retornar una nueva instancia para evitar estado compartido");
    }
}
