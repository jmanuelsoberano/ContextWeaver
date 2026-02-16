using System.Text;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera la sección de Análisis de Inestabilidad con la métrica I de Robert C. Martin.
/// </summary>
public class InstabilitySection : IReportSection
{
    public string Render(ReportContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# 📊 Análisis de Inestabilidad");
        sb.AppendLine();
        sb.AppendLine(
            "Esta sección estima la métrica de Inestabilidad (I) para cada módulo de nivel superior (carpeta/proyecto) basándose en sus dependencias (importaciones).");
        sb.AppendLine("`I = Ce / (Ca + Ce)`");
        sb.AppendLine("- `Ce` (Eferente): Cuántos otros módulos usa este módulo (apunta hacia afuera).");
        sb.AppendLine(
            "- `Ca` (Aferente): Cuántos otros módulos dependen de este módulo (apunta hacia adentro).");
        sb.AppendLine();
        sb.AppendLine("## Resumen de Inestabilidad del Módulo:");
        sb.AppendLine();
        sb.AppendLine("| Módulo | Ca (Eferente) | Ce (Aferente) | Inestabilidad (I) | Descripción |");
        sb.AppendLine("|---|---|---|---|---|");

        foreach (var entry in context.InstabilityMetrics.OrderBy(e => e.Key))
        {
            var module = entry.Key;
            var (ca, ce, instability) = entry.Value;
            var description = GetInstabilityDescription(instability);
            sb.AppendLine($"| `{module}` | {ca} | {ce} | {instability:F2} | {description} |");
        }

        sb.AppendLine();

        sb.AppendLine("## Guía de Interpretación:");
        sb.AppendLine(
            "- `I ≈ 0`: Muy estable (muchos dependen de él; depende poco de otros). A menudo son contratos/interfaces principales.");
        sb.AppendLine(
            "- `I ≈ 1`: Muy inestable (depende de muchos; pocos o ninguno dependen de él). A menudo son implementaciones concretas como UI/adaptadores.");
        sb.AppendLine("- `I ≈ 0.5`: Estabilidad intermedia.");
        sb.AppendLine(
            "Idealmente, los módulos estables deben ser abstractos y los inestables concretos. Evite módulos abstractos muy inestables o módulos concretos muy estables.");
        sb.AppendLine();

        return sb.ToString();
    }

    private static string GetInstabilityDescription(double instability)
    {
        if (instability <= 0.2)
            return "Muy estable / Core";
        if (instability >= 0.8)
            return "Muy inestable / Concreto";
        return "Estabilidad intermedia";
    }
}
