using System.ComponentModel;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

public class AnalyzeSettings : CommandSettings
{
    [CommandOption("-d|--directory <DIRECTORY>")]
    [Description("El directorio raíz del proyecto a analizar. Por defecto, es el directorio actual.")]
    public string? Directory { get; set; }

    [CommandOption("-o|--output <OUTPUT>")]
    [Description("El archivo de salida para el reporte consolidado.")]
    [DefaultValue("analysis_report.md")]
    public string? Output { get; set; }

    [CommandOption("-f|--format <FORMAT>")]
    [Description("El formato del reporte de salida.")]
    [DefaultValue("markdown")]
    public string? Format { get; set; }
}
