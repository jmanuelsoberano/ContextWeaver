using ContextWeaver.Core;
using ContextWeaver.Utilities;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Engine.Tests.Utilities;

/// <summary>
///     Pruebas para <see cref="InstabilityCalculator"/>.
/// </summary>
public class InstabilityCalculatorTests
{
    // ─── Helpers ───

    private static FileAnalysisResult CreateResult(string relativePath, List<string>? definedTypes = null,
        List<string>? classDependencies = null)
    {
        return new FileAnalysisResult
        {
            RelativePath = relativePath,
            DefinedTypes = definedTypes ?? new List<string>(),
            ClassDependencies = classDependencies ?? new List<string>()
        };
    }

    // ─── Escenarios Básicos ───

    /// <summary>Verifica que los resultados vacíos retornen un diccionario vacío.</summary>
    [Fact]
    public void Calculate_EmptyResults_ReturnsEmptyDictionary()
    {
        var results = new List<FileAnalysisResult>();
        var metrics = InstabilityCalculator.Calculate(results);
        metrics.Should().BeEmpty();
    }

    /// <summary>Verifica que un solo módulo sin dependencias tenga cero inestabilidad.</summary>
    [Fact]
    public void Calculate_SingleModuleNoDependencies_ReturnsZeroInstability()
    {
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Core/ClassA.cs", definedTypes: new List<string> { "ClassA" })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        metrics.Should().ContainKey("Core");
        metrics["Core"].Ca.Should().Be(0);
        metrics["Core"].Ce.Should().Be(0);
        metrics["Core"].Instability.Should().Be(0.0);
    }

