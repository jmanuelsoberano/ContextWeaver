using System.Threading.Tasks;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Step that prompts the user for the output file name and format.
/// </summary>
public class OutputConfigStep : IWizardStep
{
    private static readonly string[] _supportedFormats = { "markdown", "json", "xml" };

    /// <inheritdoc/>
    public bool ShouldExecute(WizardContext context) => true;

    /// <inheritdoc/>
    public Task<StepResult> ExecuteAsync(WizardContext context)
    {
        if (string.IsNullOrEmpty(context.Settings.Format))
        {
            var formatPrompt = new SelectionPrompt<string>()
                .Title("Seleccione el [green]formato de salida[/]:")
                .PageSize(4);

            if (!context.IsFirstInteractiveStep)
            {
                formatPrompt.AddChoice(WizardConstants.BackOption);
            }

            formatPrompt.AddChoices(_supportedFormats);

            var format = AnsiConsole.Prompt(formatPrompt);

            if (format == WizardConstants.BackOption)
            {
                return Task.FromResult(StepResult.Previous);
            }

            context.OutputFormat = format;
            context.IsFirstInteractiveStep = false;
        }
        else
        {
            context.OutputFormat = context.Settings.Format;
        }

        if (string.IsNullOrEmpty(context.Settings.Output))
        {
            var fileNamePrompt = new TextPrompt<string>("Ingrese el nombre del [green]archivo de salida[/]:")
                .DefaultValue(context.OutputFileName ?? "context.md")
                .Validate(name =>
                    string.IsNullOrWhiteSpace(name)
                        ? ValidationResult.Error("[red]El nombre del archivo no puede estar vacío[/]")
                        : ValidationResult.Success());

            var outputFileName = AnsiConsole.Prompt(fileNamePrompt);

            context.OutputFileName = outputFileName;
            context.IsFirstInteractiveStep = false;
        }
        else
        {
            context.OutputFileName = context.Settings.Output;
        }

        context.IsFirstInteractiveStep = false;

        return Task.FromResult(StepResult.Next);
    }
}
