using System.Threading.Tasks;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Defines a step in the interactive wizard.
/// </summary>
public interface IWizardStep
{
    /// <summary>
    ///     Gets a value indicating whether this step has an interactive UI.
    ///     Used to determine if backwards navigation is possible from subsequent steps.
    /// </summary>
    bool IsInteractive => true;

    /// <summary>
    ///     Determines if this step should be executed based on the current context.
    /// </summary>
    /// <param name="context">The wizard context.</param>
    /// <returns>True if the step should execute, false to skip it.</returns>
    bool ShouldExecute(WizardContext context);

    /// <summary>
    ///     Executes the wizard step, potentially interacting with the user and mutating the context.
    /// </summary>
    /// <param name="context">The wizard context.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the step.</returns>
    Task<StepResult> ExecuteAsync(WizardContext context);
}
