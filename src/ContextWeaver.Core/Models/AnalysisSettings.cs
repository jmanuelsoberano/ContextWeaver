namespace ContextWeaver.Core;

/// <summary>
///     BUENA PRÁCTICA: Options Pattern.
///     Esta es una clase POCO (Plain Old CLR Object) que se usa para vincular
///     fuertemente las secciones del archivo appsettings.json. Esto proporciona
///     seguridad de tipos al acceder a la configuración.
/// </summary>
public class AnalysisSettings
{
    /// <summary>Gets or sets obtiene o establece la lista de extensiones de archivo a incluir en el análisis (ej. .cs, .md).</summary>
    public string[] IncludedExtensions { get; set; } = [];

    /// <summary>Gets or sets obtiene o establece la lista de patrones a excluir del análisis (ej. bin, obj, .git).</summary>
    public string[] ExcludePatterns { get; set; } = [];
}
