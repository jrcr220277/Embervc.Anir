using Anir.Application.Common.Interfaces;
using Anir.Data;
using Anir.Data.Entities;
using Anir.Infrastructure.Settings; // <-- USAMOS TU CLASE
using Microsoft.Extensions.Options;

namespace Anir.Infrastructure.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly ApplicationDbContext _db;
    private readonly string _rootPath;

    // CORREGIDO: Inyectamos tu clase Settings en lugar de IConfiguration
    public FileStorageService(ApplicationDbContext db, IOptions<FileStorageSettings> settings)
    {
        _db = db;

        // Si es relativa, la combinamos con el directorio base. Si es absoluta (C:\...), la usa tal cual.
        var path = settings.Value.RootPath;
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), path);
        }

        _rootPath = path;

        if (!Directory.Exists(_rootPath))
            Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredFile> SaveAsync(Stream stream, string originalName, string folder, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_rootPath, folder, fileName);

        var directory = Path.GetDirectoryName(fullPath)!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream, ct);
        }

        var fileInfo = new FileInfo(fullPath);
        var mimeType = GetMimeType(extension);

        var storedFile = new StoredFile
        {
            FileName = fileName,
            OriginalName = originalName,
            MimeType = mimeType,
            SizeBytes = fileInfo.Length,
            Folder = folder,
            UploadedAt = DateTime.UtcNow
        };

        _db.StoredFiles.Add(storedFile);
        await _db.SaveChangesAsync(ct);

        return storedFile;
    }

    public async Task DeleteAsync(int fileId, CancellationToken ct = default)
    {
        // CORREGIDO: new object[] { fileId } en lugar de new[] { fileId }
        var storedFile = await _db.StoredFiles.FindAsync(new object[] { fileId }, ct);
        if (storedFile is null) return;

        var fullPath = Path.Combine(_rootPath, storedFile.Folder, storedFile.FileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        _db.StoredFiles.Remove(storedFile);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(Stream Stream, string MimeType, string OriginalName)?> ReadAsync(int fileId, CancellationToken ct = default)
    {
        // CORREGIDO: new object[] { fileId } en lugar de new[] { fileId }
        var storedFile = await _db.StoredFiles.FindAsync(new object[] { fileId }, ct);
        if (storedFile is null) return null;

        var fullPath = Path.Combine(_rootPath, storedFile.Folder, storedFile.FileName);
        if (!File.Exists(fullPath)) return null;

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return (stream, storedFile.MimeType, storedFile.OriginalName);
    }

    private static string GetMimeType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        _ => "application/octet-stream"
    };
}