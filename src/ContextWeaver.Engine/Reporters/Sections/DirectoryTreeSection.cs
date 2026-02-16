using System.Text;
using ContextWeaver.Core;
using ContextWeaver.Utilities;

namespace ContextWeaver.Reporters.Sections;

/// <summary>
///     Genera la estructura de directorios con formato de árbol y enlaces a secciones de archivo.
/// </summary>
public class DirectoryTreeSection : IReportSection
{
    private static readonly char[] PathSeparators = { '/', '\\' };

    /// <inheritdoc />
    public string Render(ReportContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Directory Structure");
        sb.AppendLine();

        var root = BuildTree(context.SortedResults);

        sb.AppendLine($"- {context.Directory.Name}/");
        AppendDirectoryStructureWithLinks(root.Children.Values, sb, 1);

        sb.AppendLine();
        return sb.ToString();
    }

    private static TreeNode BuildTree(List<FileAnalysisResult> results)
    {
        var root = new TreeNode { Name = string.Empty };
        foreach (var result in results)
        {
            var currentNode = root;
            var pathParts = result.RelativePath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < pathParts.Length; i++)
            {
                var part = pathParts[i];
                if (!currentNode.Children.ContainsKey(part))
                    currentNode.Children[part] = new TreeNode { Name = part };
                currentNode = currentNode.Children[part];
                if (i == pathParts.Length - 1)
                    currentNode.Path = result.RelativePath;
            }
        }

        return root;
    }

    private static void AppendDirectoryStructureWithLinks(
        IEnumerable<TreeNode> nodes, StringBuilder sb, int level)
    {
        var indent = new string(' ', level * 4);

        var directories = nodes.Where(n => n.Path == null).OrderBy(n => n.Name);
        var files = nodes.Where(n => n.Path != null).OrderBy(n => n.Name);

        foreach (var dir in directories)
        {
            sb.AppendLine($"{indent}- {dir.Name}/");
            AppendDirectoryStructureWithLinks(dir.Children.Values, sb, level + 1);
        }

        foreach (var file in files)
        {
            var headerText = $"File: {file.Path}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            sb.AppendLine($"{indent}- [{file.Name}](#{anchor})");
        }
    }

    private sealed class TreeNode
    {
        public string Name { get; set; } = string.Empty;
        public string? Path { get; set; }
        public SortedDictionary<string, TreeNode> Children { get; } = new();
    }
}
