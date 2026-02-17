using System.Linq;
using System.Threading.Tasks;
using ContextWeaver.Core;
using ContextWeaver.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ContextWeaver.Cli.Commands;

public class WizardCommand : AsyncCommand<WizardSettings>
{
    private static readonly string[] _supportedFormats = { "markdown", "json", "xml" };
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

        // 3. Interacción: Selección de archivos (Árbol)
        var prompt = new MultiSelectionPrompt<FileSystemInfo>()
            .Title("Seleccione los [green]archivos[/] que desea incluir en el contexto:")
            .PageSize(20)
            .MoreChoicesText("[grey](Muevase arriba y abajo para ver más archivos)[/]")
            .InstructionsText(
                "[grey](Presione [blue]<espacio>[/] para seleccionar/deseleccionar carpetas o archivos, " +
                "[green]<enter>[/] para confirmar)[/]")
            .UseConverter(item => item.Name);

        // Añadir nodos al prompt recursivamente
        AddNodesToPrompt(prompt, rootNode);

        var selectedItems = AnsiConsole.Prompt(prompt);
        // Filtrar solo los archivos (ignorar carpetas seleccionadas que son grupos)
        var selectedFiles = selectedItems.OfType<FileInfo>().ToList();

        if (selectedFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No se seleccionaron archivos. Operación cancelada.[/]");
            return 0;
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

        // 5. Ejecución
        await _service.AnalyzeFiles(selectedFiles, directoryInfo, outputFile, format);

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

    private static void AddNodesToPrompt(MultiSelectionPrompt<FileSystemInfo> prompt, FileNode node)
    {
        // Carpetas primero (Nivel Raíz)
        foreach (var child in node.Children.OrderBy(c => c.Item is FileInfo))
        {
            var item = prompt.AddChoice(child.Item);
            AddNodesToPromptRecursive(item, child);
        }
    }

    private static void AddNodesToPromptRecursive(IMultiSelectionItem<FileSystemInfo> parent, FileNode node)
    {
        // Carpetas primero (Nivel Hijo)
        foreach (var child in node.Children.OrderBy(c => c.Item is FileInfo))
        {
            // Usar reflexión para invocar 'AddChild' en el objeto concreto (ListPromptItem<T> que es interno/privado)
            var addChildMethod = parent.GetType().GetMethod("AddChild", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (addChildMethod != null)
            {
                var childItem = (IMultiSelectionItem<FileSystemInfo>)addChildMethod.Invoke(parent, new object[] { child.Item })!;
                AddNodesToPromptRecursive(childItem, child);
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
