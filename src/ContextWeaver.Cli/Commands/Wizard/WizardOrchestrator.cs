using System.Collections.Generic;
using System.Threading.Tasks;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Orchestrates the execution of a series of wizard steps.
/// </summary>
public class WizardOrchestrator
{
    private readonly IReadOnlyList<IWizardStep> _steps;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WizardOrchestrator"/> class.
    /// </summary>
    /// <param name="steps">The sequence of steps to execute.</param>
    public WizardOrchestrator(IReadOnlyList<IWizardStep> steps)
    {
        _steps = steps;
    }

    /// <summary>
    ///     Executes the wizard steps in order, allowing for backwards navigation.
    /// </summary>
    /// <param name="context">The wizard context.</param>
    /// <returns>1 if cancelled or failed, 0 if successful.</returns>
    public async Task<int> ExecuteAsync(WizardContext context)
    {
        var history = new Stack<int>();
        int currentIndex = 0;

        while (currentIndex >= 0 && currentIndex < _steps.Count)
        {
            var step = _steps[currentIndex];

            if (!step.ShouldExecute(context))
            {
                // Skip this step and move forward
                currentIndex++;
                continue;
            }

            var result = await step.ExecuteAsync(context);

            switch (result)
            {
                case StepResult.Next:
                    history.Push(currentIndex);
                    currentIndex++;
                    break;
                case StepResult.Previous:
                    if (history.Count > 0)
                    {
                        currentIndex = history.Pop();
                    }
                    else
                    {
                        // Cannot go back from the first step
                        AnsiConsole.MarkupLine("[yellow]No se puede retroceder más.[/]");
                    }

                    break;
                case StepResult.Cancel:
                    AnsiConsole.MarkupLine("[yellow]Operación cancelada por el usuario.[/]");
                    return 1;
                case StepResult.Finish:
                    return 0; // Terminate early with success
            }
        }

        return 0;
    }
}
