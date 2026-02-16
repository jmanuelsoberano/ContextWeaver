namespace ContextWeaver.Core;

/// <summary>
///     Typed metrics for a file analysis result.
///     Replaces the previous <c>Dictionary&lt;string, object&gt;</c> to provide
///     compile-time safety and eliminate stringly-typed lookups with runtime casts.
/// </summary>
public class FileMetrics
{
    /// <summary>Cyclomatic complexity of the file (C# only).</summary>
    public int? CyclomaticComplexity { get; set; }

    /// <summary>Maximum nesting depth found in the file (C# only).</summary>
    public int? MaxNestingDepth { get; set; }

    /// <summary>Public API signatures extracted by Roslyn (C# only).</summary>
    public List<string> PublicApiSignatures { get; set; } = new();
}
