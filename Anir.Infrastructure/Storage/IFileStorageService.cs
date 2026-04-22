using Anir.Data.Entities;

namespace Anir.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Guarda el archivo en disco y retorna el registro StoredFile creado.
    /// </summary>
    Task<StoredFile> SaveAsync(Stream stream, string originalName, string folder, CancellationToken ct = default);

    /// <summary>
    /// Elimina el archivo de disco y el registro de la base de datos.
    /// </summary>
    Task DeleteAsync(int fileId, CancellationToken ct = default);

    /// <summary>
    /// Abre el stream de lectura del archivo desde disco.
    /// </summary>
    Task<(Stream Stream, string MimeType, string OriginalName)?> ReadAsync(int fileId, CancellationToken ct = default);
}