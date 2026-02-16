using System.Text.Json;
using ContextWeaver.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContextWeaver.Services;

/// <summary>
///     Gestiona la carga y validación de la configuración de análisis.
///     Busca el archivo .contextweaver.json o crea uno por defecto si no existe.
/// </summary>
public class SettingsProvider
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly ILogger<SettingsProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsProvider"/> class.
    ///     Inicializa una nueva instancia de la clase <see cref="SettingsProvider"/>.
    /// </summary>
    /// <param name="logger">Logger para diagnósticos de configuración.</param>
    public SettingsProvider(ILogger<SettingsProvider> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Carga la configuración para un directorio específico.
    /// </summary>
    /// <param name="directory">Directorio raíz del análisis.</param>
    /// <returns>La configuración cargada o los valores por defecto si falla la carga.</returns>
    public AnalysisSettings LoadSettingsFor(DirectoryInfo directory)
    {
        var localConfigPath = Path.Combine(directory.FullName, ".contextweaver.json");

        if (File.Exists(localConfigPath))
        {
            _logger.LogInformation("Se encontró '.contextweaver.json'. Usándolo para este análisis.");
            try
            {
                using var stream = File.OpenRead(localConfigPath);
                var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
                var section = config.GetSection("AnalysisSettings");

                if (!section.Exists())
                {
                    _logger.LogWarning("La sección 'AnalysisSettings' no existe en '.contextweaver.json'. Se usará la configuración por defecto.");
                    return DefaultSettings.Get();
                }

                var localSettings = section.Get<AnalysisSettings>();
                if (localSettings != null && ((localSettings.IncludedExtensions?.Length ?? 0) > 0 ||
                                              (localSettings.ExcludePatterns?.Length ?? 0) > 0))
                {
                    _logger.LogInformation("Configuración local aplicada exitosamente.");
                    return localSettings;
                }

                _logger.LogInformation("La configuración local está vacía o incompleta. Se usará la configuración por defecto.");
                return DefaultSettings.Get();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al leer '{ConfigPath}'. Se usará la configuración por defecto.", localConfigPath);
                return DefaultSettings.Get();
            }
        }

        // --- NUEVA LÓGICA: El archivo NO existe, así que lo creamos ---
        _logger.LogInformation("No se encontró '.contextweaver.json'. Se creará uno nuevo con valores por defecto.");
        var defaultSettings = DefaultSettings.Get();

        try
        {
            // H10: Validar que el path resuelto permanece dentro del directorio esperado.
            var resolvedPath = Path.GetFullPath(localConfigPath);
            var resolvedDir = Path.GetFullPath(directory.FullName);
            if (!resolvedPath.StartsWith(resolvedDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Ruta de configuración resuelta '{ResolvedPath}' está fuera del directorio objetivo. No se escribirá el archivo.", resolvedPath);
                return defaultSettings;
            }

            var jsonString = JsonSerializer.Serialize(new { AnalysisSettings = defaultSettings }, _jsonOptions);
            File.WriteAllText(resolvedPath, jsonString);
            _logger.LogInformation("Archivo de configuración creado en: {ConfigPath}", resolvedPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Sin permisos para escribir en '{ConfigPath}'. Se continuará con la configuración por defecto en memoria.", localConfigPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo crear el archivo de configuración. Se continuará con la configuración por defecto en memoria.");
        }

        return defaultSettings;
    }
}
