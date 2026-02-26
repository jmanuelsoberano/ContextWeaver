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
        bool movingBackward = false;

        var logPath = System.IO.Path.Combine(context.Directory.FullName, "wizard-debug.log");
        if (!System.IO.File.Exists(logPath))
        {
            System.IO.File.WriteAllText(logPath, $"--- Inicia Wizard ({System.DateTime.Now}) ---\n");
        }

        while (currentIndex >= 0 && currentIndex < _steps.Count)
        {
            var step = _steps[currentIndex];

            // If we are moving backwards, we MUST execute the step we landed on,
            // because it was already executed previously (it's in history).
            if (!movingBackward && !step.ShouldExecute(context))
            {
                System.IO.File.AppendAllText(logPath, $"[Salto] Index {currentIndex} ({step.GetType().Name}) omitido (ShouldExecute=false)\n");
                // Skip this step and move forward
                currentIndex++;
                continue;
            }

            // Calculate if the Back button should be shown.
            // It should be shown if there is ANY step in history that is Interactive.
            context.ShowBackButton = System.Linq.Enumerable.Any(history, idx => _steps[idx].IsInteractive);

            System.IO.File.AppendAllText(logPath, $"[Entra] Index {currentIndex} ({step.GetType().Name}) | History={history.Count} | ShowBack={context.ShowBackButton} | MovingBack={movingBackward}\n");

            movingBackward = false;

            var result = await step.ExecuteAsync(context);

            System.IO.File.AppendAllText(logPath, $"[Sale]  Index {currentIndex} ({step.GetType().Name}) | Result={result}\n");

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
                        movingBackward = true;
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
