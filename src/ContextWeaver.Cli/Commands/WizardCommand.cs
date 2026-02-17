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

    private static readonly string[] BulkSelectionOptions =
    {
        "Usar selección por defecto / guardada",
        "Seleccionar TODAS las secciones opcionales",
        "Seleccionar NINGUNA sección opcional (empezar limpio)"
    };

    private static readonly IReportSection[] _availableSections =
    {
        new HeaderSection(),
        new HotspotSection(),
        new InstabilitySection(),
        new MermaidDependencyGraphSection(),
        new PlantUmlDependencyGraphSection(),
        new MermaidModuleDiagramSection(),
        new PlantUmlModuleDiagramSection(),
        new DirectoryTreeSection(),
        new FileContentSection()
    };

    private readonly CodeAnalyzerService _service;
    private readonly SettingsProvider _settingsProvider;

    public WizardCommand(CodeAnalyzerService service, SettingsProvider settingsProvider)
    {
        _service = service;
        _settingsProvider = settingsProvider;
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

        // 1b. Filtro opcional por extensión
        if (!settings.All)
        {
            var extensions = files.Select(f => f.Extension.ToLowerInvariant()).Distinct().OrderBy(e => e).ToList();
            if (extensions.Count > 1)
            {
                var extPrompt = new MultiSelectionPrompt<string>()
                    .Title("¿Desea filtrar por [green]extensión[/]? (deseleccione las que no necesite)")
                    .PageSize(15)
                    .InstructionsText(
                        "[grey]([blue]<espacio>[/] seleccionar/deseleccionar, [green]<enter>[/] confirmar)[/]");

                foreach (var ext in extensions)
                {
                    var count = files.Count(f => f.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase));
                    extPrompt.AddChoice($"{ext} ({count} archivos)");
                    extPrompt.Select($"{ext} ({count} archivos)");
                }

                var selectedExtLabels = AnsiConsole.Prompt(extPrompt);
                var selectedExtensions = selectedExtLabels
                    .Select(label => label.Split(' ')[0])
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                files = files.Where(f => selectedExtensions.Contains(f.Extension.ToLowerInvariant())).ToList();

                if (files.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No hay archivos con las extensiones seleccionadas. Operación cancelada.[/]");
                    return 0;
                }
            }
        }

        // 2. Construir árbol de selección
        var rootNode = BuildFileTree(files, directoryInfo);

        // 2b. Pregunta previa: ¿Seleccionar todos o ninguno?
        bool selectAll;
        if (settings.All)
        {
            selectAll = true;
        }
        else
        {
            var selectionMode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("¿Cómo desea empezar la [green]selección de archivos[/]?")
                    .AddChoices("Todos seleccionados (deseleccionar lo que no quiero)",
                        "Ninguno seleccionado (seleccionar lo que quiero)"));

            selectAll = selectionMode.StartsWith("Todos", StringComparison.Ordinal);
        }

        // 3. Interacción: Selección de archivos (Árbol)
        List<FileInfo> selectedFiles;
        if (settings.All)
        {
            // --all flag: selecciona todo y omite el prompt
            selectedFiles = files;
        }
        else
        {
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
            selectedFiles = selectedItems.OfType<FileInfo>().ToList();
        }

        if (selectedFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No se seleccionaron archivos. Operación cancelada.[/]");
            return 0;
        }

        // 3b. Selección de secciones del reporte
        var optionalSections = _availableSections.Where(s => !s.IsRequired).ToList();
        List<string> enabledSectionNames;

        if (!string.IsNullOrEmpty(settings.Sections))
        {
            // --sections flag: incluir solo las especificadas (fuzzy match)
            var inputs = settings.Sections
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            enabledSectionNames = new List<string>();
            foreach (var input in inputs)
            {
                var match = _availableSections.FirstOrDefault(s => s.Name.Contains(input, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    enabledSectionNames.Add(match.Name);
                }
            }
        }
        else if (!string.IsNullOrEmpty(settings.ExcludeSections))
        {
            // --exclude-sections flag: incluir todas menos las excluidas (fuzzy match)
            var excludedInputs = settings.ExcludeSections
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var excludedNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var input in excludedInputs)
            {
                var match = _availableSections.FirstOrDefault(s => s.Name.Contains(input, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    excludedNames.Add(match.Name);
                }
            }

            enabledSectionNames = optionalSections
                .Where(s => !excludedNames.Contains(s.Name))
                .Select(s => s.Name)
                .ToList();
        }
        else
        {
            // Modo interactivo: mostrar selector
            // Verificar si hay preferencias guardadas
            var savedSections = config.EnabledSections != null
                ? new HashSet<string>(config.EnabledSections, StringComparer.Ordinal)
                : null;

            var sectionPrompt = new MultiSelectionPrompt<string>()
                .Title("Seleccione las [green]secciones opcionales[/] que desea incluir en el reporte:\n[grey](Las secciones obligatorias como 'Header' se incluirán automáticamente)[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Muevase arriba y abajo para ver más secciones)[/]")
                .InstructionsText(
                    "[grey]([blue]<espacio>[/] seleccionar/deseleccionar, [green]<enter>[/] confirmar)[/]")
                .Required(); // Validar que se seleccione al menos una

            // 3a. Selección inicial (Bulk Selection)
            var selectionMode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("¿Cómo desea comenzar la selección de secciones?")
                    .AddChoices(BulkSelectionOptions));

            foreach (var section in optionalSections)
            {
                var label = $"{section.Name} — {section.Description}";
                sectionPrompt.AddChoice(label);

                bool shouldSelect = false;
                if (selectionMode.StartsWith(BulkSelectionOptions[0], StringComparison.Ordinal))
                {
                    // Pre-seleccionar: si hay preferencias guardadas, usar esas; si no, seleccionar todas
                    shouldSelect = savedSections == null || savedSections.Contains(section.Name);
                }
                else if (selectionMode.StartsWith(BulkSelectionOptions[1], StringComparison.Ordinal))
                {
                    shouldSelect = true;
                }
                else
                {
                    shouldSelect = false;
                }

                if (shouldSelect)
                    sectionPrompt.Select(label);
            }

            if (savedSections != null)
                AnsiConsole.MarkupLine("[grey]  (Se cargaron preferencias de secciones guardadas)[/]");

            var selectedSectionLabels = AnsiConsole.Prompt(sectionPrompt);

            enabledSectionNames = selectedSectionLabels
                .Select(label => label.Split(" — ")[0])
                .ToList();

            // Ofrecer guardar preferencias si difieren del default (todos)
            var allOptionalSelected = enabledSectionNames.Count >= optionalSections.Count;
            if (!allOptionalSelected)
            {
                var savePref = AnsiConsole.Confirm("¿Guardar estas preferencias de secciones para futuros análisis?", defaultValue: false);
                if (savePref)
                {
                    config.EnabledSections = enabledSectionNames.ToArray();
                    _settingsProvider.SaveSettings(directoryInfo, config);
                    AnsiConsole.MarkupLine("[green]Preferencias guardadas en .contextweaver.json[/]");
                }
            }
        }

        // Validar: al menos 1 sección opcional seleccionada
        var optionalSelectedCount = enabledSectionNames
            .Count(name => optionalSections.Any(s => s.Name == name));

        if (optionalSelectedCount == 0)
        {
            AnsiConsole.MarkupLine("[red]Debe seleccionar al menos una sección opcional. Operación cancelada.[/]");
            return 1;
        }

        // 4. Configuración de salida
        var outputFileName = settings.Output ?? AnsiConsole.Prompt(
            new TextPrompt<string>("Ingrese el nombre del [green]archivo de salida[/]:")
                .DefaultValue("context.md")
                .Validate(name =>
                    !string.IsNullOrWhiteSpace(name)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]El nombre del archivo no puede estar vacío[/]")));

        var format = settings.Format ?? AnsiConsole.Prompt(
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

        if (AnsiConsole.Profile.Capabilities.Interactive)
        {
            var confirm = AnsiConsole.Confirm("¿Desea continuar con la ejecución?", defaultValue: true);
            if (!confirm)
            {
                AnsiConsole.MarkupLine("[yellow]Operación cancelada por el usuario.[/]");
                return 0;
            }
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
