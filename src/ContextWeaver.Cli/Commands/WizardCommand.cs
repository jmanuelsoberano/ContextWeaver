using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ContextWeaver.Cli.Commands.Wizard;
using ContextWeaver.Reporters;
using ContextWeaver.Reporters.Sections;
using ContextWeaver.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

/// <summary>
///     Command that starts the interactive wizard to configure the analysis.
/// </summary>
public class WizardCommand : AsyncCommand<WizardSettings>
{
    private static readonly IReportSection[] _availableSections =
    {
        new HeaderSection(),
        new HotspotSection(),
        new InstabilitySection(),
        new MermaidDependencyGraphSection(),
        new PlantUmlDependencyGraphSection(),
        new MermaidModuleDiagramSection(),
        new PlantUmlModuleDiagramSection(),
        new DirectoryTreeSection(),
        new FileContentSection()
    };

    private readonly CodeAnalyzerService _service;
    private readonly SettingsProvider _settingsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WizardCommand"/> class.
    /// </summary>
    /// <param name="service">Code analyzer service.</param>
    /// <param name="settingsProvider">Settings provider.</param>
    public WizardCommand(CodeAnalyzerService service, SettingsProvider settingsProvider)
    {
        _service = service;
        _settingsProvider = settingsProvider;
    }

    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, WizardSettings settings, CancellationToken cancellationToken)
    {
        var directoryInfo = new DirectoryInfo(settings.Directory ?? ".");

        var wizardContext = new WizardContext(settings, directoryInfo);

        // Instantiate the workflow steps
        var steps = new List<IWizardStep>
        {
            new FileDiscoveryStep(_service),
            new FilterExtensionStep(),
            new SelectionModeStep(),
            new FileSelectionStep(),
            new SectionSelectionModeStep(),
            new SectionSelectionStep(_availableSections, _settingsProvider),
            new OutputConfigStep(),
            new SummaryStep(_availableSections)
        };

        var orchestrator = new WizardOrchestrator(steps);

        // Run Interactive Wizard Loop (allowing backwards navigation)
        var result = await orchestrator.ExecuteAsync(wizardContext);

        if (result != 0)
        {
            // Cancelled or errored in Wizard loop
            return result;
        }

        // Execute final action with gathered context
        var outputFile = new FileInfo(Path.Combine(directoryInfo.FullName, wizardContext.OutputFileName!));

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("Analizando archivos y generando reporte...", async ctx =>
            {
                await _service.AnalyzeFiles(
                    wizardContext.SelectedFiles,
                    directoryInfo,
                    outputFile,
                    wizardContext.OutputFormat!,
                    wizardContext.EnabledSections);
            });

        AnsiConsole.MarkupLine($"\n[green]✅ Reporte generado exitosamente en:[/] [link]{outputFile.FullName}[/]");

        return 0;
    }
}
