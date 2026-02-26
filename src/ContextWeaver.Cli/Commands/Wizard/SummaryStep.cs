using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContextWeaver.Reporters;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Final step that displays a summary of the selected configuration and asks for confirmation.
/// </summary>
public class SummaryStep : IWizardStep
{
    private readonly IReadOnlyList<IReportSection> _availableSections;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SummaryStep"/> class.
    /// </summary>
    /// <param name="availableSections">The sections available for the report.</param>
    public SummaryStep(IReadOnlyList<IReportSection> availableSections)
    {
        _availableSections = availableSections;
    }

    /// <inheritdoc/>
    public bool ShouldExecute(WizardContext context) => true;

    /// <inheritdoc/>
    public Task<StepResult> ExecuteAsync(WizardContext context)
    {
        var requiredSectionNames = _availableSections
            .Where(s => s.IsRequired)
            .Select(s => s.Name);

        var allSectionNames = requiredSectionNames.Concat(context.EnabledSections).Distinct().ToList();

        var outputFile = new FileInfo(Path.Combine(context.Directory.FullName, context.OutputFileName!));

        var summaryTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Configuración[/]")
            .AddColumn("[bold]Valor[/]");

        summaryTable.AddRow("📂 Archivos seleccionados", $"[green]{context.SelectedFiles.Count}[/]");
        summaryTable.AddRow("📝 Secciones del reporte", string.Join("\n", allSectionNames.Select(n => $"  • {n}")));
        summaryTable.AddRow("💾 Archivo de salida", $"[blue]{outputFile.FullName}[/]");
        summaryTable.AddRow("📄 Formato", $"[blue]{context.OutputFormat}[/]");

        AnsiConsole.Write(new Rule("[yellow]Resumen[/]").RuleStyle("grey"));
        AnsiConsole.Write(summaryTable);
        AnsiConsole.WriteLine();

        if (AnsiConsole.Profile.Capabilities.Interactive)
        {
            var summaryPrompt = new SelectionPrompt<string>()
                .Title("¿Desea continuar con la ejecución?")
                .AddChoices("✅ Sí, ejecutar");

            if (!context.IsFirstInteractiveStep)
            {
                summaryPrompt.AddChoice(WizardConstants.BackOption);
            }

            summaryPrompt.AddChoice("❌ No, cancelar");

            var confirmChoice = AnsiConsole.Prompt(summaryPrompt);

            if (confirmChoice == WizardConstants.BackOption)
            {
                return Task.FromResult(StepResult.Previous);
            }

            if (confirmChoice.Contains("cancelar"))
            {
                return Task.FromResult(StepResult.Cancel);
            }
        }

        context.IsFirstInteractiveStep = false;

        return Task.FromResult(StepResult.Finish);
    }
}
