using System.Linq;
using System.Threading.Tasks;
using ContextWeaver.Core;
using ContextWeaver.Reporters;
using ContextWeaver.Reporters.Sections;
using ContextWeaver.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

public class WizardCommand : AsyncCommand<WizardSettings>
{
    private static readonly string[] _supportedFormats = { "markdown", "json", "xml" };

    private static readonly IReportSection[] _availableSections =
    {
        new HeaderSection(),
        new HotspotSection(),
        new InstabilitySection(),
        new DependencyGraphSection(),
        new ModuleDiagramSection(),
        new DirectoryTreeSection(),
        new FileContentSection()
    };

    private readonly CodeAnalyzerService _service;

    public WizardCommand(CodeAnalyzerService service)
    {
        _service = service;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, WizardSettings settings, CancellationToken cancellationToken)
    {
        var directoryInfo = new DirectoryInfo(settings.Directory ?? ".");

        // 1. Obtener archivos gestionados (Discovery)
        var (files, config) = _service.GetManagedFiles(directoryInfo);

        if (files.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No se encontraron archivos gestionados en el directorio especificado.[/]");
            return 1;
        }

        // 2. Construir árbol de selección
        var rootNode = BuildFileTree(files, directoryInfo);

        // 2b. Pregunta previa: ¿Seleccionar todos o ninguno?
        var selectionMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("¿Cómo desea empezar la [green]selección de archivos[/]?")
                .AddChoices("Todos seleccionados (deseleccionar lo que no quiero)",
                    "Ninguno seleccionado (seleccionar lo que quiero)"));

        var selectAll = selectionMode.StartsWith("Todos", StringComparison.Ordinal);

        // 3. Interacción: Selección de archivos (Árbol)
        var prompt = new MultiSelectionPrompt<FileSystemInfo>()
            .Title("Seleccione los [green]archivos[/] que desea incluir en el contexto:")
            .PageSize(20)
            .MoreChoicesText("[grey](Muevase arriba y abajo para ver más archivos)[/]")
            .InstructionsText(
                "[grey](Presione [blue]<espacio>[/] para seleccionar/deseleccionar, " +
                "[blue]<i>[/] para invertir selección, " +
                "[green]<enter>[/] para confirmar)[/]")
            .UseConverter(item => item.Name);

        // Añadir nodos al prompt recursivamente
        AddNodesToPrompt(prompt, rootNode, selectAll);

        var selectedItems = AnsiConsole.Prompt(prompt);
        // Filtrar solo los archivos (ignorar carpetas seleccionadas que son grupos)
        var selectedFiles = selectedItems.OfType<FileInfo>().ToList();

        if (selectedFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No se seleccionaron archivos. Operación cancelada.[/]");
            return 0;
        }

        // 3b. Interacción: Selección de secciones del reporte
        var optionalSections = _availableSections.Where(s => !s.IsRequired).ToList();

        var sectionPrompt = new MultiSelectionPrompt<string>()
            .Title("Seleccione las [green]secciones[/] que desea incluir en el reporte:")
            .PageSize(10)
            .MoreChoicesText("[grey](Muevase arriba y abajo para ver más secciones)[/]")
            .InstructionsText(
                "[grey]([blue]<espacio>[/] seleccionar/deseleccionar, [green]<enter>[/] confirmar)[/]")
            .AddChoiceGroup(
                "✅ Obligatorias (siempre incluidas)",
                _availableSections.Where(s => s.IsRequired).Select(s => $"{s.Name} — {s.Description}"));

        foreach (var section in optionalSections)
        {
            sectionPrompt.AddChoice($"{section.Name} — {section.Description}");
            sectionPrompt.Select($"{section.Name} — {section.Description}");
        }

        var selectedSectionLabels = AnsiConsole.Prompt(sectionPrompt);

        // Extraer nombres de sección (antes del " — ")
        var enabledSectionNames = selectedSectionLabels
            .Select(label => label.Split(" — ")[0])
            .ToList();

        // Validar: al menos 1 sección opcional seleccionada
        var optionalSelectedCount = enabledSectionNames
            .Count(name => optionalSections.Any(s => s.Name == name));

        if (optionalSelectedCount == 0)
        {
            AnsiConsole.MarkupLine("[red]Debe seleccionar al menos una sección opcional. Operación cancelada.[/]");
            return 1;
        }

        // 4. Interacción: Configuración de salida
        var outputFileName = AnsiConsole.Prompt(
            new TextPrompt<string>("Ingrese el nombre del [green]archivo de salida[/]:")
                .DefaultValue("context.md")
                .Validate(name =>
                    !string.IsNullOrWhiteSpace(name)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]El nombre del archivo no puede estar vacío[/]")));

        var format = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Seleccione el [green]formato de salida[/]:")
                .PageSize(3)
                .AddChoices(_supportedFormats));

