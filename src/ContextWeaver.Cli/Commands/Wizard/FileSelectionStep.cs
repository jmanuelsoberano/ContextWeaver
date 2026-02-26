using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Step that prompts the user to select files from a directory tree.
/// </summary>
public class FileSelectionStep : IWizardStep
{
    /// <inheritdoc/>
    public bool ShouldExecute(WizardContext context) => !context.Settings.All;

    /// <inheritdoc/>
    public Task<StepResult> ExecuteAsync(WizardContext context)
    {
        var rootNode = BuildFileTree(context.ManagedFiles, context.Directory);

        var prompt = new MultiSelectionPrompt<object>()
            .Title("Seleccione los [green]archivos[/] que desea incluir en el contexto:")
            .PageSize(20)
            .MoreChoicesText("[grey](Muevase arriba y abajo para ver más archivos)[/]")
            .InstructionsText("[grey](Presione [blue]<espacio>[/] para seleccionar/deseleccionar, [blue]<i>[/] para invertir selección, [green]<enter>[/] para confirmar)[/]")
            .UseConverter(item => item is FileSystemInfo fsi ? fsi.Name : item.ToString()!);

        // Add back option at the top if it's not the first step
        if (!context.IsFirstInteractiveStep)
        {
            prompt.AddChoice(WizardConstants.BackOption);
        }

        // Recursively add choices
        AddNodesToPrompt(prompt, rootNode, context.SelectAllFilesByDefault);

        var selectedItems = AnsiConsole.Prompt(prompt);

        if (selectedItems.Contains(WizardConstants.BackOption))
        {
            return Task.FromResult(StepResult.Previous);
        }

        // Filter only the files (ignore selected folders representing groups)
        context.SelectedFiles = selectedItems.OfType<FileInfo>().ToList();

        if (context.SelectedFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No se seleccionaron archivos. Operación cancelada.[/]");
            return Task.FromResult(StepResult.Cancel);
        }

        context.IsFirstInteractiveStep = false;

        return Task.FromResult(StepResult.Next);
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

    private static void AddNodesToPrompt(MultiSelectionPrompt<object> prompt, FileNode node, bool selectAll)
    {
        foreach (var child in node.Children.OrderBy(c => c.Item is FileInfo))
        {
            var item = prompt.AddChoice(child.Item);

            if (selectAll)
            {
                item.Select();
            }

            AddNodesToPromptRecursive(item, child, selectAll);
        }
    }

    private static void AddNodesToPromptRecursive(IMultiSelectionItem<object> parent, FileNode node, bool selectAll)
    {
        foreach (var child in node.Children.OrderBy(c => c.Item is FileInfo))
        {
            // Use reflection to invoke 'AddChild' on the internal ListPromptItem<T>
            var addChildMethod = parent.GetType().GetMethod("AddChild", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (addChildMethod != null)
            {
                var childItem = (IMultiSelectionItem<object>)addChildMethod.Invoke(parent, new object[] { child.Item })!;

                if (selectAll)
                {
                    childItem.Select();
                }

                AddNodesToPromptRecursive(childItem, child, selectAll);
            }
            else
            {
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
