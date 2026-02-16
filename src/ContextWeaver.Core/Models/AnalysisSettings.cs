namespace ContextWeaver.Core;

/// <summary>
///     BUENA PRÁCTICA: Options Pattern.
///     Esta es una clase POCO (Plain Old CLR Object) que se usa para vincular
///     fuertemente las secciones del archivo appsettings.json. Esto proporciona
///     seguridad de tipos al acceder a la configuración.
/// </summary>
public class AnalysisSettings
{
    /// <summary>Gets or sets the list of file extensions to include in the analysis (e.g., .cs, .md).</summary>
    public string[] IncludedExtensions { get; set; } = [];

    /// <summary>Gets or sets the list of patterns to exclude from analysis (e.g., bin, obj, .git).</summary>
    public string[] ExcludePatterns { get; set; } = [];
}
