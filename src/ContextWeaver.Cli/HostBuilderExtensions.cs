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
    ///     Creates and configures the host builder with all required services.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<SettingsProvider>();
                services.AddSingleton<CodeAnalyzerService>();

                services.AddSingleton<IFileAnalyzer, CSharpFileAnalyzer>();
                services.AddSingleton<IFileAnalyzer, GenericFileAnalyzer>();

                services.AddSingleton<IReportGenerator, MarkdownReportGenerator>();
            });
    }
}
