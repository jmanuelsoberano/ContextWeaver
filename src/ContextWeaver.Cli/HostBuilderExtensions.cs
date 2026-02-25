using ContextWeaver.Analyzers;
using ContextWeaver.Core;
using ContextWeaver.Reporters;
using ContextWeaver.Services;
using ContextWeaver.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ContextWeaver.Extensions;

/// <summary>
///     Composition Root — el único lugar donde las implementaciones concretas
///     se conectan a sus abstracciones (Information Hiding, Parnas 1972).
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    ///     Crea y configura el constructor del host con todos los servicios requeridos.
    /// </summary>
    /// <param name="args">Argumentos de línea de comandos.</param>
    /// <returns>El <see cref="IHostBuilder"/> configurado.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                ConfigureServices(services);
            });
    }

    /// <summary>
    ///     Registra los servicios de la aplicación en el contenedor de dependencias.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SettingsProvider>();
        services.AddSingleton<CodeAnalyzerService>();

        services.AddSingleton<IFileAnalyzer, CSharpFileAnalyzer>();
        services.AddSingleton<IFileAnalyzer, GenericFileAnalyzer>();

        services.AddSingleton<IReportGenerator, MarkdownReportGenerator>();
    }
}
