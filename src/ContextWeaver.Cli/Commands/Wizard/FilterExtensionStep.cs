using System;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Step that prompts the user to filter files by extension.
/// </summary>
public class FilterExtensionStep : IWizardStep
{
    /// <inheritdoc/>
    public bool ShouldExecute(WizardContext context)
        => !context.Settings.All && context.DiscoveredFiles.Select(f => f.Extension.ToLowerInvariant()).Distinct().Count() > 1;

    /// <inheritdoc/>
    public Task<StepResult> ExecuteAsync(WizardContext context)
    {
        var extensions = context.DiscoveredFiles
            .Select(f => f.Extension.ToLowerInvariant())
            .Distinct()
            .OrderBy(e => e)
            .ToList();

        var extPrompt = new MultiSelectionPrompt<string>()
            .Title("¿Desea filtrar por [green]extensión[/]? (deseleccione las que no necesite)")
            .PageSize(15)
            .InstructionsText(
                "[grey]([blue]<espacio>[/] seleccionar/deseleccionar, [green]<enter>[/] confirmar)[/]\n[yellow]⚠️ ATENCIÓN: Si desea Volver, primero debe MARCAR la opción '[/][blue]🔙[/][yellow]' con <espacio>.[/]");

        if (context.ShowBackButton)
        {
            extPrompt.AddChoice(WizardConstants.BackOption);
        }

        foreach (var ext in extensions)
        {
            var count = context.DiscoveredFiles.Count(f => f.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase));
            var choice = $"{ext} ({count} archivos)";
            extPrompt.AddChoice(choice);

            // PRE-SELECT only if it's currently in ManagedFiles (remembers previous selection natively)
            if (context.ManagedFiles.Any(f => f.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            {
                extPrompt.Select(choice);
            }
        }

        var selectedExtLabels = AnsiConsole.Prompt(extPrompt);

        if (selectedExtLabels.Contains(WizardConstants.BackOption))
        {
            return Task.FromResult(StepResult.Previous);
        }

        var selectedExtensions = selectedExtLabels
            .Select(label => label.Split(' ')[0])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Update the managed files based on selection from the full DiscoveredFiles list
        context.ManagedFiles = context.DiscoveredFiles
            .Where(f => selectedExtensions.Contains(f.Extension.ToLowerInvariant()))
            .ToList();

        if (context.ManagedFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No hay archivos con las extensiones seleccionadas. Operación cancelada.[/]");
            return Task.FromResult(StepResult.Cancel);
        }

        return Task.FromResult(StepResult.Next);
    }
}
