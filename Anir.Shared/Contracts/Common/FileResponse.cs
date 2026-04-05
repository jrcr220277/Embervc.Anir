using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Anir.Shared.Contracts.Common;

public class FileResponse
{
    public string Id { get; set; } = default!; // Identificador único del archivo en tu storage
    public string Url { get; set; } = default!; // Enlace para mostrar/descargar el archivo
    public string Name { get; set; } = default!; // Nombre original del archivo
    public long Size { get; set; } // Tamaño en bytes
    public string Type { get; set; } = default!; // Tipo MIME (ej: image/png, application/pdf)
}
