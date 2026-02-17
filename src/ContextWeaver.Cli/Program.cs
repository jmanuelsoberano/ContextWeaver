using System.Linq;
using System.Resources;
using ContextWeaver.Cli.Commands;
using ContextWeaver.Cli.Infrastructure;
using ContextWeaver.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

[assembly: NeutralResourcesLanguage("en")]

// ARQUITECTURA: Top-Level Statements
// Este archivo actúa como el "Application Entry Point". Reduce el ruido visual ("boilerplate")
// de la declaración explícita de namespace y clase Program.Main.

// PRINCIPIO: Composition Root
// Aquí es donde ensamblamos todo el grafo de dependencias de la aplicación.
// Es el ÚNICO lugar donde se conocen las implementaciones concretas y se vinculan a sus interfaces.
var services = new ServiceCollection();

// BEST PRACTICE: Logging Configurado Explícitamente
// Configuramos el logging antes de cualquier otra cosa para asegurar observabilidad desde el inicio.
services.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Information);
});

// PRINCIPIO: DRY (Don't Repeat Yourself) & Modularity
// Reutilizamos la configuración de servicios centralizada en HostBuilderExtensions.
// Esto nos permite compartir la misma configuración de inyección de dependencias
// entre diferentes "Hosts" (por ejemplo, si tuviéramos una API y una CLI).
HostBuilderExtensions.ConfigureServices(services);

// PATRÓN DE DISEÑO: Adapter Pattern
// `TypeRegistrar` actúa como un adaptador que permite a Spectre.Console.Cli (el cliente)
// interactuar con Microsoft.Extensions.DependencyInjection (el adaptado).
// Esto nos permite usar nuestro contenedor de DI preferido dentro de la librería de CLI.
var registrar = new TypeRegistrar(services);

// PATRÓN DE DISEÑO: Command Pattern
// Spectre.Console.Cli implementa el patrón Command.
// `CommandApp` encapsula la solicitud.
// CAMBIO: WizardCommand es ahora el comando por defecto para mejorar la experiencia de usuario.
var app = new CommandApp<WizardCommand>(registrar);

app.Configure(config =>
{
    config.SetApplicationName("contextweaver");
    config.SetApplicationVersion("1.0.7");

    config.AddCommand<AnalyzeCommand>("analyze")
        .WithDescription("Ejecuta el análisis automático sin interacción (ideal para CI/CD).");
});

return await app.RunAsync(args);
