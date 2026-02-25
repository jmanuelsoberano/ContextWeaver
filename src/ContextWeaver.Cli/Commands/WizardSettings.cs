using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

/// <summary>
///     Configuración para el comando del asistente interactivo (Wizard).
/// </summary>
public class WizardSettings : CommandSettings
{
    /// <summary>
    ///     Gets or sets the root directory of the project to analyze.
    /// </summary>
    [CommandOption("-d|--directory <DIRECTORY>")]
    [Description("El directorio raíz del proyecto a analizar. Por defecto, es el directorio actual.")]
    public string? Directory { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to include all managed files automatically.
    /// </summary>
    [CommandOption("--all")]
    [Description("Inicia con todos los archivos seleccionados (omite la pregunta de selección).")]
    [DefaultValue(false)]
    public bool All { get; set; }

    /// <summary>
    ///     Gets or sets the sections to include, separated by commas.
    /// </summary>
    [CommandOption("--sections <SECTIONS>")]
    [Description("Secciones a incluir separadas por coma (omite el selector). Ejemplo: '🔥 Hotspots,📁 Contenido de Archivos'.")]
    public string? Sections { get; set; }

    /// <summary>
    ///     Gets or sets the sections to exclude, separated by commas.
    /// </summary>
    [CommandOption("--exclude-sections <EXCLUDE_SECTIONS>")]
    [Description("Secciones a excluir separadas por coma (omite el selector).")]
    public string? ExcludeSections { get; set; }

    /// <summary>
    ///     Gets or sets the name of the output file.
    /// </summary>
    [CommandOption("-o|--output <OUTPUT>")]
    [Description("Nombre del archivo de salida (omite el prompt del nombre).")]
    public string? Output { get; set; }

    /// <summary>
    ///     Gets or sets the output format.
    /// </summary>
    [CommandOption("-f|--format <FORMAT>")]
    [Description("Formato de salida: markdown, json, xml (omite el selector de formato).")]
    public string? Format { get; set; }

    /// <inheritdoc />
    public override ValidationResult Validate()
    {
        if (!string.IsNullOrEmpty(Sections) && !string.IsNullOrEmpty(ExcludeSections))
        {
            return ValidationResult.Error("No puede usar --sections y --exclude-sections al mismo tiempo.");
        }

        return ValidationResult.Success();
    }
}
