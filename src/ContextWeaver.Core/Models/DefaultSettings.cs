namespace ContextWeaver.Core;

/// <summary>
///     Proporciona los valores de configuración por defecto para la aplicación.
/// </summary>
public static class DefaultSettings
{
    /// <summary>
    ///     Obtiene la configuración predeterminada recomendada.
    /// </summary>
    /// <returns>Una instancia de <see cref="AnalysisSettings"/> con valores por defecto.</returns>
    public static AnalysisSettings Get()
    {
        return new AnalysisSettings();
    }
}
