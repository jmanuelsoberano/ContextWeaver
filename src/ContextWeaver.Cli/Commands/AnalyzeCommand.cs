using System.Threading;
using System.Threading.Tasks;
using ContextWeaver.Services;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

/// <summary>
///     Command to execute automatic analysis without interaction.
/// </summary>
public class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    private readonly CodeAnalyzerService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnalyzeCommand"/> class.
    /// </summary>
    /// <param name="service">Code analyzer service.</param>
    public AnalyzeCommand(CodeAnalyzerService service)
    {
        _service = service;
    }

    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings, CancellationToken cancellationToken)
    {
        var directoryInfo = new DirectoryInfo(settings.Directory ?? ".");
        var fileInfo = new FileInfo(settings.Output ?? "analysis_report.md");
        var format = settings.Format ?? "markdown";

        await _service.AnalyzeAndGenerateReport(directoryInfo, fileInfo, format);

        return 0;
    }
}
