using System;
using System.Collections.Generic;
using System.Text;

namespace Anir.Infrastructure.Settings
{
    /// <summary>
    /// Opciones de configuración para el sistema de almacenamiento.
    /// Se cargan desde appsettings.json mediante IOptions.
    /// Permite definir rutas y carpetas sin modificar código.
    /// </summary>
    public class FileStorageSettings
    {
        public string RootPath { get; set; } = null!;
        public string ImagesFolder { get; set; } = "images";
        public string DocsFolder { get; set; } = "docs";
        public string BackupsFolder { get; set; } = "AnirBackups";
    }

}
