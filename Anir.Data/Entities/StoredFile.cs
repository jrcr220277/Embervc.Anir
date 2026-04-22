namespace Anir.Data.Entities;

/// <summary>
/// Registro centralizado de cada archivo almacenado en disco.
/// Todas las entidades que manejan archivos (AnirWork, Person, etc.)
/// referencian esta tabla mediante FK (ImageFileId, PdfFileId, etc.).
/// </summary>
public class StoredFile
{
    public int Id { get; set; }

    /// <summary>
    /// Nombre generado en disco (UUID + extensión). Ej: "a1b2c3d4-5678.jpg"
    /// Nunca se expone al usuario final.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Nombre original que el usuario seleccionó. Ej: "foto_trabajo.jpg"
    /// </summary>
    public string OriginalName { get; set; } = null!;

    /// <summary>
    /// Tipo MIME del archivo. Ej: "image/png", "application/pdf"
    /// </summary>
    public string MimeType { get; set; } = null!;

    /// <summary>
    /// Tamaño del archivo en bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Carpeta relativa dentro del storage. Ej: "images/avatars", "docs/works"
    /// Determina la ubicación física en disco.
    /// </summary>
    public string Folder { get; set; } = null!;

    /// <summary>
    /// Fecha y hora en UTC cuando se subió el archivo.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}