using SampleApp;

namespace SampleApp;

/// <summary>
///     Una clase de servicio que usa la calculadora.
/// </summary>
public class MathService
{
    private readonly Calculator _calculator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MathService"/> class.
    /// </summary>
    /// <param name="calculator">La instancia de la calculadora.</param>
    public MathService(Calculator calculator)
    {
        _calculator = calculator;
    }

    /// <summary>
    ///     Calcula la suma de dos números a través de la calculadora.
    /// </summary>
    /// <param name="a">Primer entero.</param>
    /// <param name="b">Segundo entero.</param>
    /// <returns>El resultado.</returns>
    public int ComputeSum(int a, int b) => _calculator.Add(a, b);
}
