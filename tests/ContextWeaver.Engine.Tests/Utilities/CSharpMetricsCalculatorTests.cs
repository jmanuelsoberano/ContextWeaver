using ContextWeaver.Utilities;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ContextWeaver.Engine.Tests.Utilities;

/// <summary>
///     Pruebas para <see cref="CSharpMetricsCalculator"/>.
/// </summary>
public class CSharpMetricsCalculatorTests
{
    private static Microsoft.CodeAnalysis.SyntaxNode ParseRoot(string code)
        => CSharpSyntaxTree.ParseText(code).GetRoot();

    // ─── Complejidad Ciclomática ───

    /// <summary>Verifica que un método vacío tenga una complejidad de 1.</summary>
    [Fact]
    public void CyclomaticComplexity_EmptyMethod_Returns1()
    {
        var code = "class C { void M() { } }";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(1);
    }

    /// <summary>Verifica que una raíz nula retorne la complejidad por defecto de 1.</summary>
    [Fact]
    public void CyclomaticComplexity_NullRoot_Returns1()
    {
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(null!);
        result.Should().Be(1);
    }

    /// <summary>Verifica que una sentencia if simple incremente la complejidad en 1.</summary>
    [Fact]
    public void CyclomaticComplexity_SingleIf_Returns2()
    {
        var code = @"
class C {
    void M(bool x) {
        if (x) { }
    }
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(2);
    }

    /// <summary>Verifica que una estructura if-else if incremente la complejidad correctamente.</summary>
    [Fact]
    public void CyclomaticComplexity_IfElseIf_Returns3()
    {
        var code = @"
class C {
    void M(int x) {
        if (x > 0) { }
        else if (x < 0) { }
        else { }
    }
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(3); // base(1) + if(1) + else-if(1)
    }

    /// <summary>Verifica que un bucle for incremente la complejidad en 1.</summary>
    [Fact]
    public void CyclomaticComplexity_ForLoop_Returns2()
    {
        var code = @"
class C {
    void M() {
        for (int i = 0; i < 10; i++) { }
    }
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(2);
    }

    /// <summary>Verifica que un bucle foreach incremente la complejidad en 1.</summary>
    [Fact]
    public void CyclomaticComplexity_ForEach_Returns2()
    {
        var code = @"
class C {
    void M(int[] items) {
        foreach (var x in items) { }
    }
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(2);
    }

    /// <summary>Verifica que un bucle while incremente la complejidad en 1.</summary>
    [Fact]
    public void CyclomaticComplexity_While_Returns2()
    {
        var code = @"
class C {
    void M(bool x) {
        while (x) { break; }
    }
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(2);
    }

    /// <summary>Verifica que una sentencia switch con casos incremente la complejidad.</summary>
    [Fact]
    public void CyclomaticComplexity_SwitchWithThreeCases_Returns4()
    {
        var code = @"
class C {
    void M(int x) {
        switch (x) {
            case 1: break;
            case 2: break;
            case 3: break;
        }
    }
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(4); // base(1) + 3 casos
    }

    /// <summary>Verifica que el operador ternario incremente la complejidad en 1.</summary>
    [Fact]
    public void CyclomaticComplexity_TernaryOperator_Returns2()
    {
        var code = @"
class C {
    int M(bool x) => x ? 1 : 0;
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(2);
    }

    /// <summary>Verifica que el operador lógico AND incremente la complejidad en 1.</summary>
    [Fact]
    public void CyclomaticComplexity_LogicalAnd_Returns3()
    {
        var code = @"
class C {
    void M(bool a, bool b) {
        if (a && b) { }
    }
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(3); // base(1) + if(1) + &&(1)
    }

    /// <summary>Verifica que el operador lógico OR incremente la complejidad en 1.</summary>
    [Fact]
    public void CyclomaticComplexity_LogicalOr_Returns3()
    {
        var code = @"
class C {
    void M(bool a, bool b) {
        if (a || b) { }
    }
}";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(3); // base(1) + if(1) + ||(1)
    }

    /// <summary>Verifica que un método complejo sume correctamente varios incrementos de complejidad.</summary>
    [Fact]
    public void CyclomaticComplexity_ComplexMethod_CountsAllBranches()
    {
        var code = @"
class C {
    void M(int x, bool flag) {
        if (x > 0) {
            for (int i = 0; i < x; i++) {
                if (flag && i > 5) { }
            }
        }
        while (x-- > 0) { }
    }
}";
        // base(1) + if(1) + for(1) + if(1) + &&(1) + while(1) = 6
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(6);
    }

    // ─── Profundidad Máxima de Anidamiento ───

    /// <summary>Verifica que un método plano tenga una profundidad de 0.</summary>
    [Fact]
    public void MaxNestingDepth_FlatMethod_Returns0()
    {
        var code = @"
class C {
    void M() {
        var x = 1;
    }
}";
        var result = CSharpMetricsCalculator.CalculateMaxNestingDepth(ParseRoot(code));
        result.Should().Be(0);
    }

    /// <summary>Verifica que un solo if incremente la profundidad a 1.</summary>
    [Fact]
    public void MaxNestingDepth_SingleIf_Returns1()
    {
        var code = @"
class C {
    void M(bool x) {
        if (x) {
            var a = 1;
        }
    }
}";
        var result = CSharpMetricsCalculator.CalculateMaxNestingDepth(ParseRoot(code));
        result.Should().Be(1);
    }

    /// <summary>Verifica que las estructuras anidadas incrementen la profundidad correctamente.</summary>
    [Fact]
    public void MaxNestingDepth_NestedIfInFor_Returns2()
    {
        var code = @"
class C {
    void M() {
        for (int i = 0; i < 10; i++) {
            if (i > 5) {
                var x = i;
            }
        }
    }
}";
        var result = CSharpMetricsCalculator.CalculateMaxNestingDepth(ParseRoot(code));
        result.Should().Be(2);
    }

    /// <summary>Verifica que las estructuras profundamente anidadas se calculen correctamente.</summary>
    [Fact]
    public void MaxNestingDepth_DeeplyNested_Returns4()
    {
        var code = @"
class C {
    void M(bool a) {
        if (a) {
            foreach (var x in new[]{1}) {
                while (true) {
                    try {
                        break;
                    } catch { }
                }
            }
        }
    }
}";
        // if(1) + foreach(2) + while(3) + try(4)
        var result = CSharpMetricsCalculator.CalculateMaxNestingDepth(ParseRoot(code));
        result.Should().Be(4);
    }
}
