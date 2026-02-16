namespace ContextWeaver.Core;

/// <summary>
///     Typed metrics for a file analysis result.
///     Properties are <c>init</c>-only for immutability after construction.
/// </summary>
public class FileMetrics
{
    /// <summary>Cyclomatic complexity of the file (C# only).</summary>
    public int? CyclomaticComplexity { get; init; }

    /// <summary>Maximum nesting depth found in the file (C# only).</summary>
    public int? MaxNestingDepth { get; init; }

    /// <summary>Public API signatures extracted by Roslyn (C# only).</summary>
    public List<string> PublicApiSignatures { get; init; } = new();
}
