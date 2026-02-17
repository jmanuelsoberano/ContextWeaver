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
    ///     Crea una cadena de ancla compatible con URL a partir del texto dado.
    ///     Convierte a minúsculas, elimina caracteres especiales y reemplaza espacios con guiones.
    /// </summary>
    /// <param name="text">El texto a convertir.</param>
    /// <returns>Una cadena de ancla limpia adecuada para enlaces Markdown.</returns>
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
