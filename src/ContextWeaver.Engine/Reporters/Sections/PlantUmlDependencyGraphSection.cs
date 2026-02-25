using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera el gráfico global de dependencias de clases usando PlantUML.
/// </summary>
public class PlantUmlDependencyGraphSection : BaseDependencyGraphSection
{
    /// <inheritdoc />
    public override string Name => "📈 Grafo de Dependencias (PlantUML)";

    /// <inheritdoc />
    public override string Description => "Diagrama global de dependencias con PlantUML";

    /// <inheritdoc />
    protected override void RenderDiagram(StringBuilder sb, DependencyGraphData data, ReportContext context)
    {
        sb.AppendLine("# 📈 Gráfico de Dependencias de Clases (PlantUML)");
        sb.AppendLine();
        sb.AppendLine("Este gráfico visualiza las relaciones entre clases del proyecto mediante PlantUML. Requiere un renderizador compatible.");
        sb.AppendLine();
        sb.AppendLine("```plantuml");
        sb.AppendLine("@startuml");
        sb.AppendLine("hide empty members");
        sb.AppendLine();

        foreach (var module in data.Modules.OrderBy(m => m.Key))
        {
            if (module.Value.Count > 0)
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
        }

        foreach (var dependency in data.Dependencies.OrderBy(d => d))
        {
            var plantUmlDep = dependency.Replace("-.->", "..>").Replace("-->", "-->");
            sb.AppendLine($"  {plantUmlDep}");
        }

        sb.AppendLine("@enduml");
        sb.AppendLine("```");
        sb.AppendLine();
    }
}
