using System.Text;
using ContextWeaver.Core;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera el gráfico global de dependencias de clases usando Mermaid.js.
/// </summary>
public class MermaidDependencyGraphSection : BaseDependencyGraphSection
{
    /// <inheritdoc />
    public override string Name => "📈 Grafo de Dependencias (Mermaid)";

    /// <inheritdoc />
    public override string Description => "Diagrama global de dependencias con Mermaid.js";

    /// <inheritdoc />
    protected override void RenderDiagram(StringBuilder sb, DependencyGraphData data, ReportContext context)
    {
        sb.AppendLine("# 📈 Gráfico de Dependencias de Clases (Mermaid)");
        sb.AppendLine();
        sb.AppendLine(
            "Este gráfico visualiza las relaciones jerárquicas (línea punteada) y de colaboración (línea sólida) entre las clases del proyecto. Renderizado con Mermaid.js.");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TD;");
        sb.AppendLine();

        foreach (var module in data.Modules.OrderBy(m => m.Key))
        {
            if (module.Value.Count > 0)
            {
                sb.AppendLine($"  subgraph {module.Key}");
                foreach (var className in module.Value.OrderBy(n => n))
                {
                    sb.AppendLine($"    {className}");
                }

                sb.AppendLine("  end");
                sb.AppendLine();
            }
        }

        foreach (var dependency in data.Dependencies.OrderBy(d => d))
        {
            sb.AppendLine($"  {dependency}");
        }

        sb.AppendLine();

        if (data.Interfaces.Count > 0)
        {
            sb.AppendLine("  %% Estilos");
            sb.AppendLine("  classDef interface fill:#ccf,stroke:#333,stroke-width:2px");
            sb.AppendLine($"  class {string.Join(",", data.Interfaces)} interface");
        }

        sb.AppendLine("```");
        sb.AppendLine();
    }
}
