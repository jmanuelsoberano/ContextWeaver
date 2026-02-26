using System;
using System.Threading.Tasks;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Step that prompts the user to select the initial state of the file selection tree.
/// </summary>
public class SelectionModeStep : IWizardStep
{
    private const string OptionAll = "Todos seleccionados (deseleccionar lo que no quiero)";
    private const string OptionNone = "Ninguno seleccionado (seleccionar lo que quiero)";

    /// <inheritdoc/>
    public bool ShouldExecute(WizardContext context) => !context.Settings.All;

    /// <inheritdoc/>
    public Task<StepResult> ExecuteAsync(WizardContext context)
    {
        var selectionMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("¿Cómo desea empezar la [green]selección de archivos[/]?")
                .AddChoices(OptionAll, OptionNone, WizardConstants.BackOption));

        if (selectionMode == WizardConstants.BackOption)
        {
            return Task.FromResult(StepResult.Previous);
        }

        context.SelectAllFilesByDefault = selectionMode.StartsWith("Todos", StringComparison.Ordinal);

        return Task.FromResult(StepResult.Next);
    }
}
