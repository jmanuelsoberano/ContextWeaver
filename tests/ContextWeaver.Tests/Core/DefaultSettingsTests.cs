using ContextWeaver.Core;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Core;

/// <summary>Tests for <see cref="DefaultSettings"/>.</summary>
public class DefaultSettingsTests
{
    /// <summary>Verifies that the default settings are not null.</summary>
    [Fact]
    public void Get_ReturnsNonNullSettings()
    {
        var settings = DefaultSettings.Get();
        settings.Should().NotBeNull();
    }

    /// <summary>Verifies that .cs is included in the default extensions.</summary>
    [Fact]
    public void Get_IncludedExtensions_ContainsCSharp()
    {
        var settings = DefaultSettings.Get();
        settings.IncludedExtensions.Should().Contain(".cs");
    }

    /// <summary>Verifies that all expected extensions are included.</summary>
    [Fact]
    public void Get_IncludedExtensions_ContainsAllExpected()
    {
        var settings = DefaultSettings.Get();
        settings.IncludedExtensions.Should().Contain(new[]
        {
            ".cs", ".csproj", ".sln", ".json", ".ts", ".html", ".scss", ".css", ".md"
        });
    }

    /// <summary>Verifies that bin and obj are excluded by default.</summary>
    [Fact]
    public void Get_ExcludePatterns_ContainsBinAndObj()
    {
        var settings = DefaultSettings.Get();
        settings.ExcludePatterns.Should().Contain("bin");
        settings.ExcludePatterns.Should().Contain("obj");
    }

    /// <summary>Verifies that node_modules is excluded by default.</summary>
    [Fact]
    public void Get_ExcludePatterns_ContainsNodeModules()
    {
        var settings = DefaultSettings.Get();
        settings.ExcludePatterns.Should().Contain("node_modules");
    }

    /// <summary>Verifies that a new instance is returned each time.</summary>
    [Fact]
    public void Get_ReturnsFreshInstance_EachCall()
    {
        var a = DefaultSettings.Get();
        var b = DefaultSettings.Get();
        a.Should().NotBeSameAs(b, "each call should return a new instance to avoid shared state");
    }
}
