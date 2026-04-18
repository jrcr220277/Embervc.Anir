using Anir.Infrastructure.Settings;
using Anir.Infrastructure.Storage;
using Microsoft.Extensions.Options;

/// <summary>
/// Implementación del sistema de almacenamiento de archivos.
/// Usa carpetas privadas fuera de wwwroot, siguiendo buenas prácticas de seguridad.
/// Maneja carpetas temporales y finales.
/// </summary>
public class FileStorageService : IFileStorage
{
    private readonly string _root;
    private readonly string _temp;
    private readonly string _images;
    private readonly string _docs;

    /// <summary>
    /// Inicializa rutas y garantiza que las carpetas existan.
    /// </summary>
    public FileStorageService(IOptions<FileStorageSettings> options)
    {
        _root = options.Value.RootPath;
        _temp = Path.Combine(_root, options.Value.TempFolder);
        _images = Path.Combine(_root, options.Value.ImagesFolder);
        _docs = Path.Combine(_root, options.Value.DocsFolder);

        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_temp);
        Directory.CreateDirectory(_images);
        Directory.CreateDirectory(_docs);
    }

    /// <inheritdoc />
    public async Task<string> SaveTempAsync(Stream stream, string extension)
    {
        string fileName = $"{Guid.NewGuid()}{extension}";
        string path = Path.Combine(_temp, fileName);

        await using var fs = new FileStream(path, FileMode.Create);
        await stream.CopyToAsync(fs);

        return fileName;
    }

    /// <inheritdoc />
    public async Task<string> CommitAsync(string tempFileName, string finalFolder)
    {
        string source = Path.Combine(_temp, tempFileName);
        if (!File.Exists(source))
            throw new FileNotFoundException("Archivo temporal no encontrado.");

        string targetFolder = finalFolder.ToLower() switch
        {
            "images" => _images,
            "docs" => _docs,
            _ => throw new ArgumentException("Carpeta final inválida.")
        };

        string target = Path.Combine(targetFolder, tempFileName);

        File.Move(source, target);

        return tempFileName;
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string fileName, string folder)
    {
        string folderPath = folder.ToLower() switch
        {
            "images" => _images,
            "docs" => _docs,
            "temp" => _temp,
            _ => throw new ArgumentException("Carpeta inválida.")
        };

        string path = Path.Combine(folderPath, fileName);

        if (File.Exists(path))
        {
            File.Delete(path);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<(Stream Stream, string ContentType, string FileName)?> GetAsync(string fileName, string folder)
    {
        string folderPath = folder.ToLower() switch
        {
            "images" => _images,
            "docs" => _docs,
            _ => throw new ArgumentException("Carpeta inválida.")
        };

        string path = Path.Combine(folderPath, fileName);

        if (!File.Exists(path))
            return null;

        string contentType = GetMimeType(path);

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

        return (stream, contentType, fileName);
    }

    /// <summary>
    /// Determina el MIME type según la extensión del archivo.
    /// </summary>
    private static string GetMimeType(string path)
    {
        return Path.GetExtension(path).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}

