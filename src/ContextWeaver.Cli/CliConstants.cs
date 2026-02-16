namespace ContextWeaver.Cli;

/// <summary>
///     Define constantes y alias para los argumentos de la CLI.
/// </summary>
internal static class CliConstants
{
    /// <summary>Alias para el argumento de directorio.</summary>
    public static readonly string[] DirectoryAliases = { "-d", "--directorio" };

    /// <summary>Alias para el argumento de archivo de salida.</summary>
    public static readonly string[] OutputAliases = { "-o", "--output" };

    /// <summary>Alias para el argumento de formato de reporte.</summary>
    public static readonly string[] FormatAliases = { "-f", "--format" };
}
