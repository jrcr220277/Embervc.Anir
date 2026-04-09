namespace Anir.Data.Entities
{
    /// <summary>
    /// Representa un municipio dentro de una provincia,
    /// incluyendo su nombre, si es capital provincial
    /// y las empresas asociadas.
    /// </summary>
    public class Municipality
    {
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la provincia a la que pertenece el municipio.
        /// </summary>
        public int ProvinceId { get; set; }

        /// <summary>
        /// Nombre del municipio.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Indica si el municipio es la capital de la provincia.
        /// </summary>
        public bool IsProvinceCapital { get; set; } = false;

        /// <summary>
        /// Provincia a la que pertenece el municipio.
        /// </summary>
        public Province Province { get; set; } = null!;

        /// <summary>
        /// Empresas ubicadas en este municipio.
        /// </summary>
        public ICollection<Company>? Companies { get; set; }
        public ICollection<Ueb>? Uebs { get; set; }
    }
}
