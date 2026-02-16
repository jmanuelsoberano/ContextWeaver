using ContextWeaver.Core;
using ContextWeaver.Utilities;
using FluentAssertions;
using Xunit;

namespace ContextWeaver.Tests.Utilities;

public class InstabilityCalculatorTests
{
    private readonly InstabilityCalculator _calculator = new();

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

    // ─── Basic Scenarios ───

    [Fact]
    public void Calculate_EmptyResults_ReturnsEmptyDictionary()
    {
        var results = new List<FileAnalysisResult>();
        var metrics = _calculator.Calculate(results);
        metrics.Should().BeEmpty();
    }

    [Fact]
    public void Calculate_SingleModuleNoDependencies_ReturnsZeroInstability()
    {
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Core/ClassA.cs", definedTypes: new List<string> { "ClassA" })
        };

        var metrics = _calculator.Calculate(results);

        metrics.Should().ContainKey("Core");
        metrics["Core"].Ca.Should().Be(0);
        metrics["Core"].Ce.Should().Be(0);
        metrics["Core"].Instability.Should().Be(0.0);
    }

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

        var metrics = _calculator.Calculate(results);

        // Services depends on Utilities → Ce=1 for Services
        metrics["Services"].Ce.Should().Be(1);
        metrics["Services"].Ca.Should().Be(0);
        // Utilities is depended upon by Services → Ca=1 for Utilities
        metrics["Utilities"].Ca.Should().Be(1);
        metrics["Utilities"].Ce.Should().Be(0);
    }

    [Fact]
    public void Calculate_FullyUnstableModule_ReturnsInstability1()
    {
        // A module that only depends on others but nobody depends on it
        var results = new List<FileAnalysisResult>
        {
            CreateResult("UI/View.cs",
                definedTypes: new List<string> { "View" },
                classDependencies: new List<string> { "View --> Service" }),
            CreateResult("Services/Service.cs",
                definedTypes: new List<string> { "Service" })
        };

        var metrics = _calculator.Calculate(results);

        // UI has Ce=1, Ca=0 → I = 1/(0+1) = 1.0
        metrics["UI"].Instability.Should().Be(1.0);
    }

    [Fact]
    public void Calculate_FullyStableModule_ReturnsInstability0()
    {
        // A module that is depended upon but depends on nothing
        var results = new List<FileAnalysisResult>
        {
            CreateResult("UI/View.cs",
                definedTypes: new List<string> { "View" },
                classDependencies: new List<string> { "View --> CoreHelper" }),
            CreateResult("Core/CoreHelper.cs",
                definedTypes: new List<string> { "CoreHelper" })
        };

        var metrics = _calculator.Calculate(results);

        // Core has Ce=0, Ca=1 → I = 0/(1+0) = 0.0
        metrics["Core"].Instability.Should().Be(0.0);
    }

    // ─── Intra-module Dependencies ───

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

        var metrics = _calculator.Calculate(results);

        // Both in "Core" → no efferent dependency
        metrics["Core"].Ce.Should().Be(0);
        metrics["Core"].Ca.Should().Be(0);
    }

    // ─── Inheritance Dependencies ───

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

        var metrics = _calculator.Calculate(results);

        metrics["Services"].Ce.Should().Be(1);
        metrics["Interfaces"].Ca.Should().Be(1);
    }

    // ─── Root Module ───

    [Fact]
    public void Calculate_FileAtRoot_UsesRootModuleName()
    {
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Program.cs", definedTypes: new List<string> { "Program" })
        };

        var metrics = _calculator.Calculate(results);
        metrics.Should().ContainKey("Root");
    }

    // ─── Multiple Dependencies ───

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

        var metrics = _calculator.Calculate(results);

        // Services depends on Utilities (1 unique target module) → Ce=1
        metrics["Services"].Ce.Should().Be(1);
        // I = 1/(0+1) = 1.0
        metrics["Services"].Instability.Should().Be(1.0);
    }

    // ─── Negative / Edge Cases ───

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

        // Should not throw; null dependencies are skipped
        var act = () => _calculator.Calculate(results);
        act.Should().NotThrow();
    }

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

        var act = () => _calculator.Calculate(results);
        act.Should().NotThrow();
    }

    [Fact]
    public void Calculate_DependencyToUnknownType_IsIgnoredGracefully()
    {
        // ClassA depends on "Phantom" which is not defined in any file
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Core/ClassA.cs",
                definedTypes: new List<string> { "ClassA" },
                classDependencies: new List<string> { "ClassA --> Phantom" })
        };

        var metrics = _calculator.Calculate(results);

        // "Phantom" doesn't belong to any module, so no efferent counted
        metrics["Core"].Ce.Should().Be(0);
        metrics["Core"].Ca.Should().Be(0);
    }

    [Fact]
    public void Calculate_MalformedDependencyString_IsIgnoredGracefully()
    {
        // A dependency string that can't be parsed by DependencyRelation.Parse
        var results = new List<FileAnalysisResult>
        {
            CreateResult("Core/ClassA.cs",
                definedTypes: new List<string> { "ClassA" },
                classDependencies: new List<string> { "not a valid dependency", "", "   " })
        };

        var metrics = _calculator.Calculate(results);

        // Should still produce a result for the module, just with 0 dependencies
        metrics.Should().ContainKey("Core");
        metrics["Core"].Ce.Should().Be(0);
    }
}

