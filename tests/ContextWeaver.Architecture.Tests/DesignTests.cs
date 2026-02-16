using ContextWeaver.Core;
using ContextWeaver.Services;
using NetArchTest.Rules;
using Xunit;

namespace ContextWeaver.Architecture.Tests;

public class DesignTests
{
    /// <summary>
    /// Regla: Todas las interfaces deben comenzar con 'I'.
    /// </summary>
    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        var result = Types.InAssembly(typeof(AnalysisSettings).Assembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        Assert.True(result.IsSuccessful, "Todas las interfaces deben comenzar con 'I'.");
    }

    /// <summary>
    /// Regla: Las clases en el namespace 'Analyzers' deben implementar IFileAnalyzer.
    /// </summary>
    [Fact]
    public void Analyzers_Should_Implement_IFileAnalyzer()
    {
        var result = Types.InAssembly(typeof(CodeAnalyzerService).Assembly)
            .That()
            .ResideInNamespace("ContextWeaver.Engine.Analyzers")
            .And()
            .AreClasses()
            .Should()
            .ImplementInterface(typeof(IFileAnalyzer))
            .GetResult();

        Assert.True(result.IsSuccessful, "Las clases en el namespace 'Analyzers' deben implementar IFileAnalyzer.");
    }

    /// <summary>
    /// Regla: Los servicios deben ser sellados (sealed) por defecto.
    /// Esto mejora el rendimiento (devirtualización) y comunica claramente que la clase no está diseñada para herencia.
    /// </summary>
    [Fact]
    public void Services_Should_Be_Sealed()
    {
        var result = Types.InAssembly(typeof(CodeAnalyzerService).Assembly)
            .That()
            .ResideInNamespace("ContextWeaver.Services")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, "Los servicios deberían ser sealed por defecto (Open/Closed Principle).");
    }

    /// <summary>
    /// Regla: Los modelos de dominio NO deben exponer campos públicos.
    /// Deben usar Propiedades para garantizar encapsulamiento.
    /// </summary>
    [Fact]
    public void Models_Should_Not_Have_Public_Fields()
    {
        var result = Types.InAssembly(typeof(AnalysisSettings).Assembly)
            .That()
            .ResideInNamespace("ContextWeaver.Core")
            .Should()
            .MeetCustomRule(new NoPublicFieldsRule())
            .GetResult();

        Assert.True(result.IsSuccessful, "Los modelos en Core no deben tener campos públicos (usar propiedades).");
    }
}
