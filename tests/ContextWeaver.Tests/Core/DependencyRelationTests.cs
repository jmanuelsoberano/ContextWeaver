using ContextWeaver.Core;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Core;

/// <summary>Pruebas para <see cref="DependencyRelation"/>.</summary>
public class DependencyRelationTests
{
    // ─── Parse: Happy Path ───

    /// <summary>Verifica que una dependencia de uso se parsee correctamente.</summary>
    [Fact]
    public void Parse_UsageDependency_ReturnsDependencyWithUsageKind()
    {
        var result = DependencyRelation.Parse("ClassA --> ClassB");

        result.Should().NotBeNull();
        result!.Source.Should().Be("ClassA");
        result.Target.Should().Be("ClassB");
        result.Kind.Should().Be(DependencyKind.Usage);
    }

    /// <summary>Verifica que una dependencia de herencia se parsee correctamente.</summary>
    [Fact]
    public void Parse_InheritanceDependency_ReturnsDependencyWithInheritanceKind()
    {
        var result = DependencyRelation.Parse("ClassA -.-> IDisposable");

        result.Should().NotBeNull();
        result!.Source.Should().Be("ClassA");
        result.Target.Should().Be("IDisposable");
        result.Kind.Should().Be(DependencyKind.Inheritance);
    }

    /// <summary>Verifica que los espacios en blanco se eliminen del origen y destino.</summary>
    [Fact]
    public void Parse_TrimsWhitespace_FromSourceAndTarget()
    {
        var result = DependencyRelation.Parse("  ClassA   -->   ClassB  ");

        result.Should().NotBeNull();
        result!.Source.Should().Be("ClassA");
        result.Target.Should().Be("ClassB");
    }

    // ─── Parsear: Entrada Nula / Inválida ───

    /// <summary>Verifica que una entrada nula o con espacios en blanco retorne null.</summary>
    /// <param name="input">La cadena de entrada a verificar.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullOrWhitespace_ReturnsNull(string? input)
    {
        var result = DependencyRelation.Parse(input!);
        result.Should().BeNull();
    }

    /// <summary>Verifica que una entrada mal formada retorne null.</summary>
    /// <param name="input">La cadena de entrada a verificar.</param>
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

    // ─── A Mermaid ───

    /// <summary>Verifica que una dependencia de uso se serialice a una flecha de Mermaid.</summary>
    [Fact]
    public void ToMermaid_UsageKind_ReturnsArrow()
    {
        var dep = new DependencyRelation("A", "B", DependencyKind.Usage);
        dep.ToMermaid().Should().Be("A --> B");
    }

    /// <summary>Verifica que una dependencia de herencia se serialice a una flecha punteada de Mermaid.</summary>
    [Fact]
    public void ToMermaid_InheritanceKind_ReturnsDottedArrow()
    {
        var dep = new DependencyRelation("A", "B", DependencyKind.Inheritance);
        dep.ToMermaid().Should().Be("A -.-> B");
    }

    // ─── A PlantUml ───

    /// <summary>Verifica que una dependencia de uso se serialice a una flecha de PlantUML.</summary>
    [Fact]
    public void ToPlantUml_UsageKind_ReturnsSolidArrow()
    {
        var dep = new DependencyRelation("A", "B", DependencyKind.Usage);
        dep.ToPlantUml().Should().Be("A --> B");
    }

    /// <summary>Verifica que una dependencia de herencia se serialice a una flecha punteada de PlantUML.</summary>
    [Fact]
    public void ToPlantUml_InheritanceKind_ReturnsDottedArrow()
    {
        var dep = new DependencyRelation("A", "B", DependencyKind.Inheritance);
        dep.ToPlantUml().Should().Be("A ..> B");
    }

    // ─── Ida y Vuelta ───

    /// <summary>Verifica el parseo y serialización de ida y vuelta para dependencias de uso.</summary>
    [Fact]
    public void Roundtrip_ParseThenToMermaid_PreservesUsageDependency()
    {
        var original = "ClassA --> ClassB";
        var parsed = DependencyRelation.Parse(original);
        parsed.Should().NotBeNull();
        parsed!.ToMermaid().Should().Be("ClassA --> ClassB");
    }

    /// <summary>Verifica el parseo y serialización de ida y vuelta para dependencias de herencia.</summary>
    [Fact]
    public void Roundtrip_ParseThenToMermaid_PreservesInheritanceDependency()
    {
        var original = "ClassA -.-> IService";
        var parsed = DependencyRelation.Parse(original);
        parsed.Should().NotBeNull();
        parsed!.ToMermaid().Should().Be("ClassA -.-> IService");
    }

    // ─── Igualdad de Registros ───

    /// <summary>Verifica que los registros con los mismos valores sean iguales.</summary>
    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new DependencyRelation("X", "Y", DependencyKind.Usage);
        var b = new DependencyRelation("X", "Y", DependencyKind.Usage);
        a.Should().Be(b);
    }

    /// <summary>Verifica que los registros con valores diferentes no sean iguales.</summary>
    [Fact]
    public void RecordEquality_DifferentKind_AreNotEqual()
    {
        var a = new DependencyRelation("X", "Y", DependencyKind.Usage);
        var b = new DependencyRelation("X", "Y", DependencyKind.Inheritance);
        a.Should().NotBe(b);
    }
}
