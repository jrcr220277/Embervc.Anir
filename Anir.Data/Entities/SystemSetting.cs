namespace Anir.Data.Entities
{
    /// <summary>
    /// Representa la configuración general del sistema,
    /// incluyendo nombre, logo y datos de contacto.
    /// </summary>
    public class SystemSetting
    {
        public int Id { get; set; }

        /// <summary>
        /// Nombre del sistema o institución.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del logo almacenado en el sistema.
        /// </summary>
        public string? LogoId { get; set; }

        /// <summary>
        /// Dirección física de la institución.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Teléfono de contacto.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Correo electrónico de contacto.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Sitio web oficial.
        /// </summary>
        public string? Website { get; set; }
    }
}
