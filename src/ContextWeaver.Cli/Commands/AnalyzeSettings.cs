using System.ComponentModel;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

/// <summary>
///     Configuración para el comando de análisis automático.
/// </summary>
public class AnalyzeSettings : CommandSettings
{
    /// <summary>
    ///     Gets or sets the root directory of the project to analyze.
    /// </summary>
    [CommandOption("-d|--directory <DIRECTORY>")]
    [Description("El directorio raíz del proyecto a analizar. Por defecto, es el directorio actual.")]
    public string? Directory { get; set; }

    /// <summary>
    ///     Gets or sets the output file for the consolidated report.
    /// </summary>
    [CommandOption("-o|--output <OUTPUT>")]
    [Description("El archivo de salida para el reporte consolidado.")]
    [DefaultValue("analysis_report.md")]
    public string? Output { get; set; }

    /// <summary>
    ///     Gets or sets the output report format.
    /// </summary>
    [CommandOption("-f|--format <FORMAT>")]
    [Description("El formato del reporte de salida.")]
    [DefaultValue("markdown")]
    public string? Format { get; set; }
}
