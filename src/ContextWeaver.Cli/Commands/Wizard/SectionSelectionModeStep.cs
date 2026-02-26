using System;
using System.Threading.Tasks;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Step that prompts the user to select how they want to start the section selection.
/// </summary>
public class SectionSelectionModeStep : IWizardStep
{
    private static readonly string[] BulkSelectionOptions =
    {
        "Usar selección por defecto / guardada",
        "Seleccionar TODAS las secciones opcionales",
        "Seleccionar NINGUNA sección opcional (empezar limpio)"
    };

    /// <inheritdoc/>
    public bool ShouldExecute(WizardContext context)
        => string.IsNullOrEmpty(context.Settings.Sections) && string.IsNullOrEmpty(context.Settings.ExcludeSections);

    /// <inheritdoc/>
    public Task<StepResult> ExecuteAsync(WizardContext context)
    {
        var selectionMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("¿Cómo desea comenzar la selección de secciones?")
                .AddChoices(BulkSelectionOptions[0], BulkSelectionOptions[1], BulkSelectionOptions[2], WizardConstants.BackOption));

        if (selectionMode == WizardConstants.BackOption)
        {
            return Task.FromResult(StepResult.Previous);
        }

        if (selectionMode.StartsWith(BulkSelectionOptions[0], StringComparison.Ordinal))
        {
            context.ModeForSections = SectionSelectionMode.SavedOrDefault;
        }
        else if (selectionMode.StartsWith(BulkSelectionOptions[1], StringComparison.Ordinal))
        {
            context.ModeForSections = SectionSelectionMode.All;
        }
        else
        {
            context.ModeForSections = SectionSelectionMode.None;
        }

        return Task.FromResult(StepResult.Next);
    }
}
