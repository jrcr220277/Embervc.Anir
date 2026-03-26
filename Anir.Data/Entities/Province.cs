namespace Anir.Data.Entities
{
    /// <summary>
    /// Representa una provincia dentro del sistema,
    /// incluyendo su nombre corto, nombre completo,
    /// si es capital y sus relaciones con municipios y empresas.
    /// </summary>
    public class Province
    {
        public int Id { get; set; }

        /// <summary>
        /// Nombre corto o abreviado de la provincia.
        /// </summary>
        public string ShortName { get; set; } = null!;

        /// <summary>
        /// Nombre completo de la provincia.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Indica si esta provincia es considerada capital.
        /// </summary>
        public bool IsCapital { get; set; } = false;

        /// <summary>
        /// Municipios pertenecientes a esta provincia.
        /// </summary>
        public ICollection<Municipality>? Municipalities { get; set; }

        /// <summary>
        /// Empresas ubicadas en esta provincia.
        /// </summary>
        public ICollection<Company>? Companies { get; set; }
    }
}
