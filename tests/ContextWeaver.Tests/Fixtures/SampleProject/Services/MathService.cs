using SampleApp;

namespace SampleApp;

public class MathService
{
    private readonly Calculator _calculator;

    public MathService(Calculator calculator)
    {
        _calculator = calculator;
    }

    public int Sum(int a, int b) => _calculator.Add(a, b);
}
