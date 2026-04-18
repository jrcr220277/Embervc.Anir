using System;
using System.Collections.Generic;
using System.Text;

namespace Anir.Shared.Contracts.Files;

/// <summary>
/// Representa la solicitud para finalizar la subida de un archivo temporal.
/// Se usa para mover un archivo desde la carpeta temporal hacia su carpeta final.
/// </summary>
public class FileFinalizeRequest
{
    /// <summary>
    /// Identificador del archivo temporal generado durante la subida.
    /// </summary>
    public string TempId { get; set; } = null!;

    /// <summary>
    /// Carpeta destino donde se moverá el archivo (images, docs, etc.).
    /// </summary>
    public string Folder { get; set; } = null!;
}
