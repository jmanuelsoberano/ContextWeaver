using System.ComponentModel;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

public class WizardSettings : CommandSettings
{
    [CommandOption("-d|--directory <DIRECTORY>")]
    [Description("El directorio raíz del proyecto a analizar. Por defecto, es el directorio actual.")]
    public string? Directory { get; set; }
}
