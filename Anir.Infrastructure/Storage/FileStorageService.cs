using System;
using System.IO;
using System.Threading.Tasks;
using Anir.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Anir.Infrastructure.Storage
{
    public class FileStorageService : IFileStorage
    {
        private readonly string _root;
        private readonly string _images;
        private readonly string _docs;

        public FileStorageService(IOptions<FileStorageSettings> options)
        {
            if (options?.Value == null || string.IsNullOrWhiteSpace(options.Value.RootPath))
                throw new ArgumentException("FileStorageSettings.RootPath no puede ser nulo o vacío.");

            _root = Path.GetFullPath(options.Value.RootPath);
            _images = Path.Combine(_root, options.Value.ImagesFolder);
            _docs = Path.Combine(_root, options.Value.DocsFolder);

            Directory.CreateDirectory(_root);
            Directory.CreateDirectory(_images);
            Directory.CreateDirectory(_docs);
        }

        public async Task<string> SaveAsync(Stream stream, string extension, string folder)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(extension)) throw new ArgumentException("Extension inválida.", nameof(extension));

            if (!extension.StartsWith('.')) extension = "." + extension;

            string fileName = $"{Guid.NewGuid()}{extension}";
            string targetFolder = folder.ToLowerInvariant() switch
            {
                "images" => _images,
                "docs" => _docs,
                _ => throw new ArgumentException("Carpeta inválida.")
            };

            string path = Path.Combine(targetFolder, fileName);
            string full = Path.GetFullPath(path);

            if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Ruta fuera del storage.");

            await using var fs = new FileStream(full, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await stream.CopyToAsync(fs);
            return fileName;
        }

        public Task<bool> DeleteAsync(string fileName, string folder)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName inválido.", nameof(fileName));

            string folderPath = folder.ToLowerInvariant() switch
            {
                "images" => _images,
                "docs" => _docs,
                _ => throw new ArgumentException("Carpeta inválida.")
            };

            string full = Path.GetFullPath(Path.Combine(folderPath, fileName));
            if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Ruta fuera del storage.");

            if (File.Exists(full))
            {
                File.Delete(full);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<(Stream Stream, string ContentType, string FileName)?> GetAsync(string fileName, string folder)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName inválido.", nameof(fileName));

            string folderPath = folder.ToLowerInvariant() switch
            {
                "images" => _images,
                "docs" => _docs,
                _ => throw new ArgumentException("Carpeta inválida.")
            };

            string full = Path.GetFullPath(Path.Combine(folderPath, fileName));
            if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Ruta fuera del storage.");

            if (!File.Exists(full))
                return Task.FromResult<(Stream, string, string)?>(null);

            string contentType = GetMimeTypeByExtension(Path.GetExtension(full));
            var stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);

            return Task.FromResult<(Stream, string, string)?>((stream, contentType, fileName));
        }

        // Simple, self-contained MIME mapping to avoid extra package references.
        // Extend this map if you need more types.
        private static string GetMimeTypeByExtension(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return "application/octet-stream";

            extension = extension.ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                _ => "application/octet-stream"
            };
        }
    }
}
