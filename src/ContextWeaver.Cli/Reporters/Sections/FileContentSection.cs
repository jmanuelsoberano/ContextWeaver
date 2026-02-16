using System.Text;
using ContextWeaver.Core;
using ContextWeaver.Utilities;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera el contenido de todos los archivos con métricas, Repo Map, y código fuente.
/// </summary>
public class FileContentSection : IReportSection
{
    public string Render(ReportContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Archivos");
        sb.AppendLine();

        foreach (var result in context.SortedResults)
        {
            sb.AppendLine($"## File: {result.RelativePath}");
            sb.AppendLine();

            // Diagrama de Contexto
            sb.Append(RenderFileContextDiagram(result, context.TypeKindMap));

            // Referencias Entrantes ("Used By")
            if (result.IncomingDependencies != null && result.IncomingDependencies.Any())
            {
                var usedByFiles = result.IncomingDependencies
                    .Select(d => DependencyRelation.Parse(d))
                    .Where(r => r != null)
                    .Select(r => r!.Source)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                if (usedByFiles.Any())
                {
                    sb.AppendLine($"**Used By:** {string.Join(", ", usedByFiles)}");
                    sb.AppendLine();
                }
            }

            // Repo Map: Public API
            if (result.Metrics.PublicApiSignatures.Any())
            {
                var publicApi = result.Metrics.PublicApiSignatures;
                sb.AppendLine("### Repo Map: Extraer solo firmas públicas y imports de cada archivo");
                sb.AppendLine("#### API Publica:");
                foreach (var signature in publicApi)
                {
                    sb.AppendLine(signature);

                    var lineTrimmed = signature.TrimStart('-', ' ');
                    var firstWord = lineTrimmed.Split(' ').FirstOrDefault();
                    if (DiagramHelper.IsTypeKeyword(firstWord))
                    {
                        var parts = lineTrimmed.Split(new[] { ' ', ':' },
                            StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var typeName = parts[1];
                            if (result.DefinedTypeSemantics != null &&
                                result.DefinedTypeSemantics.TryGetValue(typeName, out var semantics))
                            {
                                if (semantics.Modifiers.Any())
                                    sb.AppendLine(
                                        $"    - Modifiers: {string.Join(", ", semantics.Modifiers)}");
                                if (semantics.Attributes.Any())
                                    sb.AppendLine(
                                        $"    - Attributes: {string.Join(", ", semantics.Attributes)}");
                                if (semantics.Interfaces.Any())
                                    sb.AppendLine(
                                        $"    - Implements: {string.Join(", ", semantics.Interfaces)}");
                            }
                        }
                    }
                }

                sb.AppendLine();
            }

            // Imports
            if (result.Usings.Any())
            {
                sb.AppendLine("#### Imports:");
                foreach (var singleUsing in result.Usings)
                    sb.AppendLine($"- {singleUsing}");
                sb.AppendLine();
            }

            // Métricas
            sb.AppendLine("#### Métricas");
            sb.AppendLine($"* **Lineas de Código (LOC):** {result.LinesOfCode}");
            if (result.Metrics.CyclomaticComplexity.HasValue)
                sb.AppendLine($"* **CyclomaticComplexity:** {result.Metrics.CyclomaticComplexity.Value}");
            if (result.Metrics.MaxNestingDepth.HasValue)
                sb.AppendLine($"* **MaxNestingDepth:** {result.Metrics.MaxNestingDepth.Value}");
            sb.AppendLine();

            // Código Fuente
            sb.AppendLine("#### Source Code");
            sb.AppendLine("```" + result.Language);
            sb.AppendLine(result.CodeContent.Trim());
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string RenderFileContextDiagram(
        FileAnalysisResult result, Dictionary<string, string> typeKindMap)
    {
        var connections = new HashSet<string>();

        // Salientes
        if (result.ClassDependencies != null)
            foreach (var dep in result.ClassDependencies)
                connections.Add(dep);

        // Entrantes
        if (result.IncomingDependencies != null && result.DefinedTypes != null)
            foreach (var incoming in result.IncomingDependencies)
            {
                var myMainType = result.DefinedTypes.FirstOrDefault() ??
                                 Path.GetFileNameWithoutExtension(result.RelativePath);
                connections.Add($"{incoming} --> {myMainType}");
            }

        if (connections.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("#### Contexto");

        // Mermaid
        sb.AppendLine("##### Mermaid");
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph LR;");
        foreach (var conn in connections.OrderBy(c => c))
            sb.AppendLine($"  {conn}");

        if (result.DefinedTypes != null)
            foreach (var type in result.DefinedTypes)
                sb.AppendLine($"  style {type} fill:#f9f,stroke:#333,stroke-width:2px");
        sb.AppendLine("```");

        // PlantUML
        sb.AppendLine("##### PlantUML");
        sb.AppendLine("```plantuml");
        sb.AppendLine("@startuml");
        sb.AppendLine("left to right direction");
        sb.AppendLine("hide empty members");

        var participants = new HashSet<string>();
        foreach (var conn in connections)
        {
            var relation = DependencyRelation.Parse(conn);
            if (relation == null)
                continue;
            participants.Add(relation.Source);
            participants.Add(relation.Target);
        }

        foreach (var p in participants.OrderBy(x => x))
        {
            var (keyword, stereotype) = DiagramHelper.GetPlantUMLMeta(p, typeKindMap);
            if (result.DefinedTypes != null && result.DefinedTypes.Contains(p))
                sb.AppendLine($"{keyword} {p} {stereotype} #Pink");
            else
                sb.AppendLine($"{keyword} {p} {stereotype}");
        }

        foreach (var conn in connections.OrderBy(c => c))
        {
            var pUml = conn.Replace("-->", "-->").Replace("-.->", "..>");
            sb.AppendLine($"  {pUml}");
        }

        sb.AppendLine("@enduml");
        sb.AppendLine("```");
        sb.AppendLine();

        return sb.ToString();
    }
}
