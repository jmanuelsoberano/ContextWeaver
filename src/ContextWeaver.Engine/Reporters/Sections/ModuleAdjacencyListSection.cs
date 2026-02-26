using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera una lista de adyacencia de módulos en formato YAML.
///     Ideal para ser procesada por LLMs.
/// </summary>
public class ModuleAdjacencyListSection : IReportSection
{
    private const string Indent = "  ";

    /// <inheritdoc />
    public string Name => "🔗 Lista de Adyacencia de Módulos (YAML)";

    /// <inheritdoc />
    public string Description => "Dependencias entre módulos en formato estructurado (YAML)";

    /// <inheritdoc />
    public bool IsRequired => false;

    /// <inheritdoc />
    public string Render(ReportContext context)
    {
        var modules = BuildModuleAdjacency(context);

        if (modules.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("# Lista de Adyacencia de Módulos");
        sb.AppendLine();
        sb.AppendLine("```yaml");

        foreach (var module in modules.OrderBy(m => m.Key))
        {
            sb.AppendLine($"{module.Key}:");

            if (module.Value.Count > 0)
            {
                foreach (var dep in module.Value.OrderBy(d => d))
                {
                    sb.AppendLine($"{Indent}- {dep}");
                }
            }
            else
            {
                sb.AppendLine($"{Indent}[]");
            }
        }

        sb.AppendLine("```");
        sb.AppendLine();

        return sb.ToString();
    }

    private static Dictionary<string, HashSet<string>> BuildModuleAdjacency(ReportContext context)
    {
        var adjacencyList = new Dictionary<string, HashSet<string>>();

        // Initialize all modules
        foreach (var result in context.SortedResults)
        {
            if (!adjacencyList.ContainsKey(result.ModuleName))
            {
                adjacencyList[result.ModuleName] = new HashSet<string>();
            }
        }

        // Populate dependencies
        foreach (var result in context.SortedResults)
        {
            var sourceModule = result.ModuleName;

            if (result.ClassDependencies != null)
            {
                foreach (var dependency in result.ClassDependencies)
                {
                    var relation = DependencyRelation.Parse(dependency);
                    if (relation == null)
                        continue;

                    // Find the target module
                    var targetResult = context.SortedResults.FirstOrDefault(r =>
                        r.DefinedTypes != null && r.DefinedTypes.Contains(relation.Target));

                    if (targetResult != null)
                    {
                        var targetModule = targetResult.ModuleName;

                        // We only care about inter-module dependencies
                        if (sourceModule != targetModule)
                        {
                            adjacencyList[sourceModule].Add(targetModule);
                        }
                    }
                }
            }
        }

        return adjacencyList;
    }
}
