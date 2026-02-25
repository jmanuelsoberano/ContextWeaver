namespace ContextWeaver.Reporters;

/// <summary>
///     Contrato para una sección de reporte autocontenida.
///     Cada implementación renderiza un bloque lógico del reporte Markdown.
/// </summary>
public interface IReportSection
{
    /// <summary>Gets the display name a human-readable label shown in the wizard selector.</summary>
    string Name { get; }

    /// <summary>Gets the short description of what this section generates.</summary>
    string Description { get; }

    /// <summary>Gets a value indicating whether this section is mandatory and cannot be deselected.</summary>
    bool IsRequired { get; }

    /// <summary>
    ///     Renderiza el contenido de la sección usando el contexto de reporte proporcionado.
    /// </summary>
    /// <param name="context">Datos de contexto disponibles para la generación del reporte.</param>
    /// <returns>La cadena markdown renderizada para esta sección.</returns>
    string Render(ReportContext context);
}
