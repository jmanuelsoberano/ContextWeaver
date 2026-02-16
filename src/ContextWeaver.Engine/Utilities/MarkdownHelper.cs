using System.Text.RegularExpressions;

namespace ContextWeaver.Utilities;

/// <summary>
///     BUENA PRÁCTICA: Clase de Utilidad Estática.
///     Encapsula una lógica muy específica y reutilizable: la creación de anclas de Markdown.
///     Esto mantiene la lógica de formato fuera del generador de reportes, siguiendo el SRP.
/// </summary>
public static class MarkdownHelper
{
    /// <summary>
    ///     Creates a URL-friendly anchor string from the given text.
    ///     Converts to lowercase, removes special characters, and replaces spaces with hyphens.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>A clean anchor string suitable for Markdown links.</returns>
    public static string CreateAnchor(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var anchor = text.Trim().ToLowerInvariant();
        anchor = Regex.Replace(anchor, @"[^a-z0-9\s-]", string.Empty);
        anchor = Regex.Replace(anchor, @"[\s-]+", "-");
        return anchor.Trim('-');
    }
}
