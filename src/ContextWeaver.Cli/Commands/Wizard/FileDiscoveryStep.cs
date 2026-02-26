using System.Threading.Tasks;
using ContextWeaver.Services;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Step responsible for discovering managed files in the base directory.
/// </summary>
public class FileDiscoveryStep : IWizardStep
{
    private readonly CodeAnalyzerService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileDiscoveryStep"/> class.
    /// </summary>
    /// <param name="service">The code analyzer service.</param>
    public FileDiscoveryStep(CodeAnalyzerService service)
    {
        _service = service;
    }

    /// <inheritdoc/>
    public bool IsInteractive => false;

    /// <inheritdoc/>
    public bool ShouldExecute(WizardContext context) => true; // Always execute first

    /// <inheritdoc/>
    public Task<StepResult> ExecuteAsync(WizardContext context)
    {
        var (files, config) = _service.GetManagedFiles(context.Directory);

        if (files.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No se encontraron archivos gestionados en el directorio especificado.[/]");
            return Task.FromResult(StepResult.Cancel);
        }

        context.DiscoveredFiles = new System.Collections.Generic.List<System.IO.FileInfo>(files);
        context.ManagedFiles = new System.Collections.Generic.List<System.IO.FileInfo>(files);
        context.Config = config;

        return Task.FromResult(StepResult.Next);
    }
}
