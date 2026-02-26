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
        if (string.IsNullOrEmpty(context.Settings.Output))
        {
            var fileNamePrompt = new TextPrompt<string>("Ingrese el nombre del [green]archivo de salida[/] (o '<' para volver):")
                .DefaultValue(context.OutputFileName ?? "context.md")
                .Validate(name =>
                    string.IsNullOrWhiteSpace(name)
                        ? ValidationResult.Error("[red]El nombre del archivo no puede estar vacío[/]")
                        : ValidationResult.Success());

            var outputFileName = AnsiConsole.Prompt(fileNamePrompt);

            if (outputFileName == "<")
            {
                return Task.FromResult(StepResult.Previous);
            }

            context.OutputFileName = outputFileName;
        }
        else
        {
            context.OutputFileName = context.Settings.Output;
        }

        if (string.IsNullOrEmpty(context.Settings.Format))
        {
            var formatPrompt = new SelectionPrompt<string>()
                .Title("Seleccione el [green]formato de salida[/]:")
                .PageSize(4)
                .AddChoices(WizardConstants.BackOption)
                .AddChoices(_supportedFormats);

            var format = AnsiConsole.Prompt(formatPrompt);

            if (format == WizardConstants.BackOption)
            {
                // Note: since this is the second prompt in the step, going back could mean
                // going back to the file name prompt. For simplicity in SRP, if they abort the format,
                // we treat it as going back to the previous step entirely. Alternatively, we could
                // loop inside this step, but keeping logic flat is preferred.
                return Task.FromResult(StepResult.Previous);
            }

            context.OutputFormat = format;
        }
        else
        {
            context.OutputFormat = context.Settings.Format;
        }

        return Task.FromResult(StepResult.Next);
    }
}
