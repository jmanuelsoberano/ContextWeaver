using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

public class WizardSettings : CommandSettings
{
    [CommandOption("-d|--directory <DIRECTORY>")]
    [Description("El directorio raíz del proyecto a analizar. Por defecto, es el directorio actual.")]
    public string? Directory { get; set; }

    [CommandOption("--all")]
    [Description("Inicia con todos los archivos seleccionados (omite la pregunta de selección).")]
    [DefaultValue(false)]
    public bool All { get; set; }

    [CommandOption("--sections <SECTIONS>")]
    [Description("Secciones a incluir separadas por coma (omite el selector). Ejemplo: '🔥 Hotspots,📁 Contenido de Archivos'.")]
    public string? Sections { get; set; }

    [CommandOption("--exclude-sections <EXCLUDE_SECTIONS>")]
    [Description("Secciones a excluir separadas por coma (omite el selector).")]
    public string? ExcludeSections { get; set; }

    [CommandOption("-o|--output <OUTPUT>")]
    [Description("Nombre del archivo de salida (omite el prompt del nombre).")]
    public string? Output { get; set; }

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
