namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Represents the result of executing a wizard step.
/// </summary>
public enum StepResult
{
    /// <summary>
    ///     Proceed to the next step.
    /// </summary>
    Next,

    /// <summary>
    ///     Go back to the previous step.
    /// </summary>
    Previous,

    /// <summary>
    ///     Cancel the wizard execution.
    /// </summary>
    Cancel,

    /// <summary>
    ///     Finish the wizard successfully.
    /// </summary>
    Finish
}
