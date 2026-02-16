using System.Text;
using ContextWeaver.Utilities;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera la sección de Hotspots: top 5 archivos por LOC y por imports.
/// </summary>
public class HotspotSection : IReportSection
{
    /// <inheritdoc />
    public string Render(ReportContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# 🔥 Análisis de Hotspots");
        sb.AppendLine();

        // --- Top 5 por Líneas de Código (LOC) ---
        sb.AppendLine("## 5 Principales Archivos por Líneas de Código (LOC)");
        var topByLoc = context.SortedResults.OrderByDescending(r => r.LinesOfCode).Take(5);
        foreach (var result in topByLoc)
        {
            var headerText = $"File: {result.RelativePath}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            sb.AppendLine($"* **({result.LinesOfCode} LOC)** - [`{result.RelativePath}`](#{anchor})");
        }

        sb.AppendLine();

        // --- Top 5 por Número de Imports ---
        sb.AppendLine("## 5 Principales Archivos por Número de Importaciones");
        var topByImports = context.SortedResults
            .Select(r => new { Result = r, ImportCount = r.Usings.Count })
            .Where(x => x.ImportCount > 0)
            .OrderByDescending(x => x.ImportCount)
            .Take(5);

        foreach (var item in topByImports)
        {
            var headerText = $"File: {item.Result.RelativePath}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            sb.AppendLine($"* **({item.ImportCount} Imports)** - [`{item.Result.RelativePath}`](#{anchor})");
        }

        sb.AppendLine();
        return sb.ToString();
    }
}
