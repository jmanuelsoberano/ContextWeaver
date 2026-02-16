namespace ContextWeaver.Reporters;

/// <summary>
///     Contract for a self-contained report section.
///     Each implementation renders one logical block of the Markdown report.
/// </summary>
public interface IReportSection
{
    /// <summary>
    ///     Renders the section content using the provided report context.
    /// </summary>
    /// <param name="context">Context data available for report generation.</param>
    /// <returns>The rendered markdown string for this section.</returns>
    string Render(ReportContext context);
}
