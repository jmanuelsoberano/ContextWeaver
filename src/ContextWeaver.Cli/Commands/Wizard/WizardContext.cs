using System.Collections.Generic;
using System.IO;

namespace ContextWeaver.Cli.Commands.Wizard;

/// <summary>
///     Specifies the mode for selecting sections in the interactive wizard.
/// </summary>
public enum SectionSelectionMode
{
    /// <summary>
    ///     Use sections saved in the configuration file, or defaults if none exist.
    /// </summary>
    SavedOrDefault,

    /// <summary>
    ///     Select all available report sections.
    /// </summary>
    All,

    /// <summary>
    ///     Select none of the sections initially, allowing manual selection.
    /// </summary>
    None
}

/// <summary>
///     Holds the state for the wizard execution across different steps.
/// </summary>
public class WizardContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WizardContext"/> class.
    /// </summary>
    /// <param name="settings">The initial settings provided to the command.</param>
    /// <param name="directory">The root directory being analyzed.</param>
    public WizardContext(WizardSettings settings, DirectoryInfo directory)
    {
        Settings = settings;
        Directory = directory;
        DiscoveredFiles = new List<FileInfo>();
        ManagedFiles = new List<FileInfo>();
        SelectedFiles = new List<FileInfo>();
        EnabledSections = new List<string>();
    }

    /// <summary>
    ///     Gets the settings provided by the user via command line.
    /// </summary>
    public WizardSettings Settings { get; }

    /// <summary>
    ///     Gets the root directory for the analysis.
    /// </summary>
    public DirectoryInfo Directory { get; }

    /// <summary>
    ///     Gets or sets the original full list of discovered files before filtering.
    /// </summary>
    public List<FileInfo> DiscoveredFiles { get; set; }

    /// <summary>
    ///     Gets or sets the list of all files managed by the analyzer in the directory (active filtered list).
    /// </summary>
    public List<FileInfo> ManagedFiles { get; set; }

    /// <summary>
    ///     Gets or sets the system configuration loaded from the directory.
    /// </summary>
    public ContextWeaver.Core.AnalysisSettings? Config { get; set; }

    /// <summary>
    ///     Gets or sets the files selected by the user to be included in the context.
    /// </summary>
    public List<FileInfo> SelectedFiles { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to select all files by default in the selection step.
    /// </summary>
    public bool SelectAllFilesByDefault { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether all optional sections should be selected.
    /// </summary>
    public SectionSelectionMode ModeForSections { get; set; }

    /// <summary>
    ///     Gets or sets the names of the report sections enabled by the user.
    /// </summary>
    public List<string> EnabledSections { get; set; }

    /// <summary>
    ///     Gets or sets the output file name.
    /// </summary>
    public string? OutputFileName { get; set; }

    /// <summary>
    ///     Gets or sets the output format.
    /// </summary>
    public string? OutputFormat { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether a "Back" option should be displayed in the current step.
    ///     This is dynamically managed by the WizardOrchestrator.
    /// </summary>
    public bool ShowBackButton { get; set; }
}
