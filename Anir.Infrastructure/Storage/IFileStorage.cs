namespace Anir.Infrastructure.Storage;

/// <summary>
/// Contrato para el servicio de almacenamiento de archivos.
/// Define operaciones de subida temporal, commit final, obtención y eliminación.
/// Basado en streams para eficiencia y escalabilidad.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Guarda un archivo en la carpeta temporal.
    /// Se usa para previsualización antes de confirmar el formulario.
    /// </summary>
    Task<string> SaveTempAsync(Stream stream, string extension);

    /// <summary>
    /// Mueve un archivo temporal a su carpeta final (images/docs).
    /// Se ejecuta cuando el usuario guarda el formulario.
    /// </summary>
    Task<string> CommitAsync(string tempFileName, string finalFolder);

    /// <summary>
    /// Elimina un archivo de cualquier carpeta (temp/images/docs).
    /// </summary>
    Task<bool> DeleteAsync(string fileName, string folder);

    /// <summary>
    /// Obtiene un archivo como stream para ser servido por la API.
    /// </summary>
    Task<(Stream Stream, string ContentType, string FileName)?> GetAsync(string fileName, string folder);
}

