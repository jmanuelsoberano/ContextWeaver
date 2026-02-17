using System.Resources;
using ContextWeaver.Cli.Commands;
using ContextWeaver.Cli.Infrastructure;
using ContextWeaver.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

[assembly: NeutralResourcesLanguage("en")]

var services = new ServiceCollection();

// Configurar Logging básico para la CLI
services.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Information);
});

// Registrar los servicios de la aplicación reusando la lógica existente
HostBuilderExtensions.ConfigureServices(services);

// Configurar la integración de DI con Spectre.Console
var registrar = new TypeRegistrar(services);
var app = new CommandApp<AnalyzeCommand>(registrar);

app.Configure(config =>
{
    config.SetApplicationName("contextweaver");
    config.SetApplicationVersion("1.0.7");
});

return await app.RunAsync(args);
