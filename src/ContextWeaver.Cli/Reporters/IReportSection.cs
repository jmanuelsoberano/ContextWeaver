namespace ContextWeaver.Reporters;

/// <summary>
///     Contract for a self-contained report section.
///     Each implementation renders one logical block of the Markdown report.
/// </summary>
public interface IReportSection
{
    string Render(ReportContext context);
}
