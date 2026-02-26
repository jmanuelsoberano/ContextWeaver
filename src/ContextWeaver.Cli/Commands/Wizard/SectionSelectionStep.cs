using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContextWeaver.Reporters;
using ContextWeaver.Services;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Step that handles the interactive or flag-based selection of report sections.
/// </summary>
public class SectionSelectionStep : IWizardStep
{
    private readonly IReadOnlyList<IReportSection> _availableSections;
    private readonly SettingsProvider _settingsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SectionSelectionStep"/> class.
    /// </summary>
    /// <param name="availableSections">The sections available for the report.</param>
    /// <param name="settingsProvider">The settings provider.</param>
    public SectionSelectionStep(IReadOnlyList<IReportSection> availableSections, SettingsProvider settingsProvider)
    {
        _availableSections = availableSections;
        _settingsProvider = settingsProvider;
    }

    /// <inheritdoc/>
    public bool ShouldExecute(WizardContext context) => true;

    /// <inheritdoc/>
    public Task<StepResult> ExecuteAsync(WizardContext context)
    {
        var optionalSections = _availableSections.Where(s => !s.IsRequired).ToList();
        List<string> enabledSectionNames;

        if (!string.IsNullOrEmpty(context.Settings.Sections))
        {
            var inputs = context.Settings.Sections
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            enabledSectionNames = new List<string>();
            foreach (var input in inputs)
            {
                var match = _availableSections.FirstOrDefault(s => s.Name.Contains(input, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    enabledSectionNames.Add(match.Name);
                }
            }

            context.EnabledSections = enabledSectionNames;
            return Task.FromResult(StepResult.Next); // Auto-advance because flag handles it
        }

        if (!string.IsNullOrEmpty(context.Settings.ExcludeSections))
        {
            var excludedInputs = context.Settings.ExcludeSections
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var excludedNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var input in excludedInputs)
            {
                var match = _availableSections.FirstOrDefault(s => s.Name.Contains(input, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    excludedNames.Add(match.Name);
                }
            }

            enabledSectionNames = optionalSections
                .Where(s => !excludedNames.Contains(s.Name))
                .Select(s => s.Name)
                .ToList();

            context.EnabledSections = enabledSectionNames;
            return Task.FromResult(StepResult.Next); // Auto-advance because flag handles it
        }

        // Interactive Mode
        var savedSections = context.Config?.EnabledSections != null
            ? new HashSet<string>(context.Config.EnabledSections, StringComparer.Ordinal)
            : null;

        var sectionPrompt = new MultiSelectionPrompt<string>()
            .Title("Seleccione las [green]secciones opcionales[/] que desea incluir en el reporte:\n[grey](Las secciones obligatorias como 'Header' se incluirán automáticamente)[/]")
            .PageSize(10)
            .MoreChoicesText("[grey](Muevase arriba y abajo para ver más secciones)[/]")
            .InstructionsText(
                "[grey]([blue]<espacio>[/] seleccionar/deseleccionar, [green]<enter>[/] confirmar)[/]");
        // Note: removed .Required() to let the user select 'Back' without forcing selection.
        if (!context.IsFirstInteractiveStep)
        {
            sectionPrompt.AddChoice(WizardConstants.BackOption);
        }

        foreach (var section in optionalSections)
        {
            var label = $"{section.Name} — {section.Description}";
            sectionPrompt.AddChoice(label);

            bool shouldSelect = false;

            if (context.ModeForSections == SectionSelectionMode.SavedOrDefault)
            {
                shouldSelect = savedSections == null || savedSections.Contains(section.Name);
            }
            else if (context.ModeForSections == SectionSelectionMode.All)
            {
                shouldSelect = true;
            }

            if (shouldSelect)
            {
                sectionPrompt.Select(label);
            }
        }

        if (savedSections != null && context.ModeForSections == SectionSelectionMode.SavedOrDefault)
        {
            AnsiConsole.MarkupLine("[grey]  (Se cargaron preferencias de secciones guardadas)[/]");
        }

        var selectedSectionLabels = AnsiConsole.Prompt(sectionPrompt);

        if (selectedSectionLabels.Contains(WizardConstants.BackOption))
        {
            return Task.FromResult(StepResult.Previous);
        }

        enabledSectionNames = selectedSectionLabels
            .Select(label => label.Split(" — ")[0])
            .ToList();

        var optionalSelectedCount = enabledSectionNames.Count;

        if (optionalSelectedCount == 0)
        {
            AnsiConsole.MarkupLine("[red]Debe seleccionar al menos una sección opcional. Operación cancelada.[/]");
            return Task.FromResult(StepResult.Cancel);
        }

        var allOptionalSelected = enabledSectionNames.Count >= optionalSections.Count;
        if (!allOptionalSelected)
        {
            var savePref = AnsiConsole.Confirm("¿Guardar estas preferencias de secciones para futuros análisis?", defaultValue: false);
            if (savePref)
            {
                if (context.Config == null)
                {
                    context.Config = new ContextWeaver.Core.AnalysisSettings();
                }

                context.Config.EnabledSections = enabledSectionNames.ToArray();
                _settingsProvider.SaveSettings(context.Directory, context.Config);
                AnsiConsole.MarkupLine("[green]Preferencias guardadas en .contextweaver.json[/]");
            }
        }

        context.EnabledSections = enabledSectionNames;
        context.IsFirstInteractiveStep = false;

        return Task.FromResult(StepResult.Next);
    }
}
