namespace ContextWeaver.Reporters;

/// <summary>
///     Contrato para una sección de reporte autocontenida.
///     Cada implementación renderiza un bloque lógico del reporte Markdown.
/// </summary>
public interface IReportSection
{
    /// <summary>
    ///     Renderiza el contenido de la sección usando el contexto de reporte proporcionado.
    /// </summary>
    /// <param name="context">Datos de contexto disponibles para la generación del reporte.</param>
    /// <returns>La cadena markdown renderizada para esta sección.</returns>
    string Render(ReportContext context);
}
