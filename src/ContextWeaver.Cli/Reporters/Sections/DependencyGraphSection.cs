using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera el gráfico global de dependencias de clases (Mermaid + PlantUML).
/// </summary>
public class DependencyGraphSection : IReportSection
{
    public string Render(ReportContext context)
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
                foreach (var dependency in result.ClassDependencies)
                {
                    var relation = DependencyRelation.Parse(dependency);
                    if (relation == null)
                        continue;

                    allDependencies.Add(dependency);
                    modules[moduleName].Add(relation.Source);

                    if (context.TypeKindMap.TryGetValue(relation.Target, out var targetKind) &&
                        targetKind == "interface")
                        interfaces.Add(relation.Target);
                }
        }

        if (allDependencies.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("# 📈 Gráfico de Dependencias de Clases");
        sb.AppendLine();
        sb.AppendLine(
            "Este gráfico visualiza las relaciones jerárquicas (línea punteada) y de colaboración (línea sólida) entre las clases del proyecto. Renderizado con Mermaid.js.");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TD;");
        sb.AppendLine();

        foreach (var module in modules.OrderBy(m => m.Key))
            if (module.Value.Any())
            {
                sb.AppendLine($"  subgraph {module.Key}");
                foreach (var className in module.Value.OrderBy(n => n))
                    sb.AppendLine($"    {className}");
                sb.AppendLine("  end");
                sb.AppendLine();
            }

        foreach (var dependency in allDependencies.OrderBy(d => d))
            sb.AppendLine($"  {dependency}");
        sb.AppendLine();

        if (interfaces.Any())
        {
            sb.AppendLine("  %% Estilos");
            sb.AppendLine("  classDef interface fill:#ccf,stroke:#333,stroke-width:2px");
            sb.AppendLine($"  class {string.Join(",", interfaces)} interface");
        }

        sb.AppendLine("```");
        sb.AppendLine();

        // PlantUML Version
        sb.AppendLine("### Alternativa: PlantUML");
        sb.AppendLine("```plantuml");
        sb.AppendLine("@startuml");
        sb.AppendLine("hide empty members");
        sb.AppendLine();

        foreach (var module in modules.OrderBy(m => m.Key))
            if (module.Value.Any())
            {
                sb.AppendLine($"package \"{module.Key}\" {{");
                foreach (var className in module.Value.OrderBy(n => n))
                {
                    var (keyword, stereotype) = DiagramHelper.GetPlantUMLMeta(className, context.TypeKindMap);
                    sb.AppendLine($"  {keyword} {className} {stereotype}");
                }

                sb.AppendLine("}");
                sb.AppendLine();
            }

        foreach (var dependency in allDependencies.OrderBy(d => d))
        {
            var plantUmlDep = dependency.Replace("-.->", "..>").Replace("-->", "-->");
            sb.AppendLine($"  {plantUmlDep}");
        }

        sb.AppendLine("@enduml");
        sb.AppendLine("```");
        sb.AppendLine();

        return sb.ToString();
    }
}
