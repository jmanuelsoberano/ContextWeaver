using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Base abstract class for global dependency graph sections.
///     Encapsulates the logic to build the graph data from the analysis results.
/// </summary>
public abstract class BaseDependencyGraphSection : IReportSection
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public bool IsRequired => false;

    /// <inheritdoc />
    public string Render(ReportContext context)
    {
        var data = BuildGraphData(context);

        if (data.Dependencies.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        RenderDiagram(sb, data, context);
        return sb.ToString();
    }

    /// <summary>
    ///     Renders the specific diagram syntax (Mermaid, PlantUML, etc.)
    /// </summary>
    protected abstract void RenderDiagram(StringBuilder sb, DependencyGraphData data, ReportContext context);

    private static DependencyGraphData BuildGraphData(ReportContext context)
    {
        var allDependencies = new HashSet<string>();
        var modules = new Dictionary<string, HashSet<string>>();
        var interfaces = new HashSet<string>();

        foreach (var result in context.SortedResults)
        {
            var moduleName = result.ModuleName;
            if (!modules.ContainsKey(moduleName))
                modules[moduleName] = new HashSet<string>();

            if (result.ClassDependencies != null)
            {
                foreach (var dependency in result.ClassDependencies)
                {
                    var relation = DependencyRelation.Parse(dependency);
                    if (relation == null)
                        continue;

                    allDependencies.Add(dependency);
                    modules[moduleName].Add(relation.Source);

                    if (context.TypeKindMap.TryGetValue(relation.Target, out var targetKind) &&
                        targetKind == "interface")
                    {
                        interfaces.Add(relation.Target);
                    }
                }
            }
        }

        return new DependencyGraphData(modules, allDependencies, interfaces);
    }
}

/// <summary>
///     Data structure holding the processed graph information.
/// </summary>
public record DependencyGraphData(
    Dictionary<string, HashSet<string>> Modules,
    HashSet<string> Dependencies,
    HashSet<string> Interfaces);
