using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Base abstract class for module-level diagram sections.
///     Encapsulates the logic to group files by module and extract dependencies.
/// </summary>
public abstract class BaseModuleDiagramSection : IReportSection
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
        var modules = BuildModuleData(context);

        if (modules.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        RenderPrologue(sb);

        foreach (var module in modules)
        {
            RenderModuleDiagram(sb, module, context);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Renders the introductory text for the section.
    /// </summary>
    protected abstract void RenderPrologue(StringBuilder sb);

    /// <summary>
    ///     Renders the diagram for a single module.
    /// </summary>
    protected abstract void RenderModuleDiagram(StringBuilder sb, ModuleDiagramData moduleData, ReportContext context);

    private static List<ModuleDiagramData> BuildModuleData(ReportContext context)
    {
        var resultList = new List<ModuleDiagramData>();

        var groupedModules = context.SortedResults
            .GroupBy(r => r.ModuleName)
            .OrderBy(g => g.Key);

        foreach (var moduleGroup in groupedModules)
        {
            var moduleName = moduleGroup.Key;
            var moduleFiles = moduleGroup.ToList();
            var moduleDependencies = new HashSet<string>();
            var relatedClasses = new HashSet<string>();

            foreach (var file in moduleFiles)
            {
                if (file.ClassDependencies != null)
                {
                    foreach (var dep in file.ClassDependencies)
                    {
                        var relation = DependencyRelation.Parse(dep);
                        if (relation == null)
                            continue;

                        moduleDependencies.Add(dep);
                        relatedClasses.Add(relation.Source);
                        relatedClasses.Add(relation.Target);
                    }
                }
            }

            if (moduleDependencies.Count > 0)
            {
                resultList.Add(new ModuleDiagramData(moduleName, moduleDependencies, relatedClasses));
            }
        }

        return resultList;
    }
}

/// <summary>
///     Data structure holding dependency information for a specialized module.
/// </summary>
public record ModuleDiagramData(
    string ModuleName,
    HashSet<string> Dependencies,
    HashSet<string> RelatedClasses);