    /// <summary>Verifica el cálculo correcto de Ca y Ce para dos módulos con una dependencia.</summary>
    [Fact]
    public void Calculate_TwoModulesWithOneDependency_CorrectCaAndCe()
    {
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Services/ServiceA.cs",
                definedTypes: new List<string> { "ServiceA" },
                classDependencies: new List<string> { "ServiceA --> HelperB" }),
            CreateResult("Utilities/HelperB.cs",
                definedTypes: new List<string> { "HelperB" })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        // Services depende de Utilities → Ce=1 para Services
        metrics["Services"].Ce.Should().Be(1);
        metrics["Services"].Ca.Should().Be(0);
        // Utilities es dependido por Services → Ca=1 para Utilities
        metrics["Utilities"].Ca.Should().Be(1);
        metrics["Utilities"].Ce.Should().Be(0);
    }

    /// <summary>Verifica que un módulo totalmente inestable (I=1) se calcule correctamente.</summary>
    [Fact]
    public void Calculate_FullyUnstableModule_ReturnsInstability1()
    {
        // Un módulo que solo depende de otros pero nadie depende de él
        var results = new List<FileAnalysisResult>
        {
            CreateResult("UI/View.cs",
                definedTypes: new List<string> { "View" },
                classDependencies: new List<string> { "View --> Service" }),
            CreateResult("Services/Service.cs",
                definedTypes: new List<string> { "Service" })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        // UI tiene Ce=1, Ca=0 → I = 1/(0+1) = 1.0
        metrics["UI"].Instability.Should().Be(1.0);
    }

    /// <summary>Verifica que un módulo totalmente estable (I=0) se calcule correctamente.</summary>
    [Fact]
    public void Calculate_FullyStableModule_ReturnsInstability0()
    {
        // Un módulo del cual se depende pero no depende de nada
        var results = new List<FileAnalysisResult>
        {
            CreateResult("UI/View.cs",
                definedTypes: new List<string> { "View" },
                classDependencies: new List<string> { "View --> CoreHelper" }),
            CreateResult("Core/CoreHelper.cs",
                definedTypes: new List<string> { "CoreHelper" })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        // Core tiene Ce=0, Ca=1 → I = 0/(1+0) = 0.0
        metrics["Core"].Instability.Should().Be(0.0);
    }

    // ─── Dependencias Intra-módulo ───

    /// <summary>Verifica que las dependencias dentro del mismo módulo no cuenten para Ce.</summary>
    [Fact]
    public void Calculate_SameModuleDependency_DoesNotCountAsEfferent()
    {
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Core/ClassA.cs",
                definedTypes: new List<string> { "ClassA" },
                classDependencies: new List<string> { "ClassA --> ClassB" }),
            CreateResult("Core/ClassB.cs",
                definedTypes: new List<string> { "ClassB" })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        // Ambos en "Core" → sin dependencia eferente
        metrics["Core"].Ce.Should().Be(0);
        metrics["Core"].Ca.Should().Be(0);
    }

    // ─── Dependencias de Herencia ───

    /// <summary>Verifica que las dependencias de herencia cuenten como dependencias eferentes.</summary>
    [Fact]
    public void Calculate_InheritanceDependency_CountsAsEfferent()
    {
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Services/MyService.cs",
                definedTypes: new List<string> { "MyService" },
                classDependencies: new List<string> { "MyService -.-> IService" }),
            CreateResult("Interfaces/IService.cs",
                definedTypes: new List<string> { "IService" })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        metrics["Services"].Ce.Should().Be(1);
        metrics["Interfaces"].Ca.Should().Be(1);
    }

    // ─── Módulo Raíz ───

    /// <summary>Verifica que los archivos en el nivel raíz se asignen a un módulo "Root".</summary>
    [Fact]
    public void Calculate_FileAtRoot_UsesRootModuleName()
    {
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Program.cs", definedTypes: new List<string> { "Program" })
        };

        var metrics = InstabilityCalculator.Calculate(results);
        metrics.Should().ContainKey("Root");
    }

    // ─── Dependencias Múltiples ───

    /// <summary>Verifica el cálculo de inestabilidad para un módulo con múltiples dependencias.</summary>
    [Fact]
    public void Calculate_ModuleWithMultipleDependencies_CorrectInstability()
    {
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Services/Orchestrator.cs",
                definedTypes: new List<string> { "Orchestrator" },
                classDependencies: new List<string>
                {
                    "Orchestrator --> HelperA",
                    "Orchestrator --> HelperB"
                }),
            CreateResult("Utilities/HelperA.cs",
                definedTypes: new List<string> { "HelperA" }),
            CreateResult("Utilities/HelperB.cs",
                definedTypes: new List<string> { "HelperB" })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        // Services depende de Utilities (1 módulo destino único) → Ce=1
        metrics["Services"].Ce.Should().Be(1);
        // I = 1/(0+1) = 1.0
        metrics["Services"].Instability.Should().Be(1.0);
    }

    // ─── Negativos / Casos Borde ───

    /// <summary>Verifica que una lista de dependencias de clase nula no cause una excepción.</summary>
    [Fact]
    public void Calculate_NullClassDependencies_DoesNotThrow()
    {
        var results = new List<FileAnalysisResult>
        {
            new()
            {
                RelativePath = "Core/ClassA.cs",
                DefinedTypes = new List<string> { "ClassA" },
                ClassDependencies = null!
            }
        };

        // No debería lanzar excepción; las dependencias nulas se omiten
        var act = () => InstabilityCalculator.Calculate(results);
        act.Should().NotThrow();
    }

    /// <summary>Verifica que una lista de tipos definidos nula no cause una excepción.</summary>
    [Fact]
    public void Calculate_NullDefinedTypes_DoesNotThrow()
    {
        var results = new List<FileAnalysisResult>
        {
            new()
            {
                RelativePath = "Core/ClassA.cs",
                DefinedTypes = null!,
                ClassDependencies = new List<string>()
            }
        };

        var act = () => InstabilityCalculator.Calculate(results);
        act.Should().NotThrow();
    }

    /// <summary>Verifica que las dependencias a tipos desconocidos se ignoren.</summary>
    [Fact]
    public void Calculate_DependencyToUnknownType_IsIgnoredGracefully()
    {
        // ClassA depende de "Phantom" que no está definido en ningún archivo
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Core/ClassA.cs",
                definedTypes: new List<string> { "ClassA" },
                classDependencies: new List<string> { "ClassA --> Phantom" })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        // "Phantom" no pertenece a ningún módulo, así que no se cuenta eferente
        metrics["Core"].Ce.Should().Be(0);
        metrics["Core"].Ca.Should().Be(0);
    }

    /// <summary>Verifica que las cadenas de dependencia mal formadas se ignoren.</summary>
    [Fact]
    public void Calculate_MalformedDependencyString_IsIgnoredGracefully()
    {
        // Una cadena de dependencia que no puede ser parseada por DependencyRelation.Parse
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Core/ClassA.cs",
                definedTypes: new List<string> { "ClassA" },
                classDependencies: new List<string> { "not a valid dependency", string.Empty, "   " })
        };

        var metrics = InstabilityCalculator.Calculate(results);

        // Todavía debería producir un resultado para el módulo, solo que con 0 dependencias
        metrics.Should().ContainKey("Core");
        metrics["Core"].Ce.Should().Be(0);
    }
}

