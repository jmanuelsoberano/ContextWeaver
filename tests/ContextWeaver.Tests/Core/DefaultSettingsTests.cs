using ContextWeaver.Core;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Core;

public class DefaultSettingsTests
{
    [Fact]
    public void Get_ReturnsNonNullSettings()
    {
        var settings = DefaultSettings.Get();
        settings.Should().NotBeNull();
    }

    [Fact]
    public void Get_IncludedExtensions_ContainsCSharp()
    {
        var settings = DefaultSettings.Get();
        settings.IncludedExtensions.Should().Contain(".cs");
    }

    [Fact]
    public void Get_IncludedExtensions_ContainsAllExpected()
    {
        var settings = DefaultSettings.Get();
        settings.IncludedExtensions.Should().Contain(new[]
        {
            ".cs", ".csproj", ".sln", ".json", ".ts", ".html", ".scss", ".css", ".md"
        });
    }

    [Fact]
    public void Get_ExcludePatterns_ContainsBinAndObj()
    {
        var settings = DefaultSettings.Get();
        settings.ExcludePatterns.Should().Contain("bin");
        settings.ExcludePatterns.Should().Contain("obj");
    }

    [Fact]
    public void Get_ExcludePatterns_ContainsNodeModules()
    {
        var settings = DefaultSettings.Get();
        settings.ExcludePatterns.Should().Contain("node_modules");
    }

    [Fact]
    public void Get_ReturnsFreshInstance_EachCall()
    {
        var a = DefaultSettings.Get();
        var b = DefaultSettings.Get();
        a.Should().NotBeSameAs(b, "each call should return a new instance to avoid shared state");
    }
}
