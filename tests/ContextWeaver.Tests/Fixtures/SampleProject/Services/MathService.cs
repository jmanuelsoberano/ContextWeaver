using SampleApp;

namespace SampleApp;

/// <summary>
///     A sample service that uses <see cref="Calculator"/> to perform operations.
/// </summary>
public class MathService
{
    private readonly Calculator _calculator;

    /// <summary>Initializes a new instance of the <see cref="MathService"/> class.</summary>
    /// <param name="calculator">The calculator instance.</param>
    public MathService(Calculator calculator)
    {
        _calculator = calculator;
    }

    /// <summary>Sums two integers using the calculator.</summary>
    /// <param name="a">First integer.</param>
    /// <param name="b">Second integer.</param>
    /// <returns>The sum.</returns>
    public int Sum(int a, int b) => _calculator.Add(a, b);
}
