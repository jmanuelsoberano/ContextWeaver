using ContextWeaver.Core;
using ContextWeaver.Services;
using NetArchTest.Rules;
using Xunit;

namespace ContextWeaver.Architecture.Tests;

public class DependencyTests
{
    private const string CoreNamespace = "ContextWeaver.Core";
    private const string EngineNamespace = "ContextWeaver.Engine";
    private const string CliNamespace = "ContextWeaver.Cli";

    /// <summary>
    /// Regla: Core NO debe depender de Engine ni de Cli.
    /// Core es el centro de la arquitectura y debe ser agnóstico.
    /// </summary>
    [Fact]
    public void Core_Should_Not_Depend_On_Engine_Or_Cli()
    {
        var result = Types.InAssembly(typeof(AnalysisSettings).Assembly)
            .ShouldNot()
            .HaveDependencyOn(EngineNamespace)
            .And()
            .HaveDependencyOn(CliNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Core no debe tener dependencias hacia capas exteriores (Engine/Cli).");
    }

    /// <summary>
    /// Regla: Engine NO debe depender de Cli.
    /// Engine contiene la lógica de negocio pero no sabe cómo se entrega (CLI, Web, etc).
    /// </summary>
    [Fact]
    public void Engine_Should_Not_Depend_On_Cli()
    {
        var result = Types.InAssembly(typeof(CodeAnalyzerService).Assembly)
            .ShouldNot()
            .HaveDependencyOn(CliNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Engine no debe depender de la capa de presentación (Cli).");
    }

    /// <summary>
    /// Regla: Core NO debe depender de System.Console.
    /// Core no debe escribir directamente en la consola.
    /// </summary>
    [Fact]
    public void Core_Should_Not_Use_System_Console()
    {
        var result = Types.InAssembly(typeof(AnalysisSettings).Assembly)
            .ShouldNot()
            .HaveDependencyOn("System.Console")
            .GetResult();

        Assert.True(result.IsSuccessful, "Core no debe escribir en System.Console directamente.");
    }
}
