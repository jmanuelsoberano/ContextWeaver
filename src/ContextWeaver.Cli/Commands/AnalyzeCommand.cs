using System.Threading;
using System.Threading.Tasks;
using ContextWeaver.Services;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

public class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    private readonly CodeAnalyzerService _service;

    public AnalyzeCommand(CodeAnalyzerService service)
    {
        _service = service;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings, CancellationToken cancellationToken)
    {
        var directoryInfo = new DirectoryInfo(settings.Directory ?? ".");
        var fileInfo = new FileInfo(settings.Output ?? "analysis_report.md");
        var format = settings.Format ?? "markdown";

        await _service.AnalyzeAndGenerateReport(directoryInfo, fileInfo, format);

        return 0;
    }
}
