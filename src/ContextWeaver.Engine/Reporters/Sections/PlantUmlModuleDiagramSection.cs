using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera diagramas por módulo (carpeta de primer nivel) usando PlantUML.
/// </summary>
public class PlantUmlModuleDiagramSection : BaseModuleDiagramSection
{
    /// <inheritdoc />
    public override string Name => "🧩 Diagramas por Módulo (PlantUML)";

    /// <inheritdoc />
    public override string Description => "Diagramas de dependencia por carpeta con PlantUML";

    /// <inheritdoc />
    protected override void RenderPrologue(StringBuilder sb)
    {
        sb.AppendLine("# 🧩 Diagramas de Módulo (PlantUML)");
        sb.AppendLine();
        sb.AppendLine(
            "A continuación se presentan diagramas de dependencia detallados por cada módulo usando PlantUML.");
        sb.AppendLine();
    }

    /// <inheritdoc />
    protected override void RenderModuleDiagram(StringBuilder sb, ModuleDiagramData moduleData, ReportContext context)
    {
        sb.AppendLine($"## Módulo: {moduleData.ModuleName}");
        sb.AppendLine();
        sb.AppendLine("```plantuml");
        sb.AppendLine($"@startuml {moduleData.ModuleName}");
        sb.AppendLine("hide empty members");

        foreach (var cls in moduleData.RelatedClasses.OrderBy(c => c))
        {
            var (keyword, stereotype) = DiagramHelper.GetPlantUMLMeta(cls, context.TypeKindMap);
            sb.AppendLine($"{keyword} {cls} {stereotype}");
        }

        sb.AppendLine();

        foreach (var dep in moduleData.Dependencies.OrderBy(d => d))
        {
            var plantUmlDep = dep.Replace("-.->", "..>").Replace("-->", "-->");
            sb.AppendLine($"  {plantUmlDep}");
        }

        sb.AppendLine("@enduml");
        sb.AppendLine("```");
        sb.AppendLine();
    }
}