        var outputFile = new FileInfo(Path.Combine(directoryInfo.FullName, outputFileName));

        // 5. Resumen de confirmación
        var requiredSectionNames = _availableSections
            .Where(s => s.IsRequired)
            .Select(s => s.Name);
        var allSectionNames = requiredSectionNames.Concat(enabledSectionNames).Distinct().ToList();

        var summaryTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Configuración[/]")
            .AddColumn("[bold]Valor[/]");

        summaryTable.AddRow("📂 Archivos seleccionados", $"[green]{selectedFiles.Count}[/]");
        summaryTable.AddRow("📝 Secciones del reporte", string.Join("\n", allSectionNames.Select(n => $"  • {n}")));
        summaryTable.AddRow("💾 Archivo de salida", $"[blue]{outputFile.FullName}[/]");
        summaryTable.AddRow("📄 Formato", $"[blue]{format}[/]");

        AnsiConsole.Write(new Rule("[yellow]Resumen[/]").RuleStyle("grey"));
        AnsiConsole.Write(summaryTable);
        AnsiConsole.WriteLine();

        var confirm = AnsiConsole.Confirm("¿Desea continuar con la ejecución?", defaultValue: true);
        if (!confirm)
        {
            AnsiConsole.MarkupLine("[yellow]Operación cancelada por el usuario.[/]");
            return 0;
        }

        // 6. Ejecución con indicador de progreso
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("Analizando archivos y generando reporte...", async ctx =>
            {
                await _service.AnalyzeFiles(selectedFiles, directoryInfo, outputFile, format, enabledSectionNames);
            });

        AnsiConsole.MarkupLine($"\n[green]✅ Reporte generado exitosamente en:[/] [link]{outputFile.FullName}[/]");

        return 0;
    }

    private static FileNode BuildFileTree(List<FileInfo> files, DirectoryInfo rootDir)
    {
        var root = new FileNode("Root", rootDir);

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(rootDir.FullName, file.FullName);
            var parts = relativePath.Split(Path.DirectorySeparatorChar);

            var currentNode = root;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                var existingChild = currentNode.Children.FirstOrDefault(c => c.Name == part);
                if (existingChild == null)
                {
                    // Reconstruir path completo para crear DirectoryInfo
                    var currentPath = Path.Combine(currentNode.Item.FullName, part);
                    var dirInfo = new DirectoryInfo(currentPath);
                    existingChild = new FileNode(part, dirInfo);
                    currentNode.Children.Add(existingChild);
                }

                currentNode = existingChild;
            }

            currentNode.Children.Add(new FileNode(parts.Last(), file));
        }

        return root;
    }

    private static void AddNodesToPrompt(MultiSelectionPrompt<FileSystemInfo> prompt, FileNode node, bool selectAll)
    {
        // Carpetas primero (Nivel Raíz)
        foreach (var child in node.Children.OrderBy(c => c.Item is FileInfo))
        {
            var item = prompt.AddChoice(child.Item);

            if (selectAll)
                item.Select();

            AddNodesToPromptRecursive(item, child, selectAll);
        }
    }

    private static void AddNodesToPromptRecursive(IMultiSelectionItem<FileSystemInfo> parent, FileNode node, bool selectAll)
    {
        // Carpetas primero (Nivel Hijo)
        foreach (var child in node.Children.OrderBy(c => c.Item is FileInfo))
        {
            // Usar reflexión para invocar 'AddChild' en el objeto concreto (ListPromptItem<T> que es interno/privado)
            var addChildMethod = parent.GetType().GetMethod("AddChild", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (addChildMethod != null)
            {
                var childItem = (IMultiSelectionItem<FileSystemInfo>)addChildMethod.Invoke(parent, new object[] { child.Item })!;

                if (selectAll)
                    childItem.Select();

                AddNodesToPromptRecursive(childItem, child, selectAll);
            }
            else
            {
                // Fallback si no se encuentra el método (no debería ocurrir basado en análisis previo)
                AnsiConsole.MarkupLine($"[red]Error interno: No se pudo añadir el nodo hijo '{child.Name}'. Método 'AddChild' no encontrado.[/]");
            }
        }
    }

    private sealed class FileNode
    {
        public FileNode(string name, FileSystemInfo item)
        {
            Name = name;
            Item = item;
        }

        public string Name { get; }

        public FileSystemInfo Item { get; }

        public List<FileNode> Children { get; } = new();
    }
}
