using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera diagramas separados por cada módulo (carpeta de primer nivel).
/// </summary>
public class ModuleDiagramSection : IReportSection
{
    /// <inheritdoc />
    public string Name => "🧩 Diagramas por Módulo";

    /// <inheritdoc />
    public string Description => "Diagramas de dependencia por carpeta de primer nivel";

    /// <inheritdoc />
    public bool IsRequired => false;

    /// <inheritdoc />
    public string Render(ReportContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# 🧩 Diagramas de Módulo");
        sb.AppendLine();
        sb.AppendLine(
            "A continuación se presentan diagramas de dependencia detallados por cada módulo para facilitar la visualización.");
        sb.AppendLine();

        var modules = context.SortedResults
            .GroupBy(r => r.ModuleName)
            .OrderBy(g => g.Key);

        foreach (var moduleGroup in modules)
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
                sb.AppendLine($"## Módulo: {moduleName}");
                sb.AppendLine();

                // Mermaid
                sb.AppendLine("### Mermaid");
                sb.AppendLine("```mermaid");
                sb.AppendLine("graph TD;");
                foreach (var dep in moduleDependencies.OrderBy(d => d))
                    sb.AppendLine($"  {dep}");
                sb.AppendLine("```");
                sb.AppendLine();

                // PlantUML
                sb.AppendLine("### PlantUML");
                sb.AppendLine("```plantuml");
                sb.AppendLine($"@startuml {moduleName}");
                sb.AppendLine("hide empty members");

                foreach (var cls in relatedClasses.OrderBy(c => c))
                {
                    var (keyword, stereotype) = DiagramHelper.GetPlantUMLMeta(cls, context.TypeKindMap);
                    sb.AppendLine($"{keyword} {cls} {stereotype}");
                }

                sb.AppendLine();

                foreach (var dep in moduleDependencies.OrderBy(d => d))
                {
                    var plantUmlDep = dep.Replace("-.->", "..>").Replace("-->", "-->");
                    sb.AppendLine($"  {plantUmlDep}");
                }

                sb.AppendLine("@enduml");
                sb.AppendLine("```");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}
