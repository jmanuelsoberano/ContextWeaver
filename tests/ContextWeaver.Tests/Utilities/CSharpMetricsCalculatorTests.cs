using ContextWeaver.Utilities;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ContextWeaver.Tests.Utilities;

/// <summary>
///     Tests for <see cref="CSharpMetricsCalculator"/>.
/// </summary>
public class CSharpMetricsCalculatorTests
{
    private static Microsoft.CodeAnalysis.SyntaxNode ParseRoot(string code)
        => CSharpSyntaxTree.ParseText(code).GetRoot();

    // ─── Cyclomatic Complexity ───

    /// <summary>Verifies that an empty method has a complexity of 1.</summary>
    [Fact]
    public void CyclomaticComplexity_EmptyMethod_Returns1()
    {
        var code = "class C { void M() { } }";
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(ParseRoot(code));
        result.Should().Be(1);
    }

    /// <summary>Verifies that a null root returns the default complexity of 1.</summary>
    [Fact]
    public void CyclomaticComplexity_NullRoot_Returns1()
    {
        var result = CSharpMetricsCalculator.CalculateCyclomaticComplexity(null!);
        result.Should().Be(1);
    }

    /// <summary>Verifies that a single if statement increases complexity by 1.</summary>
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

    /// <summary>Verifies that an if-else if structure increases complexity correctly.</summary>
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

    /// <summary>Verifies that a for loop increases complexity by 1.</summary>
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

    /// <summary>Verifies that a foreach loop increases complexity by 1.</summary>
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

    /// <summary>Verifies that a while loop increases complexity by 1.</summary>
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

    /// <summary>Verifies that a switch statement with cases increases complexity.</summary>
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
        result.Should().Be(4); // base(1) + 3 cases
    }

    /// <summary>Verifies that the ternary operator increases complexity by 1.</summary>
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

    /// <summary>Verifies that the logical AND operator increases complexity by 1.</summary>
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

    /// <summary>Verifies that the logical OR operator increases complexity by 1.</summary>
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

    /// <summary>Verifies that a complex method sums up various complexity increments correctly.</summary>
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

    // ─── Max Nesting Depth ───

    /// <summary>Verifies that a flat method has 0 depth.</summary>
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

    /// <summary>Verifies that a single if increases depth to 1.</summary>
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

    /// <summary>Verifies that nested structures increase depth correctly.</summary>
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

    /// <summary>Verifies that deeply nested structures are calculated correctly.</summary>
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
