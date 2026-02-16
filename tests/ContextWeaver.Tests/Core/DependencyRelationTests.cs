using ContextWeaver.Core;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Core;

/// <summary>Tests for <see cref="DependencyRelation"/>.</summary>
public class DependencyRelationTests
{
    // ─── Parse: Happy Path ───

    /// <summary>Verifies that a usage dependency is parsed correctly.</summary>
    [Fact]
    public void Parse_UsageDependency_ReturnsDependencyWithUsageKind()
    {
        var result = DependencyRelation.Parse("ClassA --> ClassB");

        result.Should().NotBeNull();
        result!.Source.Should().Be("ClassA");
        result.Target.Should().Be("ClassB");
        result.Kind.Should().Be(DependencyKind.Usage);
    }

    /// <summary>Verifies that an inheritance dependency is parsed correctly.</summary>
    [Fact]
    public void Parse_InheritanceDependency_ReturnsDependencyWithInheritanceKind()
    {
        var result = DependencyRelation.Parse("ClassA -.-> IDisposable");

        result.Should().NotBeNull();
        result!.Source.Should().Be("ClassA");
        result.Target.Should().Be("IDisposable");
        result.Kind.Should().Be(DependencyKind.Inheritance);
    }

    /// <summary>Verifies that whitespace is trimmed from source and target.</summary>
    [Fact]
    public void Parse_TrimsWhitespace_FromSourceAndTarget()
    {
        var result = DependencyRelation.Parse("  ClassA   -->   ClassB  ");

        result.Should().NotBeNull();
        result!.Source.Should().Be("ClassA");
        result.Target.Should().Be("ClassB");
    }

    // ─── Parse: Null / Invalid Input ───

    /// <summary>Verifies that null or whitespace input returns null.</summary>
    /// <param name="input">The input string to check.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullOrWhitespace_ReturnsNull(string? input)
    {
        var result = DependencyRelation.Parse(input!);
        result.Should().BeNull();
    }

    /// <summary>Verifies that malformed input returns null.</summary>
    /// <param name="input">The input string to check.</param>
    [Theory]
    [InlineData("ClassA")]
    [InlineData("ClassA -> ClassB")]      // Wrong arrow
    [InlineData("--> ClassB")]            // Missing source
    [InlineData("ClassA -->")]            // Missing target
    [InlineData("A --> B --> C")]         // Too many parts
    public void Parse_MalformedInput_ReturnsNull(string input)
    {
        var result = DependencyRelation.Parse(input);
        result.Should().BeNull();
    }

    // ─── ToMermaid ───

    /// <summary>Verifies that usage dependency is serialized to Mermaid arrow.</summary>
    [Fact]
    public void ToMermaid_UsageKind_ReturnsArrow()
    {
        var dep = new DependencyRelation("A", "B", DependencyKind.Usage);
        dep.ToMermaid().Should().Be("A --> B");
    }

    /// <summary>Verifies that inheritance dependency is serialized to Mermaid dotted arrow.</summary>
    [Fact]
    public void ToMermaid_InheritanceKind_ReturnsDottedArrow()
    {
        var dep = new DependencyRelation("A", "B", DependencyKind.Inheritance);
        dep.ToMermaid().Should().Be("A -.-> B");
    }

    // ─── ToPlantUml ───

    /// <summary>Verifies that usage dependency is serialized to PlantUML arrow.</summary>
    [Fact]
    public void ToPlantUml_UsageKind_ReturnsSolidArrow()
    {
        var dep = new DependencyRelation("A", "B", DependencyKind.Usage);
        dep.ToPlantUml().Should().Be("A --> B");
    }

    /// <summary>Verifies that inheritance dependency is serialized to PlantUML dotted arrow.</summary>
    [Fact]
    public void ToPlantUml_InheritanceKind_ReturnsDottedArrow()
    {
        var dep = new DependencyRelation("A", "B", DependencyKind.Inheritance);
        dep.ToPlantUml().Should().Be("A ..> B");
    }

    // ─── Roundtrip ───

    /// <summary>Verifies roundtrip parsing and serialization for usage dependencies.</summary>
    [Fact]
    public void Roundtrip_ParseThenToMermaid_PreservesUsageDependency()
    {
        var original = "ClassA --> ClassB";
        var parsed = DependencyRelation.Parse(original);
        parsed.Should().NotBeNull();
        parsed!.ToMermaid().Should().Be("ClassA --> ClassB");
    }

    /// <summary>Verifies roundtrip parsing and serialization for inheritance dependencies.</summary>
    [Fact]
    public void Roundtrip_ParseThenToMermaid_PreservesInheritanceDependency()
    {
        var original = "ClassA -.-> IService";
        var parsed = DependencyRelation.Parse(original);
        parsed.Should().NotBeNull();
        parsed!.ToMermaid().Should().Be("ClassA -.-> IService");
    }

    // ─── Record Equality ───

    /// <summary>Verifies that records with same values are equal.</summary>
    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new DependencyRelation("X", "Y", DependencyKind.Usage);
        var b = new DependencyRelation("X", "Y", DependencyKind.Usage);
        a.Should().Be(b);
    }

    /// <summary>Verifies that records with different values are not equal.</summary>
    [Fact]
    public void RecordEquality_DifferentKind_AreNotEqual()
    {
        var a = new DependencyRelation("X", "Y", DependencyKind.Usage);
        var b = new DependencyRelation("X", "Y", DependencyKind.Inheritance);
        a.Should().NotBe(b);
    }
}
