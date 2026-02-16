namespace ContextWeaver.Tests.Helpers;

/// <summary>
///     Helper IDisposable que crea un archivo temporal con la extensión deseada
///     y lo elimina al salir del scope. Evita el patrón repetitivo
///     File.Move + try/finally/File.Delete en tests de I/O.
/// </summary>
public sealed class TempFile : IDisposable
{
    public string Path { get; }

    public TempFile(string extension, string content = "")
    {
        var tempPath = System.IO.Path.GetTempFileName();
        Path = System.IO.Path.ChangeExtension(tempPath, extension);

        if (tempPath != Path)
            File.Move(tempPath, Path, overwrite: true);

        if (!string.IsNullOrEmpty(content))
            File.WriteAllText(Path, content);
    }

    public void Dispose()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }
}
