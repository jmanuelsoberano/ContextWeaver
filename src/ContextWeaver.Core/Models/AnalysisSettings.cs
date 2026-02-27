namespace ContextWeaver.Core;

/// <summary>
///     BUENA PRÁCTICA: Options Pattern.
///     Esta es una clase POCO (Plain Old CLR Object) que se usa para vincular
///     fuertemente las secciones del archivo appsettings.json. Esto proporciona
///     seguridad de tipos al acceder a la configuración.
/// </summary>
public class AnalysisSettings
{
    /// <summary>Gets or sets the list of file extensions to include in the analysis (e.g. .cs, .md).</summary>
    public string[] IncludedExtensions { get; set; } =
    [
        ".cs", ".csproj", ".sln", ".json", ".ts", ".html", ".scss", ".css", ".md"
    ];

    /// <summary>Gets or sets the list of patterns to exclude from the analysis (e.g. bin, obj, .git).</summary>
    public string[] ExcludePatterns { get; set; } =
    [
        "bin", "obj", "node_modules", ".angular", ".vs", "dist", "wwwroot", "Publish", "packages", "Scripts", "Content"
    ];

    /// <summary>Gets or sets the list of section names to include in the report. Null means all sections.</summary>
#pragma warning disable SA1011 // Closing square bracket should be followed by a space — conflicts with SA1018
    public string[]? EnabledSections { get; set; }
#pragma warning restore SA1011

    /// <summary>Gets or sets the list of wrapper directories used to compute module names (e.g. src, tests).</summary>
    public string[] WrapperDirectories { get; set; } =
    [
        "src", "source", "test", "tests", "lib"
    ];
}
