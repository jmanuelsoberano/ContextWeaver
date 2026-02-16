using System.Text;
using ContextWeaver.Core;
using ContextWeaver.Interfaces;
using ContextWeaver.Reporters.Sections;

namespace ContextWeaver.Reporters;

/// <summary>
///     PATRÓN DE DISEÑO: Concrete Strategy (Estrategia Concreta) + Composite.
///     Implementación de IReportGenerator que orquesta secciones independientes
///     para construir un reporte en formato Markdown.
///     Cada sección implementa <see cref="IReportSection"/> y se renderiza en orden.
/// </summary>
public class MarkdownReportGenerator : IReportGenerator
{
    private readonly IReportSection[] _sections =
    {
        new HeaderSection(),
        new HotspotSection(),
        new InstabilitySection(),
        new DependencyGraphSection(),
        new ModuleDiagramSection(),
        new DirectoryTreeSection(),
        new FileContentSection()
    };

    public string Format => "markdown";

    public string Generate(DirectoryInfo directory, List<FileAnalysisResult> results,
        Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics)
    {
        var sortedResults = results.OrderBy(r => r.RelativePath).ToList();
        var typeKindMap = BuildTypeKindMap(sortedResults);

        var context = new ReportContext(directory, sortedResults, instabilityMetrics, typeKindMap);

        var reportBuilder = new StringBuilder();
        foreach (var section in _sections)
            reportBuilder.Append(section.Render(context));

        return reportBuilder.ToString();
    }

    private static Dictionary<string, string> BuildTypeKindMap(List<FileAnalysisResult> results)
    {
        var map = new Dictionary<string, string>();
        foreach (var result in results)
        {
            if (result.DefinedTypeKinds != null)
            {
                foreach (var kvp in result.DefinedTypeKinds)
                    map[kvp.Key] = kvp.Value;
            }
        }

        return map;
    }
}