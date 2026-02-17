using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera diagramas por módulo (carpeta de primer nivel) usando Mermaid.js.
/// </summary>
public class MermaidModuleDiagramSection : BaseModuleDiagramSection
{
    /// <inheritdoc />
    public override string Name => "🧩 Diagramas por Módulo (Mermaid)";

    /// <inheritdoc />
    public override string Description => "Diagramas de dependencia por carpeta con Mermaid.js";

    /// <inheritdoc />
    protected override void RenderPrologue(StringBuilder sb)
    {
        sb.AppendLine("# 🧩 Diagramas de Módulo (Mermaid)");
        sb.AppendLine();
        sb.AppendLine(
            "A continuación se presentan diagramas de dependencia detallados por cada módulo usando Mermaid.");
        sb.AppendLine();
    }

    /// <inheritdoc />
    protected override void RenderModuleDiagram(StringBuilder sb, ModuleDiagramData moduleData, ReportContext context)
    {
        sb.AppendLine($"## Módulo: {moduleData.ModuleName}");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TD;");
        foreach (var dep in moduleData.Dependencies.OrderBy(d => d))
        {
            sb.AppendLine($"  {dep}");
        }

        sb.AppendLine("```");
        sb.AppendLine();
    }
}
